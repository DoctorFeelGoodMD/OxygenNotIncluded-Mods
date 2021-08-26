using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace DebrisMeltsToDebris
{
    [HarmonyPatch(typeof(SimTemperatureTransfer), "DoOreMeltTransition")] //"DoStateTransition")], old name prior to harmony 2.0 update
    public class DebrisMeltsToDebris_Patches : KMod.UserMod2
    {
        static bool Prefix(int sim_handle, ref Dictionary<int,SimTemperatureTransfer> ___handleInstanceMap)
        {

            SimTemperatureTransfer value = null;
            if (!___handleInstanceMap.TryGetValue(sim_handle, out value) || value == null || value.HasTag(GameTags.Sealed))
            {
                return false; //wow that three underscores thing worked, holy cow I am not a good programmer
            }
            Pickupable pickupable = value.GetComponent<Pickupable>();
            if (!pickupable)
            {
                return true;
            }
            Element element = pickupable.PrimaryElement.Element;
            if (element.IsSolid)
            {

                PrimaryElement primaryElement = pickupable.PrimaryElement;
                Element newElement = ElementLoader.elements[SimMessages.GetElementIndex(element.highTempTransitionTarget)];

                if (newElement.IsSolid) //new and old elements both solid for this to work (I think this is default condition of DoStateTransition...but idk)
                {
                    GameObject gameObject = newElement.substance.SpawnResource(pickupable.transform.GetPosition(), pickupable.PrimaryElement.Mass, primaryElement.Temperature, primaryElement.DiseaseIdx, primaryElement.DiseaseCount);
                    if (gameObject.GetComponent<Pickupable>() != null) 
                    {
                        PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Resource, Mathf.RoundToInt(primaryElement.Mass) + " " + newElement.name, gameObject.transform);
                        value.OnDestroy();
                        Util.KDestroyGameObject(value.gameObject);
                        return false;
                    }   // spawns a resource the same way one does when you mine it (WorldDamage.OnDigComplete)

                }

            }
            return true;
        }
    }
}