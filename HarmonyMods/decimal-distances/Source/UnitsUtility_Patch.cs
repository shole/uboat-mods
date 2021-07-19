using Harmony;
using UnityEngine;
using System;
using System.Collections.Generic;
using UBOAT.Game.Core;
using UBOAT.Game.Core.Data;

namespace UBOAT.Mods.NearMeters {
	[HarmonyPatch(typeof(UnitsUtility), "KilometersToPreferredUnits", new Type[] { typeof(float) })]
	public class UnitsUtility_Patch {
		public static bool Prefix(ref string __result, float value) {
			__result=MainScript.KilometersToPreferredUnits(value);
			return false;
		}
	}
}
