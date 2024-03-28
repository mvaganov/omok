using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Omok {
	public class OmokHistoryGraphUi : MonoBehaviour {
		public RectTransform _container;
		public ListElement _stateElementPrefab;
		public RectTransform _branchElementPrefab;
		[ContextMenuItem(nameof(Refresh), nameof(Refresh))]
		public OmokGame game;
		public OmokHistoryGraph graph;

		public List<OmokHistoryGraph> states = new List<OmokHistoryGraph>();
		public MemoryPool<RectTransform> _branches = new MemoryPool<RectTransform>();
		public MemoryPool<ListElement> _elements = new MemoryPool<ListElement>();

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
			OmokHistoryNode cursor = rootState;
			ListElement element;
			//element = _elements.Get();
			//element.GameState = cursor;
			//element.transform.SetParent(_container, false);
			while (cursor.GetEdgeCount() > 0) {
				RectTransform possibilities = _branches.Get();
				//cursor.AssertNoEdgeDuplicates();// DEBUG
				for (int i = 0; i < cursor.GetEdgeCount(); ++i) {
					OmokMovePath edge = cursor.GetEdge(i);
					if (!edge.nextNode.Traversed) { continue; } // no UI for preview moves
					element = _elements.Get();
					element.Game = game;
					element.OmokNode = edge.nextNode;
					element.IsSelected = (edge.nextNode == currentState);
					element.transform.SetParent(possibilities, false);
					possibilities.name = edge.move.ToString();
				}
				possibilities.SetParent(_container, false);
				cursor = cursor.GetEdge(0).nextNode;
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(_container.GetComponent<RectTransform>());
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

		}

		internal void SetState(OmokHistoryNode nextState) {
			graph.SetState(nextState, this, null);
		}
	}
}
