using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Omok {
	public class OmokHistoryGraphBehaviour : MonoBehaviour, IBelongsToOmokGame {
		[System.Serializable] public class UnityEvent_Move : UnityEvent<OmokMove> { }
		public OmokGame game;
		public OmokHistoryGraph graph;

		public TMPro.TMP_Text debugOutput;
		public GameObject predictionPrefab;
		public List<GameObject> predictionTokenPool = new List<GameObject>();
		public Transform predictionTokens;
		private List<Coord> nextMovesToTry = new List<Coord>();
		private int indexToTry = 0;
		[SerializeField] protected bool _showAllPredictionTokens = true;
		[SerializeField] protected bool _showSinglePredictionToken = true;
		[SerializeField] protected GameObject _singlePredictionToken;
		private OmokHistoryNode _visualizedHistoryState;

		public Gradient optionColors = new Gradient() {
			colorKeys = new GradientColorKey[] {
				new GradientColorKey(Color.red, 0),
				new GradientColorKey(Color.yellow, 0.5f),
				new GradientColorKey(Color.green, 1),
			}
		};
		public UnityEvent_Move onMoveDiscoveryFinish;

		public OmokHistoryNode CurrentNode => graph.currentNode;

		public bool ShowPredictionToken {
			get => _showAllPredictionTokens;
			set {
				_showAllPredictionTokens = value;
				SetPredictionTokenVisibility(_showAllPredictionTokens);
				if (_showAllPredictionTokens) {
					RefreshAllPredictionTokens();
				}
			}
		}

		public OmokGame omokGame => game;

		public IBelongsToOmokGame reference => null;

		private void Awake() {
		}

		public void Start() {
			//Debug.Log("creating graph structure");
			graph = new OmokHistoryGraph();
			OmokBoardState state = game.Board.ReadStateFromBoard();
			graph.CreateNewRoot(0, state, this, RefreshStateVisuals);
			_visualizedHistoryState = graph.currentNode;
			graph.OnNodeChange += Graph_OnNodeChange;
		}

		private void Graph_OnNodeChange(OmokHistoryGraph obj) {
			//Debug.Log($"NEW STATE! {obj.currentNode.Turn}");
			//if (obj.currentNode.Turn > 0 || Graph.timeline.Count > 1) {
			StartCoroutine(RefreshMapStateNextFrame());
			IEnumerator RefreshMapStateNextFrame() {
				yield return null;
				omokGame.Board.LoadState(omokGame.State);
				//Debug.Log($"({obj.currentNode.Turn}) next person's turn: {obj.currentNode.whosTurnIsItNow} after {obj.currentNode}");
				omokGame.WhosTurn = obj.currentNode.whosTurnIsItNow;
			}
			//}
		}

		public bool NewState = false;

		void Update() {
			 if (NewState) {
				NewState = false;
				//Debug.Log("new state to analyze?! "+ graph.currentNode.state.ToDebugString());
				_visualizedHistoryState = graph.currentNode;
				// TODO find out why the visuals are not recalculating.
				RefreshStateVisuals(null);
			}
		}

		private void RefreshStateVisuals(OmokHistoryNode move) {
			UpdateDebugText();
			OmokHistoryNode currentNode = graph.currentNode;
			game.analysisVisual.RenderAnalysis(currentNode.BoardAnalysis);
			List<OmokLine> allLines = new List<OmokLine>();
			currentNode.BoardAnalysis.ForEachLine(coordLine => {
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

			GenerateTestingTransformsForNextMovesToTry();
			ContinueIndividualMoveAnalysis();
		}

		private void UpdateDebugText() {
			debugOutput.text = graph.currentNode.BoardAnalysis.DebugText();
		}

		/// <summary>
		/// referenced by OnHover
		/// </summary>
		public void CalculateCurrentMove(Coord coord) {
			DoMoveCalculation(coord);
			if (_showSinglePredictionToken && _singlePredictionToken != null) {
				OmokMove move = new OmokMove(coord, omokGame.WhosTurn);
				SetSinglePredictionToken(_singlePredictionToken, move);
			}
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

		public NextStateMovementResult DoMoveCalculation(Coord coord) {
			OmokMove move = new OmokMove(coord, game.WhosTurn);
			OmokHistoryNode currentNode = graph.currentNode;
			bool StillSameNode() => graph.currentNode == currentNode;
			return graph.DoMoveCalculation(move, game.NextTurn, this, OnMoveCalcFinish, StillSameNode);
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

		public void OnMoveCalcFinish(OmokHistoryNode node) {
			if (_showAllPredictionTokens) {
				RefreshAllPredictionTokens((byte)game.WhosTurn);
			}
			ContinueIndividualMoveAnalysis(3);
			onMoveDiscoveryFinish.Invoke(node.sourceMove);
		}

		private void ContinueIndividualMoveAnalysis(int checksPerBatch = 5) {
			int checks = 0;
			OmokHistoryNode currentNode = graph.currentNode;
			while (indexToTry < nextMovesToTry.Count) {
				Coord next = nextMovesToTry[indexToTry];
				//Debug.Log($"... {indexToTry}/{nextMovesToTry.Count}   {next}");
				currentNode.boardState.TryGetState(next, out OmokUnitState coordState);
				bool validMove = coordState == OmokUnitState.None &&
					currentNode.GetMove(new OmokMove(next, (byte)game.WhosTurn)) == null;
				Transform t = test.transform.GetChild(indexToTry);
				t.name += coordState;
				++indexToTry;
				if (validMove) {
					if (DoMoveCalculation(next) == NextStateMovementResult.StartedCalculating) {
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

		public void RefreshAllPredictionTokens() => RefreshAllPredictionTokens(omokGame.WhosTurn);

		public void RefreshAllPredictionTokens(byte player) {
			FreeAllPredictionTokens();
			OmokHistoryNode currentNode = graph.currentNode;
			MinMax minmax = currentNode.CalculateScoreRangeForPaths(player);
			//Debug.Log($"minmax {player}: {string.Join(",",minmax)}");
			for (int i = 0; i < currentNode.movePaths.Length; ++i) {
				CreatePredictionTokenIfDataAvailable(currentNode.movePaths[i].move, minmax);
			}
		}

		private bool CreatePredictionTokenIfDataAvailable(OmokMove move, MinMax minmax) {
			OmokHistoryNode node = graph.currentNode.GetMove(move);
			if (node == null || node.BoardAnalysis.IsDoingAnalysis) {
				return false;
			}
			GameObject token = GetPredictionToken();
			return SetPredictionToken(token, move, minmax, _showAllPredictionTokens);
		}

		public bool SetSinglePredictionToken(GameObject token, OmokMove move) {
			MinMax minmax = graph.currentNode.CalculateScoreRangeForPaths(move.player);
			bool itWorked = SetPredictionToken(token, move, minmax, _showSinglePredictionToken);
			if (!itWorked) {
				SetPredictionTokenVisibility(token, false);
			}
			return itWorked;
		}

		private bool SetPredictionToken(GameObject token, OmokMove move, MinMax minmax, bool visible) {
			float[] nextStateScores = GetMoveScoringSummary(move, out float netScore);
			if (nextStateScores == null) {
				return false;
			}
			float delta = minmax.Delta;
			float p = delta == 0 ? 0.5f : (netScore - minmax.min) / delta;
			Color color = optionColors.Evaluate(p);
			string bStart = p == 0 || p == 1 ? "<b>[[" : "";
			string bEnd = p == 0 || p == 1 ? "]]</b>" : "";
			string text;
			if (nextStateScores != null && nextStateScores.Length == 2) {
				text = $"<#000>{nextStateScores[0]}</color>\n" +
					$"<#{ColorUtility.ToHtmlStringRGBA(color)}>{bStart}{netScore}{bEnd}</color>\n" +
					$"<#fff>{nextStateScores[1]}</color>";
			} else {
				text = "";
			}
			TMPro.TMP_Text tmpText = token.GetComponentInChildren<TMPro.TMP_Text>();
			tmpText.text = text;
			token.transform.position = game.Board.GetPosition(move.coord);
			SpriteRenderer sprite = token.GetComponentInChildren<SpriteRenderer>();
			if (sprite != null) {
				Color playerColor = omokGame.players[move.player].Color;
				playerColor.a = sprite.color.a;
				sprite.color = playerColor;
			}
			SetPredictionTokenVisibility(token, visible);
			return true;
		}

		public float[] GetMoveScoringSummary(OmokMove move, out float netScore) {
			return graph.GetMoveScoringSummary(move, out netScore);
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

		private void SetPredictionTokenVisibility(bool visible) {
			for (int i = 0; i < predictionTokenPool.Count; i++) {
				GameObject go = predictionTokenPool[i];
				if (go.activeInHierarchy) {
					SetPredictionTokenVisibility(go, visible);
				}
			}
		}

		private void SetPredictionTokenVisibility(GameObject go, bool visible) {
			Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
			System.Array.ForEach(renderers, r => r.enabled = visible);
		}
	}
}
