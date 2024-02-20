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

	public string DebugPrint() {
		//string text = BlockLower + BlockUpper + BlockBoth;
		List<OmokPiece> pieces = new List<OmokPiece>();
		pieces.AddRange(map.Values);
		Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue), max = new Vector2Int(int.MinValue, int.MinValue);
		pieces.ForEach(a => {
			Vector2Int coord = a.Coord;// GetCoord(a.transform.position);
			min.x = Mathf.Min(min.x, coord.x);
			min.y = Mathf.Min(min.y, coord.y);
			max.x = Mathf.Max(max.x, coord.x);
			max.y = Mathf.Max(max.y, coord.y);
		});
		pieces.Sort((a, b) => {
			Vector2Int coordA = a.Coord;// GetCoord(a.position);
			Vector2Int coordB = b.Coord;// GetCoord(b.position);
			if (coordA.y < coordB.y) { return 1; } else if (coordA.y > coordB.y) { return -1; }
			if (coordA.x < coordB.x) { return -1; } else if (coordA.x > coordB.x) { return 1; }
			return 0;
		});
		//Debug.Log(pieces.Count + "\n" + string.Join("\n", pieces.ConvertAll(t=> CoordOf(t.position)+":"+t.name)));
		StringBuilder sb = new StringBuilder();
		sb.Append(min + "->" + max);
		int i = 0;
		Vector2Int nextPosition = pieces[i].Coord;// GetCoord(pieces[i].position);
		for (int row = max.y; row >= min.y; --row) {
			sb.Append("\n");
			for (int col = min.x; col <= max.x; ++col) {
				if (row == nextPosition.y && col == nextPosition.x) {
					sb.Append("!");
					if (i < pieces.Count - 1) {
						nextPosition = pieces[++i].Coord;// GetCoord(pieces[++i].position);
						//Debug.Log(nextPosition);
					}
				} else {
					sb.Append(".");
				}
			}
		}
		return sb.ToString();
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
