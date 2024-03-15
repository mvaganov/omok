using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omok {
	public class OmokGoogleDirector : MonoBehaviour, IBelongsToOmokGame {
		[System.Serializable]
		public class LookTarget {
			public Vector3 position;
			public float weight;
			public float distance;
			public LookTarget(Vector3 position, float weight) {
				this.position = position;
				this.weight = weight;
				distance = 0;
			}
		}
		[ContextMenuItem(nameof(TestBSearch), nameof(TestBSearch))]
		public bool a;
		public List<LookTarget> targets = new List<LookTarget>();
		private IBelongsToOmokGame gameReference;

		public OmokGame omokGame => gameReference != null ? gameReference.omokGame
			: (gameReference = GetComponent<IBelongsToOmokGame>()).omokGame;

		void Start() {
			gameReference = GetComponent<IBelongsToOmokGame>();
		}

		void Update() {

		}

		public void AddLookTarget(Coord coord) {
			//omokGame.Ana
			// Get the canonical current state
		}

		public void AddLookTarget(Coord position, float weight) {
			int properIndex = Util.BinarySearch(targets, weight, (t, v) => t.weight == v, (t, v) => t.weight < v);
			if (properIndex < 0) {
				properIndex = ~properIndex;
			}
			targets.Insert(properIndex, new LookTarget(position, weight));
			CalculateTotalTargetWeight();
		}

		public float CalculateTotalTargetWeight() {
			float v = 0;
			for (int i = 0; i < targets.Count; i++) {
				targets[i].distance = v;
				v += targets[i].weight;
			}
			return v;
		}

		public int GetTargetWeighted(float value) {
			int index = Util.BinarySearch(targets, value, (t, v) => t.distance == v, (t, v) => t.distance < v);
			if (index < 0) {
				index = ~index;
			}
			return index;
		}

		public void TestBSearch() {
			float[] nums = { 1, 3, 5, 9 };
			Debug.Log(string.Join(", ", nums));
			float[] tests = { 2, 3, 4, 5, 9, 10, 0 };
			for (int i = 0; i < tests.Length; ++i) {
				int index = Util.BinarySearch(nums, tests[i]);
				bool missing = index < 0;
				int value = missing ? ~index : index;
				Debug.Log($"{tests[i]} : {value} {missing}");
			}
		}
	}
}
