using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
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

			private static List<Modifier> speakerVolumeModifier;

			static bool       isImmersiveMapPresent = false;
			static bool       inited => initedObject != null;
			static GameObject initedObject;

			static void Postfix(SpeakerSystem __instance, ref DeepAudioSource[] ___speakers, ref MainCamera ___mainCamera) {
				if ( !inited ) {
					initedObject          = new GameObject("[Radio3D] init canary");                                                                     // make a gameobject to follow scene lifecycle
					isImmersiveMapPresent = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetType("UBOAT.Mods.ImmersiveMap.ImmersiveMap") != null); // detect ImmersiveMap
					if ( isImmersiveMapPresent ) {
						Debug.Log("[Radio3D] Init: ImmersiveMap mod detected! Using special map code.");
					} else {
						Debug.Log("[Radio3D] Init");
					}
					speakerVolumeModifier = new List<Modifier>();
					for ( var i = 0; i < ___speakers.Length; i++ ) {
						speakerVolumeModifier.Add(___speakers[i].volumeMultiplier.AddScaleModifier("Radio3DVolume" + i));
					}
				}
				if ( __instance.LoudspeakersMode == LoudspeakersMode.Music && ___speakers.Length > 0 && ___speakers[0].isPlaying && speakerVolumeModifier != null ) { // something has to be playing

					if ( ___mainCamera.CurrentMode == CameraMode.Map ) {                                          // on map view
						if ( !isImmersiveMapPresent ) {                                                           // regular Map view function
							for ( int i = 0; i < ___speakers.Length; i++ ) {                                      // map loads first so these are practically the default for all
								___speakers[i].minDistance            = 1f;                                       // speaker near distance
								___speakers[i].outputAudioMixerGroup  = Globals.Instance.InteriorAudioMixerGroup; // interior mixer not affected by "music volume" slider
								___speakers[i].spatialBlend           = 0f;                                       // 2D audio
								___speakers[i].ignoreGlobalPitchScale = true;
								___speakers[i].dopplerLevel           = 0;
							}
							foreach ( Modifier modifier in speakerVolumeModifier ) {
								modifier.Value = .25f; // interior mixer on map is too loud
							}

						} else {                                                                                  // special case when ImmersiveMap mod is loaded
							for ( int i = 0; i < ___speakers.Length; i++ ) {                                      // map loads first so these are practically the default for all
								___speakers[i].minDistance            = 1f;                                       // speaker near distance
								___speakers[i].outputAudioMixerGroup  = Globals.Instance.InteriorAudioMixerGroup; // interior mixer not affected by "music volume" slider
								___speakers[i].spatialBlend           = 0.75f;                                    // 2D audio
								___speakers[i].ignoreGlobalPitchScale = true;
								___speakers[i].dopplerLevel           = 0;
							}
							foreach ( Modifier modifier in speakerVolumeModifier ) {
								modifier.Value = 1f;
							}

						}

					} else if ( ___mainCamera.CurrentMode == CameraMode.FPP ) { // in first person
						for ( int i = 0; i < ___speakers.Length; i++ ) {
							___speakers[i].spatialBlend = 0.9f; // never fully 3d
						}
						foreach ( Modifier modifier in speakerVolumeModifier ) {
							modifier.Value = 1f; // when NOT in 3d view, reset volume multiplier
						}

					} else {
						float speakerDistance = float.MaxValue;
						for ( int i = 0; i < ___speakers.Length; i++ ) {
							speakerDistance = Mathf.Min(speakerDistance, Vector3.Distance(listener.transform.position, ___speakers[i].UnityAudioSource.transform.position));
						}
						float fadeValue = Mathf.InverseLerp(minDistance, maxDistance, speakerDistance);
						for ( int i = 0; i < ___speakers.Length; i++ ) {
							___speakers[i].minDistance = 1f;          // speaker near distance
							___speakers[i].spatialBlend = Mathf.Lerp( // fade to 2D at distance
								0.9f,                                 // never fully 3d when near
								0.0f,                                 // fully 2d
								fadeValue);
						}
						foreach ( Modifier modifier in speakerVolumeModifier ) {
							modifier.Value = Mathf.Lerp(1f, 0.3f, fadeValue); // fade volume at distance
						}

					}
				}
			}
		}
	}
}
