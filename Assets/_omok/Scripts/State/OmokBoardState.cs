using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;

namespace Omok {
	public enum OmokUnitState {
		None,    // 00
		Unknown, // 01
		Player0, // 10
		Player1, // 11
	}

	public struct OmokUnit {
		public OmokUnitState unitState;
		public Coord coord;
		public OmokUnit(OmokUnitState unitState, Coord coord) {
			this.unitState = unitState;
			this.coord = coord;
		}
	}

	public interface IOmokBoardState {
		public bool TryGetState(Coord coord, out OmokUnitState state);
		public bool TrySetState(Coord coord, OmokUnitState unitState);
		public void ForEachPiece(Action<Coord, OmokUnitState> action);
		public IEnumerator ForEachPiece(Action<Coord, OmokUnitState> action, Action onForLoopComplete);
		public Coord size { get; }
		public Coord start { get; }
		public void Copy(IOmokBoardState source);
		public bool Equals(IOmokBoardState other);

		public static OmokUnitState GetStateFrom(OmokPiece piece) {
			int playerIndex = -1;
			OmokUnitState state = OmokUnitState.None;
			OmokGame game = piece.Player.omokGame;
			if (game == null || (playerIndex = game.GetPlayerIndex(piece.Player)) < 0) {
				state = OmokUnitState.Unknown;
			}
			switch (playerIndex) {
				case 0: state = OmokUnitState.Player0; break;
				case 1: state = OmokUnitState.Player1; break;
			}
			return state;
		}

		public static int SortPiecesInvertRow(OmokPiece a, OmokPiece b) => 
			Coord.ComparerInverseY.compare(GetCoordFromPiece(a), GetCoordFromPiece(b));

		public static int SortUnitsInvertRow(OmokUnit a, OmokUnit b) =>
			Coord.ComparerInverseY.compare(GetCoordFromUnit(a), GetCoordFromUnit(b));

		public static Coord GetCoordFromPiece(OmokPiece piece) => piece.Coord;
		public static Coord GetCoordFromUnit(OmokUnit unit) => unit.coord;
	}

	public static class IBoardOmokState_Extension {
		public static Dictionary<OmokUnitState, string> textOutputTable = new Dictionary<OmokUnitState, string>() {
			[OmokUnitState.Player0] = "x",
			[OmokUnitState.Player1] = "*",
			[OmokUnitState.None] = "  ",
			[OmokUnitState.Unknown] = "..",
		};
		
		public static OmokUnitState GetState(this IOmokBoardState self, Coord coord) {
			self.TryGetState(coord, out OmokUnitState state);
			return state;
		}

		public static string ToDebugString(this IOmokBoardState self) {
			List<StringBuilder> lines = new List<StringBuilder>();
			int count = 0;
			for (int row = 0; row < self.size.y; ++row) {
				lines.Add(new StringBuilder());
				for (int col = 0; col < self.size.x; ++col) {
					Coord coord = new Coord(col, row) + self.start;
					OmokUnitState state = self.GetState(coord);
					string c = textOutputTable[state];
					lines[row].Append(c);//.Append(' ').Append(coord).Append(' ');
					if (state != OmokUnitState.None) {
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

		public static bool TrySetState(this IOmokBoardState self, OmokMove move) {
			return self.TrySetState(move.coord, move.UnitState);
		}

		public static bool Equals(this IOmokBoardState self, IOmokBoardState other) {
			if (ReferenceEquals(self, other)) { return true; }
			bool allEqual = true;
			self.ForEachPiece((coord, state) => {
				if (allEqual && (!other.TryGetState(coord, out OmokUnitState otherState) || otherState != state)) {
					allEqual = false;
				}
			});
			return allEqual;
		}

		public static int HashCode(this IOmokBoardState self) {
			int value = 0;
			self.ForEachPiece((coord, state) => {
				value ^= coord.GetHashCode() | state.GetHashCode();
			});
			return value;
		}
	}

	[Serializable]
	public class OmokBoardState : IOmokBoardState {
		private IOmokBoardState dataBackstop;
		public Coord start => dataBackstop.start;
		public Coord size => dataBackstop.size;
		public bool Equals(IOmokBoardState other) => IBoardOmokState_Extension.Equals(this, other);
		public override bool Equals(object obj) => obj is IOmokBoardState omokState && IBoardOmokState_Extension.Equals(this, omokState);
		public override int GetHashCode() => IBoardOmokState_Extension.HashCode(this);

		public bool TryGetState(Coord coord, out OmokUnitState state) => dataBackstop.TryGetState(coord, out state);

		public bool TrySetState(Coord coord, OmokUnitState unitState) {
			if (dataBackstop.TrySetState(coord, unitState)) {
				return true;
			}
			ConvertToDynamicState();
			return TrySetState(coord, unitState);
		}

		public void ForEachPiece(Action<Coord, OmokUnitState> action) => dataBackstop.ForEachPiece(action);

		public IEnumerator ForEachPiece(Action<Coord, OmokUnitState> action, Action onForLoopComplete) {
			yield return dataBackstop.ForEachPiece(action, onForLoopComplete);
		}

		public OmokBoardState() {
			dataBackstop = new OmokBoardState_Archived();
		}

		public OmokBoardState(IOmokBoardState source) => Copy(source);

		public void Copy(IOmokBoardState source) {
			dataBackstop = new OmokBoardState_Archived(source);
		}

		/// <summary>
		/// call this method to collapse the state data structure into it's likely most memory efficient form.
		/// </summary>
		public void Archive() {
			switch (dataBackstop) {
				case OmokBoardState_Dictionary:
					ConvertDictionaryToSerializedState();
					break;
			}
		}

		public void SetState(Dictionary<Coord, OmokPiece> map) {
			switch (dataBackstop) {
				case OmokBoardState_Archived archive:
					archive.SetState(map);
					break;
				default:
					dataBackstop = new OmokBoardState_Archived(map);
					break;
			}
		}

		public string DebugSerialized() {
			switch (dataBackstop) {
				case OmokBoardState_Archived archive:
					return archive.DebugSerialized();
			}
			return "non-serialized databackstop";
		}

		protected void ConvertToDynamicState() {
			dataBackstop = new OmokBoardState_Dictionary(dataBackstop);
		}

		protected void ConvertDictionaryToSerializedState() {
			dataBackstop = new OmokBoardState_Archived(dataBackstop);
		}

		public static Coord GetCoordFromPiece(OmokPiece piece) => piece.Coord;

		public static string ToString(Dictionary<Coord, OmokPiece> map, string empty = "  ") {
			List<OmokPiece> pieces = new List<OmokPiece>();
			pieces.AddRange(map.Values);
			Coord.CalculateCoordRange(pieces, GetCoordFromPiece, out Coord min, out Coord max);
			pieces.Sort(IOmokBoardState.SortPiecesInvertRow);
			//SortPieces(pieces, GetCoordFromPiece);
			//Debug.Log(pieces.Count + "\n" + string.Join("\n", pieces.ConvertAll(t => t.Coord + ":" + t.name)));
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
							string c = piece.Player != null ? piece.Player.gamePieces[piece.Index].Character : empty;
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
