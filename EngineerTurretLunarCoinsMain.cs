using Harmony;
using RoR2;
using System;
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
    [HarmonyPatch(typeof(RoR2.HealthComponent))]
    [HarmonyPatch("TakeDamage")]
    [HarmonyPatch(new Type[] { typeof(DamageInfo) })]
    class TakeDamagePatch
    {
        static GameObject lastAttacker;
        static void Prefix(DamageInfo damageInfo)
        {
            lastAttacker = damageInfo.attacker;
            if (damageInfo.attacker != null)
            {
                CharacterBody component = damageInfo.attacker.GetComponent<CharacterBody>();
                if (component != null)
                {
                    GameObject masterObject = component.masterObject;
                    if (masterObject != null && masterObject.name == "EngiTurretMaster(Clone)")
                    {
                        Deployable component2 = masterObject.GetComponent<Deployable>();
                        if (component2 != null && component2.ownerMaster != null)
                        {
                            PlayerCharacterMasterController component3 = component2.ownerMaster.GetComponent<PlayerCharacterMasterController>();
                            if (component3 != null && component3.master != null)
                            {
                                GameObject body = component3.master.GetBodyObject();
                                if (body != null)
                                {
                                    damageInfo.attacker = body;
                                }
                            }
                        }
                    }
                }
            }
        }

        static void Postfix(DamageInfo damageInfo)
        {
            damageInfo.attacker = lastAttacker;
        }
    }
}