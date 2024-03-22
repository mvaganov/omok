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
		public OmokGoogle google;
		private float maxWeight;
		public float gazeTimer = 1;
		public float timerWiggleRoom = .25f;
		private float timer;
		public List<LookTarget> targets = new List<LookTarget>();
		private IBelongsToOmokGame gameReference;
		public IBelongsToOmokGame reference => gameReference;
		public float minChanceToLookAtBest = 0.75f;

		public OmokGame omokGame => gameReference != null ? gameReference.omokGame
			: (gameReference = this.GetOmokGamePeer()).omokGame;

		public OmokGoogle Google => google != null ? google : google = GetComponent<OmokGoogle>();

		void Start() {
			gameReference = this.GetOmokGamePeer();
		}

		void Update() {
			timer += Time.deltaTime;
			if (timer >= gazeTimer) {
				if (targets.Count > 0) {
					int targetIndex = Random.value <= minChanceToLookAtBest ? 0 : GetTargetWeightedIndex(maxWeight * Random.value);
					if (targetIndex < 0 || targetIndex >= targets.Count) {
						//Debug.Log("wrong index " + targetIndex + " ...  max " + targets.Count);
						targetIndex = System.Math.Clamp(targetIndex, 0, targets.Count -1);
					}
					LookTarget target = targets[targetIndex];
					Google.GazeTarget = omokGame.Board.LookOffsetPosition + target.position;
				}
				timer = Random.value * timerWiggleRoom;
				timer -= Random.value * timerWiggleRoom;
			}
		}

		public void AddLookTarget(OmokMove move) {
			OmokGame game = omokGame;
			//OmokState gState = game.State;
			//OmokState bState = game.Board.State;
			UnitState unitState = game.State.GetState(move.coord);
			if (unitState != UnitState.None) {
				Debug.LogWarning($"non-empty look target given: {move.coord}");
				return;
			}
			game.graphBehaviour.graph.GetMoveScoringSummary(move, out float netScore);
			Vector3 position = game.Board.GetPosition(move.coord);
			AddLookTarget(position, netScore);
		}

		public void AddLookTarget(Vector3 position, float weight) {
			int properIndex = Util.BinarySearch(targets, weight, IsTargetWeight, IsTargetEarlierThanWeight);
			if (properIndex < 0) {
				properIndex = ~properIndex;
			}
			targets.Insert(properIndex, new LookTarget(position, weight));
			CalculateTotalTargetWeight();
		}

		private static bool IsTargetWeight(LookTarget t, float v) => t.weight == v;
		private static bool IsTargetEarlierThanWeight(LookTarget t, float v) => t.weight > v;

		public float CalculateTotalTargetWeight() {
			maxWeight = 0;
			for (int i = 0; i < targets.Count; i++) {
				targets[i].distance = maxWeight;
				maxWeight += 1f / (i + 1);//targets[i].weight;
			}
			return maxWeight;
		}

		public int GetTargetWeightedIndex(float value) {
			int index = Util.BinarySearch(targets, value, (t, v) => t.distance == v, (t, v) => t.distance < v);
			if (index < 0) {
				index = ~index;
			}
			return index;
		}

		public void ClearTargets() {
			targets.Clear();
			CalculateTotalTargetWeight();
		}
	}
}
