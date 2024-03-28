// TODO UnityAgnostic
#if UNITY_5_3_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class MemoryPool<T> where T : Component {
	public Transform unallocatedLocation;
	public Component prefab;
	public List<T> unallocated = new List<T>();
	private List<T> activeAllocated = null;
	public Action<T> onReclaim;
	public Action<T> onAllocate;
	public Action<T> onInitialize;

	public MemoryPool() {
	}
	public void SetData(Transform unallocatedLocation, T prefab, bool keepTrackOfActiveAllocated) {
		this.unallocatedLocation = unallocatedLocation;
		this.prefab = prefab;
		if (keepTrackOfActiveAllocated) { activeAllocated = new List<T>(); }
	}
	public T Get() {
		T thing;
		if (unallocated.Count > 0) {
			thing = unallocated[unallocated.Count - 1];
			unallocated.RemoveAt(unallocated.Count - 1);
			thing.gameObject.SetActive(true);
		} else {
			thing = Mem.CreateObjectPossiblyPrefab(prefab.gameObject).GetComponent<T>();
			if (unallocatedLocation != null) {
				thing.transform.SetParent(null, false);
			}
			thing.gameObject.SetActive(true);
			onAllocate?.Invoke(thing);
		}
		if (activeAllocated != null) { activeAllocated.Add(thing); }
		onInitialize?.Invoke(thing);
		return thing;
	}
	public void Reclaim(T thing) {
		onReclaim?.Invoke(thing);
		thing.gameObject.SetActive(false);
		if (unallocatedLocation != null) {
			thing.transform.SetParent(unallocatedLocation, false);
		}
		unallocated.Add(thing);
		if (activeAllocated != null) { activeAllocated.Remove(thing); }
	}
	public void ReclaimAll(Func<T, bool> predicate = null) {
		if (activeAllocated == null) { throw new Exception("must be created with 'keepTrackOfAllocated' set to true"); }
		List<T> toReclaim = new List<T>(activeAllocated);
		for(int i = toReclaim.Count-1; i >= 0; --i) {
			if (predicate == null || predicate.Invoke(toReclaim[i])) {
				Reclaim(toReclaim[i]);
			}
		}
	}
}
#endif
