using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Coord
{
	public static Coord zero = new Coord();
	public static Coord one = new Coord(1,1);
	public static Coord MIN = new Coord(short.MinValue, short.MinValue);
	public static Coord MAX = new Coord(short.MaxValue, short.MaxValue);
	private short _x, _y;
	public int x { get => _x; set => _x = (short)value; }
	public int y { get => _y; set => _y = (short)value; }
	public Coord(int x, int y) { _x = (short)x; _y = (short)y; }
	public override string ToString() => $"({x},{y})";
	public override int GetHashCode() => ((_x & 0xffff) | (_y << 16));
	public override bool Equals(object obj) => (obj is Coord c) ? Equals(c) : false;
	public bool Equals(Coord other) => x == other.x && y == other.y;
	public static bool operator ==(Coord a, Coord b) => a.Equals(b);
	public static bool operator !=(Coord a, Coord b) => !a.Equals(b);
	public static Coord operator-(Coord a, Coord b) => new Coord(a._x - b._x, a._y - b._y);
	public static Coord operator+(Coord a, Coord b) => new Coord(a._x + b._x, a._y + b._y);
	public static Coord operator -(Coord coord) => new Coord(-coord._x, -coord._y);
	public static Coord operator *(Coord coord, float scalar) => new Coord((short)(coord._x*scalar), (short)(coord._y*scalar));
	public static void CalculateCoordRange<T>(IEnumerable<T> list, System.Func<T, Coord> getCoord,
		out Coord _min, out Coord _max) {
		Coord min = Coord.MAX, max = Coord.MIN;
		short count = 0;
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
