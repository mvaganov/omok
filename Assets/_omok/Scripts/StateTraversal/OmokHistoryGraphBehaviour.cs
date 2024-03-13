using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Omok {
	public class OmokHistoryGraphBehaviour : MonoBehaviour {
		public OmokGame game;
		public OmokHistoryGraph graph = new OmokHistoryGraph();

		public TMPro.TMP_Text debugOutput;
		public GameObject predictionPrefab;
		public List<GameObject> predictionTokenPool = new List<GameObject>();
		public Transform predictionTokens;
		private List<Coord> nextMovesToTry = new List<Coord>();
		private int indexToTry = 0;

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
			if (graph.currentNode == null) {
				//CreateNewRoot();
				OmokState state = game.Board.ReadStateFromBoard();
				graph.CreateNewRoot(state, this, RefreshStateVisuals);
			}
		}

		private void RefreshStateVisuals(OmokMove move) {
			UpdateDebugText();
			OmokHistoryNode currentNode = graph.currentNode;
			game.analysisVisual.RenderAnalysis(currentNode.analysis);
			List<OmokLine> allLines = new List<OmokLine>();
			currentNode.analysis.ForEachLine(coordLine => {
				allLines.Add(coordLine);
			});
			allLines.Sort();
			nextMovesToTry.Clear();

			List<Coord> coordListLine = new List<Coord>();
			HashSet<Coord> coordSet = new HashSet<Coord>();
			allLines.ForEach(coordLine => {
				coordLine.ForEachCoord(coord => {
					if (!coordSet.Contains(coord)) {
						coordSet.Add(coord);
						coordListLine.Add(coord);
					}
				});
			});
			nextMovesToTry.AddRange(coordListLine);
			indexToTry = 0;
			
			//Debug.Log($"moves: {string.Join(", ", nextMovesToTry)}");
			// and then every square within 2 squares of the ones in lines
			//List<Coord> coordListNearLine = new List<Coord>();
			//for (int i = 0; i < coordListLine.Count; i++) {
			//	Coord min = coordListLine[i] - Coord.one;
			//	Coord max = coordListLine[i] + Coord.one;
			//	Coord.ForEach(min, max, coord => {
			//		if (!coordSet.Contains(coord)) {
			//			coordSet.Add(coord);
			//			coordListNearLine.Add(coord);
			//		}
			//	});
			//}
			//nextMovesToTry.AddRange(coordListNearLine);

			GenerateTestingTransformsForNextMovesToTry();
			ContinueIndividualMoveAnalysis();

		}

		private void UpdateDebugText() {
			debugOutput.text = graph.currentNode.analysis.DebugText();
		}


		public void CalculateCurrentMove(Coord coord) {
			DoMoveCalculation(coord);
		}

		private void GenerateTestingTransformsForNextMovesToTry() {
			if (test == null) {
				test = new GameObject();
			}
			for (int i = test.transform.childCount - 1; i >= 0; --i) {
				Destroy(test.transform.GetChild(i).gameObject);
			}
			for (int i = 0; i < nextMovesToTry.Count; i++) {
				GameObject go = new GameObject("i" + i);
				go.transform.SetParent(test.transform, false);
				go.transform.position = game.Board.GetPosition(nextMovesToTry[i]);
			}
		}

		public bool DoMoveCalculation(Coord coord) {
			return graph.DoMoveCalculation(coord, this, OnMoveCalcFinish, (byte)game.WhosTurn);
			//OmokHistoryNode currentNode = graph.currentNode;
			//bool validMove = currentNode != null && currentNode.state != null &&
			//	currentNode.state.GetState(coord) == UnitState.None;
			//if (!validMove) {
			//	return false;
			//}

			//OmokMove move = new OmokMove(coord, (byte)game.WhosTurn);
			//if (currentNode.AddMove(move, OnMoveCalcFinish, this)) {
			//	OnMoveCalcStart(coord);
			//	return true;
			//}
			//return false;
		}

		public GameObject test;
		public void OnMoveCalcStart(Coord coord) {
			//Debug.Log(string.Join(", ", spiral));
		}

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



		public void OnMoveCalcFinish(OmokMove move) {
			RefreshAllPredictionTokens((byte)game.WhosTurn);
			ContinueIndividualMoveAnalysis(3);
		}

		private void ContinueIndividualMoveAnalysis(int checksPerBatch = 5) {
			int checks = 0;
			OmokHistoryNode currentNode = graph.currentNode;
			while (indexToTry < nextMovesToTry.Count) {
				Coord next = nextMovesToTry[indexToTry];
				//Debug.Log($"... {indexToTry}/{nextMovesToTry.Count}   {next}");
				currentNode.state.TryGetState(next, out UnitState coordState);
				bool validMove = coordState == UnitState.None &&
					currentNode.GetMove(new OmokMove(next, (byte)game.WhosTurn)) == null;
				Transform t = test.transform.GetChild(indexToTry);
				t.name += coordState;
				++indexToTry;
				if (validMove) {
					if (DoMoveCalculation(next)) {
						++checks;
						if (checks >= checksPerBatch) {
							break;
						}
					}
				} else {
					t.gameObject.SetActive(false);
				}
			}
		}

		public void RefreshAllPredictionTokens(byte player) {
			float[] minmax = graph.currentNode.CalculateScoreRangeForPaths(player);
			FreeAllPredictionTokens();
			OmokHistoryNode currentNode = graph.currentNode;
			for (int i = 0; i < currentNode.movePaths.Length; ++i) {
				CreatePredictionToken(currentNode.movePaths[i].move, minmax);
			}
		}

		private void CreatePredictionToken(OmokMove move, float[] minmax) {
			GameObject token = GetPredictionToken();
			OmokHistoryNode currentNode = graph.currentNode;
			OmokHistoryNode node = currentNode.GetMove(move);
			if (node.analysis.IsDoingAnalysis) {
				return;
			}
			float[] currentScore = currentNode.analysis.scoring;
			float[] nextStateScores = Sub(node.analysis.scoring, currentScore);
			TMPro.TMP_Text tmpText = token.GetComponentInChildren<TMPro.TMP_Text>();
			//string text = node.analysis.DebugText();
			float netScore = OmokStateAnalysis.SummarizeScore(game.WhosTurn, nextStateScores);
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

		public GameObject GetPredictionToken() {
			for (int i = 0; i < predictionTokenPool.Count; ++i) {
				GameObject go = predictionTokenPool[i];
				if (!go.activeInHierarchy) {
					go.SetActive(true);
					return go;
				}
			}
			GameObject foundFree = Instantiate(predictionPrefab);
			foundFree.transform.SetParent(predictionTokens, true);
			predictionTokenPool.Add(foundFree);
			return foundFree;
		}

		public void FreeAllPredictionTokens() {
			predictionTokenPool.ForEach(t => t.SetActive(false));
		}
	}
}