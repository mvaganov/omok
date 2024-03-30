using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Omok {
	public class OmokHistoryGraphUi : MonoBehaviour {
		public RectTransform _container;
		public ListElement _stateElementPrefab;
		public RectTransform _branchElementPrefab;
		[ContextMenuItem(nameof(Refresh), nameof(Refresh))]
		public OmokGame game;
		public OmokHistoryGraph Graph => game.Graph;

		public MemoryPool<RectTransform> _branches = new MemoryPool<RectTransform>();
		public MemoryPool<ListElement> _elements = new MemoryPool<ListElement>();
		private OmokHistoryNode _currentlyCalculated = null;

		private void Awake() {
			_branches.SetData(transform, _branchElementPrefab, false);
			_elements.SetData(transform, _stateElementPrefab, false);
			_elements.onReclaim += e => e.IsSelected = false;
		}

		public void Refresh() {
			OmokHistoryNode current = game.Graph.currentNode;
			OmokHistoryNode root = current.FindRoot();
			PopulateStates(root, current);
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

		public void PopulateStates(OmokHistoryNode rootState, OmokHistoryNode currentState) {
			ClearUi();
			OmokHistoryGraph graph = Graph;
			List<OmokHistoryNode> history = graph.timeline;
			StringBuilder sb = new StringBuilder("!!!!!!!!!!!!!!!!\n");

			RectTransform gameStart = _branches.Get();
			AddEdgeUi(history[0], history.Count == 1, true, gameStart);
			gameStart.name = "game start";
			gameStart.SetParent(_container, false);

			for (int n = 0; n < history.Count; ++n) {
				OmokHistoryNode cursor = history[n];
				sb.Append(cursor+"\n");
				OmokHistoryNode nextOnPath = history.Count > n ? history[n] : null;
				RectTransform possibilities = _branches.Get();
				//cursor.AssertNoEdgeDuplicates();// DEBUG
				for (int i = 0; i < cursor.GetEdgeCount(); ++i) {
					OmokMovePath edge = cursor.GetEdge(i);
					if (!edge.nextNode.Traversed) { continue; } // no UI for preview moves
					AddEdgeUi(edge.nextNode, edge.nextNode == currentState, edge.nextNode == nextOnPath, possibilities);
				}
				possibilities.name = cursor.ToString();
				possibilities.SetParent(_container, false);
			}
			Debug.Log(sb);
			LayoutRebuilder.ForceRebuildLayoutImmediate(_container.GetComponent<RectTransform>());
		}

		private void AddEdgeUi(OmokHistoryNode nextNode, bool isSelected, bool isOnPath, Transform possibilities) {
			ListElement element = _elements.Get();
			element.Game = game;
			element.OmokNode = nextNode;
			element.IsSelected = isSelected;
			element.IsOnPath = isOnPath;
			element.transform.SetParent(possibilities, false);
			element.name = (isOnPath ? "*" : "") + nextNode.ToString();
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
			ListElement element = child.GetComponent<ListElement>();
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
				_currentlyCalculated = Graph.currentNode;
				Debug.Log($"refreshing {_currentlyCalculated}");
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
