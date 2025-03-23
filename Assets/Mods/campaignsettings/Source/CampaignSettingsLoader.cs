using System;
using UnityEngine;
using UBOAT.Game;
using UBOAT.Game.Core.Serialization;

namespace UBOAT.Mods.CampaignSettings {
	[NonSerializedInGameState]
	public class CampaignSettingsLoader : IUserMod {
		public static readonly string MODNAME = "[CampaignSettings]";

		public void OnLoaded() {
			try {
				GameObject go = new GameObject("[CampaignSettings]");
				go.AddComponent<CampaignSettings>();
				GameObject.DontDestroyOnLoad(go);

				Debug.LogFormat("{0} Loaded successfuly!", MODNAME);
			} catch ( Exception e ) {
				Debug.LogFormat("{0} Something is broken!", MODNAME);
				Debug.LogException(e);
			}
		}
	}
}
