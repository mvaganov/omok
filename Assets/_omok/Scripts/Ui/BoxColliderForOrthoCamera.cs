using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class BoxColliderForOrthoCamera : MonoBehaviour
{
	public Camera _camera;
	public BoxCollider _box;
	private void Update() {
		if (_camera == null || _box == null) {
			return;
		}
		float height = _camera.orthographicSize * 2;
		float width = height * _camera.aspect;
		float depth = _camera.farClipPlane - _camera.nearClipPlane;
		_box.size = new Vector3(width, height, depth);
		_box.center = new Vector3(0, 0, _camera.nearClipPlane + depth / 2);
	}
}
