using UnityEngine;
using UnityEngine.Events;

public class ValueOverTime : MonoBehaviour {
	[System.Serializable] public class UnityEvent_float : UnityEvent<float> { }
	[SerializeField] protected float _duration = 3;
	[SerializeField] protected int _repetitions = 1;
	[SerializeField] protected bool _timerRunning = true;
	[SerializeField] protected AnimationCurve _valueOverTime = new AnimationCurve(
		new Keyframe[] { new Keyframe(0,0,1,1), new Keyframe(1,1,1,1), });
	[System.Serializable] public class TimerEvents {
		public UnityEvent_float TimerCallback = new UnityEvent_float();
		public UnityEvent OnStartTimer = new UnityEvent();
		public UnityEvent OnFinishTimer = new UnityEvent();
	}
	[SerializeField] protected TimerEvents _timerEvents = new TimerEvents();
	protected bool _timerStarted;
	protected float _timer;
	protected int _repCount;

	public void Restart() {
		_timerRunning = true;
		_timerStarted = false;
		_repCount = 0;
		_timer = 0;
	}
	private void Start() {
		if (_timerRunning) {
			Restart();
		}
	}

	void Update() {
		if (!_timerRunning) {
			return;
		}
		if (!_timerStarted) {
			_timerStarted = true;
			_timerEvents.OnStartTimer.Invoke();
		}
		AdvanceTimer();
	}

	private void AdvanceTimer() {
		_timer += Time.deltaTime;
		UpdateTiming();
		while (_timer > _duration) {
			_timer -= _duration;
			UpdateTiming();
		}
	}

	private void UpdateTiming() {
		float progress = Mathf.Clamp01(_timer / _duration);
		_timerEvents.TimerCallback.Invoke(_valueOverTime.Evaluate(progress));
		if (progress >= 1) {
			++_repCount;
			if (_repCount >= _repetitions) {
				Restart();
				_timerRunning = false;
				_timerEvents.OnFinishTimer.Invoke();
			}
		}
	}
}
