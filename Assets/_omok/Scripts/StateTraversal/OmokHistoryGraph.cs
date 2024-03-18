using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Omok {
  public class OmokHistoryGraph {
		public enum NextStateMovementResult {
			Success, StillCalculating
		}

		public List<OmokHistoryNode> historyNodes = new List<OmokHistoryNode>();
		public OmokHistoryNode currentNode;

		private Dictionary<OmokMove, List<Action<OmokMove>>> actionToDoWhenCalculationFinishes
			= new Dictionary<OmokMove, List<Action<OmokMove>>>();

		public void CreateNewRoot(OmokState state, MonoBehaviour coroutineRunner, Action<OmokMove> onComplete) {
			currentNode = new OmokHistoryNode(state, null, null);
			historyNodes.Add(currentNode);
			AddActionWhenMoveAnalysisFinishes(OmokMove.InvalidMove, onComplete);
			coroutineRunner.StartCoroutine(currentNode.analysis.AnalyzeCoroutine(OmokMove.InvalidMove, state, FinishedAnalysis));
		}

		private void FinishedAnalysis(OmokMove move) {
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

		public bool DoMoveCalculation(Coord coord, MonoBehaviour coroutineRunner, System.Action<OmokMove> onComplete, byte currentPlayer) {
			bool validMove = currentNode != null && currentNode.state != null &&
				currentNode.state.GetState(coord) == UnitState.None;
			if (!validMove) {
				return false;
			}
			OmokMove move = new OmokMove(coord, currentPlayer);
			AddActionWhenMoveAnalysisFinishes(move, onComplete);
			if (currentNode.AddMoveIfNotAlreadyCalculating(move, FinishedAnalysis, coroutineRunner)) {
				return true;
			}
			return false;
		}

		public float[] GetMoveScoringSummary(OmokMove move, out float netScore) {
			OmokHistoryNode node = currentNode.GetMove(move);
			if (node.analysis.IsDoingAnalysis) {
				netScore = 0;
				return null;
			}
			float[] currentScore = currentNode.analysis.scoring;
			float[] nextStateScores = Sub(node.analysis.scoring, currentScore);
			netScore = OmokStateAnalysis.SummarizeScore(move.player, nextStateScores);
			return nextStateScores;
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

		public NextStateMovementResult AdvanceMove(OmokMove move, MonoBehaviour coroutineRunner, Action<OmokMove> onComplete, byte player) {
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
			DoMoveCalculation(move.coord, coroutineRunner, onComplete, player);
			return NextStateMovementResult.StillCalculating;
		}
	}
}
