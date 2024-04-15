using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour, IPlane {
	public Vector3 Origin => transform.position;

	public Vector3 Normal => transform.forward;

	public float Size => transform.localScale.x / 2;

	public void Start() {
		
	}

	public void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawRay(Origin, Normal);
		UnityEditor.Handles.color = Color.red;
		UnityEditor.Handles.DrawWireDisc(Origin, Normal, Size);
	}
}
