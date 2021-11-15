using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace Refuel_Turned_off_Generators
{

    public class RefuelTurnedOffGenerators : KMod.UserMod2
    {
        [HarmonyPatch(typeof(GeneratorConfig), "ConfigureBuildingTemplate")]
        public static class RefuelTurnedOffCoalGenerators_Patch
        {
            static void Postfix(GameObject go, Tag prefab_tag)
            {
                ManualDeliveryKG manualDeliveryKG = go.GetComponent<ManualDeliveryKG>();
                manualDeliveryKG.operationalRequirement = FetchOrder2.OperationalRequirement.None; //I'll just change this to 'none', should be fine right?
            }
        }

        [HarmonyPatch(typeof(WoodGasGeneratorConfig), "DoPostConfigureComplete")]
        public static class RefuelTurnedOffWoodGenerators_Patch
        {
            static void Postfix(GameObject go)
            {
                ManualDeliveryKG manualDeliveryKG = go.GetComponent<ManualDeliveryKG>();
                manualDeliveryKG.operationalRequirement = FetchOrder2.OperationalRequirement.None;
            }
        }
    }
}
