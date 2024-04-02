using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	public class OmokStateLineAnalysisDraw : MonoBehaviour, IBelongsToOmokGame {
		
		[ContextMenuItem(nameof(test), nameof(test))]
		[ContextMenuItem(nameof(ForceUpdate), nameof(ForceUpdate))]
		public OmokBoard board;
		public float _lineDistance = 1f/128;
		public OmokBoardStateAnalysis _analysis;
		public List<Wire> wires = new List<Wire>();
		public List<Wire> extraWires = new List<Wire>();
		[SerializeField] protected bool _generateLines = true;
		[SerializeField] protected bool _showLines = true;

		private void test() {
			string test = "!@#$%^&*_+OQX0xo";
			string output = "..|\n";
			for (int i = 0; i < test.Length; i++) {
				output += test[i] + "|\n";
			}
			Debug.Log(output);
		}

		public bool ShowLines {
			get => _showLines;
			set {
				_showLines = value;
				SetLinesVisible(_showLines);
			}
		}

		public Gradient[] _lineGradients = new Gradient[] {
			new Gradient() {
				colorKeys = new GradientColorKey[] {
					new GradientColorKey(Color.black,0),
					new GradientColorKey(Color.magenta,1),
				},
			},
			new Gradient() {
				colorKeys = new GradientColorKey[] {
					new GradientColorKey(Color.white,0),
					new GradientColorKey(Color.green,1),
				},
			}
		};

		public OmokGame omokGame => board.omokGame;

		public IBelongsToOmokGame reference => board;

		private void Reset() {
			board = GetComponent<OmokBoard>();
		}

		public void ForceUpdate() {
			RenderAnalysis();
		}

		public void RenderAnalysis() {
			ClearWires();
			string printed = omokGame.State.ToDebugString();
			Debug.Log(printed);
			_analysis.Analyze(omokGame.State);
			RenderAllLines(null);
		}

		//public IEnumerator ForceUpdateCoroutine() {
		//	board.ReadFromBoardIntoState();
		//	ClearWires();
		//	yield return _analysis.AnalyzeCoroutine(OmokMove.InvalidMove, omokGame.State, RenderAllLines);
		//}

		private void RenderAllLines(OmokMove move) {
			RenderAnalysis(_analysis);
		}

		public void RenderAnalysis(OmokBoardStateAnalysis analysis) {
			if (analysis.lines.Count == 0) {
				Debug.LogError("missing lines?");
			}
			_analysis = analysis;
			if (_generateLines) {
				RenderLines(analysis.lines);
			}
		}

		public void RenderLines(List<OmokLine> lines) {
			Vector3 offset = board.Up * _lineDistance;
			for (int i = 0; i < lines.Count; i++) {
				OmokLine line = lines[i];
				float progress = (float)line.count / line.length;
				Vector3 start = board.GetPosition(line.start) + offset * (line.count + 1);
				Vector3 end = board.GetPosition(line.Last) + offset * (line.count + 1);
				Wire w = GetWire();
				Color c = _lineGradients[line.player].Evaluate(progress);
				w.Line(start, end, c);
				SetLineVisible(w, _showLines);
				wires.Add(w);
			}
		}

		public void ClearWires() {
			ClearNulls(wires);
			ClearNulls(extraWires);
			extraWires.AddRange(wires);
			extraWires.ForEach(w => w.gameObject.SetActive(false));
			wires.Clear();
		}

		private void ClearNulls(List<Wire> wires) {
			for (int i = wires.Count - 1; i >= 0; --i) {
				if (wires[i] == null) {
					wires.RemoveAt(i);
				}
			}
		}

		public Wire GetWire() {
			Wire w;
			if (extraWires.Count != 0) {
				int last = extraWires.Count - 1;
				w = extraWires[last];
				w.gameObject.SetActive(true);
				extraWires.RemoveAt(last);
			} else {
				w = Wires.MakeWire();
			}
			return w;
		}

		private void SetLinesVisible(bool visible) {
			for(int i = 0; i < wires.Count; ++i) {
				SetLineVisible(wires[i], visible);
			}
		}

		private void SetLineVisible(Wire w, bool visible) {
			Renderer[] renderers = w.GetComponentsInChildren<Renderer>();
			System.Array.ForEach(renderers, r => r.enabled = visible);
		}
	}
}
