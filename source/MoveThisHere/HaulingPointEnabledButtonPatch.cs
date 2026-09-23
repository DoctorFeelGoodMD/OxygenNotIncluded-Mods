using HarmonyLib;
using static MoveThisHere.STRINGS;

namespace MoveThisHere
{
    // HaulingPoint 的"禁用建筑"按钮立即生效，不排队等小人
    [HarmonyPatch(typeof(BuildingEnabledButton))]
    [HarmonyPatch("OnMenuToggle")]
    public static class BuildingEnabledButton_HaulingPoint_InstantToggle_Patch
    {
        public static bool Prefix(BuildingEnabledButton __instance)
        {
            var building = __instance.GetComponent<Building>();
            if (building == null || building.Def.PrefabID != HaulingPointConfig.Id)
                return true;

            // IsEnabled 的 setter 就是 OnToggle 的全部内容：
            //   设置 Operational flag、刷新菜单、切换状态项、触发事件
            __instance.IsEnabled = !__instance.IsEnabled;
            return false;
        }
    }
}