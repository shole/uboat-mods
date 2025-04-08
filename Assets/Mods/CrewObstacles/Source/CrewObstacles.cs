using System.Collections;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Scene.Characters;
using UBOAT.Game.Scene.Characters.Navigation;
using UBOAT.Game.Scene.Entities;
using UnityEngine;

namespace UBOAT.Mods.CrewObstacles {
	[NonSerializedInGameState]
	public class CrewObstacles : MonoBehaviour {
		private float      lastUpdate = 0;
		private PlayerShip playerShip;
		private PlayerCrew playercrew;

		private WaitForSecondsRealtime waitForSeconds;
		private Coroutine              updateCoroutine;

		private void Start() {
			waitForSeconds = new WaitForSecondsRealtime(0.3f);
		}
		void Update() {
			if ( updateCoroutine == null || lastUpdate - Time.realtimeSinceStartup > 10f ) { // if coroutine timed out or missing, respawn
				if ( updateCoroutine != null ) {
					StopCoroutine(updateCoroutine);
				}
				Debug.Log("[CrewObstacles] Start..");
				updateCoroutine = StartCoroutine(IUpdate());
			}
		}

		IEnumerator IUpdate() {
			while ( true ) {
				lastUpdate = Time.realtimeSinceStartup;
				if ( playerShip == null || playercrew == null ) { // wait for ingame state
					// Debug.Log("CrewObstacles ref");
					yield return waitForSeconds;
					playerShip = FindObjectOfType<PlayerShip>(); // playership is more reliable for existence check, but we don't really need it
					playercrew = FindObjectOfType<PlayerCrew>();
					yield return null;
				} else {
					if ( playercrew.Characters != null ) {
						foreach ( PlayableCharacter playercrewCharacter in playercrew.Characters ) {
							// Debug.Log("PlayableCharacter " + playercrewCharacter.gameObject.name);
							if (
								playercrewCharacter
								&&
								playercrewCharacter.Navigator
								&&
								(playercrewCharacter.Navigator.AgentMode == AgentMode.Obstacle || playercrewCharacter.Navigator.AgentMode == AgentMode.CarvedObstacle)
							) {
								playercrewCharacter.Navigator.AgentMode = AgentMode.Disabled;
							}
							// if ( playercrewCharacter && playercrewCharacter.Navigator && playercrewCharacter.Navigator.Obstacle ) {  // in case AgentMode.Disabled causes issues
							// 	playercrewCharacter.Navigator.Obstacle.radius  = 0;
							// 	playercrewCharacter.Navigator.Obstacle.height  = 0;
							// 	playercrewCharacter.Navigator.Obstacle.enabled = false;
							// }
						}
					}
					yield return waitForSeconds;
				}
			}
		}
	}
}
