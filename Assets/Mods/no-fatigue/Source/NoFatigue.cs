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
				for (int i = playership.Crew.Count - 1; i >= 0; i--) {
					PlayableCharacter playableCharacter = playership.Crew[i];
					playableCharacter.Energy = 1f;
				}
			}
		}
	}
}
