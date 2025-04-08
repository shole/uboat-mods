using DWS.Common.InjectionFramework;
using System;
using UnityEngine;
using UBOAT.Game;
using UBOAT.Game.Core.Serialization;

namespace UBOAT.Mods.ImmersiveMap {
	[NonSerializedInGameState]
	public class ImmersiveMapLoader : IUserMod {
		public static readonly string MODNAME = "[ImmersiveMap]";

		public void OnLoaded() {
			ScriptableObjectSingleton.LoadSingleton<SavesManager>().LoadingStarted += ImmersiveMap.Spawn; // load on save load
			ImmersiveMap.Spawn();                                                                         // new game is not loaded through savesmanager
		}
	}
}
