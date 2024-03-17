using UnityEngine;

namespace Omok {
	public interface IBelongsToOmokGame
	{
		public OmokGame omokGame { get; }
		public IBelongsToOmokGame reference { get; }
	}

	public static class IBelongsToOmokGameExtension {
		public static IBelongsToOmokGame GetOmokGamePeer(this IBelongsToOmokGame self) {
			Component selfComponent = self as Component;
			//Debug.Log("looking");
			IBelongsToOmokGame[] components = selfComponent.GetComponents<IBelongsToOmokGame>();
			for (int i = 0; i < components.Length; ++i) {
				IBelongsToOmokGame component = components[i];
				if (component == self || component.HasLineage(self)) {
					continue;
				}
				//Debug.Log($"{self} found {component}");
				return component;
			}
			return null;
		}
		private static bool HasLineage(this IBelongsToOmokGame self, IBelongsToOmokGame other) {
			IBelongsToOmokGame cursor = self;
			do {
				cursor = cursor.reference;
				if (cursor == other) {
					return true;
				}
			} while (cursor != null) ;
			return false;
		}
	}
}
