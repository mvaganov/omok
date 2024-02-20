using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmokPlayer : MonoBehaviour {
	[SerializeField]
	protected OmokGame game;
	public Material material;
	public OmokPiece[] piecePrefabs = new OmokPiece[1];
	public List<OmokPiece> pieces = new List<OmokPiece>();

	public OmokGame Game => game;

	public Color Color => material.color;

	public OmokPiece CreatePiece() => CreatePiece(0);

	public OmokPiece CreatePiece(int index) {
		OmokPiece piece = Instantiate(piecePrefabs[index].gameObject).GetComponent<OmokPiece>();
		piece.Player = this;
		piece.transform.SetParent(game.pieceArea, false);
		return piece;
	}
}
