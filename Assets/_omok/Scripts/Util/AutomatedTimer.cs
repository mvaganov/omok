using UnityEngine;
using UnityEngine.Events;

public class AutomatedTimer : MonoBehaviour {
	[System.Serializable] public class UnityEvent_float : UnityEvent<float> { }
	public enum TimerCallbackBehavior {
		// TODO add a graph line thingy
		CountUpNormalized, CountDownNormalized
	}
	[SerializeField] protected float _duration = 3;
	[SerializeField] protected int _repetitions = 1;
	[SerializeField] protected bool _timerRunning = true;
	[SerializeField] protected TimerCallbackBehavior _timerCallbackBehavior;
	[SerializeField] protected UnityEvent_float _timerCallback;
	[SerializeField] protected UnityEvent _onStartTimer;
	[SerializeField] protected UnityEvent _onFinishTimer;
	protected bool _timerStarted;
	protected float _timer;
	protected int _repCount;

	public void Restart() {
		_timerRunning = true;
		_timerStarted = false;
		_repCount = 0;
		switch (_timerCallbackBehavior) {
			case TimerCallbackBehavior.CountUpNormalized:
				_timer = 0;
				break;
			case TimerCallbackBehavior.CountDownNormalized:
				_timer = _duration;
				break;
		}

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
			_onStartTimer.Invoke();
		}
		switch (_timerCallbackBehavior) {
			case TimerCallbackBehavior.CountUpNormalized:
				AdvanceTimer();
				break;
			case TimerCallbackBehavior.CountDownNormalized:
				CountDownTimer();
				break;
		}
	}

	private void AdvanceTimer() {
		_timer += Time.deltaTime;
		UpdateTiming(1);
		while (_timer > _duration) {
			_timer -= _duration;
			UpdateTiming(1);
		}
	}

	private void CountDownTimer() {
		_timer -= Time.deltaTime;
		UpdateTiming(0);
		while (_timer < 0) {
			_timer += _duration;
			UpdateTiming(0);
		}
	}

	private void UpdateTiming(float target) {
		float progress = Mathf.Clamp01(_timer / _duration);
		_timerCallback.Invoke(progress);
		if (progress == target) {
			++_repCount;
			if (_repCount >= _repetitions) {
				_timerRunning = false;
				_onFinishTimer.Invoke();
			}
		}
	}
}
