using DWS.Common.InjectionFramework;
using System;
using System.Collections;
using UBOAT.Game.Core.Data;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Core.Time;
using UBOAT.Game.Scene.Audio;
using UBOAT.Game.Scene.Camera;
using UBOAT.Game.Scene.Effects;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene.Items;
using UBOAT.Game.Scene.Tasks;
using UBOAT.Game.UI;
using UnityEngine;

namespace UBOAT.Mods.ImmersiveMap {
	[NonSerializedInGameState]
	public class ImmersiveMap : MonoBehaviour {
		public static ImmersiveMap Instance;
		private void Awake() {
			if ( Instance != null ) { // if already exists, kill old copy as outdated
				Debug.Log("[ImmersiveMap] Restart!");
				Destroy(Instance.gameObject);
			}
			Instance = this;
		}

		public static void Spawn() {
			try {
				GameObject go = new GameObject("[ImmersiveMap]");
				go.AddComponent<ImmersiveMap>();
				GameObject.DontDestroyOnLoad(go);
				// Debug.LogFormat("{0} Loaded successfuly!", MODNAME);
			} catch ( Exception e ) {
				Debug.Log("[ImmersiveMap] Something is broken!");
				Debug.LogException(e);
			}
		}

		private float             lastUpdate = 0;
		private PlayerShip        playership;
		private DeepAudioListener deepAudioListener;
		// private AudioController   audioController;
		private TimeCompressionController timeCompressionController;
		private SavesManager              savesManager;
		private GameUI                    gameUI;

		private NavigationTable navigationTable;

		private DeepAudioSource waterDrops;
		private Modifier        waterDropsVolume;

		private bool inMapLastFrame = false; // were we on map last frame
		private bool inMapNow {
			get {
				return
					hasReferences
					&&
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

		void Start() {
			Debug.Log("[ImmersiveMap] Start..");
			updateCoroutine = StartCoroutine(IUpdate());
		}

		void Update() {
			if ( updateCoroutine == null || lastUpdate - Time.realtimeSinceStartup > 10f ) { // if coroutine timed out or missing, respawn
				Spawn();
			}
		}

		private bool hasReferences => (
			playership != null
			&&
			deepAudioListener != null
			// &&
			// audioController != null
			&&
			timeCompressionController != null
			// &&
			// savesManager!=null
			&&
			gameUI != null
			&&
			navigationTable != null
		);

		IEnumerator IGetReferences() {
			while ( !hasReferences ) {
				yield return new WaitForSecondsRealtime(1f);

				// Debug.Log("[ImmersiveMap]  Getting references ref");

				playership        = FindObjectOfType<PlayerShip>();
				deepAudioListener = FindObjectOfType<DeepAudioListener>();
				gameUI            = FindObjectOfType<GameUI>();
				// audioController   = ScriptableObjectSingleton.LoadSingleton<AudioController>();

				BackgroundTasksManager backgroundTasksManager = ScriptableObjectSingleton.LoadSingleton<BackgroundTasksManager>();
				timeCompressionController = backgroundTasksManager?.GetRunningTask<TimeCompressionController>();
				if ( timeCompressionController != null ) {
					timeCompressionController.TimeCompressionChanged -= TimeCompressionChanged;
					timeCompressionController.TimeCompressionChanged += TimeCompressionChanged;
				}

				navigationTable = FindObjectOfType<NavigationTable>(true);

				// gameUI.ModeChanged+=  // possible future improvement?

				// Debug.Log("ref playership: " + playership);
				// Debug.Log("ref deepAudioListener: " + deepAudioListener);
				// Debug.Log("ref gameUI: " + gameUI);
				// Debug.Log("ref audioController: " + audioController);
				// Debug.Log("ref timeCompressionController: " + timeCompressionController);
				// Debug.Log("ref navigationTable: " + navigationTable);

				lastUpdate = Time.realtimeSinceStartup;
				yield return null;
			}
		}

		IEnumerator IUpdate() {
			while ( true ) {
				yield return IGetReferences(); // wait for ingame state & get references

				if ( inMapNow && !inMapLastFrame ) {
					yield return null; // let map init happen
					EnteredMap();
				} else if ( inMapNow && inMapLastFrame ) {
					InMapUpdate();
				} else if ( !inMapNow && inMapLastFrame ) {
					ExitedMap();
				}

				lastUpdate     = Time.realtimeSinceStartup;
				inMapLastFrame = inMapNow;
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
			// MainCamera.Instance.ResetDopplerEffect();  // doppler not useful

			// audioController.SetFullscreenInterfaceMode(false);

			// MainCamera.Instance.CustomAudioMixerControl = true;

			// audioController.AudioMixer.TransitionToSnapshots(new AudioMixerSnapshot[] {
			// 	audioController.AudioMixer.FindSnapshot("Interior"),
			// 	audioController.AudioMixer.FindSnapshot("Exterior")
			// }, new float[] { 0.1f, 0.1f }, 0);
			// audioController.AudioMixer.FindSnapshot("Exterior").TransitionTo(0);

			// MainCamera.Instance.loadingUIController.FinalAudioMixerSnapshot = this.snapshots[this.isInsideBoat ? 0 : 1][0];

			// Debug.Log("[ImmersiveMap] EnteredMap");
			TimeCompressionChanged();
			// Debug.Log("EnteredMap - done");
		}
		void InMapUpdate() {
			// deepAudioListener.transform.localPosition = new Vector3(0, .75f, 0); // center of boat
			deepAudioListener.transform.localPosition = new Vector3(
				0,                                   // center-boat laterally
				0.6f,                                // good sitting height
				navigationTable.transform.position.z // navigation table position length wise
			);

			deepAudioListener.transform.localRotation = Quaternion.identity; // facing forward
			// deepAudioListener.transform.localRotation = Quaternion.Euler(0, navigationTable.transform.localPosition.x > 0 ? 90 : -90, 0); // facing map (sideways)

			// waterDrops.volume = timeCompressionController.TimeCompression ? 0f : 0.5f; // annoying water dripping sound - MUTE on acceleration, half volume when not
		}
		void ExitedMap() {
			deepAudioListener.IsBelowWater  = false;
			deepAudioListener.IsInsideBoat  = false;
			deepAudioListener.Compartment   = null;
			deepAudioListener.SubmergeLevel = 0f;

			MainCamera.Instance.IsAudioListener      = true;
			MainCamera.Instance.ForceInsideBoatState = false;
			// Debug.Log("ExitedMap - propellers");
			// foreach ( PropellerEffects propellerEffectse in FindObjectsOfType<PropellerEffects>(true) ) { // disabling propellers might have negative effects elsewhere - assume other pages handle this their own way
			// 	propellerEffectse.enabled = false;
			// }
			// Debug.Log("[ImmersiveMap] ExitedMap");
		}
		void TimeCompressionChanged() {
			// Debug.Log("[ImmersiveMap] Time compression changed. inMapNow "+ inMapNow);
			if ( inMapNow ) {
				if ( waterDrops == null ) {
					waterDrops       = FindObjectOfType<WaterDropsEffect>()?.GetComponent<DeepAudioSource>();
					waterDropsVolume = null;
				}
				// Debug.Log("[ImmersiveMap] Time compression changed. waterDrops "+ waterDrops);
				if ( waterDrops != null && waterDropsVolume == null ) {
					waterDropsVolume = waterDrops.volumeMultiplier.AddScaleModifier("ImmersiveMapVolume", false);
				}
				// Debug.Log("[ImmersiveMap] Time compression changed. waterDropsVolume "+ waterDropsVolume);
				if ( waterDropsVolume != null ) {
					waterDropsVolume.Value = (timeCompressionController.TimeCompression ? 0f : 0.5f);
				}
				// Debug.Log("[ImmersiveMap] Time compression changed. waterDropsVolume.Value "+ waterDropsVolume.Value);

				foreach ( PropellerEffects propellerEffect in FindObjectsOfType<PropellerEffects>(true) ) {
					// Debug.Log("[ImmersiveMap] Activate propeller: " + propellerEffect.name);
					propellerEffect.enabled = true;
					// if ( propellerEffect.GetComponent<AudioSource>() ) {  // doppler not useful
					// 	Debug.Log("propeller audiosource: " + propellerEffect.GetComponent<AudioSource>()?.gameObject.name);
					// 	propellerEffect.GetComponent<AudioSource>().dopplerLevel = 1;
					// }
				}
			}
		}
	}
}
