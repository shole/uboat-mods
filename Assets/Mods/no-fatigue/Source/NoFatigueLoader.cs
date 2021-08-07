using System;
using UnityEngine;
using UBOAT.Game;
using UBOAT.Game.Core.Serialization;

namespace UBOAT.Mods.NoFatigue {
	[NonSerializedInGameState]
	public class NoFatigueLoader : IUserMod {
		public static readonly string MODNAME = "[NoFatigue]";

		public void OnLoaded() {
			try {
				GameObject go = new GameObject("[NoFatigue]");
				go.AddComponent<NoFatigue>();
				GameObject.DontDestroyOnLoad(go);
				
				Debug.LogFormat("{0} Loaded successfuly!", MODNAME);
			} catch ( Exception e ) {
				Debug.LogFormat("{0} Something is broken!", MODNAME);
				Debug.LogException(e);
			}
		}
	}
}
