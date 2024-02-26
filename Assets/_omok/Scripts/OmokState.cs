using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class OmokState {
	private const int ElementPlayer = 0;
	private const int ElementPiece = 1;
	private const int ElementBitCount = 2;

	public enum UnitState {
		None,    // 00
		Unknown, // 01
		Player0, // 10
		Player1, // 11
	}

	[SerializeField]
	protected Coord start, size;
	[SerializeField]
	protected BitArray serialized;

	protected Coord Max => start + size - Coord.one;

	public OmokState() { }

	public OmokState(OmokState toCopy) {
		start = toCopy.start;
		size = toCopy.size;
		serialized = new BitArray(toCopy.serialized);
	}
	public OmokState(Dictionary<Coord, OmokPiece> map) {
		SetState(map);
	}

	public void SetState(Dictionary<Coord, OmokPiece> map) {
		List<OmokPiece> pieces = new List<OmokPiece>();
		pieces.AddRange(map.Values);
		Coord.CalculateCoordRange(pieces, GetCoordFromPiece, out Coord min, out Coord max);
		pieces.Sort(SortPiecesInvertRow);
		//OmokState.SortPieces(pieces, GetCoordFromPiece);
		//Debug.Log(pieces.Count+" "+string.Join("\n", pieces));
		start = min;
		size = max - min + Coord.one;
		int i = 0;
		serialized = new BitArray(size.x * size.y * ElementBitCount);
		if (pieces.Count > 0) {
			OmokGame game = null;
			Coord nextPosition = pieces[i].Coord;
			for (int row = max.y; row >= min.y; --row) {
				for (int col = min.x; col <= max.x; ++col) {
					if (row == nextPosition.y && col == nextPosition.x) {
						OmokPiece piece = pieces[i];
						int playerIndex = -1;
						OmokState.UnitState state = UnitState.None;
						if (game == null) {
							game = piece.Player.Game;
						}
						if (game == null || (playerIndex = game.GetPlayerIndex(piece.Player)) < 0) {
							state = UnitState.Unknown;
						}
						switch(playerIndex) {
							case 0: state = UnitState.Player0; break;
							case 1: state = UnitState.Player1; break;
						}
						SetState(new Coord(col, row), state);
						if (i < pieces.Count - 1) {
							nextPosition = pieces[++i].Coord;
							if (i >= pieces.Count) {
								break;
							}
						}
					} else {
						SetState(new Coord(col, row), UnitState.None);
					}
				}
			}
			Debug.Log(DebugSerialized());
			Debug.Log(ToDebugString());
		}
	}

	private static Dictionary<UnitState, char> textOutputTable = new Dictionary<UnitState, char>() {
		[UnitState.Player0] = 'X',
		[UnitState.Player1] = '0',
		[UnitState.None] = '_',
		[UnitState.Unknown] = '?',
	};

	public string ToDebugString() {
		List<StringBuilder> lines = new List<StringBuilder>();
		int count = 0;
		for(int row = 0; row < size.y; ++row) {
			lines.Add(new StringBuilder());
			for(int col = 0; col < size.x; ++col) {
				Coord coord = new Coord(col, row) + start;
				UnitState state = GetState(coord);
				char c = textOutputTable[state];
				lines[row].Append(c);//.Append(' ').Append(coord).Append(' ');
				if (state != UnitState.None) {
					++count;
				}
			}
		}
		StringBuilder sb = new StringBuilder();
		sb.Append(count + "," + start + "," + size);
		for (int row = lines.Count-1; row >= 0; --row) {
			sb.Append("\n");
			sb.Append(lines[row].ToString());
		}
		return sb.ToString();
	}

	public string DebugSerialized() {
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < serialized.Count; ++i) {
			if (i % 2 == 0) { sb.Append(" "); }
			sb.Append(serialized[i] ? '!' : '.');
		}
		return sb.ToString();
	}

	public bool TryGetState(Coord coord, out UnitState state) {
		//Debug.Log("get: " + coord + " start:" + start + "   local:" + (coord - start));
		Coord local = coord - start;
		Coord max = Max;
		if (local.x < 0 || local.y < 0 || local.x >= size.x || local.y >= size.y) {
			state = UnitState.None;
			//Debug.Log($"{coord} ({local}) is bad");
			return false;
		}
		state = GetLocalState(local);
		return true;
	}

	public UnitState GetState(Coord coord) {
		TryGetState(coord, out UnitState state);
		return state;
	}

	public void SetState(Coord coord, UnitState unitState) {
		//Debug.Log("set: "+coord + " " +unitState +"   start:" + start + "   local:" + (coord - start)); 
		SetLocalState(coord - start, unitState);
	}

	private UnitState GetLocalState(Coord localCoord) {
		int index = localCoord.y * size.x + localCoord.x;
		return GetLocalState(index);
	}

	public UnitState GetLocalState(int index) {
		int i = index * ElementBitCount;
		bool isPiece = serialized[i + ElementPiece];
		bool isPlayer = serialized[i + ElementPlayer];
		UnitState state = (UnitState)(byte)((isPiece ? 1 << ElementPiece : 0) | (isPlayer ? 1 << ElementPlayer : 0));
		//if (state == UnitState.Player0 || state == UnitState.Player1) {
		//	Debug.Log("index: " + i + "/" + serialized.Count + "   " + state);
		//}
		return state;
	}

	private void SetLocalState(Coord localCoord, UnitState state) {
		int index = localCoord.y * size.x + localCoord.x;
		int i = index * ElementBitCount;
		//if (state == UnitState.Player0 || state == UnitState.Player1) {
		//	Debug.Log($"index: {i}/{serialized.Count}    {state} {(localCoord+start)}");
		//}
		byte code = (byte)state;
		bool isPiece = ((1 << ElementPiece) & code) != 0;
		bool isPlayer = ((1 << ElementPlayer) & code) != 0;
		serialized[i + ElementPiece] = isPiece;
		serialized[i + ElementPlayer] = isPlayer;
	}

	public void ForEachPiece(System.Action<Coord, UnitState> action) {
		Coord cursor = start;
		int horizontalLimit = start.x + size.x;
		int boardSize = serialized.Count / ElementBitCount;
		for(int i = 0; i < boardSize; ++i) {
			UnitState state = GetLocalState(i);
			//Debug.Log($"foreach: {i} {cursor}{state}");
			if (state != UnitState.None) {
				action.Invoke(cursor, state);
			}
			cursor.x++;
			if (cursor.x >= horizontalLimit) {
				cursor.y++;
				cursor.x = start.x;
			}
		}
	}

	private static int SortPiecesInvertRow(OmokPiece a, OmokPiece b) {
		Coord coordA = GetCoordFromPiece(a), coordB = GetCoordFromPiece(b);
		if (coordA.y < coordB.y) { return 1; } else if (coordA.y > coordB.y) { return -1; }
		if (coordA.x < coordB.x) { return -1; } else if (coordA.x > coordB.x) { return 1; }
		return 0;
	}

	public static Coord GetCoordFromPiece(OmokPiece piece) => piece.Coord;

	public static string ToString(Dictionary<Coord, OmokPiece> map, char empty = '_') {
		List<OmokPiece> pieces = new List<OmokPiece>();
		pieces.AddRange(map.Values);
		Coord.CalculateCoordRange(pieces, GetCoordFromPiece, out Coord min, out Coord max);
		pieces.Sort(SortPiecesInvertRow);
		//SortPieces(pieces, GetCoordFromPiece);
		Debug.Log(pieces.Count + "\n" + string.Join("\n", pieces.ConvertAll(t => t.Coord + ":" + t.name)));
		Coord size = max - min + Coord.one;
		StringBuilder sb = new StringBuilder();
		sb.Append(pieces.Count + "," + min + "," + size);
		int i = 0;
		if (pieces.Count > 0) {
			Coord nextPosition = pieces[i].Coord;
			for (int row = max.y; row >= min.y; --row) {
				sb.Append("\n");
				for (int col = min.x; col <= max.x; ++col) {
					if (row == nextPosition.y && col == nextPosition.x) {
						OmokPiece piece = pieces[i];
						char c = piece.Player != null ? piece.Player.gamePieces[piece.Index].Character[0] : empty;
						sb.Append(c);
						if (i < pieces.Count - 1) {
							nextPosition = pieces[++i].Coord;
							if (i >= pieces.Count) {
								break;
							}
						}
					} else {
						sb.Append(empty);
					}
				}
			}
		}
		return sb.ToString();
		//return ToString(pieces, min, size, empty);
	}
	//public static string ToString(IEnumerable<OmokPiece> pieces, Coord min, Coord size, char empty = '_') {
	//	Coord max = min + size - Coord.one;
	//	List<StringBuilder> lines = new List<StringBuilder>();
	//	int count = 0;
	//	IEnumerator<OmokPiece> pieceEnumerator = pieces.GetEnumerator();
	//	if (pieceEnumerator.MoveNext()) {
	//		Debug.Log("PIECE "+pieceEnumerator.Current);
	//		Coord nextPosition = pieceEnumerator.Current.Coord;
	//		for (int row = min.y; row <= max.y; ++row) {
	//			lines.Add(new StringBuilder());
	//			int currentLine = lines.Count - 1;
	//			for (int col = min.x; col <= max.x; ++col) {
	//				if (row == nextPosition.y && col == nextPosition.x) {
	//					OmokPiece piece = pieceEnumerator.Current;
	//					char c = piece.Player != null ? piece.Player.gamePieces[piece.Index].Character[0] : empty;
	//					lines[currentLine].Append(c);
	//					++count;
	//					if (!pieceEnumerator.MoveNext()) {
	//						break;
	//					}
	//					Debug.Log("PIECE " + pieceEnumerator.Current);
	//					//if (i < pieces.Count - 1) {
	//					//	nextPosition = pieces[++i].Coord;
	//					//	if (i >= pieces.Count) {
	//					//		break;
	//					//	}
	//					//}
	//				} else {
	//					lines[currentLine].Append(empty);
	//				}
	//			}
	//		}
	//	}
	//	StringBuilder sb = new StringBuilder();
	//	sb.Append(count + "," + min + "," + size);
	//	//for (int row = lines.Count - 1; row >= 0; --row) {
	//	for (int row = 0; row < lines.Count; ++row) {
	//		sb.Append("\n");
	//		sb.Append(lines[row]);
	//	}
	//	return sb.ToString();
	//}
}
