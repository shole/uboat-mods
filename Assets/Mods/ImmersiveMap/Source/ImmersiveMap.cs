// debug messages for state changes and method calls
//#define DEBUGMAPSTEPS
// debug messages for actions that change something
// #define DEBUGMAPACTIONS
// debug messages that spam every frame
// #define DEBUGMAPFRAME

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
using UBOAT.Game.UI.Periscope;
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
				ImmersiveMap oldMap = FindObjectOfType<ImmersiveMap>();
				if ( oldMap != null ) {
					Destroy(oldMap.gameObject);
				}
				GameObject go = new GameObject("[ImmersiveMap]");
				go.AddComponent<ImmersiveMap>();
				GameObject.DontDestroyOnLoad(go);
				// Debug.LogFormat("{0} Loaded successfuly!", MODNAME);
			} catch ( Exception e ) {
				Debug.Log("[ImmersiveMap] Something is broken!");
				Debug.LogException(e);
			}
		}

		public enum ImmersiveMapState {
			Initing,
			Inited,
			Entering3D,
			In3D,
			EnteringMap,
			InMap,
			EnteringDevice,
			InDevice,
		}

#if DEBUGMAPSTEPS
		ImmersiveMapState _state = ImmersiveMapState.Initing;
		public ImmersiveMapState state {
			get => _state;
			set {
				Debug.Log("[ImmersiveMap] State: " + value);
				_state = value;
			}
		}
#else
		public ImmersiveMapState state = ImmersiveMapState.Initing;
#endif

		private float             lastUpdate = 0;
		private PlayerShip        playership;
		private string            lastPlayershipType = "unset";
		private DeepAudioListener deepAudioListener;
		// private AudioController   audioController;
		private TimeCompressionController timeCompressionController;
		private SavesManager              savesManager;
		private GameUI                    gameUI;

		private NavigationTable navigationTable;
		private PeriscopeUI     periscopeUI;

		private float           waterDropsTargetVolume = 0.5f;
		private DeepAudioSource waterDropsAudioSource;
		private Modifier        waterDropsVolume;

		private DeepAudioSource alarmAudioSource;
		private Modifier        alarmVolume;

		private bool inMapNow {
			get {
				return (
					hasReferences
					&&
					gameUI.CurrentMode == GameUI.Mode.Map // on map screen
				);

				// MainCamera.Instance.CurrentMode == CameraMode.Map; // maincamera mode is unaware of periscope and other fullscreen UI modes
				// &&
				// (
				// 	!timeCompressionController.TimeCompression // when no time compression
				// 	||
				// 	timeCompressionController.TimeCompressionScale.Value < 1000f // or if compression below this
				// );
			}
		}
		private bool inDeviceNow {
			get {
				return (
					hasReferences
					&&
					gameUI.CurrentMode == GameUI.Mode.DeviceManualMode // on specified devices
					&&
					(
						periscopeUI.gameObject.activeSelf // periscope
						// ||
						// otherdevice
					)
				);
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
			playership.Blueprint.Type.Name == lastPlayershipType // has ship changed?
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
			periscopeUI != null
			&&
			navigationTable != null
		);



		IEnumerator IGetReferences() {
#if DEBUGMAPSTEPS
			Debug.Log("[ImmersiveMap]  Getting references");
#endif
			while ( !hasReferences ) {
				yield return new WaitForSecondsRealtime(1f);

				playership = FindObjectOfType<PlayerShip>();
				if ( playership != null ) {
					lastPlayershipType     = playership.Blueprint.Type.Name;
					waterDropsTargetVolume = 0.5f;               // half volume
					if ( lastPlayershipType.Contains("VIIC") ) { // half again because VIIC is louder/closer to map
						waterDropsTargetVolume *= 0.5f;
					}
					// waterDropsTargetVolume *= .33f; // lower because of minimum range increase (this value derived with fair guesstimation)
				}
				deepAudioListener = FindObjectOfType<DeepAudioListener>(true);
				gameUI            = FindObjectOfType<GameUI>(true);
				periscopeUI       = FindObjectOfType<PeriscopeUI>(true);
				// audioController   = ScriptableObjectSingleton.LoadSingleton<AudioController>();

				BackgroundTasksManager backgroundTasksManager = ScriptableObjectSingleton.LoadSingleton<BackgroundTasksManager>();
				timeCompressionController = backgroundTasksManager?.GetRunningTask<TimeCompressionController>();
				if ( timeCompressionController != null ) {
					timeCompressionController.TimeCompressionChanged -= TimeCompressionChanged;
					timeCompressionController.TimeCompressionChanged += TimeCompressionChanged;
				}

				navigationTable = FindObjectOfType<NavigationTable>(true);

				// gameUI.ModeChanged+=  // possible future improvement?

#if DEBUGMAPACTIONS
				Debug.Log("ref playership: " + playership);
				Debug.Log("ref deepAudioListener: " + deepAudioListener);
				Debug.Log("ref gameUI: " + gameUI);
				Debug.Log("ref persicopeUI: " + periscopeUI);
				// Debug.Log("ref audioController: " + audioController);
				Debug.Log("ref timeCompressionController: " + timeCompressionController);
				Debug.Log("ref navigationTable: " + navigationTable);
#endif
				lastUpdate = Time.realtimeSinceStartup;
				yield return null;
			}
		}

		IEnumerator IUpdate() {
			while ( true ) {
				if ( !hasReferences ) {
					state = ImmersiveMapState.Initing;
					yield return IGetReferences(); // wait for ingame state & get references
					state = ImmersiveMapState.Inited;
				}
				if ( inMapNow ) {
					if ( state != ImmersiveMapState.InMap ) { // new to this state
						state = ImmersiveMapState.EnteringMap;
						yield return null; // let map init happen
						EnteredMap();
						state = ImmersiveMapState.InMap;
					} else { // in state
						InMapUpdate();
					}
				} else if ( inDeviceNow ) {
					if ( state != ImmersiveMapState.InDevice ) { // new to this state
						state = ImmersiveMapState.EnteringDevice;
						yield return null; // let map init happen
						EnteredMap();
						state = ImmersiveMapState.InDevice;
					} else { // in state
						InMapUpdate();
					}
				} else {
					if ( state == ImmersiveMapState.InMap || state == ImmersiveMapState.InDevice ) { // new to this state
						state = ImmersiveMapState.Entering3D;
						ExitedMap();
						state = ImmersiveMapState.In3D;
					} // else { // in state
					//
					// }
				}

				lastUpdate = Time.realtimeSinceStartup;

#if DEBUGMAPFRAME
				Debug.Log("[gameUI.CurrentMode] " + gameUI.CurrentMode);
				Debug.Log("[MainCamera.Instance.CurrentMode] " + MainCamera.Instance.CurrentMode);
				Debug.Log("[deepAudioListener.IsBelowWater] " + deepAudioListener.IsBelowWater);
				Debug.Log("[deepAudioListener.IsInsideBoat] " + deepAudioListener.IsInsideBoat);
				Debug.Log("[deepAudioListener.Compartment] " + deepAudioListener.Compartment);
				Debug.Log("[deepAudioListener.SubmergeLevel] " + deepAudioListener.SubmergeLevel);
				Debug.Log("[MainCamera.Instance.IsAudioListener] " + MainCamera.Instance.IsAudioListener);
#endif

				// state update cycle set here
				yield return null;
				// yield return new WaitForSeconds(1f);
			}
		}

		void EnteredMap() {
#if DEBUGMAPSTEPS
			Debug.Log("[ImmersiveMap] EnteredMap");
#endif
			deepAudioListener.IsBelowWater = playership.Depth > 1;
			deepAudioListener.IsInsideBoat = true;
			deepAudioListener.Compartment  = playership.Interior.CentralCompartment;
			// deepAudioListener.Compartment = playership.Interior.GetCompartment("Control Room");
			// deepAudioListener.Compartment   = null;
			deepAudioListener.SubmergeLevel = 1f;
			// deepAudioListener.SetPrivateValue("interiorLevel", 1f);

			MainCamera.Instance.IsAudioListener = false;
			// MainCamera.Instance.ForceInsideBoatState = true;  // do not use - can cause unknown behavior (eg. breaks mission markers)
			// MainCamera.Instance.ResetDopplerEffect();  // doppler not useful

			// audioController.SetFullscreenInterfaceMode(false);

			// MainCamera.Instance.CustomAudioMixerControl = true;

			// audioController.AudioMixer.TransitionToSnapshots(new AudioMixerSnapshot[] {
			// 	audioController.AudioMixer.FindSnapshot("Interior"),
			// 	audioController.AudioMixer.FindSnapshot("Exterior")
			// }, new float[] { 0.1f, 0.1f }, 0);
			// audioController.AudioMixer.FindSnapshot("Exterior").TransitionTo(0);

			// MainCamera.Instance.loadingUIController.FinalAudioMixerSnapshot = this.snapshots[this.isInsideBoat ? 0 : 1][0];

			if ( waterDropsAudioSource == null ) {
				waterDropsAudioSource = FindObjectOfType<WaterDropsEffect>()?.GetComponent<DeepAudioSource>();
				waterDropsVolume      = null;
			}
			if ( waterDropsVolume == null && waterDropsAudioSource != null ) {
				// waterDropsAudioSource.minDistance = 1f; // min distance for consistency
				waterDropsVolume = waterDropsAudioSource.volumeMultiplier.AddScaleModifier("ImmersiveMapVolume", false);
			}

			if ( alarmAudioSource == null ) {
				alarmAudioSource = playership?.GetPrivateValue<DeepAudioSource>("alarmAudioSource");
				alarmVolume      = null;
			}
			if ( alarmVolume == null && alarmAudioSource != null ) {
				alarmVolume = alarmAudioSource.volumeMultiplier.AddScaleModifier("ImmersiveMapVolume", false);
			}
			if ( alarmVolume != null ) {
				alarmVolume.Value = 0.1f;
			}

			TimeCompressionChanged();
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
		}
		void ExitedMap() {
			if ( gameUI.CurrentMode == GameUI.Mode.DeviceManualMode ) { // hydrophone or other device
				deepAudioListener.IsBelowWater  = true;
				deepAudioListener.IsInsideBoat  = false;
				deepAudioListener.Compartment   = null;
				deepAudioListener.SubmergeLevel = 1f;
			} else {
				deepAudioListener.IsBelowWater  = false;
				deepAudioListener.IsInsideBoat  = false;
				deepAudioListener.Compartment   = null;
				deepAudioListener.SubmergeLevel = 0f;

				MainCamera.Instance.IsAudioListener = true;
			}
			// MainCamera.Instance.ForceInsideBoatState = false;

			if ( alarmVolume != null ) {
				alarmVolume.Value = 1f;
			}

			// Debug.Log("ExitedMap - propellers");
			// foreach ( PropellerEffects propellerEffectse in FindObjectsOfType<PropellerEffects>(true) ) { // disabling propellers might have negative effects elsewhere - assume other pages handle this their own way
			// 	propellerEffectse.enabled = false;
			// }
#if DEBUGMAPSTEPS
			Debug.Log("[ImmersiveMap] ExitedMap");
#endif
		}
		void TimeCompressionChanged() {
#if DEBUGMAPSTEPS
			Debug.Log("[ImmersiveMap] Time compression changed. inMapNow:" + inMapNow + " inDeviceNow:" + inDeviceNow);
#endif
			if ( inMapNow || inDeviceNow ) {
				if ( waterDropsVolume != null && waterDropsAudioSource != null ) {
					waterDropsVolume.Value = (timeCompressionController.TimeCompression ? 0f : waterDropsTargetVolume);
#if DEBUGMAPACTIONS
					Debug.Log("[ImmersiveMap] Time compression changed. waterDropsVolume.Value " + waterDropsVolume.Value);
#endif
				}


				foreach ( PropellerEffects propellerEffect in FindObjectsOfType<PropellerEffects>(true) ) {
#if DEBUGMAPACTIONS
					Debug.Log("[ImmersiveMap] Activate propeller: " + propellerEffect.name);
#endif
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
