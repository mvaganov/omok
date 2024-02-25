using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmokStateAnalysis {
	public OmokState state;
	public List<OmokLine> lines = new List<OmokLine> ();
	public Dictionary<Coord,List<OmokLine>> lineMap = new Dictionary<Coord, List<OmokLine>> ();

	public void Analyze(OmokState state) {
		this.state = state;
		lineMap.Clear();
		lines.Clear();
		state.ForEachPiece(Analysis);
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

	public void Analysis(Coord coord, OmokState.UnitState unitState) {
		// TODO go through each piece
			// try to make a valid line in each direction
			// when checking a line going right, if there is no compliant or opposing neighbor to the left, move the start to the left up to 5 times.
	}
}
