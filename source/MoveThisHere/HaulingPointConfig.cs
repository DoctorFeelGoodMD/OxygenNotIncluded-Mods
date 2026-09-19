using TUNING;
using UnityEngine;
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
			obj.SceneLayer = Grid.SceneLayer.Front; // 置于最前层 / Frontmost render layer
			// -3	WorldSelection	世界选择
			// -2	NoLayer	无层
			// -1	Background	背景
			// 1	Backwall	背景墙
			// 2	Gas	气体
			// 3	GasConduits	气管
			// 4	GasConduitBridges	气管桥
			// 5	LiquidConduits	液管
			// 6	LiquidConduitBridges	液管桥
			// 7	SolidConduits	固管
			// 8	SolidConduitContents	固管内容物
			// 9	SolidConduitBridges	固管桥
			// 10	Wires	电线
			// 11	WireBridges	电线桥
			// 12	WireBridgesFront	电线桥前层
			// 13	LogicWires	信号线
			// 14	LogicGates	逻辑门
			// 15	LogicGatesFront	逻辑门前层
			// 16	InteriorWall	内墙
			// 17	GasFront	气体前层
			// 18	BuildingBack	建筑背面
			// 20	Building	普通建筑（默认）
			// 21	BuildingUse	建筑使用中
			// 22	BuildingFront	建筑前层
			// 23	TransferArm	传送臂
			// 26	Ore	掉落物 / 物品
			// 27	Creatures	生物
			// 28	Move	移动中
			// 29	Front	最前
			// 30	GlassTile	玻璃砖
			// 31	Liquid	液体
			// 32	Ground	地面
			// 33	TileMain	砖块主体
			// 34	TileFront	砖块前层
			// 35	FXFront	特效前层
			// 36	FXFront2	特效前层 2
			// 37	SceneMAX	最大值

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

