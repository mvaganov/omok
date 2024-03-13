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
	}
}
