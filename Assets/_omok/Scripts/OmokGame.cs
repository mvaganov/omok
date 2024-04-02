using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Omok {
	// TODO undo/redo (graph traversal)
	// TODO web compile
	// TODO scoring calc should have a depth, with new scores for each depth level. if depth is greater than 1, only evaluate top X moves. make sure scores past depth alternate player, so values change plus or minus
	// TODO scrolling, and board expansion
	// TODO eyes should dart between moves tied for best
	// TODO likelihood of looking at other moves is 1/(n^2) instead of 1/(n)
	public class OmokGame : MonoBehaviour, IBelongsToOmokGame {
		[SerializeField]
		protected OmokBoard board;
		public Transform pieceArea;
		public OmokPlayer[] players = new OmokPlayer[2];
		[SerializeField]
		protected int whosTurn = 0;
		public OmokStateLineAnalysisDraw analysisVisual;
		public OmokHistoryGraphBehaviour graphBehaviour;
		public Color NeutralColor = new Color(0.5f, 0.5f, 0.5f);

		[System.Serializable] public class GameEvents {
			[System.Serializable] public class UnityEvent_int : UnityEvent<int> { }
			[System.Serializable] public class UnityEvent_string : UnityEvent<string> { }
			public UnityEvent_int OnTurn = new UnityEvent_int();
			public UnityEvent_string OnTurnColorHex = new UnityEvent_string();
		}
		[SerializeField] protected GameEvents _gameEvents = new GameEvents();

		public byte WhosTurn {
			get => (byte)whosTurn;
			set {
				whosTurn = SafeTurn(value, 0);
				NotifyTurnChange();
			}
		}

		public byte NextTurn {
			get => SafeTurn(WhosTurn, 1);
		}

		private byte SafeTurn(byte turnNow, int change) {
			int nextTurn = turnNow + change;
			if (players.Length == 0) { return 0; }
			while (nextTurn >= players.Length) {
				nextTurn -= players.Length;
			}
			while (nextTurn < 0) {
				nextTurn += players.Length;
			}
			return (byte)nextTurn;
		}

		private void NotifyTurnChange() {
			_gameEvents.OnTurn.Invoke(whosTurn);
			_gameEvents.OnTurnColorHex.Invoke(ColorUtility.ToHtmlStringRGBA(players[whosTurn].Color));
		}

		public OmokBoard Board => board;

		public OmokGame omokGame => this;

		public IBelongsToOmokGame reference => null;

		public OmokHistoryGraph Graph => graphBehaviour.graph;

		public OmokBoardState State {
			get {
				if (Graph.currentNode == null) {
					Graph.currentNode = new OmokHistoryNode(board.ReadStateFromBoard(), null, WhosTurn, null);
					Graph.currentNode.traversed = true;
				}
				return Graph.currentNode.boardState;
			}
		}

		void Start() {
			NotifyTurnChange();
			Debug.Log("ADDING CHANGE LISTENER");
		}

		void Update() {

		}

		//public void PlacePieceForCurrentPlayer() {
		//	Coord coord = board.CurrentSelectedSpot;
		//	PlacePieceForCurrentPlayerAt(coord);
		//	graphBehaviour.graph.DoMoveCalculation(coord, this, NotifyNextMove, WhosTurn);
		//}

		private void NotifyNextMove(OmokHistoryNode nextNode) {
			//OmokHistoryNode nextNode = graph.currentNode.GetMove(move);
			Graph.SetState(nextNode, null);
			//graph.currentNode = nextNode;
			nextNode.traversed = true;
			//Debug.Log("~~~~~~new state to analyze?! " + graph.currentNode.state.ToDebugString());
			OmokGoogleTargetListener targetListener = GetComponent<OmokGoogleTargetListener>();
			targetListener.ClearLookTargets();
			++WhosTurn;
		}

		/// <summary>
		/// Callback used when mouse is clicked
		/// </summary>
		public void PlacePieceForCurrentPlayerAt(Coord coord) {
			OmokPiece piece = board.PieceAt(coord);
			if (piece == null) {
				piece = players[WhosTurn].CreatePiece(coord);
			}
			OmokMove move = new OmokMove(coord, WhosTurn);
			if (graphBehaviour.graph.IsDoneCalculating(move)) {
				NotifyNextMove(graphBehaviour.graph.currentNode.GetMove(move));
			} else {
				graphBehaviour.graph.DoMoveCalculation(move, NextTurn, this, NotifyNextMove);
			}
			graphBehaviour.RefreshAllPredictionTokens();
			graphBehaviour.NewState = true;
			//Debug.Log("NEXT TURN PLZ");
			Board.State.Copy(State);
			Board.RefreshDebug();
		}

		public int GetPlayerIndex(OmokPlayer player) => System.Array.IndexOf(players, player);
	}
}
