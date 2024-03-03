using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eye : MonoBehaviour {
	[ContextMenuItem(nameof(CalculateDirection),nameof(CalculateDirection))]
	public Transform _pupil;
	public Transform _focus;
	//public Vector3 _pupilOffset;
	//public Vector3 _currentPupilDirection;

	public float PupilDialation {
		get => _pupil.localScale.x;
		set => _pupil.localScale = Vector3.one * value;
	}

	public void CalculateDirection() {
		//_currentPupilDirection = _pupil.rotation * Vector3.forward;
		//_pupilOffset = _pupil.localPosition;
	}

	void Start() {
		CalculateDirection();
	}

	void Update() {

	}

	public void SetFocus(Vector3 position) {
		transform.LookAt(position);
	}

	public bool LookToward(Vector3 position, float amount) {
		Vector3 targetDirection = (position - transform.position).normalized;
		Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, amount);
		return (Quaternion.Angle(transform.rotation, targetRotation) < Mathf.Epsilon);
	}
}
