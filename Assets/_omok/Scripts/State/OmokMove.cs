using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	[System.Serializable]
	public class OmokMove {
		public Coord coord;
		public byte player;

		public UnitState UnitState => player switch {
			0 => UnitState.Player0,
			1 => UnitState.Player1,
			_ => UnitState.Unknown
		};

		public static readonly OmokMove InvalidMove = new OmokMove(Coord.MIN, 255);

		public OmokMove(Coord coord, byte player) {
			this.coord = coord; this.player = player;
		}

		public bool Equals(OmokMove other) => coord == other.coord && player == other.player;
		public override bool Equals(object obj) => obj is OmokMove omok && Equals(omok);
		public override int GetHashCode() => coord.GetHashCode() ^ player;
		public static bool operator==(OmokMove a, OmokMove b) => a.Equals(b);
		public static bool operator!=(OmokMove a, OmokMove b) => !a.Equals(b);
		public override string ToString() => $"({UnitState}:{coord})";
	}
}
