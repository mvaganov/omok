using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Omok {
	[System.Serializable]
	public class OmokStateAnalysis {
		public OmokState state;
		public Dictionary<Coord, List<OmokLine>> lineMap = new Dictionary<Coord, List<OmokLine>>();
		public const byte LineLength = 5;

		public void ForEachLine(System.Action<OmokLine> action) {
			foreach (var kvp in lineMap) {
				for (int i = 0; i < kvp.Value.Count; i++) {
					action(kvp.Value[i]);
				}
			}
		}

		public IEnumerator Analyze(OmokState state, Action onAnalysisComplete) {
			this.state = state;
			lineMap.Clear();
			yield return this.state.ForEachPiece(PieceAnalysis, onAnalysisComplete);
		}

		public void Analyze(OmokState state) {
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
	}
}
