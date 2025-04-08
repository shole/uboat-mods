using UBOAT.Game;
using UBOAT.Game.Core;
using UBOAT.Game.Scene.Entities;
using DWS.Common.InjectionFramework;
using UnityEngine;

public class NoDisciplineTask : BackgroundTaskBase {
	[Inject]
	private static IExecutionQueue executionQueue;

	public override void Start() {
		Debug.Log("No Discipline Task Started");
		executionQueue.AddTimedUpdateListener(DoUpdate, 0.5f);
	}

	private static PlayerShip playership;

	private static float DoUpdate() {
		if ( playership == null ) {
			playership = GameObject.FindObjectOfType<PlayerShip>();
			if ( playership != null ) {
				Debug.Log("No Discipline Applied");
			}
		} else {
			// playership.Discipline.Amount = 1f;
			playership.Discipline.SetAmountQuiet(1f);
		}
		return 0.5f;
	}

	protected override void OnFinished() {
		executionQueue.RemoveTimedUpdateListener(DoUpdate);
	}
}
