using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;


namespace UntameCritters
{
	public class UntameCrittersPatches
	{
		public static class Mod_OnLoad
		{
			public static void OnLoad()
			{
				Debug.Log("UntameCritters mod loaded");
			}
		}



		[HarmonyPatch(typeof(LayEggStates), "ShowEgg")]
		public class UntameCritters_Patches_LayEggStates
		{
			static void Prefix(LayEggStates.Instance smi, out float __state)
			{
				__state = new float();
				__state = -1f;
				float? wildness = smi.GetSMI<WildnessMonitor.Instance>()?.wildness.value;
				if (wildness != null)
				{
					if(wildness <1f && smi.GetSMI<CreatureCalorieMonitor.Instance>().IsOutOfCalories())
					{
						__state = (float)wildness;
						smi.GetSMI<WildnessMonitor.Instance>().wildness.SetValue(100f);
						//sets wildness in WildnessMonitor to 100 prior to running FertilityMonitor.ShowEgg in original method
						//ShowEgg copies wildness value to egg item, which is then used to create new critter
					}
				}
			}
			static void Postfix(LayEggStates.Instance smi, float __state)
			{
				if (__state != -1f)
				{
					smi.GetSMI<WildnessMonitor.Instance>().wildness.SetValue(__state);
					//reset wildness to previous state (invariably zero) if conditions in prefix triggered
				}

			}
		}
	}
}