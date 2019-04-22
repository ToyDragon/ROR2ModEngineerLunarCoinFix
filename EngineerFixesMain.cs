using BepInEx;
using RoR2;
using System;
using UnityEngine;

namespace Frogtown
{
    [BepInDependency("com.frogtown.shared")]
    [BepInPlugin("com.frogtown.engineerfixes", "Engineer Fixes", "1.0.6")]
    public class FrogtownEngineerFixes : BaseUnityPlugin
    {
        public FrogtownModDetails modDetails;

        public void Awake()
        {
            modDetails = new FrogtownModDetails("com.frogtown.engineerfixes")
            {
                description = "Allows turret kills to drop lunar coins, and turret damage count in score card.",
                githubAuthor = "ToyDragon",
                githubRepo = "ROR2ModEngineerLunarCoinFix",
                thunderstoreFullName = "ToyDragon-EngineerLunarCoinsFix",
            };
            modDetails.OnlyContainsBugFixesOrUIChangesThatArentContriversial();
            FrogtownShared.RegisterMod(modDetails);
           
            On.RoR2.HealthComponent.TakeDamage += (orig, instance, damageInfo) =>
            {
                HealthComponentTakeDamagePrefix(damageInfo);
                orig(instance, damageInfo);
                HealthComponentTakeDamagePostfix(damageInfo);
            };
        }

        private GameObject lastAttacker;

        /// <summary>
        /// Damage logging happens in HealthComponent.TakeDamage, but most other effects happen elsewhere. 
        /// </summary>
        /// <param name="damageInfo"></param>
        private void HealthComponentTakeDamagePrefix(DamageInfo damageInfo)
        {
            try
            {
                if (!modDetails.enabled)
                {
                    return;
                }
                lastAttacker = damageInfo?.attacker;
                if (lastAttacker != null)
                {
                    GameObject masterObject = lastAttacker.GetComponent<CharacterBody>()?.masterObject;
                    if (masterObject?.name == "EngiTurretMaster(Clone)")
                    {
                        GameObject body = masterObject.GetComponent<Deployable>()?.ownerMaster?.GetComponent<PlayerCharacterMasterController>()?.master?.GetBodyObject();
                        if (body != null)
                        {
                            damageInfo.attacker = body;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error while checking for turret owner");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
        }

        private void HealthComponentTakeDamagePostfix(DamageInfo damageInfo)
        {
            try
            {
                if (!modDetails.enabled)
                {
                    return;
                }

                damageInfo.attacker = lastAttacker;
            }
            catch(Exception e)
            {
                Debug.LogError("Error while restoring turret as damage owner");
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }
}
    }
}