using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmokPlayer : MonoBehaviour {
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

	public OmokGame Game => game;

	public Color Color => material.color;

	public OmokPiece CreatePiece() => GetPiece(0);

	public OmokPiece GetPiece(int index) {
		int freeIndex = currentPieces.FindIndex(op => op.gameObject.activeSelf == false);
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
		return piece;
	}
}
