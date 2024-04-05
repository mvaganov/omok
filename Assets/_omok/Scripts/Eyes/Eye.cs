using UnityEngine;

public class Eye : MonoBehaviour {
	[ContextMenuItem(nameof(CalculateDirection),nameof(CalculateDirection))]
	public Transform _pupil;
	public Transform _focus;

	public float PupilDialation {
		get => _pupil.localScale.x;
		set => _pupil.localScale = Vector3.one * value;
	}

	public void CalculateDirection() {
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
