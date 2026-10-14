using HarmonyLib;
using STRINGS;
using System;
using System.Reflection;
using UnityEngine;

namespace MoveThisHere
{
    // Reject tile-type buildings on cells that already have a HaulingPoint.
    //
    // HaulingPoint lives on ObjectLayer.MovePlacer, so tile buildings do not
    // collide with it through the Building or Tile layers and would silently
    // bury it inside a wall. This patch checks MovePlacer for a HaulingPoint
    // on every cell the tile would occupy and rejects the placement.
    //
    // 拒绝砖块类建筑放在已有 HaulingPoint 的格子上。
    //
    // HaulingPoint 在 ObjectLayer.MovePlacer 上，砖块放置不会撞 Building 层
    // 或 TileLayer，会把 HaulingPoint 埋进墙里。此补丁检查砖块覆盖的每一格
    // 的 MovePlacer 层，如果有 HaulingPoint 就拒绝放置。
    [HarmonyPatch]
    public static class BuildingDef_TileOverHaulingPoint_Patch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(BuildingDef),
                "IsValidPlaceLocation",
                new Type[]
                {
                    typeof(GameObject),
                    typeof(int),
                    typeof(Orientation),
                    typeof(bool),
                    typeof(string).MakeByRefType(),
                    typeof(bool)
                });
        }

		public static void Postfix(
			BuildingDef __instance,
			int cell,
			Orientation orientation,
			ref bool __result,
			ref string fail_reason)
		{
			if (!__result) return;

			// Only tile-type buildings have a TileLayer
			//
			// 只有砖块类建筑有 TileLayer
			if (__instance.TileLayer == ObjectLayer.NumLayers) return;

			// Only block buildings that turn the cell into solid ground
			// (floor tiles, mesh tiles, drywall-like solids).
			// Ladders, wires, conduits are also tile pieces but do not make the
			// cell solid, so they are allowed.
			//
			// 只拦会让格子变实心的建筑（地砖、网格砖、干板墙之类）。
			// 梯子、电线、管道也是 tile piece 但不会让格子变实心，允许放。
			if (__instance.BuildingComplete == null) return;
			if (__instance.BuildingComplete.GetComponent<SimCellOccupier>() == null) return;

			foreach (var offset in __instance.PlacementOffsets)
			{
				var rotated = Rotatable.GetRotatedCellOffset(offset, orientation);
				int c = Grid.OffsetCell(cell, rotated);
				if (!Grid.IsValidCell(c)) continue;

				var go = Grid.Objects[c, (int)ObjectLayer.MovePlacer];
				if (go == null) continue;
				if (go.GetComponent<HaulingPoint>() == null) continue;

				__result = false;
				fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_OCCUPIED;
				return;
			}
		}
    }
}