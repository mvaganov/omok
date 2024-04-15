using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorCursor : MonoBehaviour
{
	public GeneratingBoard[] boards;
	public bool IsObserving(GeneratingBoard board) {
		return System.Array.IndexOf(boards, board) != -1;
	}
}
