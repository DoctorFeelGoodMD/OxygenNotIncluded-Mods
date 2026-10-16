using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace MoveThisHere
{
    internal static class SliderSetUnitState
    {
        private static readonly Dictionary<SliderSet, bool> isGramMode = new Dictionary<SliderSet, bool>();

        public static bool GetIsGram(SliderSet set)
        {
            return isGramMode.TryGetValue(set, out bool v) && v;
        }

        public static void SetIsGram(SliderSet set, bool isGram)
        {
            isGramMode[set] = isGram;
        }
    }

    internal static class SliderSetHelper
    {
        // 反射信息缓存：AccessTools 每次调用都做反射查找，缓存到静态字段避免重复开销
        internal static readonly FieldInfo TargetField = AccessTools.Field(typeof(SliderSet), "target");
        internal static readonly FieldInfo ValueSliderField = AccessTools.Field(typeof(SliderSet), "valueSlider");
        internal static readonly MethodInfo UpdateLabelMethod = AccessTools.Method(typeof(SliderSet), "UpdateLabel");
        internal static readonly MethodInfo SetValueMethod = AccessTools.Method(typeof(SliderSet), "SetValue");

        // unitsLabel 原始位置：用 ConditionalWeakTable，SliderSet 被回收后条目自动清理，
        // 不会像 Dictionary 一样留下悬挂引用导致内存泄漏
        private sealed class Vector2Box { public Vector2 Value; }
        private static readonly ConditionalWeakTable<SliderSet, Vector2Box> originalUnitsPos =
            new ConditionalWeakTable<SliderSet, Vector2Box>();

        public static bool IsHaulingPoint(SliderSet set)
        {
            if (set == null || TargetField == null) return false;
            return TargetField.GetValue(set) is HaulingPoint;
        }

        public static ISliderControl GetTarget(SliderSet set)
        {
            if (set == null || TargetField == null) return null;
            return TargetField.GetValue(set) as ISliderControl;
        }

        // 第一次访问时记录 unitsLabel 的原始 anchoredPosition，之后返回同一个值
        public static Vector2 GetOrRecordOriginalUnitsPos(SliderSet set, RectTransform unitsRect)
        {
            if (!originalUnitsPos.TryGetValue(set, out var box))
            {
                box = new Vector2Box { Value = unitsRect.anchoredPosition };
                originalUnitsPos.Add(set, box);
            }
            return box.Value;
        }
    }

	// SetTarget 后：按克模式的最大位数撑好输入框、右移 unitsLabel 容器、输入框文本居中
	[HarmonyPatch(typeof(SliderSet))]
	[HarmonyPatch(nameof(SliderSet.SetTarget))]
	public static class SliderSet_SetTarget_Patch
	{
		private static readonly Dictionary<SliderSet, Vector2> originalUnitsPos = new Dictionary<SliderSet, Vector2>();

		public static void Postfix(SliderSet __instance, ISliderControl target, int index)
		{
			if (!(target is HaulingPoint)) return;

			SliderSetUnitState.SetIsGram(__instance, false);

			float min = target.GetSliderMin(index);
			float max = target.GetSliderMax(index);
			int decimalPlaces = target.SliderDecimalPlaces(index);

			float maxDisplay = max * 1000f;
			int charLimit = Mathf.FloorToInt(1f + Mathf.Log10(maxDisplay + (float)decimalPlaces));
			if (charLimit < 5) charLimit = 5;
			__instance.numberInput.field.characterLimit = charLimit;

			var inputRect = __instance.numberInput.GetComponent<RectTransform>();
			var inputSize = inputRect.sizeDelta;
			inputSize.x = (charLimit + 0.5f) * 9;
			inputRect.sizeDelta = inputSize;

			__instance.numberInput.minValue = min;
			__instance.numberInput.maxValue = max;
			__instance.numberInput.decimalPlaces = decimalPlaces;

			// ---- 输入框文本居中（或右对齐）----
			var tmpField = __instance.numberInput.field;
			if (tmpField != null && tmpField.textComponent != null)
			{
				tmpField.textComponent.alignment = TMPro.TextAlignmentOptions.Center;
				// 右对齐：TMPro.TextAlignmentOptions.Right
			}

			float value = target.GetSliderValue(index);
			var updateLabel = AccessTools.Method(typeof(SliderSet), "UpdateLabel");
			updateLabel?.Invoke(__instance, new object[] { value });
			
			// unitsLabel 文本垂直居中 + 左对齐
			var tmpUnits = __instance.unitsLabel.GetComponent<TMPro.TextMeshProUGUI>();
			if (tmpUnits != null)
			{
				tmpUnits.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
			}

			// ---- 右移 unitsLabel 容器，避开和 numberInput 的重叠 ----
			var unitsRect = __instance.unitsLabel.rectTransform;

			if (!originalUnitsPos.ContainsKey(__instance))
			{
				originalUnitsPos[__instance] = unitsRect.anchoredPosition;
			}

			const float pad = 4f;
			float inputRight = inputRect.anchoredPosition.x + inputRect.sizeDelta.x / 2f;
			float unitsLeft = originalUnitsPos[__instance].x - unitsRect.sizeDelta.x / 2f;
			float overlap = inputRight - unitsLeft + pad;

			unitsRect.anchoredPosition = overlap > 0f
				? originalUnitsPos[__instance] + new Vector2(overlap, 0f)
				: originalUnitsPos[__instance];
		}
	}

    // 接管显示值：value < 1kg 显示克并同步扩大输入框范围，否则显示千克
    [HarmonyPatch(typeof(SliderSet))]
    [HarmonyPatch("UpdateLabel")]
    public static class SliderSet_UpdateLabel_Patch
    {
        public static bool Prefix(SliderSet __instance, float value)
        {
            if (!SliderSetHelper.IsHaulingPoint(__instance)) return true;

            var target = SliderSetHelper.GetTarget(__instance);
            if (target == null) return true;

            float min = target.GetSliderMin(__instance.index);
            float max = target.GetSliderMax(__instance.index);
            int decimalPlaces = target.SliderDecimalPlaces(__instance.index);

            bool isGram = value > 0f && value < 1f;
            SliderSetUnitState.SetIsGram(__instance, isGram);

            // 输入框的 min/max/characterLimit 跟随当前单位同步
            __instance.numberInput.minValue = isGram ? min * 1000f : min;
            __instance.numberInput.maxValue = isGram ? max * 1000f : max;
            __instance.numberInput.decimalPlaces = decimalPlaces;

            float maxDisplay = isGram ? max * 1000f : max;
            int charLimit = Mathf.FloorToInt(1f + Mathf.Log10(maxDisplay + (float)decimalPlaces));
            if (charLimit < 5) charLimit = 5;
            __instance.numberInput.field.characterLimit = charLimit;

            if (isGram)
            {
                __instance.unitsLabel.text = UI.UNITSUFFIXES.MASS.GRAM;
                __instance.numberInput.SetDisplayValue((value * 1000f).ToString("0.##"));
            }
            else
            {
                __instance.unitsLabel.text = UI.UNITSUFFIXES.MASS.KILOGRAM;
                float rounded = Mathf.Round(value * 10f) / 10f;
                __instance.numberInput.SetDisplayValue(rounded.ToString());
            }
            return false;
        }
    }

    // 接管输入解析：克模式下把输入值除以 1000 还原成 kg
    [HarmonyPatch(typeof(SliderSet))]
    [HarmonyPatch("ReceiveValueFromInput")]
    public static class SliderSet_ReceiveValueFromInput_Patch
    {
        public static bool Prefix(SliderSet __instance)
        {
            if (!SliderSetHelper.IsHaulingPoint(__instance)) return true;

            var target = SliderSetHelper.GetTarget(__instance);
            if (target == null) return true;

            float min = target.GetSliderMin(__instance.index);
            float max = target.GetSliderMax(__instance.index);
            bool isGram = SliderSetUnitState.GetIsGram(__instance);

            // 临时把 min/max 设成当前单位的范围，绕过 KNumberInputField.currentValue 的 clamp
            float origMin = __instance.numberInput.minValue;
            float origMax = __instance.numberInput.maxValue;
            __instance.numberInput.minValue = isGram ? min * 1000f : min;
            __instance.numberInput.maxValue = isGram ? max * 1000f : max;

            float inputValue = __instance.numberInput.currentValue;

            __instance.numberInput.minValue = origMin;
            __instance.numberInput.maxValue = origMax;

            float kgValue = isGram ? inputValue / 1000f : inputValue;

            if (__instance.numberInput.decimalPlaces != -1)
            {
                float pow = Mathf.Pow(10f, __instance.numberInput.decimalPlaces);
                kgValue = Mathf.Round(kgValue * pow) / pow;
            }

            var slider = SliderSetHelper.ValueSliderField.GetValue(__instance) as KSlider;
            if (slider != null) slider.value = kgValue;

            SliderSetHelper.SetValueMethod?.Invoke(__instance, new object[] { kgValue });

            return false;
        }
    }
}