using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	public class EyeCatcher : MonoBehaviour {
		public OmokBoard board;
		public float eyeDistance = 2.5f;

		private void OnTriggerStay(Collider other) {
			OmokGoogle google = other.GetComponent<OmokGoogle>();
			if (google != null) {
				google.GazeTarget = board.MouseLookOffsetPosition; ;
			}
		}
	}
}
