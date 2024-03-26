using UnityEngine;

public class ChangeImage : MonoBehaviour {
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
	public string ColorHex {
		get => ColorUtility.ToHtmlStringRGBA(Color);
		set {
			string str = value;
			if (!str.StartsWith("#")) {
				str = "#" + str;
			}
			if (ColorUtility.TryParseHtmlString(str, out Color color)) {
				Color = color;
			} else {
				Debug.LogError($"could not parse {value}");
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
