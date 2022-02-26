using System.Collections.Generic;
using System;
using HarmonyLib;
using TUNING;
using STRINGS;
using KMod;
using UnityEngine;


namespace MoveThisHere
{
	public class MoveThisHere_Patch : UserMod2
	{
		public static class MoveThisHerePatches
		{
			[HarmonyPatch(typeof(GeneratedBuildings))]
			[HarmonyPatch(nameof(GeneratedBuildings.LoadGeneratedBuildings))]
			public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
			{
				public static void Prefix()
				{
					
					Utils.AddBuildingStrings(HaulingPointConfig.Id, HaulingPointConfig.DisplayName, HaulingPointConfig.Description, HaulingPointConfig.Effect);		
					// Add haulpoint to build menu with the help of utils below

					Utils.AddPlan("Base", "storage", HaulingPointConfig.Id, "StorageLocker");
				}
			}

			[HarmonyPatch(typeof(ProductInfoScreen))]
			[HarmonyPatch(nameof(ProductInfoScreen.SetMaterials))]
			public static class ProductInfoScreen_SetMaterials_Patch
			{
				public static void Postfix(BuildingDef def, ref ProductInfoScreen __instance)
				{
					if (def.name == "HaulingPoint")
                    {
						__instance.materialSelectionPanel.gameObject.SetActive(false); //remove material selector since no materials

					}
				}
			}

			[HarmonyPatch(typeof(ResourceRemainingDisplayScreen))]
			[HarmonyPatch(nameof(ResourceRemainingDisplayScreen.GetString))]
			public static class ResourceRemainingDisplayScreen_Patch
			{
				//this clumsy patch overrites the 'sandstone 500/1kg' on the hover text card when building hauling points.
				//checks the buildingdef in the hovercard, since it's public, rather than buildtool itself
				//also checks to make sure that it's exactly 1kg mass - draggable items (wires, pipes etc) otherwise cause errors since they use a drag tool not build tool
				public static string Postfix(string __result, Recipe ___currentRecipe)
				{
					if (___currentRecipe.Ingredients[0].amount == 1f)
					{
						if (BuildTool.Instance.GetComponent<BuildToolHoverTextCard>().currentDef.name == "HaulingPoint")
						{
							__result = "No resources required";
						}
					}
					return __result;
				}
			}


			[HarmonyPatch(typeof(BuildingDef))]
			[HarmonyPatch(nameof(BuildingDef.Instantiate))]
			public static class BuildingDef_Instantiate_Patch
			{
				public static bool Prefix(Vector3 pos, Orientation orientation, IList<Tag> selected_elements, int layer, BuildingDef __instance, ref GameObject __result)
				{
					//this instantiate function is used to create the construction site when a valid building spot is selected
					//since we want instant build, function detects if a haulpoint is being built and instant-builds if so
					//uses same build command called in sandbox

					BuildingDef def = __instance;
					if(__instance.name != "HaulingPoint")
					{ return true; }
					else
                    {
						selected_elements[0] = TagManager.Create("Vacuum"); //sets to vacuum element to prevent heat exchange... this must be dealt with at deconstruct or will crash
						__instance.Build(Grid.PosToCell(pos), orientation, null, selected_elements, 293.15f, playsound: false, GameClock.Instance.GetTime());
						return false; //I know, I know, it's a real sloppy implementation but do you have any better ideas?
					}

				}
			}
		}
	}


	public static class Utils
	{
		// I have shamelessly copied this romen github
		public static void AddBuildingStrings(string buildingId, string name, string description, string effect)
		{
			Strings.Add($"STRINGS.BUILDINGS.PREFABS.{buildingId.ToUpperInvariant()}.NAME", UI.FormatAsLink(name, buildingId));
			Strings.Add($"STRINGS.BUILDINGS.PREFABS.{buildingId.ToUpperInvariant()}.DESC", description);
			Strings.Add($"STRINGS.BUILDINGS.PREFABS.{buildingId.ToUpperInvariant()}.EFFECT", effect);
		}

		//thanks psyko for new building adding methods
		public static void AddPlan(HashedString category, string subcategory, string idBuilding, string addAfter = null)
		{
			Debug.Log("Adding " + idBuilding + " to category " + category);
			foreach (PlanScreen.PlanInfo menu in TUNING.BUILDINGS.PLANORDER)
			{
				if (menu.category == category)
				{
					AddPlanToCategory(menu, subcategory, idBuilding, addAfter);
					return;
				}
			}

			Debug.Log($"Unknown build menu category: ${category}");
		}

		private static void AddPlanToCategory(PlanScreen.PlanInfo menu, string subcategory, string idBuilding, string addAfter = null)
		{
			List<KeyValuePair<string, string>> data = menu.buildingAndSubcategoryData;
			if (data != null)
			{
				if (addAfter == null)
				{
					data.Add(new KeyValuePair<string, string>(idBuilding, subcategory));
				}
				else
				{
					/*
                    foreach (KeyValuePair<string, string> k in data)
                    {
                        Debug.Log($"{k.Key}/{k.Value}");
                    }
                    */
					int index = data.IndexOf(new KeyValuePair<string, string>(addAfter, subcategory));
					if (index == -1)
					{
						Debug.Log($"Could not find building {subcategory}/{addAfter} to add {idBuilding} after. Adding at the end !");
						data.Add(new KeyValuePair<string, string>(idBuilding, subcategory));
						return;
					}
					data.Insert(index + 1, new KeyValuePair<string, string>(idBuilding, subcategory));
				}
			}
		}

	}
}
