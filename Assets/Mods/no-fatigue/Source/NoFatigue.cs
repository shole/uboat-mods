using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene.Characters;
using UnityEngine;

namespace UBOAT.Mods.NoFatigue {
	[NonSerializedInGameState]
	public class NoFatigue : MonoBehaviour {
		private PlayerShip playership;

		void Start() {
			Debug.Log("[NoFatigue] Start..");
		}

		void Update() {
			if ( playership == null ) {
				// Debug.Log("[NoFatigue] waiting..");
				playership = GameObject.FindObjectOfType<PlayerShip>();
				if ( playership != null ) {
					Debug.Log("[NoFatigue] Applied..");
				}
			} else {
				PlayableCharacter[] componentsInChildren = playership.GetComponentsInChildren<PlayableCharacter>(true);
				for (int i = componentsInChildren.Length - 1; i >= 0; i--) {
					PlayableCharacter playableCharacter = componentsInChildren[i];
					playableCharacter.Energy = 1f;
				}
			}
		}
	}
}
