using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Omok {
	[System.Serializable]
	public class OmokStateAnalysis {
		private OmokState state;
		public Dictionary<Coord, List<OmokLine>> lineMap = new Dictionary<Coord, List<OmokLine>>();
		public const byte LineLength = 5;
		public Action<OmokMove> onAnalysisFinished;
		public float[] scoring;
		public static float[] DefaultScorePerLineFill = { 0, 1, 2, 4, 8, 16 };

		private bool _doingAnalysis = false;
		public OmokState State => state;

		public bool IsDoingAnalysis => _doingAnalysis;

		public OmokStateAnalysis(OmokState state) {
			this.state = state;
			lineMap = new Dictionary<Coord, List<OmokLine>>();
		}

		public void ForEachLine(Action<OmokLine> action) {
			if (this == null) {
				Debug.LogError("missing this");
			}
			if (lineMap == null) {
				Debug.LogError("missing lineMap");
			}
			foreach (var kvp in lineMap) {
				if (kvp.Value == null) {
					Debug.LogError($"missing lineMap[{kvp.Key}]");
				}
				for (int i = 0; i < kvp.Value.Count; i++) {
					if (kvp.Value[i] == null) {
						Debug.LogError($"missing lineMap[{kvp.Key}][{i}]");
					}
					action(kvp.Value[i]);
				}
			}
		}

		public IEnumerator ForEachLine(Action<OmokLine> action, Action onForLoopComplete) {
			List<List<OmokLine>> omokLinesPerCoord = new List<List<OmokLine>>(lineMap.Values);
			for(int c = 0; c < omokLinesPerCoord.Count; ++c) {
				for(int i = 0; i < omokLinesPerCoord[c].Count; ++i) {
					action(omokLinesPerCoord[c][i]);
				}
				yield return null;
			}
			onForLoopComplete?.Invoke();
		}

		public IEnumerator AnalyzeCoroutine(OmokMove move, OmokState state, Action<OmokMove> onAnalysisComplete) {
			this.state = state;
			lineMap.Clear();
			_doingAnalysis = true;
			AddCallBackOnFinish(onAnalysisComplete);
			yield return this.state.ForEachPiece(PieceAnalysis, null);
			scoring = GetPlayerScoresFromLines();
			_doingAnalysis = false;
			onAnalysisFinished?.Invoke(move);
		}

		public void AddCallBackOnFinish(Action<OmokMove> onAnalysisComplete) {
			if (onAnalysisComplete == null) {
				return;
			}
			if (onAnalysisFinished != null) {
				onAnalysisFinished -= onAnalysisComplete;
				onAnalysisFinished += onAnalysisComplete;
			} else {
				onAnalysisFinished = onAnalysisComplete;
			}
		}

		public void Analyze(OmokState state) {
			Debug.Log($"###### lineMap [{lineMap}]");
			this.state = state;
			lineMap.Clear();
			this.state.ForEachPiece(PieceAnalysis);
		}

		public void AnalyzeTest(OmokState state) {
			this.state = state;
			lineMap.Clear();
			this.state.ForEachPiece(PieceAnalysisTest);
		}
		public void PieceAnalysisTest(Coord coord, UnitState unitState) {
			bool isPlayerPiece = false;
			byte player = 0;
			switch (unitState) {
				case UnitState.Player0: player = 0; isPlayerPiece = true; break;
				case UnitState.Player1: player = 1; isPlayerPiece = true; break;
			}
			if (!isPlayerPiece) {
				return;
			}
			//for (int d = 0; d < directions.Length; ++d) {
			Coord dir = directions[0];
			//for (int i = 0; i < LineLength; ++i) {
			OmokLine line = new OmokLine(coord //- dir * i
																				 , dir, LineLength, player);
			if (!IsKnown(line) && line.Update(state)) {
				Debug.Log($"adding player{player} {line}");
				AddLine(line);
			}
			//}
			//}
		}

		private Coord[] directions = new Coord[] {
		new Coord( 1, 0),
		new Coord( 1, 1),
		new Coord( 0, 1),
		new Coord(-1, 1),
		new Coord(-1, 0),
		new Coord(-1,-1),
		new Coord( 0,-1),
		new Coord( 1,-1),
	};

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
			if (lineMap.TryGetValue(line.start, out List<OmokLine> lines) && lines.IndexOf(line) >= 0) {
				return true;
			}
			return false;
		}

		public void AddLine(OmokLine line) {
			if (!lineMap.TryGetValue(line.start, out List<OmokLine> lines)) {
				lineMap[line.start] = lines = new List<OmokLine>(1);
			}
			lines.Add(line);
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
	}
}
