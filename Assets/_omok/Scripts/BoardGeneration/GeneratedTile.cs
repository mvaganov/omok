using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedTile : MonoBehaviour {
	public GeneratedBoardBoundaries _boundaries;
	public GeneratedTile[] _neighbors = null;
	public BoxCollider _box;
	public GeneratingBoard _board;
	public bool _isMapEdge;

	public bool IsMapEdge {
		get => _isMapEdge;
		set {
			_isMapEdge = value;
			Renderer r = GetComponent<Renderer>();
			if (r == null) {
				Debug.LogError($"missing renderer for {name}");
			} else {
				r.material.color = _isMapEdge ? _board.EdgeTileColor : _board.NormalTileColor;
			}
		}
	}

	public void FindDeepMapEdges(HashSet<GeneratedTile> out_found) {
		for (int i = 0; i < _neighbors.Length; i++) {
			GeneratedTile n = _neighbors[i];
			if (n == null || !n.IsMapEdge) {
				continue;
			}
			if (n.IsEveryNeighborMapEdge()) {
				out_found.Add(n);
			}
		}
		if (this.IsEveryNeighborMapEdge()) {
			out_found.Add(this);
		}
	}

	private bool IsEveryNeighborMapEdge() {
		for (int i = 0; i < _neighbors.Length; i++) {
			GeneratedTile n = _neighbors[i];
			if (n != null && !n.IsMapEdge) {
				return false;
			}
		}
		return true;
	}

	// TODO have the board do this in a batch.
	public void InitializeNeighbors(GeneratingBoard board) {
		_board = board;
		if (_neighbors == null || _neighbors.Length != _boundaries.edges.Length) {
			_neighbors = new GeneratedTile[_boundaries.edges.Length];
		}
		FindNeighbors();
		//StartCoroutine(DoThisSoon());
		//IEnumerator DoThisSoon() {
		//	yield return null;
		//	FindNeighbors();
		//}
	}

	public Ray GetEdgeLocal(int i) {
		if (i < 0 || i >= _boundaries.edges.Length) {
			Debug.LogError($"WOAH BUDDY! {i} is OOB");
		}
		IPlane portal = _boundaries.edges[i];
		Ray ray = new Ray(portal.Origin, portal.Normal);
		return ray;
	}

	public Ray GetEdge(int i) {
		Ray ray = GetEdgeLocal(i);
		Transform _transform = transform;
		ray.origin = _transform.rotation * ray.origin + _transform.position;
		ray.direction = _transform.rotation * ray.direction;
		return ray;
	}

	public void FindNeighbors() {
		List<GeneratedTile> candidates = new List<GeneratedTile>();
		for (int neighborIndex = 0; neighborIndex < _neighbors.Length; neighborIndex++) {
			candidates.Clear();
			if (GetNeighborCandidates(neighborIndex, candidates)) {
				if (candidates.Count > 1) {
					throw new System.Exception($"multiple {_boundaries.edges[neighborIndex].name} neighbors for {name}!\n" +
						$"{string.Join(", ",candidates)}");
				}
				GeneratedTile otherTile = candidates[0];
				_neighbors[neighborIndex] = otherTile;
				otherTile.SetNeighbor(this, GetEdge(neighborIndex));
			}
		}
	}

	public bool GetNeighborCandidates(int neighborIndex, List<GeneratedTile> out_tiles) {
		Ray edge = GetEdge(neighborIndex);
		bool gotOne = _board.GetTileAt(edge.origin, out_tiles);
		int selfIndex = out_tiles.IndexOf(this);
		if (selfIndex >= 0) {
			out_tiles.RemoveAt(selfIndex);
			gotOne = out_tiles.Count > 0;
		}
		return gotOne;
	}

	public void SetNeighbor(GeneratedTile neighbor, Ray hisEdge) {
		int neighborIndex = System.Array.IndexOf(_neighbors, neighbor);
		float dist;
		if (neighborIndex != -1) {
			Ray myEdge = GetEdge(neighborIndex);
			dist = Vector3.Distance(hisEdge.origin, myEdge.origin);
			if (dist > _boundaries.edges[neighborIndex].Size) {
				throw new System.Exception($"portal {neighborIndex} dos not seem to connect with {hisEdge}...");
			}
			return;
		}
		neighborIndex = GetEdgeClosestTo(hisEdge, out dist);
		if (neighborIndex == -1) {
			//throw new System.Exception
			Debug.LogError("cannot set neighbor?");
		}
		if (_neighbors[neighborIndex] == neighbor) {
			Debug.LogWarning($"already have neighbor at edge {neighborIndex} {_boundaries.edges[neighborIndex].name}");
			return;
		}
		_neighbors[neighborIndex] = neighbor;
	}

	private int GetEdgeClosestTo(Ray edge, out float distance) {
		int closest = -1;
		distance = 0;
		for (int i = 0; i < _neighbors.Length; i++) {
			Ray e = GetEdge(i);
			float dist = Vector3.Distance(e.origin, edge.origin);
			if (closest < 0 || dist < distance) {
				distance = dist;
				closest = i;
			}
		}
		return closest;
	}

	public bool GetNeighborIndexTo(GeneratedTile possibleNeighbor, List<int> neighborIndexes) {
		bool gotOne = false;
		for(int i = 0; i < _neighbors.Length; ++i) {
			if (_neighbors[i] == possibleNeighbor) {
				neighborIndexes.Add(i);
				gotOne = true;
			}
		}
		return gotOne;
	}

	void Update() {

	}

	//private void OnTriggerEnter(Collider other) {
	//	GeneratorCursor cursor = other.GetComponent<GeneratorCursor>();
	//	if (cursor != null && cursor.IsObserving(_board)) {
	//		_board.TriggerObserver(this);
	//	}
	//}

	//private void OnTriggerExit(Collider other) {
	//	Debug.Log($"exit! {name}");
	//	GeneratorCursor cursor = other.GetComponent<GeneratorCursor>();
	//	if (cursor != null && cursor.IsObserving(_board)) {
	//		_board.UntriggerObserver(this);
	//	}
	//}
}
