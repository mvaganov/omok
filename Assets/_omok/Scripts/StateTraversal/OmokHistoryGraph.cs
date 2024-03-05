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
			StringBuilder debugText = new StringBuilder();
			Dictionary<byte, Dictionary<int, int>> lineCountPerPlayer = new Dictionary<byte, Dictionary<int, int>>();
			currentNode.analysis.ForEachLine(line => {
				if (!lineCountPerPlayer.TryGetValue(line.player, out Dictionary<int, int> map)) {
					lineCountPerPlayer[line.player] = map = new Dictionary<int, int>();
				}
				if (!map.TryGetValue(line.count, out int count)) {
					map[line.count] = 1;
				} else {
					map[line.count] = count + 1;
				}
			});
			List<byte> players = new List<byte>(lineCountPerPlayer.Keys);
			players.Sort();
			for (int p = 0; p < players.Count; ++p) {
				debugText.Append($"Player {players[p]}\n");
				Dictionary<int, int> map = lineCountPerPlayer[players[p]];
				List<int> lineLengths = new List<int>(map.Keys);
				lineLengths.Sort();
				for (int l = 0; l < lineLengths.Count; ++l) {
					int lineCount = map[lineLengths[l]];
					debugText.Append($"  {lineLengths[l]} : {lineCount}\n");
				}
			}
			debugOutput.text = debugText.ToString();
			Debug.Log(debugOutput.text);
		}


		public void CalculateCurrentMove(Coord coord) {
			OmokMove move = new OmokMove(coord, (byte)game.WhosTurn);
			if (currentNode.AddMove(move, OnMoveCalcFinish, this)) {
				OnMoveCalcStart(coord);
			}
		}

		public void OnMoveCalcStart(Coord coord) {
			Debug.Log($"Started Calculating {coord}");
		}

		public void OnMoveCalcFinish(OmokMove move) {
			Debug.Log($"Finished calculating {move.coord}");
		}
	}
}
