using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmokGame : MonoBehaviour
{
	[SerializeField]
  protected OmokBoard board;
  public Transform pieceArea;
  public OmokPlayer[] players = new OmokPlayer[2];
	[SerializeField]
	protected int whosTurn = 0;

	public int WhosTurn {
		get => whosTurn;
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

	void Update() {

	}

	public void PlacePieceForCurrentPlayer() {
		Vector2Int coord = board.CurrentSelectedSpot;
		PlacePieceForCurrentPlayerAt(coord);
	}
	public void PlacePieceForCurrentPlayerAt(Vector2Int coord) {
		OmokPiece piece = board.PieceAt(coord);
		if (piece == null) {
			piece = players[WhosTurn].CreatePiece();
			piece.Coord = coord;
			WhosTurn++;
		}
	}
}