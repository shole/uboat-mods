using DWS.Common.InjectionFramework;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UBOAT.Game;
using UBOAT.Game.Core.Serialization;

namespace UBOAT.Mods.OrbitRadius {
	[NonSerializedInGameState]
	public class OrbitRadiusLoader : IUserMod {
		public static readonly string MODNAME = "[OrbitRadius]";

		public void OnLoaded() {
			try {
				Debug.Log($"{MODNAME} loading...");
				var harmony = new Harmony(MODNAME);
				harmony.PatchAll();
				InjectionFramework.Instance.InjectIntoAssembly(Assembly.GetExecutingAssembly());
				Debug.Log($"{MODNAME} loaded OK.");
			} catch ( Exception e ) {
				Debug.LogError($"{MODNAME} ERROR! ");
				Debug.LogException(e);
			}
		}
	}
}
