// TODO UnityAgnostic
#if UNITY_5_3_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StateToggle : MonoBehaviour {
	public List<State> States = new List<State>();
	[ContextMenuItem(nameof(Next), nameof(Next))]
	[SerializeField] private int _index = 0;
	public ChangeEvents changeEvents;

	public int Index {
		get => _index;
		set => _index = value;
	}

	public int IndexWithNotify {
		get => _index;
		set {
			if (_index == value) { return; }
			_index = value;
			Refresh();
		}
	}

	[System.Serializable]
	public class ChangeEvents {
		public SetIndexChange setIndexChange;
		public SetTextChange setNameChange;
	}

	[System.Serializable] public class SetTextChange : UnityEvent<string> { }

	[System.Serializable] public class SetIndexChange : UnityEvent<int> { }

	[System.Serializable]
	public class State {
		bool active;
		public string name;
		public List<GameObject> objects;
		public UnityEvent OnActivate;
		public UnityEvent OnDeactivate;
		public void SetActive(bool active) {
			bool activate = active && !this.active;
			bool deactivate = !active && this.active;
			this.active = active;
			if (activate) { OnActivate.Invoke(); }
			if (deactivate) { OnDeactivate.Invoke(); }
			objects.ForEach(o => o.SetActive(active));
		}
	}

	private void Start() {
		Refresh();
	}

	public void Refresh() {
		for (int i = 0; i < States.Count; i++) {
			States[i].SetActive(i == _index);
		}
		changeEvents.setIndexChange.Invoke(_index);
		if (_index < 0 || _index >= States.Count) {
			changeEvents.setNameChange.Invoke("Invalid " + _index);
		} else {
			changeEvents.setNameChange.Invoke(States[_index].name);
		}
	}

	public void Next() {
		if (++_index >= States.Count) { _index = 0; }
		Refresh();
	}

	public void Prev() {
		if (--_index < 0) { _index = States.Count - 1; }
		Refresh();
	}
}
#endif
