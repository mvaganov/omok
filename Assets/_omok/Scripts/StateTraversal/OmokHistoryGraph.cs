using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Omok {
	public enum NextStateMovementResult {
		Success, StartedCalculating, StillCalculating, FinishedCalculating, Error
	}

	public class OmokHistoryGraph {
		public List<OmokHistoryNode> timeline = new List<OmokHistoryNode>();
		public OmokHistoryNode currentNode;
		public event Action<OmokHistoryGraph> OnNodeChange = delegate { };

		private Dictionary<OmokHistoryNode, List<Action<OmokHistoryNode>>> actionToDoWhenCalculationFinishes
			= new Dictionary<OmokHistoryNode, List<Action<OmokHistoryNode>>>();

		public void CreateNewRoot(byte whosTurnIsItNow, OmokState state, MonoBehaviour coroutineRunner, Action<OmokHistoryNode> onComplete) {
			timeline.Clear();
			currentNode = new OmokHistoryNode(state, null, whosTurnIsItNow, null, null);
			currentNode.traversed = true;
			timeline.Add(currentNode);
			AddActionWhenMoveAnalysisFinishes(currentNode, NotifyCurrentNodeChanged);
			AddActionWhenMoveAnalysisFinishes(currentNode, onComplete);
			coroutineRunner.StartCoroutine(currentNode.analysis.AnalyzeCoroutine(currentNode, FinishedAnalysis));
		}

		public void NotifyCurrentNodeChanged(OmokHistoryNode node) {
			OnNodeChange.Invoke(this);
		}

		private void FinishedAnalysis(OmokHistoryNode node) {
			//Debug.Log("FINISHED " + move);
			if (!actionToDoWhenCalculationFinishes.TryGetValue(node, out List<Action<OmokHistoryNode>> actions)) {
				return;
			}
			actionToDoWhenCalculationFinishes.Remove(node);
			actions.ForEach(a => a.Invoke(node));
			actions.Clear();
		}

		private bool AddActionWhenMoveAnalysisFinishes(OmokHistoryNode node, Action<OmokHistoryNode> action) {
			if (action == null) { return false; }
			if (!actionToDoWhenCalculationFinishes.TryGetValue(node, out List<Action<OmokHistoryNode>> actions)) {
				actions = new List<Action<OmokHistoryNode>>();
			}
			if (actions.IndexOf(action) < 0) {
				actions.Add(action);
			} else {
				return false;
			}
			actionToDoWhenCalculationFinishes[node] = actions;
			return true;
		}

		public bool IsDoneCalculating(OmokMove move) {
			return currentNode.IsDoneCalculating(move);
		}

		public NextStateMovementResult DoMoveCalculation(OmokMove move, byte whosTurnIsItNow, MonoBehaviour coroutineRunner, Action<OmokHistoryNode> onComplete) {
			bool validMove = currentNode != null && currentNode.state != null &&
				currentNode.state.GetState(move.coord) == UnitState.None;
			if (!validMove) {
				return NextStateMovementResult.Error;
			}
			NextStateMovementResult calcResult = currentNode.AddMoveIfNotAlreadyCalculating(move, whosTurnIsItNow, FinishedAnalysis, coroutineRunner, out OmokHistoryNode nextNode);
			switch (calcResult) {
				case NextStateMovementResult.Success:
					onComplete?.Invoke(nextNode);
					break;
				case NextStateMovementResult.StartedCalculating:
				case NextStateMovementResult.StillCalculating:
					AddActionWhenMoveAnalysisFinishes(nextNode, onComplete);
					break;
			}
			return calcResult;
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

		public NextStateMovementResult AdvanceMove(OmokMove move, byte whosTurnIsItNow, MonoBehaviour coroutineRunner, Action<OmokHistoryNode> onComplete) {
			OmokHistoryNode nextNode = currentNode.GetMove(move);
			if (nextNode != null) {
				SetState(nextNode, onComplete);
			}
			DoMoveCalculation(move, whosTurnIsItNow, coroutineRunner, onComplete);
			return NextStateMovementResult.StillCalculating;
		}

		public NextStateMovementResult SetState(OmokHistoryNode nextNode, Action<OmokHistoryNode> onComplete) {
			if (!nextNode.analysis.IsDoingAnalysis) {
				if (timeline[currentNode.Turn] != currentNode) {
					int positionInPath = timeline.IndexOf(currentNode);
					if (positionInPath < 0) {
						throw new Exception("current node is not in timeline... what happened?");
					}
					throw new Exception($"current node {positionInPath} is not where it shold be in the timeline {currentNode.Turn}... what happened?");
				}
				if (nextNode.Turn < timeline.Count) {
					if (timeline[nextNode.Turn] == nextNode) {
						Debug.Log("changing state in current timeline...");
					} else {
						Debug.Log($"going into new timeline @{nextNode.Turn}");
						OmokHistoryNode beforeNode = timeline[nextNode.Turn - 1];
						int pathIndex = beforeNode.GetMoveIndex(nextNode);
						if (pathIndex < 0) {
							throw new Exception("attempting to go into invalid reality?");
						} else {
							int forsakenFuture = timeline.Count - nextNode.Turn;
							Debug.Log($"changing to a different timeline @{nextNode.Turn}, forsaking {forsakenFuture}");
							timeline.RemoveRange(nextNode.Turn, forsakenFuture);
							timeline.Add(currentNode);
						}
					}
				} else if (nextNode.Turn == timeline.Count) {
					//Debug.Log($"advancing timeline like {nextNode.Turn} never happend before");
					timeline.Add(nextNode);
				}
				currentNode = nextNode;
				currentNode.traversed = true;
				NotifyCurrentNodeChanged(currentNode);
				onComplete?.Invoke(nextNode);
				return NextStateMovementResult.Success;
			}
			AddActionWhenMoveAnalysisFinishes(nextNode, NotifyCurrentNodeChanged);
			AddActionWhenMoveAnalysisFinishes(nextNode, onComplete);
			return NextStateMovementResult.StillCalculating;
		}
	}
}
