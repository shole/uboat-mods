using UBOAT.Game.Scene.Entities;
using UnityEngine;

public class NoDiscipline : MonoBehaviour {
	private PlayerShip playership;

	void Update() {
		if ( playership == null ) {
			playership = GameObject.FindObjectOfType<PlayerShip>();
			if ( playership != null ) {
				Debug.Log("No Discipline Applied");
			}
		} else {
			// playership.Discipline.Amount = 1f;
			playership.Discipline.SetAmountQuiet(1f);
		}
	}
}
