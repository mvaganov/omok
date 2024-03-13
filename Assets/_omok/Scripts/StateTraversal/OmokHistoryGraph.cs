using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
  public class OmokHistoryGraph {
		public List<OmokHistoryNode> historyNodes = new List<OmokHistoryNode>();
		public OmokHistoryNode currentNode;

		public void CreateNewRoot(OmokState state, MonoBehaviour coroutineRunner, System.Action<OmokMove> onComplete) {
			currentNode = new OmokHistoryNode(state, null, null);
			historyNodes.Add(currentNode);
			coroutineRunner.StartCoroutine(currentNode.analysis.AnalyzeCoroutine(OmokMove.InvalidMove, state, onComplete));// RefreshStateVisuals));
		}

		public bool DoMoveCalculation(Coord coord, MonoBehaviour coroutineRunner, System.Action<OmokMove> onComplete, byte currentPlayer) {
			bool validMove = currentNode != null && currentNode.state != null &&
				currentNode.state.GetState(coord) == UnitState.None;
			if (!validMove) {
				return false;
			}

			OmokMove move = new OmokMove(coord, currentPlayer);
			if (currentNode.AddMove(move, onComplete, coroutineRunner)) {
				return true;
			}
			return false;
		}

		public float[] GetMoveScoringSummary(byte player, OmokMove move, out float netScore) {
			OmokHistoryNode node = currentNode.GetMove(move);
			if (node.analysis.IsDoingAnalysis) {
				netScore = 0;
				return null;
			}
			float[] currentScore = currentNode.analysis.scoring;
			float[] nextStateScores = Sub(node.analysis.scoring, currentScore);
			netScore = OmokStateAnalysis.SummarizeScore(player, nextStateScores);
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
	}
}
