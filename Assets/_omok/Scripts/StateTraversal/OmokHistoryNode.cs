using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Omok {
	[Serializable]
	public class OmokHistoryNode {
		/// <summary>
		/// the Edge structure of this graph
		/// </summary>
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
		/// <summary>
		/// Managed state
		/// </summary>
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

		public bool IsDoneCalculating(OmokMove move) {
			OmokHistoryNode alreadyExistingNode = GetMove(move);
			if (alreadyExistingNode != null) {
				return !alreadyExistingNode.analysis.IsDoingAnalysis && alreadyExistingNode.analysis.scoring != null;
			}
			return false;
		}

		public NextStateMovementResult AddMoveIfNotAlreadyCalculating(OmokMove move, Action<OmokMove> whatToDoWhenMoveCalculationFinishes, MonoBehaviour coroutineRunner) {
			/// if the move is already here, AddCallBackOnFinish, return false
			MovePath nextPath = new MovePath(move);
			int index = Array.BinarySearch(movePaths, nextPath, MovePath.Comparer.Instance);
			if (index >= 0) {
				nextPath = movePaths[index];
				OmokHistoryNode alreadyExistingNode = GetMove(index);
				if (!alreadyExistingNode.analysis.IsDoingAnalysis) {
					return NextStateMovementResult.FinishedCalculating;
				}
				return NextStateMovementResult.StillCalculating;
			}
			/// create a new <see cref="MovePath">, start doing analysis of the move, AddCallBackOnFinish
			OmokState nextState = new OmokState(state);
			nextState.TrySetState(move);
			OmokHistoryNode nextNode = new OmokHistoryNode(nextState, this, null);
			nextPath.nextNode = nextNode;
			nextPath.nextNode.analysis.MarkDoingAnalysis(true);
			InsertMove(~index, nextPath);
			coroutineRunner.StartCoroutine(nextPath.nextNode.analysis.AnalyzeCoroutine(
				move, nextState, whatToDoWhenMoveCalculationFinishes));
			if (index >= 0) {
				Debug.LogError($"{nextPath.move.coord} already in list? {index}");
				return NextStateMovementResult.Error;
			}
			return NextStateMovementResult.StartedCalculating;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="move"></param>
		/// <returns></returns>
		public bool AddMoveReplaceIfItExists(MovePath move) {
			int index = Array.BinarySearch(movePaths, move, MovePath.Comparer.Instance);
			if (index >= 0) {
				movePaths[index] = move;
				return true;
			}
			InsertMove(~index, move);
			return false;
		}

		public void InsertMove(int index, MovePath move) {
			Array.Resize(ref movePaths, movePaths.Length + 1);
			for (int i = movePaths.Length - 1; i > index; --i) {
				movePaths[i] = movePaths[i - 1];
			}
			movePaths[index] = move;
		}

		public int GetMoveIndex(OmokMove move) => Array.IndexOf(movePaths, move);

		public OmokHistoryNode GetMove(OmokMove move) {
			int index = GetMoveIndex(move);
			if (index < 0) {
				return null;
			}
			return GetMove(index);
		}

		public OmokHistoryNode GetMove(int index) {
			return movePaths[index].nextNode;
		}

		public float[] CalculateScoreRangeForPaths(byte player) {
			float[] minmax = new float[] { float.MaxValue, float.MinValue };
			for (int i = 0; i < movePaths.Length; ++i) {
				OmokHistoryNode.MovePath movepath = movePaths[i];
				float[] scoring = movepath.nextNode.analysis.scoring;
				if (movepath.nextNode.analysis.IsDoingAnalysis) {
					//Debug.Log($"skipping {movepath.move.coord}");
					continue;
				}
				float score = OmokStateAnalysis.SummarizeScore(player, scoring);
				//Debug.Log($"{movepath.move.coord} {score}     {scoring[0]} v {scoring[1]}");
				if (score < minmax[0]) { minmax[0] = score; }
				if (score > minmax[1]) { minmax[1] = score; }
			}
			return minmax;
		}
	}
}
