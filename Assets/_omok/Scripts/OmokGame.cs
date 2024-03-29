using UnityEngine;
using UnityEngine.Events;

namespace Omok {
	// TODO name each state (graph node)
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
		private OmokHistoryGraph graph;

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
				whosTurn = value;
				if (players.Length == 0) { return; }
				while (whosTurn >= players.Length) {
					whosTurn -= players.Length;
				}
				while (whosTurn < 0) {
					whosTurn += players.Length;
				}
				NotifyTurnChange();
			}
		}

		private void NotifyTurnChange() {
			_gameEvents.OnTurn.Invoke(whosTurn);
			_gameEvents.OnTurnColorHex.Invoke(ColorUtility.ToHtmlStringRGBA(players[whosTurn].Color));
		}

		public OmokBoard Board => board;

		public OmokGame omokGame => this;

		public IBelongsToOmokGame reference => null;

		public OmokHistoryGraph Graph => graph != null ? graph : graph = graphBehaviour.graph;

		public OmokState State {
			get {
				if (Graph.currentNode == null) {
					Graph.currentNode = new OmokHistoryNode(board.ReadStateFromBoard(), null, null, null);
					Graph.currentNode.traversed = true;
				}
				return Graph.currentNode.state;
			}
		}

		void Start() {
			NotifyTurnChange();
		}

		void Update() {

		}

		//public void PlacePieceForCurrentPlayer() {
		//	Coord coord = board.CurrentSelectedSpot;
		//	PlacePieceForCurrentPlayerAt(coord);
		//	graphBehaviour.graph.DoMoveCalculation(coord, this, NotifyNextMove, WhosTurn);
		//}

		private void NotifyNextMove(OmokMove move) {
			OmokHistoryNode nextNode = graph.currentNode.GetMove(move);
			graph.SetState(nextNode, null);
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
				NotifyNextMove(move);
			} else {
				graphBehaviour.graph.DoMoveCalculation(move, this, NotifyNextMove);
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
