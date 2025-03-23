using System.Collections.Generic;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

namespace UBOAT.Mods.CampaignSettings {
	[NonSerializedInGameState]
	public class CampaignSettings : MonoBehaviour {
		private PlayerShip            playership;
		private GameplaySettingsPanel gameplaySettings;
		private float                 lastUpdate = 0;

		void Start() {
			Debug.Log("[CampaignSettings] Start..");
		}

		void Update() {
			if ( playership == null || gameplaySettings == null ) { // wait for ingame state
				playership       = GameObject.FindObjectOfType<PlayerShip>();
				gameplaySettings = GameObject.FindObjectOfType<GameplaySettingsPanel>(true);
			} else {
				if ( gameplaySettings != null && gameplaySettings.gameObject.activeInHierarchy && Time.time - lastUpdate > 1f ) { // if gameplay settings is found and is active
					lastUpdate = Time.time;

					// set GameplaySettingsPanel.allowCampaignSettings = true - it is unknown if this is required for some settings
					// Debug.Log(gameplaySettings);
					// var prop = gameplaySettings.GetType().GetField("allowCampaignSettings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);  // allowCampaignSettings is a private bool so reflection is required
					// prop.SetValue(gameplaySettings, true);
					// Debug.Log(prop.GetValue(gameplaySettings));

					Transform optionsTransform = gameplaySettings.transform.GetComponentInChildren<VerticalLayoutGroup>().transform;
					foreach ( Toggle option in optionsTransform.GetComponentsInChildren<Toggle>() ) {
						option.interactable = true;
					}
					foreach ( TextMeshProDropdown option in optionsTransform.GetComponentsInChildren<TextMeshProDropdown>() ) {
						option.interactable = true;
					}
				}
			}
		}
	}
}
