using TUNING;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MoveThisHere
{ 
	public class HaulingPointConfig : IBuildingConfig
	{
		public const string Id = "HaulingPoint";
		public const string DisplayName = "Hauling Point";
		public const string Description = "Relocate selected items here, then deconstruct to drop them on the ground.";
		public const string Effect = "A temporary designation to bring items to a specific place.";

		public const string DeconstructButtonText = "Remove";
		public const string DeconstructButtonTooltip = "Remove this hauling point and drop all items here";
		public const string SelfDestructButtonText = "Enable Auto-Drop";
		public const string SelfDestructButtonTooltip = "When enabled, automatically remove hauling point and drop items when storage is full";
		public const string SelfDestructButtonCancelText = "Disable Auto-Drop";
		public const string SelfDestructButtonCancelTooltip = "Cancel automatic emptying when full";
		public const string SpillButtonText = "Enable Auto-Spill";
		public const string SpillButtonTooltip = "When enabled, automatically spill liquids and gasses instead of dropping bottles when hauling point is removed";
		public const string SpillButtonCancelText = "Disable Auto-Spill";
		public const string SpillButtonCancelTooltip = "Drop bottles of liquids and gasses rather than spilling into the world";

		public override BuildingDef CreateBuildingDef()
		{
			BuildingDef obj = BuildingTemplates.CreateBuildingDef(
				Id,
				1, 1,
				//"storagelocker_kanim",
				"haulingpoint_kanim", //I'm never using spriter again what a hassle!!
				30,
				3f, 
				new float[1] { 1f }, //building mass is 1kg (of vacuum, imagine that) - less than 1kg causes graphical issues, zero mass causes error
				MATERIALS.ANY_BUILDABLE,
				9999f,
				BuildLocationRule.Anywhere,
				noise: NOISE_POLLUTION.NONE,
				decor: BUILDINGS.DECOR.PENALTY.TIER1); //decor -10 because it's a box of junk
			obj.Floodable = false;
			obj.AudioCategory = "Metal";
			obj.Overheatable = false;
			obj.Repairable = false;
			obj.Disinfectable = false;
			obj.Invincible = true; //nothing but the player can destroy the powerful haulingpoint

			
			return obj;
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
		{
			SoundEventVolumeCache.instance.AddVolume("storagelocker_kanim", "StorageLocker_Hit_metallic_low", NOISE_POLLUTION.NOISY.TIER1);
			Prioritizable.AddRef(go);
			Storage storage = go.AddOrGet<Storage>();
			storage.showInUI = true;
			storage.allowItemRemoval = false;
			storage.showDescriptor = true;
			storage.storageFilters = STORAGEFILTERS.NOT_EDIBLE_SOLIDS.Concat(STORAGEFILTERS.FOOD).Concat(STORAGEFILTERS.LIQUIDS).Concat(STORAGEFILTERS.GASES).ToList();
			//allow everything in storage except critters
			storage.storageFullMargin = 0f;//STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
			storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
			storage.showCapacityStatusItem = true;
			storage.showCapacityAsMainStatus = true;
			go.AddOrGet<HaulingPoint>().totalMaxCapacity = 20000f;
			go.AddOrGetDef<RocketUsageRestriction.Def>(); //I wish I had the DLC, somebody post an issue if whatever this is doesn't work
			Object.Destroy(go.AddOrGet<Deconstructable>()); //remove vanilla deconstructable just to be safe, it's made of vacuum so decon = crash
			go.AddOrGet<DeconstructableHaulingPoint>();
			 

		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.AddOrGetDef<StorageController.Def>();
		}
	}
}

