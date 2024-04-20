using System;
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
	public List<GeneratedTile> _needNeighborCalculation = new List<GeneratedTile>();

	public List<Vector3> tileToCreate = new List<Vector3>();

	public void EnqueueTileCreation(Vector3 position) {

		GeneratedTile foundTile = GetTileAt(position);
		if (foundTile != null) {
			//Debug.Log($"skipping another {position}");
			return;
		}
		if (tileToCreate.IndexOf(position) >= 0) {
			Debug.LogWarning("ignoring duplicate");
			return;
		}
		GeneratedTile tileHere = GetTileAt(position);
		if (tileHere != null) {
			Debug.Log($"already a tile at {position}: {tileHere}");
		}
		//Debug.Log($"Create plz {position}");
		tileToCreate.Add(position);
	}

	public void CreateQueuedTiles() {
		if (tileToCreate.Count == 0) {
			return;
		}
		List<Vector3> queueToExecute = new List<Vector3> ();
		queueToExecute.Add(tileToCreate[0]);
		tileToCreate.RemoveAt(0);

		List<GeneratedTile> createdTiles = new List<GeneratedTile>();
		for (int i = 0; i < queueToExecute.Count; ++i) {
			Vector3 position = queueToExecute[i];
			GeneratedTile newTile = CreateTile(position);
			if (newTile != null) {
				createdTiles.Add(newTile);
			}
		}
		//Debug.Log($"setting neighbors for {createdTiles.Count} tiles");
		for(int i = 0; i < createdTiles.Count; ++i) {
			GeneratedTile newTile = createdTiles[i];
			newTile.FindNeighbors();
		}
	}

	void Start()
	{
		_tilePool.SetData(transform, tile10x10, false);
		EnqueueTileCreation(transform.position);
	}

	void Update()
	{
		CreateQueuedTiles();
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
		//List<GeneratedTile> newTiles = new List<GeneratedTile> ();
		tile.GetNeighborIndexTo(null, missingNeighbors);
		tile.IsMapEdge = false;
		//Debug.Log($"{tile} missing neighbors: {missingNeighbors.Count}");
		for (int i = 0; i < missingNeighbors.Count; i++) {
			int edgeIndex = missingNeighbors[i];
			Ray edgeToFill = tile.GetEdge(edgeIndex);
			Vector3 offsetOfnextTile = edgeToFill.direction;
			offsetOfnextTile.Scale(transform.rotation * TileSize / 2);
			Vector3 nextTileCenter = edgeToFill.origin + offsetOfnextTile;

			EnqueueTileCreation(nextTileCenter);
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

	public GeneratedTile GetTileAt(Vector3 position) => GetTileAt(position, null);

	public GeneratedTile GetTileAt(Vector3 position, List<GeneratedTile> out_tiles) {
		Collider[] colliders = Physics.OverlapSphere(position, 0.5f);
		GeneratedTile gotOne = null;
		for (int c = 0; c < colliders.Length; c++) {
			GeneratedTile tile = colliders[c].GetComponent<GeneratedTile>();
			if (tile == null || tile == this) { // || tile._board != this) {
				continue;
			}
			if (out_tiles != null) {
				out_tiles.Add(tile);
			}
			gotOne = tile;
		}
		return gotOne;
	}

	public GeneratedTile CreateTile(Vector3 nextTileCenter) {
		// needed because a nextTileCenter can be enqueued in *almost* the same spot
		if (GetTileAt(nextTileCenter) != null) {
			//Debug.Log("DUPLICATE! ");
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
		newTile.Initialize(this);
		newTile.IsMapEdge = true;
		return newTile;
	}
}
