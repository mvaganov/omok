using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Omok {
	[Serializable]
	public class OmokHistoryNode {
		public struct Move {
			public OmokMove move;
			public OmokHistoryNode nextState;
			public bool Equals(Move other) {
				return other.move == move;
			}
			public bool Equals(OmokMove other) {
				return other == move;
			}
			public override bool Equals(object obj) {
				switch (obj) {
					case OmokMove omok: return Equals(omok);
					case Move move: return Equals(move);
				}
				return false;
			}
			override public int GetHashCode() => move.GetHashCode();
			public static bool operator ==(Move left, Move right) => left.Equals(right);
			public static bool operator !=(Move left, Move right) => !left.Equals(right);
		}

		public OmokHistoryNode parentNode;
		public OmokState state;
		public OmokStateAnalysis analysis;
		public Move[] moves = Array.Empty<Move>();

		public void AddMove(Move move) {
			InsertMove(moves.Length, move);
		}

		public void InsertMove(int index, Move move) {
			Array.Resize(ref moves, moves.Length + 1);
			for (int i = moves.Length - 1; i > index; --i) {
				moves[i] = moves[i - 1];
			}
			moves[index] = move;
		}

		public int GetMoveIndex(OmokMove move) => Array.IndexOf(moves, move);
	}
}
