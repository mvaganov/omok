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
			public MovePath(OmokMove move, OmokHistoryNode nextNode) {
				this.move = move;
				this.nextNode = nextNode;
			}
			public MovePath(OmokMove move) {
				this.move = move;
				this.nextNode = null;
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
			public class Comparer : IComparer<MovePath> {
				public int Compare(MovePath a, MovePath b) => a.move.coord.CompareTo(b.move.coord);
				public static Comparer Instance = new Comparer();
			}
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
		public bool AddMove(OmokMove move, Action<OmokMove> whatToDoWhenMoveCalculationFinishes, MonoBehaviour coroutineRunner) {
			/// if the move is already here, AddCallBackOnFinish, return false
			MovePath nextPath = new MovePath(move);
			int index = Array.BinarySearch(movePaths, nextPath, MovePath.Comparer.Instance);
			//int found = GetMoveIndex(move);
			if (index >= 0) {
				movePaths[index].nextNode.analysis.AddCallBackOnFinish(whatToDoWhenMoveCalculationFinishes);
				return false;
			}
			/// create a new <see cref="MovePath">, start doing analysis of the move, AddCallBackOnFinish
			OmokState nextState = new OmokState(state);
			nextState.TrySetState(move);
			OmokHistoryNode nextNode = new OmokHistoryNode(nextState, this, null);
			nextPath.nextNode = nextNode;
			coroutineRunner.StartCoroutine(nextPath.nextNode.analysis.AnalyzeCoroutine(
				move, nextState, whatToDoWhenMoveCalculationFinishes));
			if (index >= 0) {
				Debug.LogError($"{nextPath.move.coord} already in list? {index}");
				return false;
			} else {
				Debug.LogWarning($"{nextPath.move.coord} @ {~index}");
			}
			InsertMove(~index, nextPath);
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
