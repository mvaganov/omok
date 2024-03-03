using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OmokMove
{
	public Coord coord;
	public byte player;

	public OmokMove(Coord coord, byte player) {
		this.coord = coord; this.player = player;
	}

	public bool Equals(OmokMove other) => coord == other.coord && player == other.player;

	public override bool Equals(object obj) => obj is OmokMove omok && Equals(omok);

	override public int GetHashCode() => coord.GetHashCode() ^ player;
}
