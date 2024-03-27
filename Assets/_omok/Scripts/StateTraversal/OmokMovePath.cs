using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	/// <summary>
	/// the Edge structure of this graph
	/// </summary>
	public struct OmokMovePath {
		public OmokMove move;
		public OmokHistoryNode nextNode;
		public OmokMovePath(OmokMove move, OmokHistoryNode nextNode) {
			this.move = move;
			this.nextNode = nextNode;
		}
		public OmokMovePath(OmokMove move) {
			this.move = move;
			this.nextNode = null;
		}
		public bool Equals(OmokMovePath other) {
			return other.move == move;
		}
		public bool Equals(OmokMove other) {
			return other == move;
		}
		public override bool Equals(object obj) {
			switch (obj) {
				case OmokMove omok: return Equals(omok);
				case OmokMovePath move: return Equals(move);
			}
			return false;
		}
		override public int GetHashCode() => move.GetHashCode();
		public static bool operator ==(OmokMovePath left, OmokMovePath right) => left.Equals(right);
		public static bool operator !=(OmokMovePath left, OmokMovePath right) => !left.Equals(right);
		public class Comparer : IComparer<OmokMovePath> {
			public int Compare(OmokMovePath a, OmokMovePath b) => a.move.coord.CompareTo(b.move.coord);
			public static Comparer Instance = new Comparer();
		}
		public static OmokMovePath None = new OmokMovePath(null, null);
	}
}
