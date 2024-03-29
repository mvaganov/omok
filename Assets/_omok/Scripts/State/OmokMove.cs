using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	public interface IOmokMove {
		public Sprite GetIcon(OmokGame game);
		public Color GetColor(OmokGame game);
	}

	[System.Serializable]
	public class OmokMove : IOmokMove {
		public Coord coord;
		public byte player;
		public byte piece;

		public UnitState UnitState => player switch {
			0 => UnitState.Player0,
			1 => UnitState.Player1,
			_ => UnitState.Unknown
		};

		public static readonly OmokMove InvalidMove = new OmokMove(Coord.MIN, 255);

		public OmokMove(Coord coord, byte player, byte piece) {
			this.coord = coord; this.player = player; this.piece = piece;
		}

		public OmokMove(Coord coord, byte player) : this(coord, player, 0) { }

		public bool Equals(OmokMove other) => !ReferenceEquals(other, null) && coord == other.coord && player == other.player;
		public override bool Equals(object obj) => obj is OmokMove omok && Equals(omok);
		public override int GetHashCode() => coord.GetHashCode() ^ player;
		public static bool operator==(OmokMove a, OmokMove b) => operator_equals(a, b);
		public static bool operator!=(OmokMove a, OmokMove b) => !operator_equals(a, b);
		private static bool operator_equals(OmokMove a, OmokMove b) => ReferenceEquals(a, b) ||
			(ReferenceEquals(a, null) == ReferenceEquals(b, null) && a.Equals(b));
		public override string ToString() => $"({UnitState}:{coord})";

		public Sprite GetIcon(OmokGame game) => game.players[player].gamePieces[piece].Icon;
		public Color GetColor(OmokGame game) => game.players[player].gamePieces[piece].color;
	}
}
