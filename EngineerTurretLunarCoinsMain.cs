using Harmony;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace Frogtown
{
    public class EngineerTurretLunarCoinsMain
    {
        public static bool enabled;
        public static UnityModManager.ModEntry modEntry;
        
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            EngineerTurretLunarCoinsMain.modEntry = modEntry;
            var harmony = HarmonyInstance.Create("com.frog.engilunarcoinfix");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            enabled = true;
            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            //Doesn't call FrogtownShared.ModToggled because this is a bug fix and shouldn't affect the isModded flag.
            EngineerTurretLunarCoinsMain.enabled = value;
            return true;
        }
    }

    /// <summary>
    /// Copies existing logic from Init, but filters to turret objects.
    /// </summary>
    [HarmonyPatch(typeof(RoR2.PlayerCharacterMasterController))]
    [HarmonyPatch("Init")]
    [HarmonyPatch(new Type[] { })]
    class EngiLunarCoinPatch
    {
        static void Postfix()
        {
            if (!EngineerTurretLunarCoinsMain.enabled)
            {
                return;
            }
            GlobalEventManager.onCharacterDeathGlobal += delegate (DamageReport damageReport)
            {
                GameObject attacker = damageReport.damageInfo.attacker;
                if (attacker)
                {
                    CharacterBody component = attacker.GetComponent<CharacterBody>();
                    if (component)
                    {
                        GameObject masterObject = component.masterObject;
                        if (masterObject && masterObject.name == "EngiTurretMaster(Clone)")
                        {
                            Deployable component2 = masterObject.GetComponent<Deployable>();
                            if (component2)
                            {
                                PlayerCharacterMasterController component3 = component2.ownerMaster.GetComponent<PlayerCharacterMasterController>();
                                if (component3)
                                {
                                    var coinmult = Traverse.Create(component3).Field<float>("lunarCoinChanceMultiplier");
                                    if (Util.CheckRoll(1f * coinmult.Value, 0f, null))
                                    {
                                        PickupDropletController.CreatePickupDroplet(PickupIndex.lunarCoin1, damageReport.victim.transform.position, Vector3.up * 10f);
                                        coinmult.Value *= 0.5f;
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}