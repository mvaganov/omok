using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmokPiece : MonoBehaviour {
	[SerializeField]
	protected OmokPlayer player;
	[SerializeField]
	protected Renderer[] renderers = new Renderer[1];
	public OmokPlayer Player {
		get => player;
		set {
			player = value;
			RefreshColor();
		}
	}
	public OmokBoard Board {
		get {
			if (Player != null) { return Player.Game.Board; }
			OmokGame game = GetComponentInParent<OmokGame>();
			if (game != null) {
				return game.Board;
			}
			return null;
		}
	}

	public Vector2Int Coord {
		get {
			return Board.GetCoord(transform.position);
		}
		set {
			OmokBoard board = Board;
			board.SetPieceAt(Coord, null);
			transform.position = board.GetPosition(value);
			board.SetPieceAt(value, this);
		}
	}

	private void RefreshColor() {
		if (player == null) { return; }
		for (int i = 0; i < renderers.Length; i++) {
			renderers[i].material.color = player.Color;
		}
	}

	void Start() {
		RefreshColor();
	}

	void Update() {

	}
}
