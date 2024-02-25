using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Coord
{
	public static Coord zero = new Coord();
	public static Coord one = new Coord(1,1);
	public static Coord MIN = new Coord(int.MinValue, int.MinValue);
	public static Coord MAX = new Coord(int.MaxValue, int.MaxValue);
	public int x, y;
	public Coord(int x, int y) { this.x = x; this.y = y; }
	public bool Equals(Coord other) => x == other.x && y == other.y;
	public override bool Equals(object obj) => (obj is Coord c) ? Equals(c): false;
	public override int GetHashCode() => x ^ y;
	public static Coord operator-(Coord a, Coord b) => new Coord(a.x - b.x, a.y - b.y);
	public static Coord operator+(Coord a, Coord b) => new Coord(a.x + b.x, a.y + b.y);
	public static bool operator ==(Coord a, Coord b) => a.Equals(b);
	public static bool operator !=(Coord a, Coord b) => !a.Equals(b);

	public static void CalculateCoordRange<T>(IEnumerable<T> list, System.Func<T, Coord> getCoord,
		out Coord _min, out Coord _max) {
		Coord min = Coord.MAX, max = Coord.MIN;
		int count = 0;
		foreach(T item in list) {
			Coord coord = getCoord(item);
			min.x = Mathf.Min(min.x, coord.x);
			min.y = Mathf.Min(min.y, coord.y);
			max.x = Mathf.Max(max.x, coord.x);
			max.y = Mathf.Max(max.y, coord.y);
			++count;
		}
		if (count == 0) {
			_min = _max = zero;
		} else {
			_min = min;
			_max = max;
		}
	}
}
