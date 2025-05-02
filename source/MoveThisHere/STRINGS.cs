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
                    public static LocString DECONSTRUCT = "Remove";
                    public static LocString DECONSTRUCT_TOOLTIP = "Remove this hauling point and drop all items here";

                    public static LocString SELF_DESTRUCT_ON = "Enable Auto-Drop";
                    public static LocString SELF_DESTRUCT_ON_TOOLTIP = "When enabled, automatically remove hauling point and drop items when storage is full";
                    public static LocString SELF_DESTRUCT_OFF = "Disable Auto-Drop";
                    public static LocString SELF_DESTRUCT_OFF_TOOLTIP = "Cancel automatic emptying when full";

                    public static LocString SPILL_ON = "Enable Auto-Spill";
                    public static LocString SPILL_ON_TOOLTIP = "When enabled, automatically spill liquids and gasses instead of dropping bottles when hauling point is removed";
                    public static LocString SPILL_OFF = "Disable Auto-Spill";
                    public static LocString SPILL_OFF_TOOLTIP = "Drop bottles of liquids and gasses rather than spilling into the world";
                }
            }
        }
    }
}
