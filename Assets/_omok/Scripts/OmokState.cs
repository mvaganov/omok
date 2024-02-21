using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OmokState {
	[System.Serializable]
	public struct UnitState {
		public byte player;
		public byte unit;
	}

	private Dictionary<Vector2Int, UnitState> map = new Dictionary<Vector2Int, UnitState>();

	//protected BitArray serialized = new BitArray();
}
