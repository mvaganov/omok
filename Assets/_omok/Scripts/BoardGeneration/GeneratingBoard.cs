using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratingBoard : MonoBehaviour
{
	public GeneratedTile tile10x10;
	public MemoryPool<GeneratedTile> _tilePool = new MemoryPool<GeneratedTile>();
	public BoxCollider cursor;
	public Color NormalTileColor = Color.white;
	public Color EdgeTileColor = Color.red;
	public Vector3 TileSize = new Vector3(10, 10, 0);

	void Start()
	{
		_tilePool.SetData(transform, tile10x10, false);
		//_tilePool.onReclaim += e => e.gameObject.SetActive(false);
		//_tilePool.onInitialize += e => e.gameObject.SetActive(true);
		GeneratedTile newTile = CreateBoard(transform.position);
		newTile.transform.localRotation = Quaternion.identity;
		newTile.InitializeNeighbors(this);
		TriggerObserver(newTile);
	}

	void Update()
	{
	}

	public bool IsEdge(GeneratedTile tile) {
		return tile.IsMapEdge;
	}

	public void TriggerObserver(GeneratedTile tile) {
		if (!IsEdge(tile)) {
			//Debug.Log("not edge?");
			return;
		}
		List<int> missingNeighbors = new List<int>();
		List<GeneratedTile> newTiles = new List<GeneratedTile> ();
		tile.GetNeighborIndexTo(null, missingNeighbors);
		tile.IsMapEdge = false;
		//Debug.Log($"{tile} missing neighbors: {missingNeighbors.Count}");
		for (int i = 0; i < missingNeighbors.Count; i++) {
			int edgeIndex = missingNeighbors[i];
			Ray edgeToFill = tile.GetEdge(edgeIndex);
			Vector3 offsetOfnextTile = edgeToFill.direction;
			offsetOfnextTile.Scale(transform.rotation * TileSize / 2);
			Vector3 nextTileCenter = edgeToFill.origin + offsetOfnextTile;
			//GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			//cube.transform.position = nextTileCenter;
			GeneratedTile newTile = CreateBoard(nextTileCenter);
			if (newTile == null) {
				Debug.Log($"tried to make tile at {tile.name}.{tile._boundaries.edges[edgeIndex]}, but its there already?");
			}
			newTiles.Add(newTile);
		}
	}

	public void UntriggerObserver(GeneratedTile tile) {
		tile.IsMapEdge = true;
		HashSet<GeneratedTile> deepEdges = new HashSet<GeneratedTile>();
		tile.FindDeepMapEdges(deepEdges);
		foreach (GeneratedTile oldTile in deepEdges) {
			DisconnectTileNeighbors(oldTile);
			_tilePool.Reclaim(oldTile);
		}
	}

	public void DisconnectTileNeighbors(GeneratedTile tile) {
		List<int> indexes = new List<int>();
		for (int i = 0; i < tile._neighbors.Length; ++i) {
			GeneratedTile neighbor = tile._neighbors[i];
			if (neighbor == null) {
				continue;
			}
			neighbor.GetNeighborIndexTo(tile, indexes);
			for (int n = 0; n < indexes.Count; ++n) {
				neighbor._neighbors[indexes[n]] = null;
			}
			tile._neighbors[i] = null;
		}
	}

	public bool GetTileAt(Vector3 position, List<GeneratedTile> out_tiles) {
		Collider[] colliders = Physics.OverlapSphere(position, 0.5f);
		bool gotOne = false;
		for (int c = 0; c < colliders.Length; c++) {
			GeneratedTile tile = colliders[c].GetComponent<GeneratedTile>();
			if (tile == null || tile == this || tile._board != this) {
				continue;
			}
			if (out_tiles != null) {
				out_tiles.Add(tile);
			}
			gotOne = true;
		}
		if (out_tiles != null && out_tiles.Count != 0) {
			GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.transform.position = position;
			sphere.name = string.Join(",", out_tiles);
		}
		return gotOne;
	}

	public GeneratedTile CreateBoard(Vector3 nextTileCenter) {
		if (GetTileAt(nextTileCenter, null)) {
			Debug.Log("DUPLICATE! ");
			return null;
		}
		Transform self = transform;
		//GeneratedTile newTile = Instantiate(tile10x10.gameObject, nextTileCenter, self.rotation).GetComponent<GeneratedTile>();
		GeneratedTile newTile = _tilePool.Get();
		Transform tileTransform = newTile.transform;
		tileTransform.position = nextTileCenter;
		tileTransform.rotation = self.rotation;

		newTile.name = "tile "+(int)nextTileCenter.x/TileSize.x+" "+(int)nextTileCenter.z / TileSize.y;
		newTile.transform.SetParent(self, true);
		newTile.InitializeNeighbors(this);
		newTile.IsMapEdge = true;
		return newTile;
	}
}
