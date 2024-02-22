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
	public List<PieceElement> currentPieces = new List<PieceElement>();

	public OmokGame Game => game;

	public Color Color => material.color;

	public OmokPiece CreatePiece() => CreatePiece(0);

	public OmokPiece CreatePiece(int index) {
		OmokPiece piece = Instantiate(gamePieces[index].Piece.gameObject).GetComponent<OmokPiece>();
		piece.Index = index;
		piece.Player = this;
		piece.transform.SetParent(game.pieceArea, false);
		return piece;
	}
}
