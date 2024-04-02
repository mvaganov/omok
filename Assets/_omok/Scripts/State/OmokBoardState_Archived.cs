using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Omok {
	public class OmokBoardState_Archived : IOmokBoardState {
		private const int ElementPlayer = 0;
		private const int ElementPiece = 1;
		private const int ElementBitCount = 2;

		[SerializeField]
		protected Coord _start, _size;
		[SerializeField]
		protected BitArray serialized;
		protected Dictionary<Coord, OmokUnitState> stateMap = null;

		protected Coord Max => _start + _size - Coord.one;

		public Coord size => _size;
		public Coord start => _start;

		public bool Equals(IOmokBoardState other) => IBoardOmokState_Extension.Equals(this, other);
		public override bool Equals(object obj) => obj is IOmokBoardState omokState && IBoardOmokState_Extension.Equals(this, omokState);
		public override int GetHashCode() => IBoardOmokState_Extension.HashCode(this);

		public OmokBoardState_Archived() { }

		public OmokBoardState_Archived(IOmokBoardState source) {
			Copy(source);
		}

		public void Copy(IOmokBoardState source) {
			_start = source.start;
			_size = source.size;
			serialized = new BitArray(size.x * size.y * ElementBitCount);
			Coord max = _start + _size;
			//Debug.Log($"copying {_start}:{_size}");
			for (int row = _start.y; row < max.y; ++row) {
				for (int col = _start.x; col < max.x; ++col) {
					Coord coord = new Coord(col, row);
					//Debug.Log($"{coord}");
					source.TryGetState(coord, out OmokUnitState state);
					TrySetState(coord, state);
				}
			}
		}

		public void ForEachPiece(Action<Coord, OmokUnitState> action) {
			Coord cursor = start;
			int horizontalLimit = start.x + size.x;
			int boardSize = serialized.Count / ElementBitCount;
			for (int i = 0; i < boardSize; ++i) {
				ForEachPieceSerializedIterate(i, ref cursor, horizontalLimit, action);
			}
		}

		private void ForEachPieceSerializedIterate(int i, ref Coord cursor, int horizontalLimit, Action<Coord, OmokUnitState> action) {
			OmokUnitState state = GetLocalSerializedState(i);
			//Debug.Log($"foreach: {i} {cursor}{state}");
			if (state != OmokUnitState.None) {
				action.Invoke(cursor, state);
			}
			cursor.x++;
			if (cursor.x >= horizontalLimit) {
				cursor.y++;
				cursor.x = start.x;
			}
		}

		private OmokUnitState GetLocalSerializedState(int index) {
			int i = index * ElementBitCount;
			bool isPiece = serialized[i + ElementPiece];
			bool isPlayer = serialized[i + ElementPlayer];
			OmokUnitState state = (OmokUnitState)(byte)((isPiece ? 1 << ElementPiece : 0) | (isPlayer ? 1 << ElementPlayer : 0));
			//if (state == UnitState.Player0 || state == UnitState.Player1) {
			//	Debug.Log("index: " + i + "/" + serialized.Count + "   " + state);
			//}
			return state;
		}

		public IEnumerator ForEachPiece(Action<Coord, OmokUnitState> action, Action onForLoopComplete) {
			Coord cursor = start;
			int horizontalLimit = start.x + size.x;
			int boardSize = serialized.Count / ElementBitCount;
			for (int i = 0; i < boardSize; ++i) {
				ForEachPieceSerializedIterate(i, ref cursor, horizontalLimit, action);
				yield return null;
			}
			if (onForLoopComplete != null) {
				onForLoopComplete.Invoke();
			}
		}

		public bool TrySetState(Coord coord, OmokUnitState unitState) {
			Coord local = coord - start;
			if (local.x < 0 || local.y < 0 || local.x >= size.x || local.y >= size.y) {
				return false;
			}
			//Debug.Log("set: "+coord + " " +unitState +"   start:" + start + "   local:" + (coord - start)); 
			SetLocalStateSerialized(local, unitState);
			return true;
		}

		public bool TryGetState(Coord coord, out OmokUnitState state) {
			//Debug.Log("get: " + coord + " start:" + start + "   local:" + (coord - start));
			Coord local = coord - start;
			if (local.x < 0 || local.y < 0 || local.x >= size.x || local.y >= size.y) {
				state = OmokUnitState.None;
				//Debug.Log($"{coord} ({local}) is bad");
				return false;
			}
			state = GetLocalStateSerialized(local);
			return true;
		}

		private OmokUnitState GetLocalStateSerialized(Coord localCoord) {
			int index = localCoord.y * size.x + localCoord.x;
			return GetLocalSerializedState(index);
		}

		public OmokUnitState GetState(Coord coord) => IBoardOmokState_Extension.GetState(this, coord);

		public OmokBoardState_Archived(Dictionary<Coord, OmokPiece> map) {
			SetState(map);
		}

		public void SetState(Dictionary<Coord, OmokPiece> map) {
			List<OmokPiece> pieces = new List<OmokPiece>();
			pieces.AddRange(map.Values);
			Coord.CalculateCoordRange(pieces, OmokBoardState.GetCoordFromPiece, out Coord min, out Coord max);
			pieces.Sort(IOmokBoardState.SortPiecesInvertRow);
			//OmokState.SortPieces(pieces, GetCoordFromPiece);
			//Debug.Log(pieces.Count+" "+string.Join("\n", pieces));
			SetStateSerialized(pieces, min, max, OmokBoardState.GetCoordFromPiece, IOmokBoardState.GetStateFrom);
		}

		private void SetStateSerialized<T>(List<T> pieces, Coord min, Coord max,
		Func<T, Coord> getCoord, Func<T, OmokUnitState> getState) {
			serialized = new BitArray(_size.x * _size.y * ElementBitCount);
			_start = min;
			_size = max - min + Coord.one;
			int i = 0;
			serialized = new BitArray(_size.x * _size.y * ElementBitCount);
			if (pieces.Count > 0) {
				Coord nextPosition = getCoord(pieces[i]);
				for (int row = max.y; row >= min.y; --row) {
					for (int col = min.x; col <= max.x; ++col) {
						Coord coord = new Coord(col, row);
						if (row == nextPosition.y && col == nextPosition.x) {
							OmokUnitState state = getState(pieces[i]);
							if (!TrySetState(coord, state)) {
								Debug.LogWarning($"unable to set state {coord} {state}");
							}
							if (i < pieces.Count - 1) {
								nextPosition = getCoord(pieces[++i]);
								if (i >= pieces.Count) {
									break;
								}
							}
						} else {
							if (!TrySetState(coord, OmokUnitState.None)) {
								Debug.LogWarning($"unable to set state {coord} {OmokUnitState.None}");
							}
						}
					}
				}
				Debug.Log(DebugSerialized());
				Debug.Log(this.ToDebugString());
			}
		}

		private void SetLocalStateSerialized(Coord localCoord, OmokUnitState state) {
			int index = localCoord.y * size.x + localCoord.x;
			int i = index * ElementBitCount;
			//if (state == UnitState.Player0 || state == UnitState.Player1) {
			//	Debug.Log($"index: {i}/{serialized.Count}    {state} {(localCoord+start)}");
			//}
			byte code = (byte)state;
			bool isPiece = ((1 << ElementPiece) & code) != 0;
			bool isPlayer = ((1 << ElementPlayer) & code) != 0;
			serialized[i + ElementPiece] = isPiece;
			serialized[i + ElementPlayer] = isPlayer;
		}

		public string DebugSerialized() {
			StringBuilder sb = new StringBuilder();
			if (serialized == null) { sb.Append("null"); }
			else for (int i = 0; i < serialized.Count; ++i) {
				if (i % 2 == 0) { sb.Append(" "); }
				sb.Append(serialized[i] ? '!' : '.');
			}
			return sb.ToString();
		}
	}
}
