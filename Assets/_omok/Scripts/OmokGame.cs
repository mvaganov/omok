using UnityEngine;

namespace Omok {
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
			}
		}

		public OmokBoard Board => board;

		public OmokGame omokGame => this;

		public IBelongsToOmokGame reference => null;

		public OmokHistoryGraph Graph => graph != null ? graph : graph = graphBehaviour.graph;

		public OmokState State {
			get {
				if (Graph.currentNode == null) {
					Graph.currentNode = new OmokHistoryNode(board.ReadStateFromBoard(), null, null);
				}
				return Graph.currentNode.state;
			}
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
			graph.currentNode = nextNode;
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
