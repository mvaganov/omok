using System.Collections.Generic;

public static partial class Util
{
	public static bool BinarySearchEquals(float a, float b) => a == b;
	public static bool BinarySearchLessThan(float a, float b) => a < b;
	public static int BinarySearch(IList<float> arr, float x) {
		return BinarySearch(arr, x, BinarySearchEquals, BinarySearchLessThan);
	}
	public static int BinarySearch<T1,T2>(IList<T1> sortedList, T2 value,
	System.Func<T1, T2, bool> equals, System.Func<T1, T2, bool> lessThan,
	int startInclusive = 0, int endInclusive = -1) {
		if (endInclusive < 0) {
			endInclusive = sortedList.Count - 1;
		}
		while (startInclusive <= endInclusive) {
			int m = startInclusive + (endInclusive - startInclusive) / 2;
			if (equals(sortedList[m], value)) { return m; }
			if (lessThan(sortedList[m], value)) { startInclusive = m + 1; } else { endInclusive = m - 1; }
		}
		return ~startInclusive;
	}
}
