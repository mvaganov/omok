using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour {
	public float speed = 5;
	public Vector3 moveX = Vector3.right, moveY = Vector3.forward;
	public Rigidbody _rigidbody;

	void Start() {

	}

	void Update() {

	}

	public void OnLocalPositionChange(Vector2 input) {
		_rigidbody.velocity = (input.x * moveX + input.y * moveY) * speed;
	}
}
