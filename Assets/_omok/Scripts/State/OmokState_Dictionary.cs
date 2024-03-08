using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	public class OmokState_Dictionary : IOmokState {
		[SerializeField]
		protected Coord _min, _max;
		protected Dictionary<Coord, UnitState> stateMap = new Dictionary<Coord, UnitState>();

		public Coord size {
			get {
				Coord size = _max - _min + Coord.one;
				if (size.x < 0 && size.y < 0) { return Coord.zero; }
				return size;
			}
		}

		public Coord start => _min;

		public OmokState_Dictionary(IOmokState source) {
			Copy(source);
		}

		public void Copy(IOmokState source) {
			_min = Coord.MAX; _max = Coord.MIN;
			stateMap.Clear();
			if (source != null) {
				source.ForEachPiece((coord, unitState) => TrySetState(coord, unitState));
			}
		}

		public void ForEachPiece(Action<Coord, UnitState> action) {
			foreach (KeyValuePair<Coord, UnitState> kvp in stateMap) {
				action(kvp.Key, kvp.Value);
			}
		}

		public IEnumerator ForEachPiece(Action<Coord, UnitState> action, Action onForLoopComplete) {
			List<Coord> keys = new List<Coord>(stateMap.Keys);
			for (int i = 0; i < keys.Count; ++i) {
				action(keys[i], stateMap[keys[i]]);
				yield return null;
			}
			onForLoopComplete?.Invoke();
		}

		public bool TryGetState(Coord coord, out UnitState state) => stateMap.TryGetValue(coord, out state);

		public bool TrySetState(Coord coord, UnitState unitState) {
			stateMap[coord] = unitState;
			if (coord.x < _min.x) { _min.x = coord.x; }
			if (coord.y < _min.y) { _min.y = coord.y; }
			if (coord.x > _max.x) { _max.x = coord.x; }
			if (coord.y > _max.y) { _max.y = coord.y; }
			return true;
		}
	}
}
