using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Scene.Entities;
using UnityEngine;

namespace UBOAT.Mods.NoDiscipline {
	[NonSerializedInGameState]
	public class NoDiscipline : MonoBehaviour {
		private PlayerShip playership;

		void Start() {
			Debug.Log("[NoDiscipline] Start..");
		}

		void Update() {
			if ( playership == null ) {
				// Debug.Log("[NoDiscipline] waiting..");
				playership = GameObject.FindObjectOfType<PlayerShip>();
				if ( playership != null ) {
					Debug.Log("[NoDiscipline] Applied..");
				}
			} else {
				// playership.Discipline.Amount = 1f;
				playership.Discipline.SetAmountQuiet(1f);
			}
		}
	}
}
