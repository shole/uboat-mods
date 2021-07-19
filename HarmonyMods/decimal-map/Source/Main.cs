using System.Reflection;
using DWS.Common.InjectionFramework;
using Harmony;
using UBOAT.Game;
using UBOAT.Game.Core.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using DWS.Common.InjectionFramework;
using UBOAT.Game.Core.Data;

namespace UBOAT.Mods.NearMeters
{

	[NonSerializedInGameState]
	public class MainScript : IUserMod
	{
		//versioning
		internal static readonly string VERSION = "1.0";

		public void OnLoaded()
		{
			Debug.Log("[Near Meters] Version " + VERSION + " is loading...");
			//Harmony
			HarmonyInstance harmony = HarmonyInstance.Create("com.dws.uboat");
			harmony.PatchAll();
			//Scene listeners
			SceneEventsListener.OnSceneAwake += SceneEventsListener_OnSceneAwake;
			SceneEventsListener.OnSceneDestroy += SceneEventsListener_OnSceneDestroy;
			Debug.Log("[Near Meters] Loaded succesfully!");
		}

		private static void SceneEventsListener_OnSceneAwake(Scene scene)
		{
			InjectionFramework.Instance.InjectIntoAssembly(Assembly.GetExecutingAssembly());
			Debug.Log("[Near Meters] Injecting...");
		}

		private static void SceneEventsListener_OnSceneDestroy(Scene scene)
		{
		}
		
		/*
		public static string KilometersToPreferredUnits(float value)
		{
			Units units = UnitsUtility.userSettings.GameplaySettings.units;
			if (units > Units.MetricWithKnots && units == Units.Nautical)
			{
				value *= 0.5399568f;
				int num = Mathf.FloorToInt(value);
				if (num != 0)
				{
					return num + " nmi";
				}
				return Mathf.FloorToInt(value * 10f) + " cables";
			}
			else
			{
				int num2 = Mathf.FloorToInt(value);
				if (num2 != 0)
				{
					return num2 + " km";
				}
				return Mathf.FloorToInt(value * 100f) * 10 + " m";
			}
		}
		*/
		[Inject] private static UserSettings userSettings;
		public static string KilometersToPreferredUnits(float value) {
			Units units = userSettings.GameplaySettings.units;
			if (units > Units.MetricWithKnots && units == Units.Nautical) {
				value *= 0.5399568f;
				if ( 3f < value ) {
					return Mathf.FloorToInt(value) + " nmi";
				}
				if ( 1f < value ) {
					return value.ToString("0.0") + " nmi";
				}
				return Mathf.FloorToInt(value * 10f) + " cables";
			} else {
				if ( 5f < value ) {
					return Mathf.FloorToInt(value) + " km";
				}
				if ( 1f < value ) {
					return value.ToString("0.0")+ " km";
				}
				return Mathf.FloorToInt(value * 100f) * 10 + " m";
			}
		}
	}
}



