using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OmokLine
{
	public Coord start;
	public Coord direction;
	public byte length;
	public byte player;
	public byte count;

	public OmokLine(Coord start, Coord direction, byte length, byte player) {
		this.start = start; this.direction = direction; this.length = length; this.player = player; this.count = 0;
		if (direction.x < 0 || (direction.x == 0 && direction.y < 0)) {
			Invert();
		}
	}

	public bool Update(OmokState state) {
		count = 0;
		OmokState.UnitState compliant, opposing;
		switch(player){
			case 0:
				compliant = OmokState.UnitState.Player0;
				opposing = OmokState.UnitState.Player1;
				break;
			case 1:
				compliant = OmokState.UnitState.Player1;
				opposing = OmokState.UnitState.Player0;
				break;
			default:
				throw new System.Exception("unacceptable player");
		}
		bool valid = true;
		int lineCount = 0;
		ForEachTest(c => {
			OmokState.UnitState unitState = state.GetState(c);
			if (unitState == opposing) {
				valid = false;
			}
			if (unitState == compliant) {
				++lineCount;
			}
			return false;
		});
		count = (byte)lineCount;
		return valid;
	}

	private void Invert() {
		start = start + (direction * length);
		direction = -direction;
	}

	public bool Contains(Coord point) {
		if (start == point) {
			return true;
		}
		return ForEachTest(c => c == point);
	}

	public bool ForEachTest(System.Func<Coord, bool> action) {
		Coord cursor = start;
		for (int i = 0; i < length; i++) {
			cursor += direction;
			if (action.Invoke(cursor)) {
				return true;
			}
		}
		return false;
	}

	public bool Equals(OmokLine other) {
		if (length != other.length) { return false; }
		if (start == other.start && direction == other.direction) {
			return true;
		}
		Coord invertedDir = -direction;
		if (invertedDir == other.direction) {
			Coord switchedPoint = start + (direction * length);
			if (switchedPoint == other.start) {
				return true;
			}
		}
		return false;
	}
}
