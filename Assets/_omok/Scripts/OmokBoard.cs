using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class OmokBoard : MonoBehaviour {
	[System.Serializable]
	public class UnityEvent_Vector2Int : UnityEvent<Vector2Int> { }

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
	protected float mouseDistance = 2.5f;
	protected Vector2Int currentSelectedSpot;
	protected Vector3 mousePosition;
	[SerializeField]
	protected UnityEvent_Vector2Int _onClick;

	private Dictionary<Vector2Int, OmokPiece> map = new Dictionary<Vector2Int, OmokPiece>();

	[ContextMenuItem(nameof(RefreshDebug), nameof(RefreshDebug))]
	[TextArea(5,10)]
	public string _debug;

	public Vector2Int CurrentSelectedSpot => currentSelectedSpot;
	public Vector3 MousePosition => mousePosition;

	public Vector3 MouseLookOffsetPosition => MousePosition - transform.forward * mouseDistance;
	private void Awake() {
		if (_boardArea == null) {
			_boardArea = transform;
		}
	}

	void Start() {
		RefreshMap();
	}

	public void RefreshMap() {
		map.Clear();
		foreach (Transform t in _gamePieces.transform) {
			OmokPiece piece = t.GetComponent<OmokPiece>();
			if (piece == null) { continue; }
			SetPieceAt(piece.Coord, piece);
		}
	}

	public void SetPieceAt(Vector2Int coord, OmokPiece piece) {
		if (piece == null) {
			map.Remove(coord);
			return;
		}
		//Debug.Log(coord + " : " + t);
		if (map.TryGetValue(coord, out OmokPiece found)) {
			Debug.LogWarning($"multiple objects at {coord}: {found}, {piece}");
			return;
		}
		map[coord] = piece;
	}

	public Vector2Int GetCoord(Vector3 position) {
		Vector3 local = _boardArea.InverseTransformPoint(position);
		return new Vector2Int((int)Mathf.Round(local.x), (int)Mathf.Round(local.y));
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


	public static Vector2Int GetCoordFromPiece(OmokPiece piece) => piece.Coord;

	public string DebugPrint() {
		return OmokState.ToString(map);
	}

	public void RefreshDebug() {
		_boardArea = transform;
		RefreshMap();
		_debug = DebugPrint();
		Debug.Log(_debug);
	}

	public Vector3 GetPosition(Vector2Int coord) {
		Vector3 v = new Vector3(coord.x, coord.y);
		return _boardArea.TransformPoint(v);
	}

	public OmokPiece PieceAt(Vector2Int coord) {
		if (map.TryGetValue(coord, out OmokPiece piece)) {
			return piece;
		}
		return null;
	}
}
