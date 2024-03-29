using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Omok {
	[Serializable]
	public class OmokHistoryNode {
		public int turnValue;
		public OmokMove sourceMove = null;
		public OmokHistoryNode parentNode;
		/// <summary>
		/// Managed state
		/// </summary>
		public OmokState state;
		public OmokStateAnalysis analysis;
		public OmokMovePath[] movePaths = Array.Empty<OmokMovePath>();
		public bool traversed;

		public int Turn => turnValue;
		public bool Traversed => traversed;
		public int GetEdgeCount() => movePaths.Length;

		public OmokHistoryNode(OmokState state, OmokHistoryNode parentNode, OmokStateAnalysis analysis, OmokMove sourceMove) {
			this.state = state;
			this.analysis = analysis;
			if (this.analysis == null) {
				this.analysis = new OmokStateAnalysis(state);
			}
			this.parentNode = parentNode;
			this.sourceMove = sourceMove;
			if (parentNode != null) {
				turnValue = parentNode.turnValue + 1;
			}
		}

		public OmokHistoryNode FindRoot() {
			OmokHistoryNode cursor = this;
			while (cursor.parentNode != null) {
				cursor = cursor.parentNode;
			}
			return cursor;
		}

		public bool IsDoneCalculating(OmokMove move) {
			OmokHistoryNode alreadyExistingNode = GetMove(move);
			if (alreadyExistingNode != null) {
				return !alreadyExistingNode.analysis.IsDoingAnalysis && alreadyExistingNode.analysis.scoring != null;
			}
			return false;
		}

		public NextStateMovementResult AddMoveIfNotAlreadyCalculating(OmokMove move,
			Action<OmokHistoryNode> whatToDoWhenMoveCalculationFinishes, MonoBehaviour coroutineRunner, out OmokHistoryNode nextNode) {
			/// if the move is already here, AddCallBackOnFinish, return false
			OmokMovePath nextPath = new OmokMovePath(move);
			int index = Array.BinarySearch(movePaths, nextPath, OmokMovePath.Comparer.Instance);
			if (index >= 0) {
				nextPath = movePaths[index];
				nextNode = nextPath.nextNode;// GetMove(index);
				if (!nextNode.analysis.IsDoingAnalysis) {
					return NextStateMovementResult.FinishedCalculating;
				}
				return NextStateMovementResult.StillCalculating;
			}
			/// create a new <see cref="OmokMovePath">, start doing analysis of the move, AddCallBackOnFinish
			OmokState nextState = new OmokState(state);
			nextState.TrySetState(move);
			nextNode = new OmokHistoryNode(nextState, this, null, move);
			nextPath.nextNode = nextNode;
			nextPath.nextNode.analysis.MarkDoingAnalysis(true);
			InsertMove(~index, nextPath);
			coroutineRunner.StartCoroutine(nextNode.analysis.AnalyzeCoroutine(nextNode,
				whatToDoWhenMoveCalculationFinishes));
			if (index >= 0) {
				Debug.LogError($"{nextPath.move.coord} already in list? {index}");
				return NextStateMovementResult.Error;
			}
			return NextStateMovementResult.StartedCalculating;
		}

		public OmokMovePath GetEdge(int i) => movePaths[i];

		/// <summary>
		/// 
		/// </summary>
		/// <param name="move"></param>
		/// <returns></returns>
		public bool AddMoveReplaceIfItExists(OmokMovePath move) {
			int index = Array.BinarySearch(movePaths, move, OmokMovePath.Comparer.Instance);
			if (index >= 0) {
				movePaths[index] = move;
				return true;
			}
			InsertMove(~index, move);
			return false;
		}

		public void InsertMove(int index, OmokMovePath move) {
			Array.Resize(ref movePaths, movePaths.Length + 1);
			for (int i = movePaths.Length - 1; i > index; --i) {
				movePaths[i] = movePaths[i - 1];
			}
			movePaths[index] = move;
		}

		public int GetMoveIndex(OmokMove move) => Array.IndexOf(movePaths, move);

		public int GetMoveIndex(OmokHistoryNode nextNode) => Array.FindIndex(movePaths, edge => edge.nextNode == nextNode);

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

		public MinMax CalculateScoreRangeForPaths(byte player) {
			MinMax minmax = MinMax.Impossible;
			for (int i = 0; i < movePaths.Length; ++i) {
				 OmokMovePath movepath = movePaths[i];
				float[] scoring = movepath.nextNode.analysis.scoring;
				if (movepath.nextNode.analysis.IsDoingAnalysis) {
					//Debug.Log($"skipping {movepath.move.coord}");
					continue;
				}
				float score = OmokStateAnalysis.SummarizeScore(player, scoring);
				//Debug.Log($"{movepath.move.coord} {score}     {scoring[0]} v {scoring[1]}");
				minmax.Update(score);
			}
			return minmax;
		}

		public override string ToString() => $"{Turn}. {{{sourceMove} : {analysis}}}";
	}
}
