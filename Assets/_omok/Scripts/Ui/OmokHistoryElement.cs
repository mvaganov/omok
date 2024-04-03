#if UNITY_5_3_OR_NEWER
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Omok {
	public class OmokHistoryElement : MonoBehaviour {
		[SerializeField] private Image _icon;
		[SerializeField] private TMP_Text _text;
		[SerializeField] private Button _button;
		[SerializeField] private OmokGame _game;
		private Color _originalBackgroundColor = Color.gray;
		private OmokHistoryNode _node;
		private bool _selected;
		private bool _onPath;
		public Image Icon => _icon;
		public TMP_Text Text => _text;
		public Button Button => _button;
		public bool IconOn {
			get => _icon.enabled;
			set => _icon.enabled = value;
		}
		public OmokGame Game {
			get => _game;
			set => _game = value;
		}

		public bool IsSelected {
			get => _selected;
			set {
				_selected = value;
				if (_selected) { MarkSelected(); } else { UnmarkSelected(); }
			}
		}

		public bool IsOnPath {
			get => _onPath;
			set {
				_onPath = value;
				Image backgroundImage = Icon.transform.parent.GetComponent<Image>();
				Color color = backgroundImage.color;
				color.a = !_onPath ? 0.5f : 1;
				backgroundImage.color = color;
			}
		}

		public OmokHistoryNode OmokNode {
			get => _node;
			set {
				_node = value;
				string toString = GetText(value);
				Text.text = toString;
				Icon.sprite = GetIcon(value, out Color color);
				Icon.color = color;
				name = toString;
			}
		}

		public OmokMove Move {
			get => _node.sourceMove;
		}

		private void Start() {
			Image backgroundImage = Icon.transform.parent.GetComponent<Image>();
			if (backgroundImage != null) { _originalBackgroundColor = backgroundImage.color; }
		}

		public static string GetText(OmokHistoryNode node) {
			return $"{node}";
		}

		public Sprite GetIcon(OmokHistoryNode state, out Color color) {
			if (state.sourceMove == null || state.sourceMove == OmokMove.InvalidMove) {
				color = _game.NeutralColor;
				return null;
			}
			color = state.sourceMove.GetColor(_game);
			return state.sourceMove.GetIcon(_game);
		}

		public void TriggerThisState() {
			OmokHistoryGraphUi stateUi = GetComponentInParent<OmokHistoryGraphUi>();
			stateUi.SetState(_node);
		}

		public void MarkSelected() {
			_selected = true;
			Image backgroundImage = Icon.transform.parent.GetComponent<Image>();
			if (backgroundImage == null) { return; }
			StartCoroutine(AnimateSelection());
		}

		private IEnumerator AnimateSelection() {
			int start = System.Environment.TickCount, now;
			Color darkest = Color.Lerp(_originalBackgroundColor, Color.black, 0.125f);
			Color lighest = Color.Lerp(_originalBackgroundColor, Color.white, 0.125f);
			Color dark, light;
			const int FullPulseDuration = 1024;
			const int HalfPulseDuration = FullPulseDuration / 2;
			const int QuarterPulseDuration = HalfPulseDuration / 2;
			Image backgroundImage = Icon.transform.parent.GetComponent<Image>();
			// stop this animation if the element is no longer selected
			while (_selected && _node == _game.Graph.currentNode) {
				now = System.Environment.TickCount;
				now -= start;
				now %= FullPulseDuration; // how far through the big pulse
				if (now > HalfPulseDuration) { // normalize back half
					now = FullPulseDuration - now;
				}
				// make sure original color is reached
				if (now < QuarterPulseDuration) {
					dark = darkest;
					light = _originalBackgroundColor;
				} else {
					dark = _originalBackgroundColor;
					light = lighest;
					now -= QuarterPulseDuration;
				}
				float percentage = (float)now / QuarterPulseDuration;
				if (backgroundImage == null || !backgroundImage.enabled) {
					if (backgroundImage != null) {
						backgroundImage.enabled = true;
						backgroundImage.color = _originalBackgroundColor;
					}
					break;
				}
				backgroundImage.color = Color.Lerp(dark, light, percentage);
				yield return null;
			}
			UnmarkSelected();
		}

		public void UnmarkSelected() {
			_selected = false;
		}

		public void ToggleIcon() {
			IconOn = !IconOn;
		}
	}
#endif
}
