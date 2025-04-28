using TUNING;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MoveThisHere
{
	public class HaulingPointConfig : IBuildingConfig
	{
		public const string Id = "HaulingPoint";

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
			Object.Destroy(go.AddOrGet<Reconstructable>()); //remove vanilla reconstructable, can't make out of anything but vacuum (bc made with no resources)
			Object.Destroy(go.AddOrGet<Deconstructable>());
			//also, if deconstructed with vanilla deconstructible will crash because 1kg of vacuum is physically impossible
			go.AddOrGet<DeconstructableHaulingPoint>();


		}

		public override void DoPostConfigureComplete(GameObject go)
		{
			go.AddOrGetDef<StorageController.Def>();
		}
	}
}

