
namespace Omok {
	public struct MinMax {
		public float min, max;
		public MinMax(float min, float max) { this.min = min; this.max = max; }
		public void Update(float value) {
			if (value < min) { min = value; }
			if (value > max) { max = value; }
		}
		public float Delta => max - min;
		public static MinMax Impossible = new MinMax(float.MaxValue, float.MinValue);
		override public string ToString() => $"({min}<={max})";
	}
}
