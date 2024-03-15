using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	public class OmokPlayer : MonoBehaviour, IBelongsToOmokGame {
		[System.Serializable]
		public class PieceElement {
			public string Character;
			public OmokPiece Piece;
		}

		[SerializeField]
		protected OmokGame game;
		public Material material;
		public List<PieceElement> gamePieces = new List<PieceElement>();
		public List<OmokPiece> currentPieces = new List<OmokPiece>();

		public OmokGame omokGame => game;

		public Color Color => material.color;

		public OmokPiece CreatePiece() => GetPiece(0);

		public int Index => game.GetPlayerIndex(this);

		public OmokPiece GetPiece(int index) {
			int freeIndex = currentPieces.FindIndex(op => op != null && op.gameObject.activeSelf == false);
			OmokPiece piece = null;
			if (freeIndex < 0) {
				piece = Instantiate(gamePieces[index].Piece.gameObject).GetComponent<OmokPiece>();
				currentPieces.Add(piece);
			} else {
				piece = currentPieces[freeIndex];
				piece.gameObject.SetActive(true);
			}
			piece.Index = index;
			piece.Player = this;
			piece.transform.SetParent(game.pieceArea, false);
			CleanUpEmptyPieceSlots();
			return piece;
		}
		private void CleanUpEmptyPieceSlots() {
			for (int i = currentPieces.Count - 1; i >= 0; i--) {
				OmokPiece piece = currentPieces[i];
				if (piece == null) {
					currentPieces.RemoveAt(i);
				}
			}
		}
	}
}
