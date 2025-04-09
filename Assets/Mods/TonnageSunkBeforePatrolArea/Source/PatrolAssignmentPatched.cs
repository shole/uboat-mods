using DWS.Common.InjectionFramework;
using HarmonyLib;
using System;
// using System.Runtime.CompilerServices;
using UBOAT.Game.Sandbox;
using UBOAT.Game.Sandbox.Missions;
using UnityEngine;

namespace UBOAT.Mods.TonnageSunkBeforePatrolArea {
	class PatrolAssignmentPatched {
		[HarmonyPatch(typeof(PatrolAssignment), "PlayerCareerOnTonnageSunkChanged")]
		class PatrolAssignmentPlayerCareerOnTonnageSunkChanged {
			// [Inject]
			// private static PlayerCareer playerCareer;
			static bool Prefix(ref PatrolAssignment __instance, ref Objective ___tonnageObjective, ref int ___grtToSink, ref PlayerCareer ___playerCareer) {
				if ( ___tonnageObjective == null ) {
					return false;
				}
				___tonnageObjective.Arguments[0] = ___playerCareer.MerchantTonnageSunkOnCurrentAssignment;
				___tonnageObjective.ValidateArguments();
				___tonnageObjective.Progress = (float)(___playerCareer.MerchantTonnageSunkOnCurrentAssignment) / (float)___grtToSink;
				if ( ___tonnageObjective.Progress >= 1f ) {
					___tonnageObjective.Id = "SinkGRT Complete";
					___tonnageObjective.Complete();
					if ( __instance.Stage <= PatrolAssignment.Stages.Patrol ) {
						__instance.Stage = PatrolAssignment.Stages.Report;
					}
				}
				return false;
			}
		}
		[HarmonyPatch(typeof(PatrolAssignment), "OnStageUpdate")]
		class PatrolAssignmentOnStageStarted {
			// [Inject]
			// private static PlayerCareer playerCareer;
			// private static PatrolAssignment.Stages lastStage            = PatrolAssignment.Stages.Patrol;
			private static Objective lastTonnageObjective = null;

			// [MethodImpl(MethodImplOptions.NoOptimization)]
			static void Prefix(PatrolAssignment __instance, ref PatrolAssignment.Stages stage, ref int ___grtToSink, ref Objective ___tonnageObjective, ref PlayerCareer ___playerCareer) {
				// Debug.Log("___tonnageObjective " + ___tonnageObjective);
				// Debug.Log("tonnageObjective "+tonnageObjective);
				// Debug.Log("OnStageUpdate Prefix " + stage);
				// Debug.Log("grtToSink " + ___grtToSink);
				// if ( stage == PatrolAssignment.Stages.ReachDestination && lastStage != PatrolAssignment.Stages.ReachDestination ) { // grtToSink is set in ReachDestination state
				// 	___grtToSink = Mathf.Max(0, ___grtToSink - playerCareer.MerchantTonnageSunkOnCurrentAssignment);
				// 	lastStage    = stage;
				// 	Debug.Log("grtToSink -> " + ___grtToSink);
				// }
				if ( ___tonnageObjective != null && ___tonnageObjective != lastTonnageObjective && ___tonnageObjective.Progress < 1f ) {
					// ___tonnageObjective.ValidateArguments();
					// Debug.Log("(int)___tonnageObjective.Arguments[0] " + (int)___tonnageObjective.Arguments[0]);
					// Debug.Log(" ___tonnageObjective.Progress " + ___tonnageObjective.Progress);
					lastTonnageObjective = ___tonnageObjective;
					Debug.Log("[TonnageSunkBeforePatrolArea] Received TonnageObjective (" + (int)___grtToSink + ")");
					if ( ___playerCareer.MerchantTonnageSunkOnCurrentAssignment > 0 ) {
						Debug.Log("[TonnageSunkBeforePatrolArea] MerchantTonnageSunkOnCurrentAssignment/TonnageObjective = " + ___playerCareer.MerchantTonnageSunkOnCurrentAssignment + " / " + (int)___grtToSink + " = " + Mathf.RoundToInt((float)___playerCareer.MerchantTonnageSunkOnCurrentAssignment / (float)(___grtToSink) * 100) + "%");
						___playerCareer.MerchantTonnageSunkOnCurrentAssignment--; // trigger TonnageSunkChanged
						___playerCareer.MerchantTonnageSunkOnCurrentAssignment++; // trigger TonnageSunkChanged
					}
				}
				// if ( ___tonnageObjective != null && ___grtToSink==(int)___tonnageObjective.Arguments[0] && ___tonnageObjective.Progress<1f){
				// 	playerCareer.MerchantTonnageSunkOnCurrentAssignment = playerCareer.MerchantTonnageSunkOnCurrentAssignment+1; // trigger TonnageSunkChanged

				// Debug.Log("tonnageObjective.Arguments[0] "+tonnageObjective.Arguments[0]);
				// tonnageObjective.Arguments[0]=Mathf.Max(0, (int)tonnageObjective.Arguments[0] - playerCareer.MerchantTonnageSunkOnCurrentAssignment);
				// tonnageObjective.ValidateArguments();
				// Debug.Log("tonnageObjective.Arguments[0] -> "+tonnageObjective.Arguments[0]);
				// }
				// PatrolAssignment.additionalObjectiveGRT.GetParameter((int)(1 + this.difficulty)).Value
				// Debug.Log("tonnageSunkBeforeArrival "+tonnageSunkBeforeArrival);
				// Debug.Log("MerchantTonnageSunkOnCurrentAssignment "+playerCareer.MerchantTonnageSunkOnCurrentAssignment);
				//
				// tonnageSunkBeforeArrival = 0; // pretend no tonnage was sunk earlier
				// // tonnageObjective.ValidateArguments();
				// playerCareer.MerchantTonnageSunkOnCurrentAssignment = playerCareer.MerchantTonnageSunkOnCurrentAssignment+1; // trigger TonnageSunkChanged
				// // PlayerCareer.TonnageSunkChanged?.Invoke();
				//
				// tonnageObjective.ValidateArguments();
				//
				// Debug.Log("tonnageSunkBeforeArrival "+tonnageSunkBeforeArrival);
				// Debug.Log("MerchantTonnageSunkOnCurrentAssignment "+playerCareer.MerchantTonnageSunkOnCurrentAssignment);
			}
		}
	}
}
