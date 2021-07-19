using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using DWS.Common.InjectionFramework;
using TMPro;
using UBOAT.Game.Core;
using UBOAT.Game.Core.Data;
using UBOAT.Game.Core.Input;
using UBOAT.Game.Core.Serialization;
using UBOAT.Game.Sandbox;
using UBOAT.Game.Scene;
using UBOAT.Game.Scene.Camera;
using UBOAT.Game.Scene.Entities;
using UBOAT.Game.Scene.Items;
using UBOAT.Game.Scene.UI;
using UBOAT.Game.UI.Map;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UBOAT.Mods.NearMeters {
	[HarmonyPatch(typeof(RouteRenderer), "PlaceLine")]
	public class RouteRenderer_Patch {

		/*
		userSettings
		gameState
		*/
		// singleton inject
		[Inject] private static UserSettings userSettings;
		[Inject] private static GameState gameState;
		[Inject] private static MainCamera mainCamera;
		[Inject] private static Locale locale;
		
		
		[Inject] private static PlayerShip playerShip;
		[Inject] private static WorldNavMesh worldNavMesh;
		[Inject] private static Sandbox sandbox;

		/*
		thickness
		dashLength
		fontSizeFactor
		route
		invalidPathColor
		validPathColor
		*/
		
		static AccessTools.FieldRef<RouteRenderer, float> thicknessRef = AccessTools.FieldRefAccess<RouteRenderer, float>("thickness");
		static AccessTools.FieldRef<RouteRenderer, float> dashLengthRef = AccessTools.FieldRefAccess<RouteRenderer, float>("dashLength");
		static AccessTools.FieldRef<RouteRenderer, float> fontSizeFactorRef = AccessTools.FieldRefAccess<RouteRenderer, float>("fontSizeFactor");
		static AccessTools.FieldRef<RouteRenderer, IRoute> routeRef = AccessTools.FieldRefAccess<RouteRenderer, IRoute>("route");
		static AccessTools.FieldRef<RouteRenderer, Color> invalidPathColorRef = AccessTools.FieldRefAccess<RouteRenderer, Color>("invalidPathColor");
		static AccessTools.FieldRef<RouteRenderer, Color> validPathColorRef = AccessTools.FieldRefAccess<RouteRenderer, Color>("validPathColor");

		public static bool Prefix(RouteRenderer __instance, ref RouteRenderer.Line line, ref Vector2 from, ref Vector2 targetPoint, bool checkForBlockades) {
		
			bool IsBlockaded(Vector2 _from, Vector2 _targetPoint) // copy of private method
			{
				if ((playerShip.transform.position.GetXZ() - _targetPoint).sqrMagnitude < 64000000f)
				{
					Vector2 that = _targetPoint - _from;
					return Physics.Raycast(_from.ToXZ(), that.ToXZ(), that.magnitude, 1, QueryTriggerInteraction.Ignore);
				}
				return worldNavMesh.RaycastLands(sandbox.SceneToEncounterSpace(_from), sandbox.SceneToEncounterSpace(_targetPoint), WorldNavMesh.RaycastMode.NavMesh, 0.1f);
			}
			
			
			/*
			private void PlaceLine(RouteRenderer.Line line, Vector2 from, Vector2 targetPoint, bool checkForBlockades) {
				RectTransform rectTransform = line.rectTransform;
				float y = RouteRenderer.mainCamera.transform.position.y;
				float num = y * this.thickness;
				float num2 = y * this.dashLength;
				float num3 = targetPoint.x - from.x;
				float num4 = targetPoint.y - from.y;
				rectTransform.eulerAngles = new Vector3(90f, -Mathf.Atan2(num4, num3) * 57.29578f, 0f);
				float num5 = Mathf.Sqrt(num3 * num3 + num4 * num4);
				rectTransform.sizeDelta = new Vector2(num5, num);
				float num6 = num5 / num2;
				line.image.uvRect = new Rect(-0.5f * num6, 0f, num6, 1f);
				rectTransform.localScale = new Vector3(1f, (num3 > 0f) ? 1f : -1f, 1f);
				rectTransform.position = new Vector3(from.x, 0f, from.y);
				int num7 = (RouteRenderer.userSettings.GameplaySettings.units <= Units.MetricWithKnots) ? Mathf.FloorToInt(num5 * 0.001f) : Mathf.FloorToInt(num5 * 0.001f * 0.5399568f);
				int num8 = (RouteRenderer.userSettings.GameplaySettings.units <= Units.MetricWithKnots) ? ((num7 == 0) ? (Mathf.FloorToInt(num5 * 0.02f) * 50) : 0) : ((num7 == 0) ? Mathf.FloorToInt(num5 * 0.01f) : 0);
				bool flag = num5 >= y * 0.03f;
				TextMeshProUGUI label = line.label;
				label.enabled = flag;
				if (flag)
				{
					label.fontSize = num * this.fontSizeFactor * RouteRenderer.userSettings.ControlSettings.uiScale * RouteRenderer.gameState.ConstantPixelSizeFactor;
					label.transform.localScale = new Vector3((num3 > 0f) ? 1f : -1f, 1f, 1f);
					if (line.lastKilometers != num7 || line.lastMeters != num8)
					{
						line.lastKilometers = num7;
						line.lastMeters = num8;
						if (this.route.Target && !this.route.IsCaptureCourseCorrect)
						{
							label.text = "\n<color=#ff4c11>" + RouteRenderer.locale["UI/MapUI/Capture Course Impossible"] + "</color>";
						}
						else
						{
							label.text = "\n" + UnitsUtility.KilometersToPreferredUnits(num5 * 0.001f);
						}
					}
				}
				line.image.color = ((checkForBlockades && RouteRenderer.IsBlockaded(from, targetPoint)) ? this.invalidPathColor : this.validPathColor);
			}
			*/

			RectTransform rectTransform = line.rectTransform;
			float y = mainCamera.transform.position.y;
			float num = y * thicknessRef(__instance);
			float num2 = y * dashLengthRef(__instance);
			float num3 = targetPoint.x - from.x;
			float num4 = targetPoint.y - from.y;
			rectTransform.eulerAngles = new Vector3(90f, -Mathf.Atan2(num4, num3) * 57.29578f, 0f);
			float num5 = Mathf.Sqrt(num3 * num3 + num4 * num4);
			rectTransform.sizeDelta = new Vector2(num5, num);
			float num6 = num5 / num2;
			line.image.uvRect = new Rect(-0.5f * num6, 0f, num6, 1f);
			rectTransform.localScale = new Vector3(1f, (num3 > 0f) ? 1f : -1f, 1f);
			rectTransform.position = new Vector3(from.x, 0f, from.y);
			int num7 = (userSettings.GameplaySettings.units <= Units.MetricWithKnots) ? Mathf.FloorToInt(num5 * 0.01f) : Mathf.FloorToInt(num5 * 0.01f * 0.5399568f);
			int num8 = (userSettings.GameplaySettings.units <= Units.MetricWithKnots) ? ((num7 < 10) ? (Mathf.FloorToInt(num5 * 0.02f) * 50) : 0) : ((num7 == 0) ? Mathf.FloorToInt(num5 * 0.01f) : 0);
			bool flag = num5 >= y * 0.03f;
			TextMeshProUGUI label = line.label;
			label.enabled = flag;
			if (flag)
			{
				label.fontSize = num * fontSizeFactorRef(__instance) * userSettings.ControlSettings.uiScale * gameState.ConstantPixelSizeFactor;
				label.transform.localScale = new Vector3((num3 > 0f) ? 1f : -1f, 1f, 1f);
				if (line.lastKilometers != num7 || line.lastMeters != num8)
				{
					line.lastKilometers = num7;
					line.lastMeters = num8;
					if (routeRef(__instance).Target && !routeRef(__instance).IsCaptureCourseCorrect)
					{
						label.text = "\n<color=#ff4c11>" + locale["UI/MapUI/Capture Course Impossible"] + "</color>";
					}
					else
					{
						label.text = "\n" + MainScript.KilometersToPreferredUnits(num5 * 0.001f);
					}
				}
			}
			line.image.color = ((checkForBlockades && IsBlockaded(from, targetPoint)) ? invalidPathColorRef(__instance) : validPathColorRef(__instance));
			return false;
		}
	}
}
