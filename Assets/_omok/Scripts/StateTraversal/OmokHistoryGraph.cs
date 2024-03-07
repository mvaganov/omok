using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Omok {
	public class OmokHistoryGraph : MonoBehaviour {
		public OmokGame game;
		private List<OmokHistoryNode> historyNodes = new List<OmokHistoryNode>();
		private OmokHistoryNode currentNode;
		public TMPro.TMP_Text debugOutput;
		public GameObject predictionPrefab;
		public List<GameObject> predictionTokenPool = new List<GameObject>();

		public void Start() {

		}

		void Update() {
			if (currentNode == null) {
				CreateNewRoot();
			}
		}

		private void CreateNewRoot() {
			OmokState state = game.Board.ReadStateFromBoard();
			currentNode = new OmokHistoryNode(state, null, null);
			historyNodes.Add(currentNode);
			StartCoroutine(currentNode.analysis.AnalyzeCoroutine(OmokMove.InvalidMove, game.Board.State, RefreshStateVisuals));
			//currentNode.analysis.Analyze(state);
			//Debug.Log($"%%%% [{currentNode.analysis.lineMap}]");
			//RefreshStateVisuals();
		}

		private void RefreshStateVisuals(OmokMove move) {
			UpdateDebugText();
			game.analysisVisual.RenderAnalysis(currentNode.analysis);
		}

		private void UpdateDebugText() {
			//StringBuilder debugText = new StringBuilder();
			//Dictionary<byte, Dictionary<int, int>> lineCountPerPlayer = new Dictionary<byte, Dictionary<int, int>>();
			//currentNode.analysis.ForEachLine(line => {
			//	if (!lineCountPerPlayer.TryGetValue(line.player, out Dictionary<int, int> map)) {
			//		lineCountPerPlayer[line.player] = map = new Dictionary<int, int>();
			//	}
			//	if (!map.TryGetValue(line.count, out int count)) {
			//		map[line.count] = 1;
			//	} else {
			//		map[line.count] = count + 1;
			//	}
			//});
			//List<byte> players = new List<byte>(lineCountPerPlayer.Keys);
			//players.Sort();
			//for (int p = 0; p < players.Count; ++p) {
			//	debugText.Append($"Player {players[p]}\n");
			//	Dictionary<int, int> map = lineCountPerPlayer[players[p]];
			//	List<int> lineLengths = new List<int>(map.Keys);
			//	lineLengths.Sort();
			//	for (int l = 0; l < lineLengths.Count; ++l) {
			//		int lineCount = map[lineLengths[l]];
			//		debugText.Append($"  {lineLengths[l]} : {lineCount}\n");
			//	}
			//}
			debugOutput.text = currentNode.analysis.DebugText();// debugText.ToString();
			Debug.Log(debugOutput.text);
		}


		public void CalculateCurrentMove(Coord coord) {
			bool validMove = currentNode != null && currentNode.state != null &&
				currentNode.state.GetState(coord) == UnitState.None;
			if (!validMove) {
				return;
			}
			OmokMove move = new OmokMove(coord, (byte)game.WhosTurn);
			if (currentNode.AddMove(move, OnMoveCalcFinish, this)) {
				OnMoveCalcStart(coord);
			}
		}

		public void OnMoveCalcStart(Coord coord) {
			Debug.Log($"Started Calculating {coord}");
		}

		// TODO automatically call this in a spiral pattern around the mouse if the graph is not doing any calculations
		public void OnMoveCalcFinish(OmokMove move) {
			//Debug.Log($"Finished calculating {move.coord}");
			OmokHistoryNode node = currentNode.GetMove(move);
			GameObject token = GetPredictionToken();
			float[] comparison = (float[])node.analysis.scoring.Clone();
			for (int i = 0; i < comparison.Length; i++) {
				if (currentNode.analysis.scoring.Length >= i) {
					comparison[i] -= currentNode.analysis.scoring[i];
				}
			}
			TMPro.TMP_Text tmpText = token.GetComponentInChildren<TMPro.TMP_Text>();
			//string text = node.analysis.DebugText();
			float netScore = comparison[game.WhosTurn] - comparison[(game.WhosTurn + 1) % 2];
			// TODO color the netscore based on how good this move is compared to the other calculated moves
			// TODO after a move is made and the board state changes, set the currentNode to the new state, FreeAllPredictionTokens()
			string text = $"<#000>{comparison[0]}</color>\n<#0f0>{netScore}</color>\n<#fff>{comparison[1]}</color>";
			tmpText.text = text;
			token.transform.position = game.Board.GetPosition(move.coord);
		}

		public GameObject GetPredictionToken() {
			for (int i = 0; i < predictionTokenPool.Count; ++i) {
				GameObject go = predictionTokenPool[i];
				if (!go.activeInHierarchy) {
					go.SetActive(true);
					return go;
				}
			}
			GameObject foundFree = Instantiate(predictionPrefab);
			predictionTokenPool.Add(foundFree);
			return foundFree;
		}

		public void FreeAllPredictionTokens() {
			predictionTokenPool.ForEach(t => t.SetActive(false));
		}
	}
}
