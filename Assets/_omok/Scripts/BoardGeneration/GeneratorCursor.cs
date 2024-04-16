using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorCursor : MonoBehaviour
{
	public GeneratingBoard[] boards;
	public BoxCollider box;
	public bool IsObserving(GeneratingBoard board) {
		return System.Array.IndexOf(boards, board) != -1;
	}


	private void OnTriggerEnter(Collider other) {
		GeneratedTile tile = other.GetComponent<GeneratedTile>();
		if (tile != null && IsObserving(tile._board)) {
			tile._board.TriggerObserver(tile);
		}
	}

	private void OnTriggerExit(Collider other) {
		//Debug.Log($"exit! {name}");
		GeneratedTile tile = other.GetComponent<GeneratedTile>();
		if (tile != null && IsObserving(tile._board)) {
			Transform self = transform, tileTransform = tile.transform;
			Collider otherCollider = other.GetComponent<Collider>();
			bool isActuallyInHere = Physics.ComputePenetration(box, self.position, self.rotation,
				otherCollider, tileTransform.position, tileTransform.rotation,
				out Vector3 collisionDirection, out float collisionOverlap);
			if (!isActuallyInHere) {
				tile._board.UntriggerObserver(tile);
			}
		}
	}
}
