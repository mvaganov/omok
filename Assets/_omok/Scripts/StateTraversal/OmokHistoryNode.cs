using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Omok {
	[Serializable]
	public class OmokHistoryNode {
		public struct MovePath {
			public OmokMove move;
			public OmokHistoryNode nextNode;
			public MovePath(OmokMove move, OmokHistoryNode nextState) {
				this.move = move;
				this.nextNode = nextState;
			}
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
		public MovePath[] movePaths = Array.Empty<MovePath>();

		public OmokHistoryNode(OmokState state, OmokHistoryNode parentNode, OmokStateAnalysis analysis) {
			this.state = state;
			this.analysis = analysis;
			if (this.analysis == null) {
				this.analysis = new OmokStateAnalysis(state);
			}
			this.parentNode = parentNode;
		}

		// TODO test this method
		public bool AddMove(OmokMove move, Action whatToDoWhenMoveCalculationFinishes, MonoBehaviour coroutineRunner) {
			/// if the move is already here, AddCallBackOnFinish, return false
			int found = GetMoveIndex(move);
			if (found >= 0) {
				movePaths[found].nextNode.analysis.AddCallBackOnFinish(whatToDoWhenMoveCalculationFinishes);
				return false;
			}
			/// create a new <see cref="MovePath">, start doing analysis of the move, AddCallBackOnFinish
			OmokState nextState = new OmokState(state);
			nextState.TrySetState(move);
			OmokHistoryNode nextNode = new OmokHistoryNode(nextState, this, null);
			MovePath nextPath = new MovePath(move, nextNode);
			coroutineRunner.StartCoroutine(nextPath.nextNode.analysis.AnalyzeCoroutine(nextState, whatToDoWhenMoveCalculationFinishes);
			return true;
		}

		private void AddMove(MovePath move) {
			InsertMove(movePaths.Length, move);
		}

		public void InsertMove(int index, MovePath move) {
			Array.Resize(ref movePaths, movePaths.Length + 1);
			for (int i = movePaths.Length - 1; i > index; --i) {
				movePaths[i] = movePaths[i - 1];
			}
			movePaths[index] = move;
		}

		public int GetMoveIndex(OmokMove move) => Array.IndexOf(movePaths, move);
	}
}
