using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UBOAT.Game.Scene.Camera;

namespace UBOAT.Mods.OrbitRadius {
	class SectionViewCameraControllerPatched {
		[HarmonyPatch(typeof(SectionViewCameraController), "Update")]
		public static class ZoomLimitPatch {
			public const float MaxExtraDistance = 250f;

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				foreach ( var instr in instructions ) {
					// Replace both occurrences of 38f (comparison + assignment)
					if ( instr.opcode == OpCodes.Ldc_R4 && (float)instr.operand == 38f ) {
						yield return new CodeInstruction(OpCodes.Ldc_R4, MaxExtraDistance);
					} else {
						yield return instr;
					}
				}
			}
		}
	}
}
