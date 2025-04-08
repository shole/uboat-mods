using System;
using UnityEngine;
using UBOAT.Game;
using UBOAT.Game.Core.Serialization;

namespace UBOAT.Mods.CrewObstacles {
	[NonSerializedInGameState]
	public class CrewObstaclesLoader : IUserMod {
		public static readonly string MODNAME = "[CrewObstacles]";

		public void OnLoaded() {
			try {
				GameObject go = new GameObject("[CrewObstacles]");
				go.AddComponent<CrewObstacles>();
				GameObject.DontDestroyOnLoad(go);

				Debug.LogFormat("{0} Loaded successfuly!", MODNAME);
			} catch ( Exception e ) {
				Debug.LogFormat("{0} Something is broken!", MODNAME);
				Debug.LogException(e);
			}
		}
	}
}
