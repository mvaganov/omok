using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLight : MonoBehaviour {
	public Transform pointLight;
	public Transform directionalLight;
	public OmokBoard board;

	void Update() {
		Vector3 direction = (board.MousePosition - directionalLight.position).normalized;
		directionalLight.rotation = Quaternion.LookRotation(direction);
		pointLight.position = board.MouseLookOffsetPosition;
	}
}
