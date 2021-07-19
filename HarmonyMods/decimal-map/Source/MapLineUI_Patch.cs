using Harmony;
using UBOAT.Game.UI.Map;
using System;
using System.Text;
using System.Collections.Generic;
using TMPro;
using UBOAT.Game.Core;
using UBOAT.Game.Core.Data;
using UBOAT.Game.Scene.Camera;
using UnityEngine;
using UnityEngine.UI;
using DWS.Common.InjectionFramework;

namespace UBOAT.Mods.NearMeters {
	[HarmonyPatch(typeof(MapLineUI), "PlaceLine")]
	public class MapLineUI_Patch {

		// private TextMeshProUGUI distanceLabel;
		static AccessTools.FieldRef<MapLineUI, TextMeshProUGUI> distanceLabelRef = AccessTools.FieldRefAccess<MapLineUI, TextMeshProUGUI>("distanceLabel");

		// private float length;
		static AccessTools.FieldRef<MapLineUI, float> lengthRef = AccessTools.FieldRefAccess<MapLineUI, float>("length");

		// usersettings singleton inject
		[Inject] private static UserSettings userSettings;

		static void Postfix(MapLineUI __instance) {  // change text result
			/*
			private void PlaceLine(Vector2 from, Vector2 targetPoint)
			{
				float y = MapLineUI.mainCamera.transform.position.y;
				float num = y * this.thickness;
				float num2 = y * this.dashLength;
				float num3 = targetPoint.x - from.x;
				float num4 = targetPoint.y - from.y;
				this.rectTransform.eulerAngles = new Vector3(90f, -Mathf.Atan2(num4, num3) * 57.29578f, 0f);
				this.length = Mathf.Sqrt(num3 * num3 + num4 * num4);
				this.rectTransform.sizeDelta = new Vector2(this.length, num);
				float num5 = this.length / num2;
				this.image.uvRect = new Rect(-0.5f * num5, 0f, num5, 1f);
				this.rectTransform.localScale = new Vector3(1f, (num3 > 0f) ? 1f : -1f, 1f);
				this.rectTransform.position = new Vector3(from.x, 0f, from.y);
				this.distanceLabel.fontSize = num * 3.5f * MapLineUI.userSettings.ControlSettings.uiScale * MapLineUI.gameState.ConstantPixelSizeFactor;
				this.distanceLabel.transform.localScale = new Vector3((num3 > 0f) ? 1f : -1f, 1f, 1f);
				this.distanceLabel.text = "\n" + UnitsUtility.KilometersToPreferredUnits(this.length * 0.001f);
			}
			*/
			if ( lengthRef(__instance) < 5000f ) {
				//distanceLabelRef(__instance).text = "\n" + UnitsUtility.KilometersToPreferredUnits(lengthRef(__instance) * 0.001f, 0.01f);
				//distanceLabelRef(__instance).text = "\n" + (Mathf.Floor(lengthRef(__instance)/10f)*10f) + " m";
				distanceLabelRef(__instance).text = "\n" + MainScript.KilometersToPreferredUnits(lengthRef(__instance) * 0.001f);
			}
		}
	}
}
