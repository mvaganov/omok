using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	public class OmokPlayer : MonoBehaviour, IBelongsToOmokGame {
		[System.Serializable]
		public class PieceElement {
			public string Character;
			public OmokPiece Piece;
			public Sprite Icon;
			public Color color;
		}

		[SerializeField]
		protected OmokGame game;
		public Material material;
		public TMPro.TMP_FontAsset fontAsset;
		public List<PieceElement> gamePieces = new List<PieceElement>();
		public List<OmokPiece> currentPieces = new List<OmokPiece>();

		public OmokGame omokGame => game;
		public IBelongsToOmokGame reference => null;


		public Color Color => material.color;

		public OmokPiece CreatePiece(Coord targetCoord) => GetPiece(0, targetCoord);

		public int Index => game.GetPlayerIndex(this);

		private bool FindFreePiece(OmokPiece op, int id) => op != null && op.gameObject.activeSelf == false && op.Index == id;

		/// <summary>
		/// Looks for a spare piece already at the given coordinate. if such a piece cannot be found, take any unused piece
		/// </summary>
		/// <param name="index"></param>
		/// <param name="targetCoord"></param>
		/// <returns></returns>
		public OmokPiece GetPiece(int index, Coord targetCoord) {
			int freeIndex = currentPieces.FindIndex(op => FindFreePiece(op, index) && op.Coord == targetCoord);
			if (freeIndex < 0) {
				freeIndex = currentPieces.FindIndex(op => FindFreePiece(op, index));
			}
			OmokPiece piece = null;
			if (freeIndex < 0) {
				piece = Instantiate(gamePieces[index].Piece.gameObject).GetComponent<OmokPiece>();
				piece.Index = index;
				piece.Player = this;
				piece.transform.SetParent(game.pieceArea, false);
				currentPieces.Add(piece);
			} else {
				piece = currentPieces[freeIndex];
			}
			piece.Coord = targetCoord;
			piece.gameObject.SetActive(true);
			CleanUpEmptyPieceSlots();
			return piece;
		}
		public void FreePiece(OmokPiece piece) {
			if (currentPieces.IndexOf(piece) < 0) {
				throw new System.Exception("piece cannot be released... it isn't known?");
			}
			piece.gameObject.SetActive(false);
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
