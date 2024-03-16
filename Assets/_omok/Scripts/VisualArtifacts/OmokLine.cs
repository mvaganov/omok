using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	[System.Serializable]
	public struct OmokLine : IComparable<OmokLine> {
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
			UnitState mySate;
			switch (player) {
				case 0: mySate = UnitState.Player0; break;
				case 1: mySate = UnitState.Player1; break;
				default: throw new Exception("unacceptable player");
			}
			int countPiecesInLine = 0;
			Coord cursor = start;
			for (int i = 0; i < length; i++) {
				UnitState unitState = state.GetState(cursor);
				cursor += direction;
				if (unitState == UnitState.None) {
					continue;
				} else if (unitState != mySate) {
					return false;
				}
				++countPiecesInLine;
			}
			count = (byte)countPiecesInLine;
			return true;
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

		public void ForEachCoord(System.Action<Coord> action) {
			Coord cursor = start;
			for (int i = 0; i < length; i++) {
				action.Invoke(cursor);
				cursor += direction;
			}
		}

		public static bool Equals(OmokLine a, OmokLine b) {
			if (a.length != b.length) { return false; }
			if (a.start == b.start && a.direction == b.direction) {
				return true;
			}
			Coord invertedDir = -a.direction;
			if (invertedDir == b.direction) {
				Coord switchedPoint = a.start + (a.direction * a.length);
				return switchedPoint == b.start;
			}
			return false;
		}

		public bool Equals(OmokLine other) => Equals(this, other);

		public static int PositionCompareTo(OmokLine a, OmokLine b) {
			int v = a.start.CompareTo(b.start);
			if (v != 0) {
				return v;
			}
			return a.direction.CompareTo(b.direction);
		}

		public static bool PositionLessThan(OmokLine a, OmokLine b) => PositionCompareTo(a, b) < 0;

		public int PositionCompareTo(OmokLine other) => PositionCompareTo(this, other);

		/// <summary>
		/// highest count should be first
		/// </summary>
		public int CompareTo(OmokLine other) => -count.CompareTo(other.count);
		public override string ToString() => $"[{start}:{direction}*{count}/{length})";
		public override int GetHashCode() => start.GetHashCode() ^ direction.GetHashCode() ^ length ^ player;
		public override bool Equals(object obj) => obj is OmokLine line && Equals(line);
		public static bool operator ==(OmokLine a, OmokLine b) => a.Equals(b);
		public static bool operator !=(OmokLine a, OmokLine b) => !a.Equals(b);
	}
}
