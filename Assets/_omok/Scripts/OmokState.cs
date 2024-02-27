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

	private static Dictionary<UnitState, char> textOutputTable = new Dictionary<UnitState, char>() {
		[UnitState.Player0] = 'X',
		[UnitState.Player1] = '0',
		[UnitState.None] = '_',
		[UnitState.Unknown] = '?',
	};

	public struct Unit {
		public UnitState unitState;
		public Coord coord;
		public Unit(UnitState unitState, Coord coord) {
			this.unitState = unitState;
			this.coord = coord;
		}
	}

	[SerializeField]
	protected Coord start, size;
	[SerializeField]
	protected BitArray serialized;
	protected Dictionary<Coord, UnitState> stateMap = null;

	protected Coord Max => start + size - Coord.one;

	public OmokState() { }

	public OmokState(OmokState toCopy) {
		start = toCopy.start;
		size = toCopy.size;
		serialized = new BitArray(toCopy.serialized);
	}

	public void Archive() {
		if (stateMap != null) {
			ConvertDictionaryToSerializedState();
			stateMap = null;
		}
	}

	public OmokState(Dictionary<Coord, OmokPiece> map) {
		SetStateSerialized(map);
	}

	public void SetStateSerialized(Dictionary<Coord, OmokPiece> map) {
		List<OmokPiece> pieces = new List<OmokPiece>();
		pieces.AddRange(map.Values);
		Coord.CalculateCoordRange(pieces, GetCoordFromPiece, out Coord min, out Coord max);
		pieces.Sort(SortPiecesInvertRow);
		//OmokState.SortPieces(pieces, GetCoordFromPiece);
		//Debug.Log(pieces.Count+" "+string.Join("\n", pieces));
		SetStateSerialized(pieces, min, max, GetCoordFromPiece, GetState);
	}

	private static UnitState GetState(OmokPiece piece) {
		int playerIndex = -1;
		OmokState.UnitState state = UnitState.None;
		OmokGame game = piece.Player.Game;
		if (game == null || (playerIndex = game.GetPlayerIndex(piece.Player)) < 0) {
			state = UnitState.Unknown;
		}
		switch (playerIndex) {
			case 0: state = UnitState.Player0; break;
			case 1: state = UnitState.Player1; break;
		}
		return state;
	}

	public void SetStateSerialized<T>(List<T> pieces, Coord min, Coord max,
		System.Func<T, Coord> getCoord, System.Func<T, UnitState> getState) {
		serialized = new BitArray(size.x * size.y * ElementBitCount);
		start = min;
		size = max - min + Coord.one;
		int i = 0;
		serialized = new BitArray(size.x * size.y * ElementBitCount);
		if (pieces.Count > 0) {
			Coord nextPosition = getCoord(pieces[i]);
			for (int row = max.y; row >= min.y; --row) {
				for (int col = min.x; col <= max.x; ++col) {
					if (row == nextPosition.y && col == nextPosition.x) {
						OmokState.UnitState state = getState(pieces[i]);
						SetState(new Coord(col, row), state);
						if (i < pieces.Count - 1) {
							nextPosition = getCoord(pieces[++i]);
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

	protected void PopulateDictionary() {
		stateMap = new Dictionary<Coord, UnitState>();
		ForEachPiece((coord, unitState) => {
			stateMap[coord] = unitState;
		});
	}

	protected void ConvertDictionaryToSerializedState() {
		SetStateSerialized(stateMap);
	}

	public void SetStateSerialized(Dictionary<Coord, UnitState> map) {
		List<Unit> pieces = new List<Unit>();
		foreach(KeyValuePair<Coord, UnitState> kvp in map) {
			pieces.Add(new Unit(kvp.Value, kvp.Key));
		}
		Coord.CalculateCoordRange(pieces, GetCoordFromUnit, out Coord min, out Coord max);
		pieces.Sort(SortUnitsInvertRow);
		SetStateSerialized(pieces, min, max, GetCoordFromUnit, GetState);
	}

	private UnitState GetState(Unit unit) => unit.unitState;

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

	public UnitState GetState(Coord coord) {
		TryGetState(coord, out UnitState state);
		return state;
	}

	public bool TryGetState(Coord coord, out UnitState state) {
		if (stateMap != null) {
			return stateMap.TryGetValue(coord, out state);
		}
		//Debug.Log("get: " + coord + " start:" + start + "   local:" + (coord - start));
		Coord local = coord - start;
		if (local.x < 0 || local.y < 0 || local.x >= size.x || local.y >= size.y) {
			state = UnitState.None;
			//Debug.Log($"{coord} ({local}) is bad");
			return false;
		}
		state = GetLocalStateSerialized(local);
		return true;
	}

	public void SetState(Coord coord, UnitState unitState) {
		if (stateMap != null) {
			stateMap[coord] = unitState;
			if (coord.x < start.x) {
				int delta = start.x - coord.x;
				start.x = coord.x;
				size.x += delta;
			}
			if (coord.y < start.y) {
				int delta = start.y - coord.y;
				start.y = coord.y;
				size.y += delta;
			}
			Coord max = Max;
			if (coord.x > max.x) {
				size.x += coord.x - max.x;
			}
			if (coord.y > max.y) {
				size.y += coord.y - max.y;
			}
			return;
		}
		Coord local = coord - start;
		if (local.x < 0 || local.y < 0 || local.x >= size.x || local.y >= size.y) {
			PopulateDictionary();
			SetState(coord, unitState);
			return;
		}
		//Debug.Log("set: "+coord + " " +unitState +"   start:" + start + "   local:" + (coord - start)); 
		SetLocalStateSerialized(local, unitState);
	}

	private UnitState GetLocalStateSerialized(Coord localCoord) {
		int index = localCoord.y * size.x + localCoord.x;
		return GetLocalSerializedState(index);
	}

	private UnitState GetLocalSerializedState(int index) {
		int i = index * ElementBitCount;
		bool isPiece = serialized[i + ElementPiece];
		bool isPlayer = serialized[i + ElementPlayer];
		UnitState state = (UnitState)(byte)((isPiece ? 1 << ElementPiece : 0) | (isPlayer ? 1 << ElementPlayer : 0));
		//if (state == UnitState.Player0 || state == UnitState.Player1) {
		//	Debug.Log("index: " + i + "/" + serialized.Count + "   " + state);
		//}
		return state;
	}

	private void SetLocalStateSerialized(Coord localCoord, UnitState state) {
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
		if (stateMap != null) {
			foreach(KeyValuePair<Coord, UnitState> kvp in stateMap) {
				action(kvp.Key, kvp.Value);
			}
			return;
		}
		Coord cursor = start;
		int horizontalLimit = start.x + size.x;
		int boardSize = serialized.Count / ElementBitCount;
		for(int i = 0; i < boardSize; ++i) {
			UnitState state = GetLocalSerializedState(i);
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

	private static int SortUnitsInvertRow(Unit a, Unit b) {
		Coord coordA = GetCoordFromUnit(a), coordB = GetCoordFromUnit(b);
		if (coordA.y < coordB.y) { return 1; } else if (coordA.y > coordB.y) { return -1; }
		if (coordA.x < coordB.x) { return -1; } else if (coordA.x > coordB.x) { return 1; }
		return 0;
	}

	public static Coord GetCoordFromPiece(OmokPiece piece) => piece.Coord;
	public static Coord GetCoordFromUnit(Unit unit) => unit.coord;

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
	}
}
