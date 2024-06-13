using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene.Characters;
using UnityEngine;

namespace UBOAT.Mods.NoFatigue {
	[NonSerializedInGameState]
	public class NoFatigue : MonoBehaviour {
		private PlayerShip playership;
		private PlayableCharacter[] crew;
		private float lastUpdate=0;

		void Start() {
			Debug.Log("[NoFatigue] Start..");
		}

		void Update() {
			if ( playership == null  ) {
				// Debug.Log("[NoFatigue] waiting..");
				playership = GameObject.FindObjectOfType<PlayerShip>();
				if ( playership != null ) {
					crew = playership.GetComponentsInChildren<PlayableCharacter>(true);
					Debug.Log("[NoFatigue] Applied..");
				}
			} else {
				if ( playership.Docked || crew == null || crew.Length==0 || Time.time-lastUpdate>60f ) { // GetComponents is expensive so use sparingly
					crew = playership.GetComponentsInChildren<PlayableCharacter>(true);
					lastUpdate = Time.time;
				}
				for (int i = crew.Length - 1; i >= 0; i--) {
					if (crew[i]!=null){
						PlayableCharacter playableCharacter = crew[i];
						playableCharacter.Energy = 1f;
					}
				}
			}
		}
	}
}
