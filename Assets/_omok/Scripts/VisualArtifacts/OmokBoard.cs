using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Omok {
	public class OmokBoard : MonoBehaviour {

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
		[SerializeField]
		protected UnityEvent_Coord _onClick;

		[SerializeField]
		protected OmokState _state = new OmokState();

		private Dictionary<Coord, OmokPiece> map = new Dictionary<Coord, OmokPiece>();

		[ContextMenuItem(nameof(RefreshDebug), nameof(RefreshDebug))]
		[TextArea(5, 10)]
		public string _debug;

		public Coord CurrentSelectedSpot => currentSelectedSpot;
		public Vector3 MousePosition => mousePosition;
		public OmokState State => _state;
		public Vector3 Up => -transform.forward;
		public Vector3 MouseLookOffsetPosition => MousePosition + Up * mouseDistance;
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

		public OmokState ReadStateFromBoard() {
			RefreshMap();
			_state.SetState(map);
			return _state;
		}

		public void LoadFromStateIntoBoard() {
			LoadState(_state);
		}

		public void LoadState(OmokState state) {
			FreeCurrentPieces();
			map.Clear();
			Debug.Log(state.DebugSerialized());
			Debug.Log(state.ToDebugString());
			state.ForEachPiece(CreatePiece);
		}

		public void CreatePiece(Coord coord, UnitState pieceType) {
			OmokPiece piece = GetPiece(pieceType);
			if (piece != null) {
				//Debug.Log($"SETPIECE {pieceType} {coord}    {piece.Player.name} {piece.Player.Index}");
				SetPieceAt(coord, piece);
			}
		}

		public OmokPiece GetPiece(UnitState unitState) {
			switch (unitState) {
				case UnitState.Player0:
					return game.players[0].CreatePiece();
				case UnitState.Player1:
					return game.players[1].CreatePiece();
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
					Debug.LogWarning($"{found} already at {coord}");
				} else {
					Debug.LogWarning($"multiple objects at {coord}: {found}, {piece}");
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
			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit, 1000, -1, QueryTriggerInteraction.Ignore)) {
				mousePosition = hit.point;
				currentSelectedSpot = GetCoord(mousePosition);
				_mouseMarker.position = hit.point;
				_mouseMarker.rotation = Quaternion.LookRotation(hit.normal);
			}
			if (Input.GetKey(click)) {
				_onClick.Invoke(currentSelectedSpot);
			}
		}


		public static Coord GetCoordFromPiece(OmokPiece piece) => piece.Coord;

		public string DebugPrint() {
			return OmokState.ToString(map);
		}

		public void RefreshDebug() {
			_boardArea = transform;
			RefreshMap();
			_debug = DebugPrint();
			Debug.Log(_debug);
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
