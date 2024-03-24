using UnityEngine;

public class SetAlpha : MonoBehaviour {
	public SpriteRenderer spriteRenderer;
	public UnityEngine.UI.Image image;
	public Color Color {
		get => spriteRenderer != null ? spriteRenderer.color : image.color;
		set {
			if (spriteRenderer != null) {
				spriteRenderer.color = value;
			} else if (image != null) {
				image.color = value;
			}
		}
	}
	public float Alpha {
		get => spriteRenderer.color.a;
		set {
			Color c = Color;
			c.a = value;
			Color = c;
		}
	}
}
