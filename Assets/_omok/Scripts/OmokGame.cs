using UnityEngine;

namespace Omok {
	public class OmokGame : MonoBehaviour, IBelongsToOmokGame {
		[SerializeField]
		protected OmokBoard board;
		public Transform pieceArea;
		public OmokPlayer[] players = new OmokPlayer[2];
		[SerializeField]
		protected int whosTurn = 0;
		public OmokStateAnalysisDraw analysisVisual;
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

		public void PlacePieceForCurrentPlayer() {
			Coord coord = board.CurrentSelectedSpot;
			PlacePieceForCurrentPlayerAt(coord);
		}
		public void PlacePieceForCurrentPlayerAt(Coord coord) {
			OmokPiece piece = board.PieceAt(coord);
			if (piece == null) {
				piece = players[WhosTurn].CreatePiece();
				piece.Coord = coord;
				WhosTurn++;
			}
		}

		public int GetPlayerIndex(OmokPlayer player) => System.Array.IndexOf(players, player);
	}
}
