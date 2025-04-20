using HarmonyLib;
using UBOAT.Game.Core.Data;
using UBOAT.Game.Scene;
using UBOAT.Game.Scene.Audio;
using UBOAT.Game.Scene.Camera;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene.Items;
using UnityEngine;

namespace UBOAT.Mods.Radio3D {
	class SpeakerSystemPatched {
		[HarmonyPatch(typeof(SpeakerSystem), "Update")]
		class PatrolAssignmentOnStageStarted {
			private static float             minDistance = 15f;
			private static float             maxDistance = 25f;
			private static DeepAudioListener _listener;
			private static DeepAudioListener listener {
				get {
					if ( _listener == null ) {
						_listener = GameObject.FindObjectOfType<DeepAudioListener>();
					}
					return _listener;
				}
			}
			// private static PlayerShipInterior _playerShipInterior;
			// private static PlayerShipInterior playerShipInterior => _playerShipInterior ??= GameObject.FindObjectOfType<PlayerShipInterior>();
			private static Modifier speakerVolumeModifier;

			// [MethodImpl(MethodImplOptions.NoOptimization)]
			static void Postfix(SpeakerSystem __instance, ref DeepAudioSource[] ___speakers, ref MainCamera ___mainCamera) {
				if ( __instance.LoudspeakersMode == LoudspeakersMode.Music && ___speakers.Length > 0 && ___speakers[0].isPlaying ) { // something has to be playing
					if (
						___mainCamera.CurrentMode != CameraMode.Map // ignore on map
						&&
						___mainCamera.CurrentMode != CameraMode.FPP // ignore in first person
					) {
						// Vector3 centerlineFrom  = new Vector3(0, 1, playerShipInterior.Ship.Bounds.min.z);
						// Vector3 centerlineTo    = new Vector3(0, 1, playerShipInterior.Ship.Bounds.max.z);

						float speakerDistance = float.MaxValue;
						for ( int i = 0; i < ___speakers.Length; i++ ) {
							___speakers[i].minDistance = 1f; // speaker near distance
							speakerDistance            = Mathf.Min(speakerDistance, Vector3.Distance(listener.transform.position, ___speakers[i].UnityAudioSource.transform.position));
						}
						float fadeValue = Mathf.InverseLerp(minDistance, maxDistance, speakerDistance);

						// Debug.Log("speaker distance: " + speakerDistance);
						DeepAudioSource deepAudioSource = ___speakers[0];
						// deepAudioSource2.outputAudioMixerGroup  = Globals.Instance.MusicAudioMixerGroup;
						deepAudioSource.spatialBlend           = 1f - fadeValue; // fade to 2D at distance
						deepAudioSource.ignoreGlobalPitchScale = true;
						// deepAudioSource2.volume                 = 0.2f;
						if ( speakerVolumeModifier == null || !deepAudioSource.volumeMultiplier.ScaleModifiers.Contains(speakerVolumeModifier) ) {
							speakerVolumeModifier = deepAudioSource.volumeMultiplier.AddScaleModifier("Radio3DVolume");
						}
						if ( speakerVolumeModifier != null ) {
							speakerVolumeModifier.Value = Mathf.Lerp(1f, 0.2f, fadeValue); // fade volume at distance
						}
					} else {
						if ( speakerVolumeModifier != null ) { // when NOT in 3d view, reset volume multiplier
							speakerVolumeModifier.Value = 1f;
						}
						if ( ___mainCamera.CurrentMode == CameraMode.Map ) { // on map view
							DeepAudioSource deepAudioSource = ___speakers[0];
							deepAudioSource.outputAudioMixerGroup = Globals.Instance.InteriorAudioMixerGroup; // interior mixer not affected by "music volume" slider
							deepAudioSource.spatialBlend          = 0f;                                       // 2D audio
							// deepAudioSource.volume                 = .5f;                                       // volume (vanilla 0.2f)
							deepAudioSource.ignoreGlobalPitchScale = true;
						}
					}
				}
			}
		}
	}
}
