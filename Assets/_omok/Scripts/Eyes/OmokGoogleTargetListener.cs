using UnityEngine;

namespace Omok {
	public class OmokGoogleTargetListener : MonoBehaviour, IBelongsToOmokGame {
		public OmokBoard board;

		public OmokGame omokGame => board.omokGame;
		public IBelongsToOmokGame reference => board;

		public void ValidMoveCalculated(OmokMove move) {
			OmokHistoryNode node = omokGame.graphBehaviour.graph.currentNode.GetMove(move);
			//Debug.Log($"target {move.coord} {string.Join(",", node.analysis.scoring)} : " +
			//	$"{node.analysis.scoring[0]- node.analysis.scoring[1]}");
			Coord.ForEach(move.coord - Coord.one, move.coord + Coord.one, c => {
				OmokPiece piece = board.PieceAt(c);
				if (piece != null) {
					OmokGoogleDirector googDirector = piece.GetComponentInChildren<OmokGoogleDirector>();
					if (googDirector != null) {
						googDirector.AddLookTarget(move);
					}
				}
			});
		}

		public void ClearLookTargets() {
			OmokGoogleDirector[] googleDirectors = board.GamePieces.GetComponentsInChildren<OmokGoogleDirector>();
			System.Array.ForEach(googleDirectors, g => g.ClearTargets());
		}
	}
}
