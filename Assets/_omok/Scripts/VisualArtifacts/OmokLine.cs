using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	[System.Serializable]
	public struct OmokLine {
		public Coord start;
		public Coord direction;
		public byte length;
		public byte player;
		public byte count;

		public Coord Last => start + (direction * (length - 1));

		public OmokLine(Coord start, Coord direction, byte length, byte player) {
			this.start = start; this.direction = direction; this.length = length; this.player = player; this.count = 0;
			if (direction.x < 0 || (direction.x == 0 && direction.y < 0)) {
				Invert();
			}
		}

		public bool Update(OmokState state) {
			count = 0;
			UnitState compliant, opposing;
			switch (player) {
				case 0:
					compliant = UnitState.Player0;
					opposing = UnitState.Player1;
					break;
				case 1:
					compliant = UnitState.Player1;
					opposing = UnitState.Player0;
					break;
				default:
					throw new System.Exception("unacceptable player");
			}
			bool valid = true;
			int countPiecesInLine = 0;
			//ForEachTest(c => {
			//	OmokState.UnitState unitState = state.GetState(c);
			//	if (unitState == opposing) {
			//		valid = false;
			//	} else if (unitState == compliant) {
			//		++countPiecesInLine;
			//	}
			//	return false;
			//});

			Coord cursor = start;
			for (int i = 0; i < length; i++) {
				UnitState unitState = state.GetState(cursor);
				//Debug.Log(state.TryGetState(cursor, out OmokState.UnitState found) + $" {cursor} {found}");
				if (unitState == opposing) {
					valid = false;
				} else if (unitState == compliant) {
					++countPiecesInLine;
				}
				cursor += direction;
			}

			count = (byte)countPiecesInLine;
			return valid;
		}

		private void Invert() {
			start = Last;
			direction = -direction;
		}

		public bool Contains(Coord point) {
			if (start == point) {
				return true;
			}
			return ForEachTest(c => c == point);
		}

		private bool ForEachTest(System.Func<Coord, bool> action) {
			Coord cursor = start;
			for (int i = 0; i < length; i++) {
				if (action.Invoke(cursor)) {
					return true;
				}
				cursor += direction;
			}
			return false;
		}

		public bool Equals(OmokLine other) {
			if (length != other.length) { return false; }
			if (start == other.start && direction == other.direction) {
				return true;
			}
			Coord invertedDir = -direction;
			if (invertedDir == other.direction) {
				Coord switchedPoint = start + (direction * length);
				if (switchedPoint == other.start) {
					return true;
				}
			}
			return false;
		}

		public override string ToString() => $"[{start}:{direction}*{count}/{length})";
		public override int GetHashCode() => start.GetHashCode() ^ direction.GetHashCode() ^ length ^ player;
		public override bool Equals(object obj) => obj is OmokLine line && Equals(line);
		public static bool operator ==(OmokLine a, OmokLine b) => a.Equals(b);
		public static bool operator !=(OmokLine a, OmokLine b) => !a.Equals(b);
	}
}
