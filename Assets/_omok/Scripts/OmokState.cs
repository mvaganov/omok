using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class OmokState {
	public enum UnitState {
		None,    // 00
		Unknown, // 01
		Player0, // 10
		Player1, // 11
	}

	protected Vector2Int start, size;
	protected BitArray serialized;

	private static readonly Vector2Int MAX = new Vector2Int(int.MaxValue, int.MaxValue);
	private static readonly Vector2Int MIN = new Vector2Int(int.MinValue, int.MinValue);
	private const int ElementPlayer = 0;
	private const int ElementPiece = 1;
	private const int ElementBitCount = 2;

	public OmokState() { }

	public OmokState(OmokState toCopy) {
		start = toCopy.start;
		size = toCopy.size;
		serialized = new BitArray(toCopy.serialized);
	}
	public OmokState(Dictionary<Vector2Int, OmokPiece> map) {
		SetState(map);
	}

	public void SetState(Dictionary<Vector2Int, OmokPiece> map) {
		List<OmokPiece> pieces = new List<OmokPiece>();
		pieces.AddRange(map.Values);
		OmokState.CalculateCoordRange(pieces, GetCoordFromPiece, out Vector2Int min, out Vector2Int max);
		OmokState.SortPieces(pieces, GetCoordFromPiece);
		//Debug.Log(pieces.Count+" "+string.Join("\n", pieces));
		start = min;
		size = max - min + Vector2Int.one;
		int i = 0;
		serialized = new BitArray(size.x * size.y * ElementBitCount);
		if (pieces.Count > 0) {
			OmokGame game = null;
			Vector2Int nextPosition = pieces[i].Coord;
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
						SetState(new Vector2Int(col, row), state);
						if (i < pieces.Count - 1) {
							nextPosition = pieces[++i].Coord;
							if (i >= pieces.Count) {
								break;
							}
						}
					} else {
						SetState(new Vector2Int(col, row), UnitState.None);
					}
				}
			}
			//Debug.Log(DebugSerialized());
		}
	}

	public string DebugSerialized() {
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < serialized.Count; ++i) {
			if (i % 2 == 0) { sb.Append(" "); }
			sb.Append(serialized[i] ? '!' : '.');
		}
		return sb.ToString();
	}

	public UnitState GetState(Vector2Int coord) {
		Debug.Log("get: " + coord + " start:" + start + "   local:" + (coord - start));
		return GetLocalState(coord - start);
	}

	public void SetState(Vector2Int coord, UnitState unitState) {
		//Debug.Log("set: "+coord + " " +unitState +"   start:" + start + "   local:" + (coord - start)); 
		SetLocalState(coord - start, unitState);
	}

	private UnitState GetLocalState(Vector2Int localCoord) {
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

	private void SetLocalState(Vector2Int localCoord, UnitState state) {
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

	public void ForEachPiece(System.Action<Vector2Int, UnitState> action) {
		Vector2Int cursor = start;
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

	public static void CalculateCoordRange<T>(List<T> pieces, System.Func<T, Vector2Int> getCoord, out Vector2Int _min, out Vector2Int _max) {
		Vector2Int min = MAX, max = MIN;
		pieces.ForEach(a => {
			Vector2Int coord = getCoord(a);
			min.x = Mathf.Min(min.x, coord.x);
			min.y = Mathf.Min(min.y, coord.y);
			max.x = Mathf.Max(max.x, coord.x);
			max.y = Mathf.Max(max.y, coord.y);
		});
		if (min == MAX && max == MIN) {
			_min = _max = Vector2Int.zero;
		} else {
			_min = min;
			_max = max;
		}
	}

	public static void SortPieces<T>(List<T> pieces, System.Func<T, Vector2Int> getCoord) {
		pieces.Sort((a, b) => {
			Vector2Int coordA = getCoord(a), coordB = getCoord(b);
			if (coordA.y < coordB.y) { return 1; } else if (coordA.y > coordB.y) { return -1; }
			if (coordA.x < coordB.x) { return -1; } else if (coordA.x > coordB.x) { return 1; }
			return 0;
		});
	}

	public static Vector2Int GetCoordFromPiece(OmokPiece piece) => piece.Coord;

	public static string ToString(Dictionary<Vector2Int, OmokPiece> map, char empty = '.') {
		List<OmokPiece> pieces = new List<OmokPiece>();
		pieces.AddRange(map.Values);
		OmokState.CalculateCoordRange(pieces, GetCoordFromPiece, out Vector2Int min, out Vector2Int max);
		OmokState.SortPieces(pieces, GetCoordFromPiece);
		Debug.Log(pieces.Count + "\n" + string.Join("\n", pieces.ConvertAll(t => t.Coord + ":" + t.name)));
		StringBuilder sb = new StringBuilder();
		sb.Append(pieces.Count + ", " + min + "," + max);
		int i = 0;
		if (pieces.Count > 0) {
			Vector2Int nextPosition = pieces[i].Coord;// GetCoord(pieces[i].position);
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
