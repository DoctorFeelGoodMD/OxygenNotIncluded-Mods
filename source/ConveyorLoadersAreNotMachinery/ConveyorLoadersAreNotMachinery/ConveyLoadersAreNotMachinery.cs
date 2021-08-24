
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;


namespace ConveyorLoadersAreNotMachinery
{
	public class ConveyorLoadersAreNotMachinery_Patches : KMod.UserMod2
	{


		[HarmonyPatch(typeof(SolidConduitInboxConfig), "DoPostConfigureComplete")]
		public class ConveyorLoadersAreNotMachinery_Patches_DoPostConfigureComplete
		{
			static void Postfix(GameObject go)
			{
				go.RemoveTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
			}
		}
	}
}
	