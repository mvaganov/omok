using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class OmokState {
	[System.Serializable]
	public struct UnitState {
		public byte player;
		public byte unit;
	}

	private Dictionary<Vector2Int, UnitState> map = new Dictionary<Vector2Int, UnitState>();

	//protected BitArray serialized = new BitArray();

	private static readonly Vector2Int MAX = new Vector2Int(int.MaxValue, int.MaxValue);
	private static readonly Vector2Int MIN = new Vector2Int(int.MinValue, int.MinValue);

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
