using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	public class OmokPiece : MonoBehaviour, IBelongsToOmokGame {
		[SerializeField]
		protected OmokPlayer player;
		[SerializeField]
		protected Renderer[] renderers = new Renderer[0];
		public OmokGame omokGame => player.omokGame;
		public int Index { get; set; }
		public OmokPlayer Player {
			get => player;
			set {
				player = value;
				RefreshColor();
			}
		}

		public OmokBoard Board {
			get {
				if (Player != null) { return Player.omokGame.Board; }
				OmokGame game = GetComponentInParent<OmokGame>();
				if (game != null) {
					return game.Board;
				}
				return null;
			}
		}

		public Coord Coord {
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
}
