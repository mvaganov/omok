// TODO UnityAgnostic
#if UNITY_5_3_OR_NEWER
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Mem {
	public static GameObject CreateObjectPossiblyPrefab(GameObject go) {
		if (go == null) { return null; }
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(go);
			if (prefab != null) { return prefab; }
		}
#endif
		return UnityEngine.Object.Instantiate(go);
	}

	public static void DestroyObject(Object go) {
		if (go == null) { return; }
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			UnityEngine.Object.DestroyImmediate(go);
			return;
		}
#endif
		UnityEngine.Object.Destroy(go);
	}

	public static void DestroyListOfThingsBackwards<T>(List<T> things) where T : MonoBehaviour {
		for (int i = things.Count - 1; i >= 0; --i) {
			T thing = things[i];
			if (thing != null) { DestroyObject(thing.gameObject); }
			//things.RemoveAt(i);
		}
		things.Clear();
	}
}
#endif
