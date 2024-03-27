using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Omok {
	public enum NextStateMovementResult {
		Success, StartedCalculating, StillCalculating, FinishedCalculating, Error
	}

	public class OmokHistoryGraph {
		public List<OmokHistoryNode> historyNodes = new List<OmokHistoryNode>();
		public OmokHistoryNode currentNode;

		private Dictionary<OmokMove, List<Action<OmokMove>>> actionToDoWhenCalculationFinishes
			= new Dictionary<OmokMove, List<Action<OmokMove>>>();

		public void CreateNewRoot(OmokState state, MonoBehaviour coroutineRunner, Action<OmokMove> onComplete) {
			currentNode = new OmokHistoryNode(state, null, null, null);
			historyNodes.Add(currentNode);
			AddActionWhenMoveAnalysisFinishes(OmokMove.InvalidMove, onComplete);
			coroutineRunner.StartCoroutine(currentNode.analysis.AnalyzeCoroutine(OmokMove.InvalidMove, state, FinishedAnalysis));
		}

		private void FinishedAnalysis(OmokMove move) {
			//Debug.Log("FINISHED " + move);
			if (!actionToDoWhenCalculationFinishes.TryGetValue(move, out List<Action<OmokMove>> actions)) {
				return;
			}
			actionToDoWhenCalculationFinishes.Remove(move);
			actions.ForEach(a => a.Invoke(move));
			actions.Clear();
		}

		private bool AddActionWhenMoveAnalysisFinishes(OmokMove move, Action<OmokMove> action) {
			if (action == null) { return false; }
			if (!actionToDoWhenCalculationFinishes.TryGetValue(move, out List<Action<OmokMove>> actions)) {
				actions = new List<Action<OmokMove>>();
			}
			if (actions.IndexOf(action) < 0) {
				actions.Add(action);
			} else {
				return false;
			}
			actionToDoWhenCalculationFinishes[move] = actions;
			return true;
		}

		public bool IsDoneCalculating(OmokMove move) {
			return currentNode.IsDoneCalculating(move);
		}

		public NextStateMovementResult DoMoveCalculation(OmokMove move, MonoBehaviour coroutineRunner, Action<OmokMove> onComplete) {
			bool validMove = currentNode != null && currentNode.state != null &&
				currentNode.state.GetState(move.coord) == UnitState.None;
			if (!validMove) {
				return NextStateMovementResult.Error;
			}
			AddActionWhenMoveAnalysisFinishes(move, onComplete);
			return currentNode.AddMoveIfNotAlreadyCalculating(move, FinishedAnalysis, coroutineRunner);
		}

		public float[] GetMoveScoringSummary(OmokMove move, out float netScore) {
			OmokHistoryNode node = currentNode.GetMove(move);
			if (node == null || node.analysis.IsDoingAnalysis) {
				netScore = 0;
				return null;
			}
			//float[] currentScore = currentNode.analysis.scoring;
			float[] nextScore = node.analysis.scoring;
			//float[] nextStateDelta = Sub(nextScore, currentScore);
			//netScore = OmokStateAnalysis.SummarizeScore(move.player, nextStateDelta);
			netScore = 0;
			for (int i = 0; i < nextScore.Length; i++) {
				netScore += nextScore[i] * (i == move.player ? 1 : -1);
			}
			return nextScore;// nextStateDelta;
		}

		private static float[] Sub(float[] a, float[] b) {
			float[] answer = (float[])a.Clone();
			if (b != null) {
				for (int i = 0; i < a.Length; i++) {
					if (i >= b.Length) { break; }
					answer[i] = a[i] - b[i];
				}
			}
			return answer;
		}

		public NextStateMovementResult AdvanceMove(OmokMove move, MonoBehaviour coroutineRunner, Action<OmokMove> onComplete) {
			OmokHistoryNode nextNode = currentNode.GetMove(move);
			if (nextNode != null) {
				if (!nextNode.analysis.IsDoingAnalysis) {
					currentNode = nextNode;
					onComplete?.Invoke(move);
					return NextStateMovementResult.Success;
				}
				AddActionWhenMoveAnalysisFinishes(move, onComplete);
				return NextStateMovementResult.StillCalculating;
			}
			DoMoveCalculation(move, coroutineRunner, onComplete);
			return NextStateMovementResult.StillCalculating;
		}
	}
}
