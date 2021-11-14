using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace HatchesDontEatMeat
{
    [HarmonyPatch(typeof(BaseHatchConfig),"FoodDiet")]
    public class HatchesDontEatMeat_Patch : KMod.UserMod2
    {
        static void Postfix(ref List<Diet.Info> __result)
        {

            for (int i = 0; i < __result.Count; i++) //iterates through list of items on diet list 'fooddiet' which includes non-mineral foods
            {
                if (__result[i].consumedTags.First().ToString() == "Meat") //this tag system is pretty confusing
                {
                    __result.RemoveAt(i);
                }
                
            }
        }
    }
}
