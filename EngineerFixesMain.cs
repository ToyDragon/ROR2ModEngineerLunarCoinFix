using BepInEx;
using RoR2;
using UnityEngine;

namespace Frogtown
{
    [BepInDependency("com.frogtown.shared")]
    [BepInPlugin("com.frogtown.engineerfixes", "Engineer Fixes", "1.0.3")]
    public class FrogtownEngineerFixes : BaseUnityPlugin
    {
        public ModDetails modDetails;

        public void Awake()
        {
            modDetails = new ModDetails("com.frogtown.engineerfixes")
            {
                description = "Allows turret kills to drop lunar coins, and turret damage count in score card.",
                githubAuthor = "ToyDragon",
                githubRepo = "ROR2ModEngineerLunarCoinFix",
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
            if (!modDetails.enabled)
            {
                return;
            }
            
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

        private void HealthComponentTakeDamagePostfix(DamageInfo damageInfo)
        {
            if (!modDetails.enabled)
            {
                return;
            }

            damageInfo.attacker = lastAttacker;
        }
    }
}