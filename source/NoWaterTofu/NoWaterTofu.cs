using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace NoWaterTofu
{
    [HarmonyPatch(typeof(MicrobeMusherConfig), "ConfigureBuildingTemplate")]
    public class NoWaterTofu_Patch : KMod.UserMod2
    {
        static void Postfix(GameObject go, Tag prefab_tag)
        {
			ComplexRecipe.RecipeElement[] tofuIngredients = new ComplexRecipe.RecipeElement[1]//[2]
			{
			new ComplexRecipe.RecipeElement("BeanPlantSeed", 6f)//,
			//new ComplexRecipe.RecipeElement("Water".ToTag(), 50f)
			};//basically just recklessly copy paste the original code from private function configurerecipe and comment out the water

			TofuConfig.recipe.ingredients = tofuIngredients;
			
		}
    }
}