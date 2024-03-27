using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Omok {
	[System.Serializable]
	public class OmokStateAnalysis {
		public const byte LineLength = 5;
		public static readonly float[] DefaultScorePerLineFill = { 0, 1, 2, 4, 8, 16 };
		private static Coord[] directions = new Coord[] {
			new Coord( 1, 0),
			new Coord( 1, 1),
			new Coord( 0, 1),
			new Coord(-1, 1),
			new Coord(-1, 0),
			new Coord(-1,-1),
			new Coord( 0,-1),
			new Coord( 1,-1),
		};

		/// <summary>
		/// Reference to a state
		/// </summary>
		private OmokState state;

		/// <summary>
		/// using sorted list instead of dictionary for tighter memory footprint
		/// </summary>
		public List<OmokLine> lines = new List<OmokLine> ();

		/// <summary>
		/// score per player
		/// </summary>
		public float[] scoring = Array.Empty<float>();

		private bool _doingAnalysis = false;

		public bool IsDoingAnalysis => _doingAnalysis;

		public OmokStateAnalysis(OmokState state) {
			this.state = state;
			lines = new List<OmokLine> ();
		}

		public void MarkDoingAnalysis(bool doingAnalysis) => _doingAnalysis = doingAnalysis;

		public void ForEachLine(Action<OmokLine> action) {
			foreach (var line in lines) {
				action(line);
			}
		}

		public IEnumerator ForEachLine(Action<OmokLine> action, Action onForLoopComplete) {
			for(int i = 0; i < lines.Count; ++i) {
				action.Invoke(lines[i]);
				yield return null;
			}
			onForLoopComplete?.Invoke();
		}

		public IEnumerator AnalyzeCoroutine(OmokMove move, OmokState state, Action<OmokMove> onAnalysisComplete) {
			this.state = state;
			lines.Clear();
			_doingAnalysis = true;
			yield return this.state.ForEachPiece(PieceAnalysis, null);
			scoring = GetPlayerScoresFromLines();
			_doingAnalysis = false;
			onAnalysisComplete.Invoke(move);
		}

		public void Analyze(OmokState state) {
			this.state = state;
			lines.Clear();
			this.state.ForEachPiece(PieceAnalysis);
			scoring = GetPlayerScoresFromLines();
		}

		public void PieceAnalysis(Coord coord, UnitState unitState) {
			bool isPlayerPiece = false;
			byte player = 0;
			switch (unitState) {
				case UnitState.Player0: player = 0; isPlayerPiece = true; break;
				case UnitState.Player1: player = 1; isPlayerPiece = true; break;
			}
			if (!isPlayerPiece) {
				return;
			}
			for (int d = 0; d < directions.Length; ++d) {
				Coord dir = directions[d];
				for (int i = 0; i < LineLength - 1; ++i) {
					OmokLine line = new OmokLine(coord - dir * i, dir, LineLength, player);
					if (IsKnown(line)) {
						continue;
					}
					if (line.Update(state)) {
						AddLine(line);
					}
				}
			}
		}

		public bool IsKnown(OmokLine line) {
			return GetIndex(line) >= 0;
		}

		private int GetIndex(OmokLine line) =>
			Util.BinarySearch(lines, line, OmokLine.Equals, OmokLine.PositionLessThan);

		public void AddLine(OmokLine line) {
			int index = GetIndex(line);
			if (index < 0) {
				lines.Insert(~index, line);
			} else {
				Debug.LogWarning($"Already have {line} : {lines[index]}");
			}
		}

		public Dictionary<byte, Dictionary<int, int>> GetLineCountPerPlayer() {
			Dictionary<byte, Dictionary<int, int>> lineCountPerPlayer = new Dictionary<byte, Dictionary<int, int>>();
			ForEachLine(line => {
				if (!lineCountPerPlayer.TryGetValue(line.player, out Dictionary<int, int> map)) {
					lineCountPerPlayer[line.player] = map = new Dictionary<int, int>();
				}
				if (!map.TryGetValue(line.count, out int count)) {
					map[line.count] = 1;
				} else {
					map[line.count] = count + 1;
				}
			});
			return lineCountPerPlayer;
		}

		public float[] GetPlayerScoresFromLines(float[] scorePerLineFill = null) {
			if (scorePerLineFill == null) {
				scorePerLineFill = DefaultScorePerLineFill;
			}
			Dictionary<byte, Dictionary<int, int>> lineCountPerPlayer = GetLineCountPerPlayer();
			int maxPlayerIndex = 0;
			foreach (var kvp in lineCountPerPlayer) {
				if (kvp.Key > maxPlayerIndex) {
					maxPlayerIndex = kvp.Key;
				}
			}
			float[] scores = new float[maxPlayerIndex+1];
			foreach (var kvp in lineCountPerPlayer) {
				foreach (var lineCount in kvp.Value) {
					scores[kvp.Key] += scorePerLineFill[lineCount.Key] * lineCount.Value;
				}
			}
			return scores;
		}

		public string DebugText() {
			StringBuilder debugText = new StringBuilder();
			Dictionary<byte, Dictionary<int, int>> lineCountPerPlayer = GetLineCountPerPlayer();
			List<byte> players = new List<byte>(lineCountPerPlayer.Keys);
			players.Sort();
			string[] colorText = { "#000", "#fff" };
			for (int p = 0; p < players.Count; ++p) {
				if (p > 0) {
					debugText.Append("\n");
				}
				debugText.Append($"<{colorText[players[p]]}>");
				Dictionary<int, int> map = lineCountPerPlayer[players[p]];
				List<int> lineLengths = new List<int>(map.Keys);
				lineLengths.Sort();
				for (int l = 0; l < lineLengths.Count; ++l) {
					int lineCount = map[lineLengths[l]];
					debugText.Append($"{lineLengths[l]}:{lineCount},");
				}
				debugText.Append("</color>");
			}
			return debugText.ToString();
		}

		public static float SummarizeScore(int player, float[] score) {
			return score[player] - score[(player + 1) % 2];
		}

		override public string ToString() => $"({string.Join(", ", scoring)})";
	}
}
