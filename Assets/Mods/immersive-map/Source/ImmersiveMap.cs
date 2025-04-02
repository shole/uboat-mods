using DWS.Common.InjectionFramework;
using System.Collections;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Core.Time;
using UBOAT.Game.Scene.Audio;
using UBOAT.Game.Scene.Camera;
using UBOAT.Game.Scene.Characters;
using UBOAT.Game.Scene.Effects;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene.Items;
using UBOAT.Game.Scene.Tasks;
using UBOAT.Game.UI;
using UnityEngine;

namespace UBOAT.Mods.ImmersiveMap {
	[NonSerializedInGameState]
	public class ImmersiveMap : MonoBehaviour {
		private float                     lastUpdate = 0;
		private PlayerShip                playership;
		private DeepAudioListener         deepAudioListener;
		private AudioController           audioController;
		private TimeCompressionController timeCompressionController;
		private GameUI                    gameUI;

		private NavigationTable navigationTable;
		private Vector3         navigationTableOffset;

		private bool inMapLastFrame = false; // were we on map last frame
		private bool inMapNow {
			get {
				return
					gameUI.CurrentMode == GameUI.Mode.Map; // on map screen
				// MainCamera.Instance.CurrentMode == CameraMode.Map; // maincamera mode is unaware of periscope and other fullscreen UI modes
				// &&
				// (
				// 	!timeCompressionController.TimeCompression // when no time compression
				// 	||
				// 	timeCompressionController.TimeCompressionScale.Value < 1000f // or if compression below this
				// );
			}
		}

		private Coroutine updateCoroutine;

		void Update() {
			if ( updateCoroutine == null || lastUpdate - Time.realtimeSinceStartup > 10f ) { // if coroutine timed out or missing, respawn
				if ( updateCoroutine != null ) {
					StopCoroutine(updateCoroutine);
				}
				Debug.Log("[ImmersiveMap] Start..");
				updateCoroutine = StartCoroutine(IUpdate());
			}
		}

		IEnumerator IUpdate() {
			while ( true ) {
				lastUpdate = Time.realtimeSinceStartup;
				yield return null;
				if ( playership == null || deepAudioListener == null || audioController == null || timeCompressionController == null || gameUI == null || navigationTable == null ) { // wait for ingame state
					yield return new WaitForSecondsRealtime(1f);

					Debug.Log("[ImmersiveMap]  ref");

					playership        = FindObjectOfType<PlayerShip>();
					deepAudioListener = FindObjectOfType<DeepAudioListener>();
					gameUI            = FindObjectOfType<GameUI>();
					audioController   = ScriptableObjectSingleton.LoadSingleton<AudioController>();
					BackgroundTasksManager backgroundTasksManager = ScriptableObjectSingleton.LoadSingleton<BackgroundTasksManager>();
					timeCompressionController = backgroundTasksManager?.GetRunningTask<TimeCompressionController>();

					navigationTable = FindObjectOfType<NavigationTable>(true);

					// MainCamera.Instance.ModeChanged+=modeChanged(); // possible future improvement?

					// Debug.Log("ref playership: " + playership);
					// Debug.Log("ref deepAudioListener: " + deepAudioListener);
					// Debug.Log("ref gameUI: " + gameUI);
					// Debug.Log("ref audioController: " + audioController);
					// Debug.Log("ref timeCompressionController: " + timeCompressionController);
					// Debug.Log("ref navigationTable: " + navigationTable);

					yield return null;
				} else {
					if ( inMapNow && !inMapLastFrame ) {
						yield return null; // let map init happen
						EnteredMap();
					} else if ( inMapNow && inMapLastFrame ) {
						InMapUpdate();
					} else if ( !inMapNow && inMapLastFrame ) {
						ExitedMap();
					}

					inMapLastFrame = inMapNow;
				}
			}
		}

		void EnteredMap() {
			deepAudioListener.IsBelowWater = playership.Depth > 1;
			deepAudioListener.IsInsideBoat = true;
			deepAudioListener.Compartment  = playership.Interior.CentralCompartment;
			// deepAudioListener.Compartment = playership.Interior.GetCompartment("Control Room");
			// deepAudioListener.Compartment   = null;
			deepAudioListener.SubmergeLevel = 1f;
			// deepAudioListener.SetPrivateValue("interiorLevel", 1f);

			MainCamera.Instance.IsAudioListener      = false;
			MainCamera.Instance.ForceInsideBoatState = true;
			// MainCamera.Instance.ResetDopplerEffect();

			// audioController.SetFullscreenInterfaceMode(false);

			// MainCamera.Instance.CustomAudioMixerControl = true;

			// audioController.AudioMixer.TransitionToSnapshots(new AudioMixerSnapshot[] {
			// 	audioController.AudioMixer.FindSnapshot("Interior"),
			// 	audioController.AudioMixer.FindSnapshot("Exterior")
			// }, new float[] { 0.1f, 0.1f }, 0);
			// audioController.AudioMixer.FindSnapshot("Exterior").TransitionTo(0);

			// MainCamera.Instance.loadingUIController.FinalAudioMixerSnapshot = this.snapshots[this.isInsideBoat ? 0 : 1][0];

			// Debug.Log("EnteredMap - PropellerEffects");
			foreach ( PropellerEffects propellerEffect in FindObjectsOfType<PropellerEffects>(true) ) {
				// Debug.Log("propeller audiosource: " + propellerEffect.name);
				propellerEffect.enabled = true;
				// if ( propellerEffect.GetComponent<AudioSource>() ) {
				// 	Debug.Log("propeller audiosource: " + propellerEffect.GetComponent<AudioSource>()?.gameObject.name);
				// 	propellerEffect.GetComponent<AudioSource>().dopplerLevel = 1;
				// }
			}
			// Debug.Log("EnteredMap - done");
		}
		void InMapUpdate() {
			// deepAudioListener.transform.localPosition = new Vector3(0, .75f, 0); // center of compartment
			deepAudioListener.transform.localPosition = new Vector3(0, 0.6f, navigationTable.transform.position.z);

			deepAudioListener.transform.localRotation = Quaternion.identity; // facing forward
			// deepAudioListener.transform.localRotation = Quaternion.Euler(0, navigationTable.transform.localPosition.x > 0 ? 90 : -90, 0); // facing map (sideways)
		}
		void ExitedMap() {
			deepAudioListener.IsBelowWater  = false;
			deepAudioListener.IsInsideBoat  = false;
			deepAudioListener.Compartment   = null;
			deepAudioListener.SubmergeLevel = 0f;

			MainCamera.Instance.IsAudioListener      = true;
			MainCamera.Instance.ForceInsideBoatState = false;

			// Debug.Log("ExitedMap - propellers");
			// foreach ( PropellerEffects propellerEffectse in FindObjectsOfType<PropellerEffects>(true) ) {
			// 	propellerEffectse.enabled = false;
			// }
			// Debug.Log("ExitedMap - done");
		}

	}
}
