using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmokGoogle : MonoBehaviour {
	public enum EyeBehavior {
		Idle, Gazing, Waiting, Blinking, Shut
	}

	public Eye[] eyes;
	public EyeBehavior behavior;
	public float eyeSpeed = 180;
	[SerializeField]
	protected Vector3 gazeTarget;

	public Vector3 GazeTarget {
		get => gazeTarget;
		set {
			gazeTarget = value;
			behavior = EyeBehavior.Gazing;
		}
	}
	public void SetFocus(Vector3 position) {
		System.Array.ForEach(eyes, eye => eye.SetFocus(position));
	}

	private void Update() {
		switch (behavior) {
			case EyeBehavior.Idle: break;
			case EyeBehavior.Gazing:
				if (!LookToward(gazeTarget, eyeSpeed * Time.deltaTime)) {
					behavior = EyeBehavior.Idle;
				}
				break;
		}
	}

	public bool LookToward(Vector3 position, float amount) {
		bool stillLooking = false;
		for(int i = 0; i < eyes.Length; ++i) {
			Eye eye = eyes[i];
			if (!eye.LookToward(position, amount)) {
				stillLooking = true;
			}
		}
		return stillLooking;
	}
}
