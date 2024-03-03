using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;

namespace Omok {
	public enum UnitState {
		None,    // 00
		Unknown, // 01
		Player0, // 10
		Player1, // 11
	}

	public struct Unit {
		public UnitState unitState;
		public Coord coord;
		public Unit(UnitState unitState, Coord coord) {
			this.unitState = unitState;
			this.coord = coord;
		}
	}

	public interface IOmokState {
		public bool TryGetState(Coord coord, out UnitState state);
		public bool TrySetState(Coord coord, UnitState unitState);
		public void ForEachPiece(Action<Coord, UnitState> action);
		public IEnumerator ForEachPiece(Action<Coord, UnitState> action, Action onForLoopComplete);
		public Coord size { get; }
		public Coord start { get; }
		public void Copy(IOmokState source);

		public static UnitState GetStateFrom(OmokPiece piece) {
			int playerIndex = -1;
			UnitState state = UnitState.None;
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

		public static int SortPiecesInvertRow(OmokPiece a, OmokPiece b) {
			Coord coordA = GetCoordFromPiece(a), coordB = GetCoordFromPiece(b);
			if (coordA.y < coordB.y) { return 1; } else if (coordA.y > coordB.y) { return -1; }
			if (coordA.x < coordB.x) { return -1; } else if (coordA.x > coordB.x) { return 1; }
			return 0;
		}

		public static int SortUnitsInvertRow(Unit a, Unit b) {
			Coord coordA = GetCoordFromUnit(a), coordB = GetCoordFromUnit(b);
			if (coordA.y < coordB.y) { return 1; } else if (coordA.y > coordB.y) { return -1; }
			if (coordA.x < coordB.x) { return -1; } else if (coordA.x > coordB.x) { return 1; }
			return 0;
		}

		public static Coord GetCoordFromPiece(OmokPiece piece) => piece.Coord;
		public static Coord GetCoordFromUnit(Unit unit) => unit.coord;
	}

	public static class IOmokState_Extension {
		public static Dictionary<UnitState, char> textOutputTable = new Dictionary<UnitState, char>() {
			[UnitState.Player0] = 'X',
			[UnitState.Player1] = '0',
			[UnitState.None] = '_',
			[UnitState.Unknown] = '?',
		};
		
		public static UnitState GetState(this IOmokState self, Coord coord) {
			self.TryGetState(coord, out UnitState state);
			return state;
		}

		public static string ToDebugString(this IOmokState self) {
			List<StringBuilder> lines = new List<StringBuilder>();
			int count = 0;
			for (int row = 0; row < self.size.y; ++row) {
				lines.Add(new StringBuilder());
				for (int col = 0; col < self.size.x; ++col) {
					Coord coord = new Coord(col, row) + self.start;
					UnitState state = self.GetState(coord);
					char c = textOutputTable[state];
					lines[row].Append(c);//.Append(' ').Append(coord).Append(' ');
					if (state != UnitState.None) {
						++count;
					}
				}
			}
			StringBuilder sb = new StringBuilder();
			sb.Append(count + "," + self.start + "," + self.size);
			for (int row = lines.Count - 1; row >= 0; --row) {
				sb.Append("\n");
				sb.Append(lines[row].ToString());
			}
			return sb.ToString();
		}
	}

	[Serializable]
	public class OmokState : IOmokState {
		private IOmokState dataBackstop;

		public Coord start => dataBackstop.start;
		public Coord size => dataBackstop.size;

		public bool TryGetState(Coord coord, out UnitState state) => dataBackstop.TryGetState(coord, out state);

		public bool TrySetState(Coord coord, UnitState unitState) {
			if (dataBackstop.TrySetState(coord, unitState)) {
				return true;
			}
			ConvertToDynamicState();
			return TrySetState(coord, unitState);
		}

		public void ForEachPiece(Action<Coord, UnitState> action) => dataBackstop.ForEachPiece(action);

		public IEnumerator ForEachPiece(Action<Coord, UnitState> action, Action onForLoopComplete) =>
			dataBackstop.ForEachPiece(action, onForLoopComplete);

		public OmokState() {
			dataBackstop = new OmokState_Archived();
		}

		public OmokState(IOmokState source) => Copy(source);

		public void Copy(IOmokState source) {
			dataBackstop = new OmokState_Archived(source);
		}

		/// <summary>
		/// call this method to collapse the state data structure into it's likely most memory efficient form.
		/// </summary>
		public void Archive() {
			switch (dataBackstop) {
				case OmokState_Dictionary:
					ConvertDictionaryToSerializedState();
					break;
			}
		}

		public void SetState(Dictionary<Coord, OmokPiece> map) {
			switch (dataBackstop) {
				case OmokState_Archived archive:
					archive.SetState(map);
					break;
				default:
					dataBackstop = new OmokState_Archived(map);
					break;
			}
		}

		public string DebugSerialized() {
			switch (dataBackstop) {
				case OmokState_Archived archive:
					return archive.DebugSerialized();
			}
			return "non-serialized databackstop";
		}

		protected void ConvertToDynamicState() {
			dataBackstop = new OmokState_Dictionary(dataBackstop);
		}

		protected void ConvertDictionaryToSerializedState() {
			dataBackstop = new OmokState_Archived(dataBackstop);
		}

		public static Coord GetCoordFromPiece(OmokPiece piece) => piece.Coord;

		public static string ToString(Dictionary<Coord, OmokPiece> map, char empty = '_') {
			List<OmokPiece> pieces = new List<OmokPiece>();
			pieces.AddRange(map.Values);
			Coord.CalculateCoordRange(pieces, GetCoordFromPiece, out Coord min, out Coord max);
			pieces.Sort(IOmokState.SortPiecesInvertRow);
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
}
