using System;
using UnityEngine;
using UBOAT.Game;
using UBOAT.Game.Core.Serialization;

namespace UBOAT.Mods.NoDiscipline {
	[NonSerializedInGameState]
	public class NoDisciplineLoader : IUserMod {
		public static readonly string MODNAME = "[NoDiscipline]";

		public void OnLoaded() {
			try {
				GameObject go = new GameObject("[NoDiscipline]");
				go.AddComponent<NoDiscipline>();
				GameObject.DontDestroyOnLoad(go);
				
				Debug.LogFormat("{0} Loaded successfuly!", MODNAME);
			} catch ( Exception e ) {
				Debug.LogFormat("{0} Something is broken!", MODNAME);
				Debug.LogException(e);
			}
		}
	}
}
