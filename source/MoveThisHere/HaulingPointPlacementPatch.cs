using HarmonyLib;
using STRINGS;
using System;
using System.Reflection;
using UnityEngine;

namespace MoveThisHere
{
    // Allow HaulingPoint to be placed on cells already occupied by another
    // building, but reject solid cells (tiles, natural solids, etc.) so it
    // cannot be embedded inside walls.
    //
    // Vanilla IsValidPlaceLocation returns HELP_BUILDLOCATION_OCCUPIED when
    // the Building layer already has an object. This patch ignores that one
    // failure reason for HaulingPoint, and explicitly rejects solid cells.
    //
    // 允许 HaulingPoint 放在已有建筑所在的格子上，但不允许放在固体格子里
    // （砖块、自然固体等），否则会嵌进墙里。
    //
    // 原版 IsValidPlaceLocation 在 Building 层已有对象时返回
    // HELP_BUILDLOCATION_OCCUPIED，这里对 HaulingPoint 忽略这一种失败；
    // 同时显式拒绝 Grid.Solid 的格子。
    [HarmonyPatch]
    public static class BuildingDef_IsValidPlaceLocation_HaulingPoint_Patch
    {
        // TargetMethod() instead of attribute arguments because the 6-arg
        // overload has an "out string" parameter, and typeof(string).MakeByRefType()
        // is not a valid attribute argument.
        //
        // 用 TargetMethod() 而不是特性实参，因为 6 参数重载带 out string，
        // typeof(string).MakeByRefType() 不能作特性实参。
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

        public static void Postfix(GameObject source_go, int cell, ref bool __result, ref string fail_reason)
        {
            if (source_go == null) return;

            // source_go is BuildTool's visualizer; its Building.Def is the def
            // of the building currently being placed.
            //
            // source_go 是 BuildTool 的 visualizer，其 Building.Def 就是当前
            // 建筑的 Def。
            var building = source_go.GetComponent<Building>();
            if (building == null || building.Def == null) return;
            if (building.Def.PrefabID != HaulingPointConfig.Id) return;

            // Solid cell: reject immediately (tiles, natural solids, etc.),
            // this takes priority over other failure reasons.
            //
            // 固体格：直接拒绝（砖块、自然固体等），优先于其他失败原因。
            if (Grid.IsValidCell(cell) && Grid.Solid[cell])
            {
                __result = false;
                fail_reason = UI.TOOLTIPS.HELP_BUILDLOCATION_INVALID_CELL;
                return;
            }

            if (__result) return;

            // Only forgive "occupied"; keep other failure reasons
            // (invalid cell, Unobtanium, etc.) intact.
            //
            // 只放过"位置被占用"，其余失败原因（非法格、Unobtanium 等）保留。
            if (fail_reason == UI.TOOLTIPS.HELP_BUILDLOCATION_OCCUPIED)
            {
                __result = true;
                fail_reason = null;
            }
        }
    }
}