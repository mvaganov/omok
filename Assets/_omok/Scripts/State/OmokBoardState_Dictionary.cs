using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	public class OmokBoardState_Dictionary : IOmokBoardState {
		[SerializeField]
		protected Coord _min, _max;
		protected Dictionary<Coord, OmokUnitState> stateMap = new Dictionary<Coord, OmokUnitState>();

		public Coord size {
			get {
				Coord size = _max - _min + Coord.one;
				if (size.x < 0 && size.y < 0) { return Coord.zero; }
				return size;
			}
		}

		public Coord start => _min;
		public bool Equals(IOmokBoardState other) => IBoardOmokState_Extension.Equals(this, other);
		public override bool Equals(object obj) => obj is IOmokBoardState omokState && IBoardOmokState_Extension.Equals(this, omokState);
		public override int GetHashCode() => IBoardOmokState_Extension.HashCode(this);

		public OmokBoardState_Dictionary(IOmokBoardState source) {
			Copy(source);
		}

		public void Copy(IOmokBoardState source) {
			_min = Coord.MAX; _max = Coord.MIN;
			stateMap.Clear();
			if (source != null) {
				source.ForEachPiece((coord, unitState) => TrySetState(coord, unitState));
			}
		}

		public void ForEachPiece(Action<Coord, OmokUnitState> action) {
			foreach (KeyValuePair<Coord, OmokUnitState> kvp in stateMap) {
				action(kvp.Key, kvp.Value);
			}
		}

		public IEnumerator ForEachPiece(Action<Coord, OmokUnitState> action, Action onForLoopComplete, Func<bool> keepCalculating) {
			List<Coord> keys = new List<Coord>(stateMap.Keys);
			for (int i = 0; i < keys.Count; ++i) {
				if (keepCalculating != null && !keepCalculating.Invoke()) {
					yield break;
				}
				action(keys[i], stateMap[keys[i]]);
				yield return null;
			}
			onForLoopComplete?.Invoke();
		}

		public bool TryGetState(Coord coord, out OmokUnitState state) => stateMap.TryGetValue(coord, out state);

		public bool TrySetState(Coord coord, OmokUnitState unitState) {
			stateMap[coord] = unitState;
			if (coord.x < _min.x) { _min.x = coord.x; }
			if (coord.y < _min.y) { _min.y = coord.y; }
			if (coord.x > _max.x) { _max.x = coord.x; }
			if (coord.y > _max.y) { _max.y = coord.y; }
			return true;
		}
	}
}
