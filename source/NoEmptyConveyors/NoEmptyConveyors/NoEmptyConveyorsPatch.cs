using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace NoEmptyConveyors
{
    [HarmonyPatch(typeof(SimTemperatureTransfer), "DoOreMeltTransition")]
    //this mod must be loaded before Debris Melts to Debris as they both prefix DoOreMeltTransition

    public class NoEmptyConveyorsPatch : KMod.UserMod2
    {

        static bool Prefix(int sim_handle, ref Dictionary<int, SimTemperatureTransfer> ___handleInstanceMap, out int __state)
        {
            SimTemperatureTransfer value = null;
            __state = new int();
            __state = 0;
            if (!___handleInstanceMap.TryGetValue(sim_handle, out value) || value == null || value.HasTag(GameTags.Sealed))
            {
                return false;
            }
            Pickupable pickupable = value.GetComponent<Pickupable>();
            if (!pickupable)
            {
                return true;
            }
            __state = Grid.PosToCell(pickupable.transform.GetPosition());
            if (!Game.Instance.solidConduitFlow.HasConduit(__state))
            { __state = 0; }
            return true;
            //if an object is melting and is a pickable in a cell with a solid conduit, prefix passes the cell ID to postfix
            //unfortunately the pickupable object itself does not know it is on a conduit
            //otherwise it sends zero
            //must pass location to postfix like this because DoOreMeltTransition destroys the pickupable gameobject
        }
        static void Postfix(int __state)
        {
            if (__state <= 0)
            {
                return; //skip everything if not pickupable or on conduit square
            }
            List<int> positions = new List<int>();
            positions.Add(__state);
            positions.Add(Grid.CellAbove(__state));
            positions.Add(Grid.CellBelow(__state));
            positions.Add(Grid.CellLeft(__state)); //check neighboring cells just to be safe, conduit movement is super odd with pickupables
            positions.Add(Grid.CellRight(__state)); //the conduit considers it to be in the next square, but the pickupable itself lags behind

            SolidConduitFlow solidConduitFlow = Game.Instance.solidConduitFlow;

            foreach (var i in positions)
            {
                if (!solidConduitFlow.HasConduit(i))
                { continue; } //skip neighboring cells without conduits

                SolidConduitFlow.Conduit conduit = solidConduitFlow.GetConduit(i);
                SolidConduitFlow.ConduitContents contents;
                contents = solidConduitFlow.GetContents(conduit.GetCell(solidConduitFlow)); //check contents of each conduit in each 5 squares

                if (contents.pickupableHandle.IsValid()) //this skips empty conduits
                {
                    Pickupable pickupable = solidConduitFlow.GetPickupable(contents.pickupableHandle);
                    if (!pickupable.isActiveAndEnabled) //for some reason 'IsNullOrDestroyed()' works only for neighboring cells but not its own
                    {
                        solidConduitFlow.EmptyConduit(conduit.GetCell(solidConduitFlow)); //there are a few functions which remove conduit contents, this one seems right
                        
                    }
                }


            }
            //before this mod is applied, empty conveyor baskets still have all the data (mass, element, etc) of what used to be inside, so I really think this is a bug fix
            //the conveyor system doesn't realize it is empty because when a pickupable is removed by melting this information is not passed to SolidConduitFlow
        }

    }
}
