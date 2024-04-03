using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Omok {
	public class OmokHistoryGraphUi : MonoBehaviour {
		public RectTransform _container;
		public OmokHistoryElement _stateElementPrefab;
		public RectTransform _branchElementPrefab;
		[ContextMenuItem(nameof(Refresh), nameof(Refresh))]
		public OmokGame game;
		public OmokHistoryGraph Graph => game.Graph;

		private MemoryPool<RectTransform> _branches = new MemoryPool<RectTransform>();
		private MemoryPool<OmokHistoryElement> _elements = new MemoryPool<OmokHistoryElement>();
		private OmokHistoryNode _currentlyCalculated = null;

		private void Awake() {
			_branches.SetData(transform, _branchElementPrefab, false);
			_elements.SetData(transform, _stateElementPrefab, false);
			_elements.onReclaim += e => e.IsSelected = false;
		}

		public void Refresh() {
			_currentlyCalculated = Graph.currentNode;
			PopulateStates(_currentlyCalculated);
			ForceScrollDown();
		}

		public void ForceScrollDown() {
			StartCoroutine(ForceScrollDownCoroutine());
		}

		IEnumerator ForceScrollDownCoroutine() {
			ScrollRect sr = GetComponent<ScrollRect>();
			if (sr == null) { yield break; }
			// Wait for end of frame AND force update all canvases before setting to bottom.
			yield return new WaitForEndOfFrame();
			Canvas.ForceUpdateCanvases();
			sr.verticalNormalizedPosition = 0f;
			Canvas.ForceUpdateCanvases();
		}

		public void PopulateStates(OmokHistoryNode currentState) {
			ClearUi();
			OmokHistoryGraph graph = Graph;
			List<OmokHistoryNode> history = graph.timeline;
			if (history == null) {
				Debug.Log("no history?");
				return;
			}
			StringBuilder sb = new StringBuilder("!!!!!!!!!!!!!!!!\n");

			RectTransform gameStart = _branches.Get();
			OmokHistoryElement rootElement = AddEdgeUi(history[0], history[0] == currentState, true, gameStart);
			gameStart.name = "game start";
			gameStart.SetParent(_container, false);
			int turnNow = currentState.turnValue;
			//Debug.Log($"######## root: {rootElement.OmokNode} ({rootElement.OmokNode.sourceMove})");

			for (int n = 0; n < history.Count; ++n) {
				OmokHistoryNode cursor = history[n];
				string extra = cursor == currentState ? "<" : "";
				sb.Append($"{cursor}{extra}\n");
				OmokHistoryNode nextOnPath = history.Count > n+1 && n < turnNow ? history[n+1] : null;
				RectTransform possibilities = _branches.Get();
				bool atEndOfHistory = history.Count == n - 1;
				OmokHistoryNode historyExtension = null;
				for (int i = 0; i < cursor.GetEdgeCount(); ++i) {
					OmokMovePath edge = cursor.GetEdge(i);
					if (edge.nextNode.Traversed == 0) { continue; } // no UI for preview moves
					AddEdgeUi(edge.nextNode, edge.nextNode == currentState, edge.nextNode == nextOnPath, possibilities);
					if (atEndOfHistory && historyExtension == null || edge.nextNode.Traversed > historyExtension.traversed) {
						historyExtension = edge.nextNode;
					}
				}
				if (possibilities.childCount == 0) {
					_branches.Reclaim(possibilities);
				} else {
					possibilities.name = cursor.ToString();
					possibilities.SetParent(_container, false);
				}
			}
			Debug.Log(sb);
			LayoutRebuilder.ForceRebuildLayoutImmediate(_container.GetComponent<RectTransform>());
		}

		private OmokHistoryElement AddEdgeUi(OmokHistoryNode nextNode, bool isSelected, bool isOnPath, Transform possibilities) {
			OmokHistoryElement element = _elements.Get();
			element.Game = game;
			element.OmokNode = nextNode;
			element.IsSelected = isSelected;
			element.IsOnPath = isOnPath;
			element.transform.SetParent(possibilities, false);
			element.name = (isOnPath ? "*" : "") + nextNode.ToString();
			//element.Text.text += (isOnPath ? "P" : "") + (isSelected ? "S" : "");
			Color textColor = element.Text.color;
			textColor.a = isOnPath ? 1 : 0.5f;
			element.Text.color = textColor;
			return element;
		}

		private void ClearUi() {
			Transform t = _container;
			for (int i = t.childCount - 1; i >= 0; --i) {
				Transform child = t.GetChild(i);
				if (ReclaimElement(child)) {
					continue;
				} else {
					ReclaimBranch(child);
				}
			}
		}

		private bool ReclaimElement(Transform child) {
			OmokHistoryElement element = child.GetComponent<OmokHistoryElement>();
			if (element == null) { return false; }
			child.SetParent(null);
			_elements.Reclaim(element);
			return true;
		}

		private void ReclaimBranch(Transform branch) {
			for (int c = branch.childCount - 1; c >= 0; --c) {
				ReclaimElement(branch.GetChild(c));
			}
			_branches.Reclaim(branch as RectTransform);
		}

		void Start() {
		}

		void Update() {
			if (_currentlyCalculated != Graph.currentNode) {
				//Debug.Log($"refreshing {_currentlyCalculated}");
				Refresh();
			}
		}

		internal void SetState(OmokHistoryNode nextState) {
			Debug.Log($"TRIGGERING {nextState}");
			Graph.SetState(nextState, null);
			Refresh();
		}
	}
}
