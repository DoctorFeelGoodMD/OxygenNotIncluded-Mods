using static STRINGS.UI;

namespace MoveThisHere
{
    public class STRINGS
    {
        public class BUILDINGS
        {
            public class PREFABS
            {
                public class HAULINGPOINT
                {
                    public static LocString NAME = FormatAsLink("Hauling Point", HaulingPointConfig.Id);
                    public static LocString DESC = "Relocate selected items here, then deconstruct to drop them on the ground.";
                    public static LocString EFFECT = "A temporary designation to bring items to a specific place.";
                }
            }

            public class BUTTONS
            {
                public class HAULINGPOINT
                {
                    // Remove
                    public static LocString REMOVE = "Remove";
                    public static LocString REMOVE_TOOLTIP = "Remove this hauling point and drop all items here";

                    // Auto-Drop
                    public static LocString AUTO_DROP_ON = "Enable Auto-Drop";
                    public static LocString AUTO_DROP_ON_TOOLTIP = "If enabled, automatically remove hauling point and drop items when storage is full";
                    public static LocString AUTO_DROP_OFF = "Disable Auto-Drop";
                    public static LocString AUTO_DROP_OFF_TOOLTIP = "Cancel automatic emptying when full";

                    // Auto-Spill
                    public static LocString AUTO_SPILL_ON = "Enable Auto-Spill";
                    public static LocString AUTO_SPILL_ON_TOOLTIP = "If enabled, automatically spill liquids and gasses instead of dropping bottles when hauling point is removed";
                    public static LocString AUTO_SPILL_OFF = "Disable Auto-Spill";
                    public static LocString AUTO_SPILL_OFF_TOOLTIP = "Drop bottles of liquids and gasses rather than spilling into the world";

                    // Auto-Bottle
                    public static LocString AUTO_BOTTLE_ON = "Enable Auto-Bottle";
                    public static LocString AUTO_BOTTLE_ON_TOOLTIP = "If enabled, Duplicants will bottle liquids and gases to deliver to this hauling point";
                    public static LocString AUTO_BOTTLE_OFF = "Disable Auto-Bottle";
                    public static LocString AUTO_BOTTLE_OFF_TOOLTIP = "If disabled, Duplicants will no longer bottle liquids and gases to deliver to this hauling point";
                }
            }
        }
    }
}
