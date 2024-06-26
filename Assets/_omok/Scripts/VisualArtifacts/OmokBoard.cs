﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Omok {
	public class OmokBoard : MonoBehaviour, IBelongsToOmokGame {

		[System.Serializable]
		public class UnityEvent_Coord : UnityEvent<Coord> { }

		[ContextMenuItem(nameof(ReadFromBoardIntoState), nameof(ReadFromBoardIntoState))]
		[ContextMenuItem(nameof(LoadFromStateIntoBoard), nameof(LoadFromStateIntoBoard))]
		[SerializeField]
		protected OmokGame game;
		[SerializeField]
		protected Transform _gamePieces;
		[SerializeField]
		protected KeyCode click = KeyCode.Mouse0;
		[SerializeField]
		protected Camera _camera;
		[SerializeField]
		protected Transform _mouseMarker;
		[SerializeField]
		protected Transform _boardArea;
		[SerializeField]
		protected Transform _offBoardArea;
		[SerializeField]
		protected float mouseDistance = 2.5f;
		protected Coord currentSelectedSpot;
		protected Vector3 mousePosition;
		protected Coord _lastHover;
		[SerializeField]
		protected UnityEvent_Coord _onClick = new UnityEvent_Coord();
		[SerializeField]
		protected UnityEvent_Coord _onHover;
		protected bool _isMouseOnBoard = true;

		/// <summary>
		/// Managed state
		/// </summary>
		[SerializeField]
		protected OmokBoardState _state = new OmokBoardState();

		private Dictionary<Coord, OmokPiece> map = new Dictionary<Coord, OmokPiece>();

		[ContextMenuItem(nameof(RefreshDebug), nameof(RefreshDebug))]
		[TextArea(5, 10)]
		public string _debug;

		public OmokGame omokGame => game;
		public IBelongsToOmokGame reference => null;

		public Coord CurrentSelectedSpot => currentSelectedSpot;
		public Vector3 MousePosition => mousePosition;
		public OmokBoardState State => _state;
		public Vector3 Up => -transform.forward;
		public Vector3 LookOffsetPosition => Up * mouseDistance;
		public Vector3 MouseLookOffsetPosition => MousePosition + LookOffsetPosition;
		public Transform GamePieces => _gamePieces;

		private void Awake() {
			if (_boardArea == null) {
				_boardArea = transform;
			}
		}

		void Start() {
			RefreshMap();
		}

		public void ReadFromBoardIntoState() {
			_state = ReadStateFromBoard();
		}

		public OmokBoardState ReadStateFromBoard() {
			RefreshMap();
			OmokBoardState state = new OmokBoardState();
			state.SetState(map);
			return state;
		}

		public void LoadFromStateIntoBoard() {
			LoadState(_state);
		}

		public void LoadState(OmokBoardState state) {
			FreeCurrentPieces();
			map.Clear();
			//Debug.Log(state.DebugSerialized());
			//Debug.Log(state.ToDebugString());
			state.ForEachPiece(CreatePiece);
		}

		public void CreatePiece(Coord coord, OmokUnitState pieceType) {
			OmokPiece piece = GetPiece(pieceType, coord);
			if (piece != null) {
				//Debug.Log($"SETPIECE {pieceType} {coord}    {piece.Player.name} {piece.Player.Index}");
				SetPieceAt(coord, piece);
			}
		}

		public OmokPiece GetPiece(OmokUnitState unitState, Coord targetCoord) {
			switch (unitState) {
				case OmokUnitState.Player0:
					return game.players[0].CreatePiece(targetCoord);
				case OmokUnitState.Player1:
					return game.players[1].CreatePiece(targetCoord);
			}
			return null;
		}

		public void FreeCurrentPieces() {
			foreach (Transform t in _gamePieces.transform) {
				OmokPiece piece = t.GetComponent<OmokPiece>();
				if (piece == null || !piece.gameObject.activeSelf) { continue; }
				piece.gameObject.SetActive(false);
			}
		}

		public void RefreshMap() {
			map.Clear();
			foreach (Transform t in _gamePieces.transform) {
				OmokPiece piece = t.GetComponent<OmokPiece>();
				if (piece == null || !piece.gameObject.activeSelf) { continue; }
				SetPieceAt(piece.Coord, piece);
				if (piece.Player.currentPieces.IndexOf(piece) < 0) {
					piece.Player.currentPieces.Add(piece);
				}
			}
		}

		public void SetPieceAt(Coord coord, OmokPiece piece) {
			if (piece == null) {
				map.Remove(coord);
				return;
			}
			//Debug.Log($"SET {coord} : {piece.Player.name} {piece.Player.Index} {piece.Index} {piece}");
			if (map.TryGetValue(coord, out OmokPiece found)) {
				if (piece == found) {
					//Debug.LogWarning($"{found} already at {coord}");
				} else {
					Debug.LogWarning($"multiple objects at {coord}? {found}, {piece}");
				}
				return;
			}
			map[coord] = piece;
			piece.transform.position = GetPosition(coord);
		}

		public Coord GetCoord(Vector3 position) {
			Vector3 local = _boardArea.InverseTransformPoint(position);
			return new Coord((int)Mathf.Round(local.x), (int)Mathf.Round(local.y));
		}

		void Update() {
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) {
				_isMouseOnBoard = false;
				return;
			}
			_isMouseOnBoard = true;
			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit, 1000, -1, QueryTriggerInteraction.Ignore)) {
				mousePosition = hit.point;
				currentSelectedSpot = GetCoord(mousePosition);
				_mouseMarker.position = hit.point;
				_mouseMarker.rotation = Quaternion.LookRotation(hit.normal);
				if (_lastHover != currentSelectedSpot) {
					_onHover.Invoke(currentSelectedSpot);
					_lastHover = currentSelectedSpot;
				}
			}
			if (Input.GetKeyDown(click) && _onClick != null) {
				_onClick.Invoke(currentSelectedSpot);
			}
		}


		public static Coord GetCoordFromPiece(OmokPiece piece) => piece.Coord;

		public string DebugPrint() {
			return OmokBoardState.ToString(map);
		}

		public void RefreshDebug() {
			_boardArea = transform;
			RefreshMap();
			string oldMap = _debug;
			_debug = DebugPrint();
			if (oldMap != _debug) {
				Debug.Log(_debug);
			}
		}

		public Vector3 GetPosition(Coord coord) {
			Vector3 v = new Vector3(coord.x, coord.y);
			return _boardArea.TransformPoint(v);
		}

		public OmokPiece PieceAt(Coord coord) {
			if (map.TryGetValue(coord, out OmokPiece piece)) {
				return piece;
			}
			return null;
		}
	}
}
