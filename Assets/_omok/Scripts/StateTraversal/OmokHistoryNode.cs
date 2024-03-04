using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Omok {
	[Serializable]
	public class OmokHistoryNode {
		public struct MovePath {
			public OmokMove move;
			public OmokHistoryNode nextState;
			public bool Equals(MovePath other) {
				return other.move == move;
			}
			public bool Equals(OmokMove other) {
				return other == move;
			}
			public override bool Equals(object obj) {
				switch (obj) {
					case OmokMove omok: return Equals(omok);
					case MovePath move: return Equals(move);
				}
				return false;
			}
			override public int GetHashCode() => move.GetHashCode();
			public static bool operator ==(MovePath left, MovePath right) => left.Equals(right);
			public static bool operator !=(MovePath left, MovePath right) => !left.Equals(right);
		}

		public OmokHistoryNode parentNode;
		public OmokState state;
		public OmokStateAnalysis analysis;
		public MovePath[] moves = Array.Empty<MovePath>();

		public OmokHistoryNode(OmokState state, OmokHistoryNode parentNode, OmokStateAnalysis analysis) {
			this.state = state;
			this.analysis = analysis;
			if (this.analysis == null) {
				this.analysis = new OmokStateAnalysis(state);
			}
			this.parentNode = parentNode;
		}

		public bool AddMove(OmokMove move, Action whatToDoWhenMoveCalculationFinishes) {
			/// TODO if the move is already here, AddCallBackOnFinish, return false
			/// create a new <see cref="MovePath">, start doing analysis of the move, AddCallBackOnFinish
			return true;
		}

		private void AddMove(MovePath move) {
			InsertMove(moves.Length, move);
		}

		public void InsertMove(int index, MovePath move) {
			Array.Resize(ref moves, moves.Length + 1);
			for (int i = moves.Length - 1; i > index; --i) {
				moves[i] = moves[i - 1];
			}
			moves[index] = move;
		}

		public int GetMoveIndex(OmokMove move) => Array.IndexOf(moves, move);
	}
}
