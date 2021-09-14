using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace PetroleumDoesntBreakPipes
{
    [HarmonyPatch(typeof(ConduitFlow), "MeltConduitContents")] //"DoStateTransition")], old name prior to harmony 2.0 update
    public class PetroleumDoesntBreakPipes_Patch : KMod.UserMod2
    {
        static bool Prefix(int conduit_idx, ConduitFlow __instance)
        {
            if (__instance.soaInfo.GetConduit(conduit_idx).GetContents(__instance).element != SimHashes.CrudeOil)
            {
                return true; //not crude oil, evaporate and damage pipes as usual
            }
            ConduitFlow.ConduitContents contents = __instance.soaInfo.GetConduit(conduit_idx).GetContents(__instance);
            contents.element = SimHashes.Petroleum; //set new pipe contents to petrol
            __instance.soaInfo.GetConduit(conduit_idx).SetContents(__instance, contents);

            return false; //skip original method which breaks pipe and dumps on floor
        }
    }
    
}
