using HarmonyLib;
using UBOAT.Game.Scene.Characters;
using UnityEngine;

namespace UBOAT.Mods.NoFatigue {
	[HarmonyPatch(typeof(PlayableCharacter), "Update")]
	public class PlayableCharacterPatched {
		static void Prefix(ref PlayableCharacterData ___data, ref float ___energy) {
			// Debug.Log("Prefix " + ___data.CharacterType + " : " + ___energy);
			// if ( ___data.CharacterType == PlayableCharacterData.Type.Skipper ) {
				___energy = 1.0f;
			// }
		}

		// static void Postfix(ref PlayableCharacterData ___data, ref float ___energy) {
		// 	// Debug.Log("postfix " + ___data.CharacterType + " : " + ___energy);
		// 	if ( ___data.CharacterType == PlayableCharacterData.Type.Skipper ) {
		// 		___energy = 1.0f;
		// 	}
		// }
	}
}
