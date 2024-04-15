using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemHelper : MonoBehaviour
{
	public EventSystem eventSystem;

	public GameObject GetSelectedGameObject() => eventSystem.currentSelectedGameObject;
	public void SetSelectedGameObject(GameObject obj) => eventSystem.SetSelectedGameObject(obj);

	public void SetSelectedGameObjectNextFrame(GameObject obj) {
		StartCoroutine(DoItNextFrame());
		IEnumerator DoItNextFrame() {
			yield return null;
			SetSelectedGameObject(obj);
		}
	}
	public void ClearSelection() {
		eventSystem.SetSelectedGameObject(null);
	}
}
