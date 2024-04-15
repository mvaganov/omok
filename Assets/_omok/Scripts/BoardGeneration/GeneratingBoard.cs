using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratingBoard : MonoBehaviour
{
	public GeneratedTile tile10x10;
	MemoryPool<GeneratedTile> tiles = new MemoryPool<GeneratedTile>();
	public BoxCollider cursor;
	public List<GeneratedTile> edges = new List<GeneratedTile>();
	public Color _normalTileColor = Color.white;
	public Color _edgeTileColor = Color.red;

	void Start()
	{
		tiles.SetData(transform, tile10x10, false);
		GeneratedTile newTile = CreateBoard(transform.position);
		newTile.transform.localRotation = Quaternion.identity;
		newTile.InitializeNeighbors(this);
		TriggerObserver(newTile);
	}

	void Update()
	{
	}

	public bool IsEdge(GeneratedTile tile) {
		return edges.IndexOf(tile) != -1;
	}

	public void TriggerObserver(GeneratedTile tile) {
		if (!IsEdge(tile)) {
			//Debug.Log("not edge?");
			return;
		}
		List<int> missingNeighbors = new List<int>();
		List<GeneratedTile> newTiles = new List<GeneratedTile> ();
		tile.GetNeighborIndexTo(null, missingNeighbors);
		edges.Remove(tile);
		tile.GetComponent<Renderer>().material.color = _normalTileColor;
		//Debug.Log($"{tile} missing neighbors: {missingNeighbors.Count}");
		for (int i = 0; i < missingNeighbors.Count; i++) {
			Ray edgeToFill = tile.GetEdge(missingNeighbors[i]);
			Vector3 nextTileCenter = edgeToFill.origin + edgeToFill.direction * 5;
			//GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			//cube.transform.position = nextTileCenter;
			GeneratedTile newTile = CreateBoard(nextTileCenter);
			if (newTile == null) {
				Debug.Log("woah buddy... {}");
			}
			newTiles.Add(newTile);
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
		return gotOne;
	}

	public GeneratedTile CreateBoard(Vector3 nextTileCenter) {
		if (GetTileAt(nextTileCenter, null)) {
			Debug.Log("DUPLICATE! ");
			return null;
		}
		Transform self = transform;
		GeneratedTile newTile = Instantiate(tile10x10.gameObject, nextTileCenter, self.rotation).GetComponent<GeneratedTile>();
		newTile.name = "tile "+nextTileCenter;
		newTile.transform.SetParent(self, true);
		newTile.GetComponent<Renderer>().material.color = _edgeTileColor;
		edges.Add(newTile);
		newTile.InitializeNeighbors(this);
		return newTile;
	}
}
