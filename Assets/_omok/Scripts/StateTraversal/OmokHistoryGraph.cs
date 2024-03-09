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
		public Gradient optionColors = new Gradient() {
			colorKeys = new GradientColorKey[] {
				new GradientColorKey(Color.red, 0),
				new GradientColorKey(Color.yellow, 0.5f),
				new GradientColorKey(Color.green, 1),
			}
		};

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
			//Debug.Log(debugOutput.text);
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

		public GameObject test;
		public void OnMoveCalcStart(Coord coord) {
			//Debug.Log(string.Join(", ", spiral));
			//if (test == null) {
			//	test = new GameObject();
			//	for (int i = 0; i < spiral.Count; i++) {
			//		GameObject go = new GameObject("i"+i);
			//		go.transform.SetParent(test.transform, false);
			//		go.transform.localPosition = spiral[i];
			//	}
			//}
		}

		private List<Coord> nextMovesToTry = new List<Coord>();

		public static List<Coord> SquareSpiral(Coord center, int count) {
			List<Coord> list = new List<Coord>();
			Coord cursor = center;
			int stride = 1;
			Coord[] directions = new Coord[] {
				new Coord( 0,-1),
				new Coord( 1, 0),
				new Coord( 0, 1),
				new Coord(-1, 0),
			};
			Coord direction;
			int directionIndex = 0;
			list.Add(cursor);
			for(int i = 0; i < count; ++i) {
				direction = directions[directionIndex];
				if (i != 0 && i % 2 == 0) {
					stride++;
				}
				for (int s = 0; s < stride; ++s) {
					cursor += direction;
					list.Add(cursor);
				}
				directionIndex = (directionIndex + 1) % directions.Length;
			}
			return list;
		}

		// TODO automatically call this in a spiral pattern around the mouse if the graph is not doing any calculations
		public void OnMoveCalcFinish(OmokMove move) {
			//if (currentNode.analysis.IsDoingAnalysis) {
			//	Debug.Log($"still doing analysis...");
			//	return;
			//}
			RefreshAllPredictionTokens((byte)game.WhosTurn);

			// TODO after calculating one move, calculate the next move
			//if (nextMovesToTry.Count == 0) {
			//	nextMovesToTry = SquareSpiral(move.coord, 8);
			//}
			//while (nextMovesToTry.Count > 0) {
			//	Coord next = nextMovesToTry[0];
			//	nextMovesToTry.RemoveAt(0);
			//	if (nextMovesToTry.Count == 0) {
			//		Debug.Log("really? nothing?");
			//	}
			//	bool haveBoardState = currentNode != null && currentNode.state != null;
			//	bool validMove = haveBoardState &&
			//		currentNode.state.TryGetState(next, out UnitState coordState) && coordState == UnitState.None &&
			//		currentNode.GetMove(new OmokMove(next, (byte)game.WhosTurn)) == null;
			//	if (validMove) {
			//		Debug.Log("I should try " + next);
			//		nextMovesToTry.Clear();
			//		CalculateCurrentMove(next);
			//		break;
			//	} else {
			//		//Debug.Log("skip " + next);
			//	}
			//}
		}

		public void RefreshAllPredictionTokens(byte player) {
			float[] minmax = CalculateOptionRange(player);
			//Debug.Log($"{minmax[0]}:{minmax[1]}");
			FreeAllPredictionTokens();
			for (int i = 0; i < currentNode.movePaths.Length; ++i) {
				CreatePredictionToken(currentNode.movePaths[i].move, minmax);
			}
		}

		private void CreatePredictionToken(OmokMove move, float[] minmax) {
			GameObject token = GetPredictionToken();
			OmokHistoryNode node = currentNode.GetMove(move);
			if (node.analysis.IsDoingAnalysis) {
				return;
			}
			float[] currentScore = currentNode.analysis.scoring;
			float[] nextStateScores = Sub(node.analysis.scoring, currentScore);
			TMPro.TMP_Text tmpText = token.GetComponentInChildren<TMPro.TMP_Text>();
			//string text = node.analysis.DebugText();
			float netScore = SummarizeScore(game.WhosTurn, nextStateScores);
			float denominator = (minmax[1] - minmax[0]);
			float p = denominator == 0 ? 0.5f : (netScore-minmax[0]) / (minmax[1] - minmax[0]);
			Color color = optionColors.Evaluate(p);
			string bStart = p == 0 || p == 1 ? "<b>" : "";
			string bEnd = p == 0 || p == 1 ? "</b>" : "";
			string text = $"<#000>{nextStateScores[0]}</color>\n" +
				$"<#{ColorUtility.ToHtmlStringRGBA(color)}>{bStart}{netScore}{bEnd}</color>\n" +
				$"<#fff>{nextStateScores[1]}</color>";
			tmpText.text = text;
			token.transform.position = game.Board.GetPosition(move.coord);
		}

		private float[] Sub(float[] a, float[] b) {
			float[] answer = (float[])a.Clone();
			if (b != null) {
				for (int i = 0; i < a.Length; i++) {
					if (i >= b.Length) { break; }
					answer[i] = a[i] - b[i];
				}
			}
			return answer;
		}

		private float SummarizeScore(int player, float[] score) {
			return score[player] - score[(player + 1) % 2];
		}

		public float[] CalculateOptionRange(byte player) {
			float[] minmax = new float[] { float.MaxValue, float.MinValue };
			for(int i = 0; i < currentNode.movePaths.Length; ++i) {
				OmokHistoryNode.MovePath movepath = currentNode.movePaths[i];
				float[] scoring = movepath.nextNode.analysis.scoring;
				if (movepath.nextNode.analysis.IsDoingAnalysis) {
					//Debug.Log($"skipping {movepath.move.coord}");
					continue;
				}
				float score = SummarizeScore(player, scoring);
				//Debug.Log($"{movepath.move.coord} {score}     {scoring[0]} v {scoring[1]}");
				if (score < minmax[0]) { minmax[0] = score; }
				if (score > minmax[1]) { minmax[1] = score; }
			}
			return minmax;
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
