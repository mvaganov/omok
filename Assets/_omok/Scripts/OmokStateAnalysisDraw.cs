using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmokStateAnalysisDraw : MonoBehaviour {
	[ContextMenuItem(nameof(ForceUpdate),nameof(ForceUpdate))]
	public OmokBoard board;
	public float _lineDistance = 1;
	public OmokState _state;
	public OmokStateAnalysis _analysis;
	public List<Wire> wires = new List<Wire>();
	public List<Wire> extraWires = new List<Wire>();

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

	private void Reset() {
		board = GetComponent<OmokBoard>();
	}

	void Update() {
		if (_analysis.state != _state) {
			ForceUpdate();
		}
	}

	public void ForceUpdate() {
		board.SaveState();
		_state = board.State;
		_analysis.Analyze(_state);
		ClearWires();
		foreach (var kvp in _analysis.lineMap) {
			RenderLines(kvp.Value);
		}
	}

	public void RenderLines(List<OmokLine> lines) {
		Vector3 offset = board.Up * _lineDistance;
		for (int i = 0; i < lines.Count; i++) {
			OmokLine line = lines[i];
			float progress = (float)line.count / line.length;
			Vector3 start = board.GetPosition(line.start) + offset * (line.count+1);
			Vector3 end = board.GetPosition(line.Last) + offset * (line.count + 1);
			Wire w = GetWire();
			Color c = _lineGradients[line.player].Evaluate(progress);
			w.Line(start, end, c);
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
}
