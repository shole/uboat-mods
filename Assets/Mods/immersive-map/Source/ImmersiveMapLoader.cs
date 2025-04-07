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
			ScriptableObjectSingleton.LoadSingleton<SavesManager>().LoadingStarted += () => {
				if ( ImmersiveMap.Instance != null ) {
					// Debug.LogFormat("{0} Restart!", MODNAME);
					GameObject.Destroy(ImmersiveMap.Instance.gameObject);
				}
				try {
					GameObject go = new GameObject("[ImmersiveMap]");
					go.AddComponent<ImmersiveMap>();
					GameObject.DontDestroyOnLoad(go);

					// Debug.LogFormat("{0} Loaded successfuly!", MODNAME);
				} catch ( Exception e ) {
					Debug.LogFormat("{0} Something is broken!", MODNAME);
					Debug.LogException(e);
				}
			};
		}
	}
}
