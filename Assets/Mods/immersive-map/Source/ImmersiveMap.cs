using DWS.Common.InjectionFramework;
using System.Collections;
using System.Reflection;
using UBOAT.Game.Core.Data;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Scene;
using UBOAT.Game.Scene.Audio;
using UBOAT.Game.Scene.Camera;
using UBOAT.Game.Scene.Effects;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene.Items;
using UnityEngine;
using UnityEngine.Audio;

namespace UBOAT.Mods.ImmersiveMap {
	[NonSerializedInGameState]
	public class ImmersiveMap : MonoBehaviour {
		private float             lastUpdate = 0;
		private PlayerShip        playership;
		private DeepAudioListener deepAudioListener;
		private AudioController   audioController;
		// private Modifier          deltaModifier;
		// private Modifier          exteriorVolumeModifier;
		// private Modifier          interiorVolumeModifier;
		private Modifier interiorUIModifier;
		private Modifier exteriorUIModifier;

		// private bool                  applied    = true;
		private bool inMap = false;

		void Start() {
			Debug.Log("[ImmersiveMap] Start..");
			StartCoroutine(IUpdate());
		}
		// void Update() {
		// 	if ( interiorUIModifier == null && playership != null && audioController != null ) {
		// 		// deltaModifier = DeepAudioSource.globalDopplerScale.AddDeltaModifier("MapAudioDoppler", false);
		// 		// exteriorVolumeModifier=audioController.ExteriorVolume.AddDeltaModifier("MapAudioExteriorVolume", false);
		// 		// interiorVolumeModifier=audioController.InteriorVolume.AddDeltaModifier("MapAudioInternalVolume", false);
		// 		// Debug.Log("interiorUIModifierField");
		// 		// FieldInfo interiorUIModifierField = typeof(AudioController).GetField("interiorUIModifier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); // private so reflection is required
		// 		// Debug.Log("interiorUIModifierObject");
		// 		// object interiorUIModifierObject = interiorUIModifierField.GetValue(audioController);
		// 		// Debug.Log("interiorUIModifier");
		// 		// interiorUIModifier = (Modifier)interiorUIModifierObject;
		// 		interiorUIModifier = audioController.GetPrivateValue<Modifier>("interiorUIModifier");
		//
		// 		// Debug.Log("exteriorUIModifierField");
		// 		// FieldInfo exteriorUIModifierField = typeof(AudioController).GetField("exteriorUIModifier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); // private so reflection is required
		// 		// Debug.Log("exteriorUIModifierObject");
		// 		// object exteriorUIModifierObject = exteriorUIModifierField.GetValue(audioController);
		// 		// Debug.Log("exteriorUIModifier");
		// 		// exteriorUIModifier = (Modifier)exteriorUIModifierObject;
		// 		exteriorUIModifier = audioController.GetPrivateValue<Modifier>("exteriorUIModifier");
		//
		// 		Debug.Log("got");
		// 	}
		// }


		void EnteredMap() {
			deepAudioListener.IsBelowWater = playership.Depth > 1;
			deepAudioListener.IsInsideBoat = true;
			// deepAudioListener.Compartment  = playership.Interior.CentralCompartment;
			deepAudioListener.Compartment   = playership.Interior.GetCompartment("Control Room");
			// deepAudioListener.Compartment   = null;
			deepAudioListener.SubmergeLevel = 1f;
			// deepAudioListener.SetPrivateValue("interiorLevel", 1f);


			MainCamera.Instance.CustomAudioMixerControl = true;

			// Debug.Log("interior: "+audioController.InteriorVolume.Value);
			// Debug.Log("exterior: "+audioController.ExteriorVolume.Value);
			audioController.SetFullscreenInterfaceMode(false);

			// audioController.AudioMixer.TransitionToSnapshots(new AudioMixerSnapshot[] {
			// 	audioController.AudioMixer.FindSnapshot("Interior"),
			// 	audioController.AudioMixer.FindSnapshot("Exterior")
			// }, new float[] { 0.5f, 0.5f }, 0);
			audioController.AudioMixer.FindSnapshot("Exterior").TransitionTo(0);

			// MainCamera.Instance.loadingUIController.FinalAudioMixerSnapshot = this.snapshots[this.isInsideBoat ? 0 : 1][0];

			MainCamera.Instance.IsAudioListener      = false;
			// MainCamera.Instance.ForceInsideBoatState = true;

			// if ( interiorUIModifier != null ) {
			// 	Debug.Log("in map");
			// 	Debug.Log("interiorUIModifier.Value " + interiorUIModifier.Value);
			// 	interiorUIModifier.Value = 100f;
			// 	Debug.Log("InteriorVolume.Value " + audioController.InteriorVolume.Value);
			// 	Debug.Log("exteriorUIModifier.Value " + exteriorUIModifier.Value);
			// 	exteriorUIModifier.Value = 100f;
			// 	Debug.Log("ExteriorVolume.Value " + audioController.ExteriorVolume.Value);
			//
			// 	// audioController.AudioMixer.SetFloat("Interior.Volume", (float)10);
			// 	// audioController.AudioMixer.SetFloat("Exterior.Volume", (float)10);
			// 	float interiorv;
			// 	audioController.AudioMixer.GetFloat("Interior.Volume",out interiorv);
			// 	Debug.Log("Interior.Volume "+interiorv);
			// 	float exteriorv;
			// 	audioController.AudioMixer.GetFloat("Exterior.Volume", out exteriorv);
			// 	Debug.Log("Exterior.Volume "+exteriorv);
			// 	// audioController.ExteriorMixerGroup.audioMixer.outputAudioMixerGroup
			// } else {
			// 	Debug.Log("modifiers do NOT exist");
			// }

			// MainCamera.Instance.ResetDopplerEffect();
			// MainCamera.Instance.MapController.
			foreach ( PropellerEffects propellerEffectse in FindObjectsOfType<PropellerEffects>(true) ) {
				propellerEffectse.enabled = true;
			}
		}
		void InMapUpdate() {
			// deepAudioListener.transform.position = playership.Interior.VirtualScene.SceneToMappedPosition(playership.Interior.transform.position);
			// deepAudioListener.transform.position = playership.Interior.VirtualScene.GetScenePosition(playership.Interior.transform.position, playership.Interior.VirtualScene);

			// deepAudioListener.transform.position = playership.NavigationTable.transform.position;
			// deepAudioListener.transform.position = playership.Interior.CentralCompartment.transform.position;
			// deepAudioListener.transform.position = playership.transform.position;
			deepAudioListener.transform.localPosition = new Vector3(0, .75f, 0);
			deepAudioListener.transform.localRotation = Quaternion.identity;
			// deepAudioListener.transform.rotation = playership.transform.rotation;
		}
		void ExitedMap() {

		}

		IEnumerator IUpdate() {
			while ( true ) {
				yield return null;
				if ( playership == null || deepAudioListener == null || audioController == null ) { // wait for ingame state
					yield return new WaitForSeconds(1f);
					playership        = FindObjectOfType<PlayerShip>();
					deepAudioListener = FindObjectOfType<DeepAudioListener>();
					audioController   = ScriptableObjectSingleton.LoadSingleton<AudioController>();
					// audioController   = Globals.Instance.GetPrivateValue<AudioController>("audioController");
					// audioController   = FindObjectOfType<AudioController>();
					// Debug.Log("playership: " + playership);
					// Debug.Log("deepAudioListener: " + deepAudioListener);
					// Debug.Log("audioController: " + audioController);
					Debug.Log("got references");
					yield return null;
					yield return null;
				} else {
					if ( MainCamera.Instance.CurrentMode == CameraMode.Map && !inMap ) {
						inMap = true;
						EnteredMap();
					} else if ( MainCamera.Instance.CurrentMode == CameraMode.Map && inMap ) {
						InMapUpdate();
					} else if ( MainCamera.Instance.CurrentMode != CameraMode.Map && inMap ) {
						inMap = false;
						ExitedMap();
					}
				}
			}
		}
	}
}
