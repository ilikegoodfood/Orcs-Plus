using Assets.Code;
using Assets.Code.Modding;
using CommunityLib;
using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Orcs_Plus
{
    public class ModCore : ModKernel
    {
        public static ModCore core;

        public static CommunityLib.ModCore comLib;

        public AgentAI comLibAI;

        private ComLibHooks comLibHooks;

        public ModData data;

        public AgentAIs agentAI;

        public List<Power> godPowers1;

        public List<Power> godPowers2;

        private static bool patched = false;

        public override void onModsInitiallyLoaded()
        {
            core = this;

            if (!patched)
            {
                patched = true;
                HarmonyPatches.PatchingInit();
            }
        }

        public override void beforeMapGen(Map map)
        {
            core.data = new ModData();
            core.comLibHooks = new ComLibHooks(map);

            getModKernels(map);
            if (comLib == null)
            {
                throw new Exception("OrcsPlus: This mod REQUIRES the Community Library mod to be installed and enabled in order to operate.");
            }
            
            HarmonyPatches_Conditional.PatchingInit();

            eventModifications(map);

            core.godPowers1 = new List<Power>();
            core.godPowers2 = new List<Power>();
            if (core.data.godTenetTypes.TryGetValue(map.overmind.god.GetType(), out Type tenetType) && tenetType != null)
            {
                switch (tenetType.Name)
                {
                    case nameof(H_Orcs_LifeMother):
                        core.godPowers1 = new List<Power>()
                        {
                            new P_Vinerva_Life(map),
                            new P_Vinerva_Health(map)
                        };

                        core.godPowers2 = new List<Power>()
                        {
                            new P_Vinerva_Thorns(map)
                        };
                        break;
                    case nameof(H_Orcs_Perfection):
                        core.godPowers2 = new List<Power>()
                        {
                            new P_Ophanim_PerfectHorde(map)
                        };
                        break;
                    default:
                        break;
                }
            }

            // Example for non-dependent God Tenet registration
            /*foreach (ModKernel kernel in map.mods)
            {
                if (kernel.GetType().Namespace == "Orcs_Plus")
                {
                    // Gets the register Function.
                    MethodInfo registerInfo = kernel.GetType().GetMethod("registerGodTenet", new Type[] { typeof(Type), typeof(Type) });

                    // Creates the args that will be passed into the function.
                    // Make sure to replace 'God_YourGod' and 'H_Orcs_YourGodTenet' with the required classes.
                    //object[] parameters = new object[] { typeof(God_YourGod), typeof(H_Orcs_YourGodTenet) };
                    // Example Line using God_Snake:
                    object[] parameters = new object[] { typeof(God_Snake), typeof(H_Orcs_SectOfTheSerpent) };

                    // The registerGodTenet function returns a bool result. It returns false if 'typeof(God_YourGod)' is not a subtype of 'God', 'typeof(H_Orcs_YourGodTenet)' is not a subtype of HolyTenet, or a tenet has already been registered to that god type.
                    bool result = (bool)registerInfo.Invoke(kernel, parameters);
                }
            }*/

            // Example for non-dependent waystation whitelist registration
            /*foreach (ModKernel kernel in map.mods)
            {
                if (kernel.GetType().Namespace == "Orcs_Plus")
                {
                    // Gets the register Function.
                    MethodInfo registerInfo = kernel.GetType().GetMethod("registerSettlementTypeForOrcWaystation", new Type[] { typeof(Type) });

                    // Creates the args that will be passed into the function.
                    // Make sure to replace 'Set_YourSettleemntType' and 'H_Orcs_YourGodTenet' with the required classes.
                    //object[] parameters = new object[] { typeof(Set_YourSettleemntType), typeof(H_Orcs_YourGodTenet) };
                    // Example Line using Set_CityRuins:
                    object[] parameters = new object[] { typeof(Set_CityRuins) };

                    // The registerGodTenet function returns a bool result. It returns false if 'typeof(Set_YourSettleemntType)' is not a subtype of 'Settlement', or the settlement type has already been registered.
                    bool result = (bool)registerInfo.Invoke(kernel, parameters);
                }
            }*/
        }

        public override void afterMapGenAfterHistorical(Map map)
        {
            if (core.data.tryGetModAssembly("Ixthus", out ModData.ModIntegrationData intDataIx) && intDataIx.assembly != null && intDataIx.typeDict.TryGetValue("Tenet", out Type t) && t != null)
            {
                foreach (HolyOrder_Orcs orcCulture in core.data.getOrcCultures(map, true))
                {
                    for (int i = 0; i < orcCulture.tenets.Count; i++)
                    {
                        if (orcCulture.tenets[i] != null && (orcCulture.tenets[i].GetType() == t || orcCulture.tenets[i].GetType().IsSubclassOf(t)))
                        {
                            orcCulture.tenets.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            if (!map.options.noOrcs)
            {
                map.hintSystem.popCustomHint("Orc Cultures", "Unlike other holy orders, orc cultures are influenced by the actions of the player and of AI agents. Many challenges state that they provide influence over orcish cultures, and how much, but some actions do not, or cannot.\n\n Orc cultures are broadly isolationist, valuing might above most else. They celebrate the arts of war, and respect those who defeat them, or their enemies, in all forms of combat. They strongly dislike those who settle too close to their borders, and will, as well as respecting others who do, conduct raids against their cities and outposts. \n\n With this information in mind, make sure to check the orc culture's moral influence tab every time you complete an action that you think might grant you influence over them. Experiment, and discover what actions grant influence over orcish cultures, and which ones reduce it.");
            }
        }

        public override void afterLoading(Map map)
        {
            core = this;
            if (core.data == null)
            {
                core.data = new ModData();
            }
            core.data.isPlayerTurn = true;
            core.data = data;
            //core.data.updateOrcSGCultureMap(map);

            getModKernels(map);

            if (comLib == null)
            {
                throw new Exception("OrcsPlus: This mod REQUIRES the Community Library mod to be installed and enabled in order to operate. The Community Library mod must come before (above) this mod in the mod load order.");
            }

            HarmonyPatches_Conditional.PatchingInit();
            eventModifications(map);
        }

        private void getModKernels(Map map)
        {
            foreach (ModKernel kernel in map.mods)
            {
                //Console.WriteLine("OrcsPlus: Found kernels with namespace " + kernel.GetType().Namespace);

                switch (kernel.GetType().Namespace)
                {
                    case "CommunityLib":
                        comLib = kernel as CommunityLib.ModCore;
                        core.comLibHooks = new ComLibHooks(map);
                        comLib.RegisterHooks(core.comLibHooks);
                        core.comLibAI = comLib.GetAgentAI();

                        comLib.forceShipwrecks();

                        core.agentAI = new AgentAIs(map);
                        if (core.data.godTenetTypes.TryGetValue(map.overmind.god.GetType(), out Type tenet) && tenet != null)
                        {
                            switch (tenet.Name)
                            {
                                case nameof(H_Orcs_HarbingersMadness):
                                    core.comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_MadnessFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                                    break;
                                case nameof(H_Orcs_LifeMother):
                                    core.comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_NurtureOrchard), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                                    core.comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_HarvestGourd), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                                    break;
                                case nameof(H_Orcs_Perfection):
                                    core.comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_PerfectionFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                                    break;
                                case nameof(H_Orcs_GlorySeeker):
                                    AIChallenge spreadCurse = new AIChallenge(typeof(Rt_H_Orcs_SpreadCurseOfGlory), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocalRandomized });
                                    spreadCurse.delegates_ValidFor.Add(core.agentAI.delegate_ValidFor_SpreadCurse);
                                    core.comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), spreadCurse);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case "ShadowsInsectGod.Code":
                        //Console.WriteLine("OrcsPlus: Found Cordyceps");
                        ModData.ModIntegrationData intDataCord = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("Cordyceps", intDataCord);

                        if (core.data.tryGetModAssembly("Cordyceps", out intDataCord) && intDataCord.assembly != null)
                        {
                            Type kernelType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.ModCore", false);
                            if (kernelType != null )
                            {
                                intDataCord.typeDict.Add("Kernel", kernelType);
                            }

                            Type godType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.God_Insect", false);
                            if (godType != null)
                            {
                                intDataCord.typeDict.Add("God", godType);
                                registerGodTenet(godType, typeof(H_Orcs_InsectileSymbiosis));
                            }

                            Type doomedType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Task_Doomed", false);
                            if (doomedType != null)
                            {
                                intDataCord.typeDict.Add("Doomed", doomedType);
                            }

                            Type droneType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.UAEN_Drone", false);
                            if (droneType != null)
                            {
                                //Console.WriteLine("OrcsPlus: Got Drone Type");
                                intDataCord.typeDict.Add("Drone", droneType);
                            }

                            Type vespidiciSwarmType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.UM_Vespidic_Swarm", false);
                            if (vespidiciSwarmType != null)
                            {
                                intDataCord.typeDict.Add("VespidicSwarm", vespidiciSwarmType);
                            }

                            Type hiveType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Set_Hive", false);
                            if (hiveType != null)
                            {
                                intDataCord.typeDict.Add("Hive", hiveType);
                            }

                            Type swarmType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.SG_Swarm", false);
                            if (swarmType != null)
                            {
                                //Console.WriteLine("OrcsPlus: Got Swarm Type");
                                intDataCord.typeDict.Add("Swarm", swarmType);
                            }
                        }
                        break;
                    case "CovenExpansion":
                        ModData.ModIntegrationData intDataCCC = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("CovensCursesCurios", intDataCCC);

                        if (core.data.tryGetModAssembly("CovensCursesCurios", out intDataCCC) && intDataCCC.assembly != null)
                        {
                            Type dominionBannerType = intDataCCC.assembly.GetType("CovenExpansion.I_BarbDominion", false);
                            intDataCCC.typeDict.Add("Banner", dominionBannerType);
                        }
                        break;
                    case "LivingCharacter":
                        ModData.ModIntegrationData intDataLC = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("LivingCharacters", intDataLC);

                        if (core.data.tryGetModAssembly("LivingCharacters", out intDataLC) && intDataLC.assembly != null)
                        {
                            Type vampireNobeType = intDataLC.assembly.GetType("LivingCharacters.UAEN_Chars_VampireNoble", false);
                            if (vampireNobeType != null)
                            {
                                intDataLC.typeDict.Add("Vampire", vampireNobeType);
                            }
                        }
                        break;
                    case "LivingWilds":
                        ModData.ModIntegrationData intDataLW = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("LivingWilds", intDataLW);

                        if (core.data.tryGetModAssembly("LivingWilds", out intDataLW))
                        {
                            Type natureCritterType = intDataLW.assembly.GetType("LivingWilds.UAEN_Nature_Critter", false);
                            if (natureCritterType != null)
                            {
                                intDataLW.typeDict.Add("NatureCritter", natureCritterType);
                            }

                            Type natureSanctuaryType = intDataLW.assembly.GetType("LivingWilds.Set_Nature_NatureSanctuary", false);
                            if (natureSanctuaryType != null)
                            {
                                intDataLW.typeDict.Add("NatureSanctuary", natureSanctuaryType);
                                comLib.registerSettlementTypeForOrcExpansion(natureSanctuaryType);
                            }

                            Type wolfRunType = intDataLW.assembly.GetType("LivingWilds.Set_Nature_WolfRun", false);
                            if (wolfRunType != null)
                            {
                                intDataLW.typeDict.Add("WolfRun", wolfRunType);
                                core.data.tryAddSettlementTypeForWaystation(wolfRunType);
                            }
                        }
                        break;
                    case "ShadowsBloodshedGod":
                        ModData.ModIntegrationData intDataKishi = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("Kishi", intDataKishi);

                        if (core.data.tryGetModAssembly("Kishi", out intDataKishi))
                        {
                            Type godType = intDataKishi.assembly.GetType("ShadowsBloodshedGod.God_Bloodshed", false);
                            if (godType != null)
                            {
                                intDataKishi.typeDict.Add("Kishi", godType);

                                core.registerGodTenet(godType, typeof(H_Orcs_BloodOffering));
                            }

                            Type traitType = intDataKishi.assembly.GetType("ShadowsBloodshedGod.T_Bloodstain");
                            if (traitType != null)
                            {
                                intDataKishi.typeDict.Add("Bloodstain", traitType);

                                intDataKishi.methodInfoDict.Add("addBloodstain", AccessTools.Method(traitType, "addBloodstain", new Type[] { typeof(Person), typeof(int) }));
                            }
                        }
                        break;
                    case "ShadowsLib":
                        ModData.ModIntegrationData intDataIx = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("Ixthus", intDataIx);

                        if (core.data.tryGetModAssembly("Ixthus", out intDataIx))
                        {
                            Type tenetType = intDataIx.assembly.GetType("ShadowsLib.H_expeditionPatrons", false);
                            if (tenetType != null)
                            {
                                intDataIx.typeDict.Add("Tenet", tenetType);
                            }
                        }
                        break;
                    case "Wonderblunder_DeepOnes":
                        ModData.ModIntegrationData intDataDOPlus = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("DeepOnesPlus", intDataDOPlus);

                        if (core.data.tryGetModAssembly("DeepOnesPlus", out intDataDOPlus))
                        {
                            Type kernelType = intDataDOPlus.assembly.GetType("Wonderblunder_DeepOnes.Modcore", false);
                            if (kernelType != null)
                            {
                                intDataDOPlus.typeDict.Add("Kernel", kernelType);

                                intDataDOPlus.methodInfoDict.Add("getAbyssalItem", AccessTools.Method(kernelType, "getItemFromAbyssalPool", new Type[] { typeof(Map), typeof(UA) }));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void eventModifications(Map map)
        {
            Dictionary<string, EventRuntime.Field> fields = EventRuntime.fields;
            Dictionary<string, EventRuntime.Property> properties = EventRuntime.properties;

            if (!properties.ContainsKey("DRINK_GROTT_HORN"))
            {
                properties.Add(
                    "DRINK_GROTT_HORN",
                    new EventRuntime.TypedProperty<string>(delegate (EventContext c, string _)
                        {
                            if (c.unit != null && c.unit.person != null)
                            {
                                I_DrinkingHorn horn = (I_DrinkingHorn)c.unit.person.items.FirstOrDefault(i => i is I_DrinkingHorn h && h.full);
                                if (horn != null)
                                {
                                    horn.full = false;
                                    c.unit.person.traits.Add(new T_Grott(c.map.param.ch_primalWatersDur));
                                }
                                
                                if (c.unit.person.species != c.map.species_orc)
                                {
                                    c.unit.hp -= 2;
                                    if (c.unit.hp <= 0)
                                    {
                                        c.unit.die(c.unit.map, "Killed by an event outcome", null);
                                        c.map.addUnifiedMessage(c.unit, null, c.unit.getName() + " dies", c.unit.getName() + " has been killed", UnifiedMessage.messageType.AGENT_DIES, true);
                                    }
                                }
                            }
                        })
                    );
            }
        }

        public override void onTurnStart(Map map)
        {
            if (comLib == null)
            {
                return;
            }

            core.data.isPlayerTurn = true;
            core.data.onTurnStart(map);

            if (core.godPowers1.Count > 0 || core.godPowers2.Count > 0)
            {
                core.updateGodPowers(map);
            }

            foreach (Unit unit in map.units)
            {
                if (!unit.isDead && unit is UAEN_OrcUpstart upstart && upstart.society is SG_Orc orcSociety)
                {
                    Item[] items = upstart.person.items;
                    if (!items.Any(i => i is I_HordeBanner banner && banner.orcs == upstart.society))
                    {
                        int itemIndex = 0;
                        for (int i = 0; i < items.Length; i++)
                        {
                            if (items[i] == null)
                            {
                                itemIndex = i;
                                break;
                            }
                        }

                        I_HordeBanner banner = new I_HordeBanner(map, orcSociety, upstart.location);
                        upstart.person.items[itemIndex] = banner;
                    }
                }
            }
        }

        public void updateGodPowers(Map map)
        {
            int status = 4;
            HolyOrder_Orcs orcCulture = null;

            foreach (HolyOrder_Orcs orcCulture2 in core.data.orcSGCultureMap.Values)
            {
                if (orcCulture2?.tenet_god.status < status)
                {
                    status = orcCulture2.tenet_god.status;
                    orcCulture = orcCulture2;
                }

                if (status == -2)
                {
                    break;
                }
            }

            string msg1 = "";
            string msg2 = "";
            if (status >= 0)
            {
                foreach (Power p in core.godPowers1)
                {
                    int index = map.overmind.god.powers.FindIndex(pow => pow == p);
                    if (index != -1)
                    {
                        msg1 += p.getName() + ", ";
                        map.overmind.god.powers.RemoveAt(index);
                        map.overmind.god.powerLevelReqs.RemoveAt(index);
                    }
                }

                foreach (Power p in core.godPowers2)
                {
                    int index = map.overmind.god.powers.FindIndex(pow => pow == p);
                    if (index != -1)
                    {
                        msg2 += p.getName() + ", ";
                        map.overmind.god.powers.RemoveAt(index);
                        map.overmind.god.powerLevelReqs.RemoveAt(index);
                    }
                }

                if (msg1 != "" || msg2 != "")
                {
                    if (msg2 == "")
                    {
                        msg1 = msg1.Remove(msg1.Length - 2, 2);
                        msg1 += ".";
                    }
                    else
                    {
                        msg2 = msg2.Remove(msg2.Length - 2, 2);
                        msg2 += ".";
                    }

                    if (orcCulture != null)
                    {
                        map.addUnifiedMessage(orcCulture, null, "Lost Access To Powers", "Changes in the tenets of an Orc Culture has resulted in you losing access to the following powers:\n" + msg1 + msg2, "Lost Powers");
                    }
                    else
                    {
                        map.addUnifiedMessage(map.locations[0], null, "Lost Access To Powers", "Changes in the tenets of an Orc Culture has resulted in you losing access to the following powers:\n" + msg1 + msg2, "Lost Powers");
                    }
                }
            }
            else if (status == -1)
            {
                foreach (Power p in core.godPowers1)
                {
                    if (!map.overmind.god.powers.Contains(p))
                    {
                        msg1 += p.getName() + " (cost " + p.getCost() +"), ";
                        map.overmind.god.powers.Add(p);
                        map.overmind.god.powerLevelReqs.Add(p.getCost() - 1);
                    }
                }

                foreach (Power p in core.godPowers2)
                {
                    int index = map.overmind.god.powers.FindIndex(pow => pow == p);
                    if (index != -1)
                    {
                        msg2 += p.getName() + ", ";
                        map.overmind.god.powers.RemoveAt(index);
                        map.overmind.god.powerLevelReqs.RemoveAt(index);
                    }
                }

                if (msg1 != "")
                {
                    msg1 = msg1.Remove(msg1.Length - 2, 2);
                    msg1 += ".";
                    if (orcCulture != null)
                    {
                        map.addUnifiedMessage(orcCulture, null, "Gained Access To Powers", "Changes in the tenets of an Orc Culture has resulted in you gaining access to the following powers:\n" + msg1 + "\n A power will not be visible in the Powers list until your maximum power is equal to, or greater than, the power's cost.", "Gained Powers");
                    }
                    else
                    {
                        map.addUnifiedMessage(map.locations[0], null, "Gained Access To Powers", "Changes in the tenets of an Orc Culture has resulted in you gaining access to the following powers:\n" + msg1 + "\n A power will not be visible in the Powers list until your maximum power is equal to, or greater than, the power's cost.", "Gained Powers");
                    }
                   
                }

                if (msg2 != "")
                {
                    msg2 = msg2.Remove(msg2.Length - 2, 2);
                    msg2 += ".";
                    if (orcCulture != null)
                    {
                        map.addUnifiedMessage(orcCulture, null, "Lost Access To Powers", "Changes in the tenets of an Orc Culture has resulted in you losing access to the following powers:\n" + msg2, "Lost Powers");
                    }
                    else
                    {
                        map.addUnifiedMessage(map.locations[0], null, "Lost Access To Powers", "Changes in the tenets of an Orc Culture has resulted in you losing access to the following powers:\n" + msg2, "Lost Powers");
                    }
                }
            }
            else if (status == -2)
            {
                foreach (Power p in core.godPowers1)
                {
                    if (!map.overmind.god.powers.Contains(p))
                    {
                        msg1 += p.getName() + " (cost " + p.getCost() + "), ";
                        map.overmind.god.powers.Add(p);
                        map.overmind.god.powerLevelReqs.Add(p.getCost() - 1);
                    }
                }

                foreach (Power p in core.godPowers2)
                {
                    if (!map.overmind.god.powers.Contains(p))
                    {
                        msg2 += p.getName() + " (cost " + p.getCost() + "), ";
                        map.overmind.god.powers.Add(p);
                        map.overmind.god.powerLevelReqs.Add(p.getCost() - 1);
                    }
                }

                if (msg1 != "" || msg2 != "")
                {
                    if (msg2 == "")
                    {
                        msg1 = msg1.Remove(msg1.Length - 2, 2);
                        msg1 += ".";
                    }
                    else
                    {
                        msg2 = msg2.Remove(msg2.Length - 2, 2);
                        msg2 += ".";
                    }

                    if (orcCulture != null)
                    {
                        map.addUnifiedMessage(orcCulture, null, "Gained Access To Powers", "Changes in the tenets of an Orc Culture has resulted in you gaining access to the following powers:\n" + msg1 + msg2 + "\n A power will not be visible in the Powers list until your maximum power is equal to, or greater than, the power's cost.", "Gained Powers");
                    }
                    else
                    {
                        map.addUnifiedMessage(map.locations[0], null, "Gained Access To Powers", "Changes in the tenets of an Orc Culture has resulted in you gaining access to the following powers:\n" + msg1 + msg2 + "\n A power will not be visible in the Powers list until your maximum power is equal to, or greater than, the power's cost.", "Gained Powers");
                    }
                }
            }
        }

        public override void onTurnEnd(Map map)
        {
            if (comLib == null)
            {
                return;
            }

            if (map.acceleratedTime != core.data.acceleratedTime)
            {
                core.data.acceleratedTime = map.acceleratedTime;
                core.data.brokenMakerSleeping = true;
                if (core.data.acceleratedTime)
                {
                    onBrokenMakerSleep_StartOfSleep(map);
                }
            }

            if (core.data.brokenMakerSleeping)
            {
                core.data.sleepDuration--;
                onBrokenMakerSleep_TurnTick(map);

                if (core.data.sleepDuration == 0)
                {
                    core.data.brokenMakerSleeping = false;
                    onBrokenMakerSleep_EndOfSleep(map);
                    core.data.sleepDuration = 50;
                }
            }

            core.data.onTurnEnd(map);
        }

        public void onBrokenMakerSleep_StartOfSleep(Map map)
        {
            manageAISleepRestrictions();

            List<HolyOrder_Orcs> mutableCultures = new List<HolyOrder_Orcs>();
            mutableCultures.AddRange(core.data.orcSGCultureMap.Values);
            foreach (HolyOrder_Orcs orcCulture in mutableCultures)
            {
                orcCulture.tenet_alignment.status = 0;
                orcCulture.tenet_intolerance.status = -1;

                if (orcCulture.tenet_god is H_Orcs_GlorySeeker glory)
                {
                    glory.cursed = false;
                }

                if (orcCulture.camps.Count + orcCulture.specializedCamps.Count > 8)
                {
                    orcCulture.triggerCivilWar();
                }
            }
        }

        public void onBrokenMakerSleep_TurnTick(Map map)
        {
            foreach (Unit unit in map.units)
            {
                if (unit is UM_RavenousDead || unit is UM_UntamedDead)
                {
                    unit.addMenace(3.0);
                }
                else if (unit is UAEN_OrcUpstart upstart)
                {
                    for (int i = 0; i < upstart.minions.Length; i++)
                    {
                        if (upstart.minions[i] != null && !(upstart.minions[i] is M_Goblin) && Eleven.random.Next(100) == 0)
                        {
                            upstart.minions[i] = new M_Goblin(map);
                        }
                    }
                }
            }

            Dictionary<SG_Orc, HolyOrder_Orcs> mutableSGCultureMap = new Dictionary<SG_Orc, HolyOrder_Orcs>();
            mutableSGCultureMap.AddRange(core.data.orcSGCultureMap);
            foreach (KeyValuePair<SG_Orc, HolyOrder_Orcs> pair in mutableSGCultureMap)
            {
                if (!pair.Key.isGone())
                {
                    manageCampDecay(map, pair.Key, pair.Value);
                }
            }
        }

        public void manageCampDecay(Map map, SG_Orc orcSociety, HolyOrder_Orcs orcCulture)
        {
            orcCulture.updateData();

            Dictionary<Location, int> stepDistances = new Dictionary<Location, int> { { map.locations[orcCulture.capital], 0 } };
            HashSet<Location> searchedLocations = new HashSet<Location> { map.locations[orcCulture.capital] };
            HashSet<Location> stepLocations = new HashSet<Location> { map.locations[orcCulture.capital] };

            for (int i = 1; i < 128; i++)
            {
                HashSet<Location> newStepLocations = new HashSet<Location>();
                foreach (Location loc in stepLocations)
                {
                    foreach (Location neighbour in loc.getNeighbours())
                    {
                        if (!searchedLocations.Contains(neighbour))
                        {
                            searchedLocations.Add(neighbour);

                            if (neighbour.soc == orcSociety || (neighbour.settlement != null && neighbour.settlement.subs.Any(sub => sub is Sub_OrcWaystation waystation && waystation.orcSociety == orcSociety)))
                            {
                                newStepLocations.Add(neighbour);
                                stepDistances.Add(neighbour, i);
                            }
                        }
                    }
                }

                stepLocations = newStepLocations;

                if (newStepLocations.Count == 0)
                {
                    break;
                }
            }

            bool contiguous = true;
            List<Set_OrcCamp> allCamps = new List<Set_OrcCamp>();
            allCamps.AddRange(orcCulture.camps);
            allCamps.AddRange(orcCulture.specializedCamps);
            if (allCamps.Any(c => !stepDistances.ContainsKey(c.location)))
            {
                contiguous = false;
            }

            List<Set_OrcCamp>[] specialisedCamps = new List<Set_OrcCamp>[6];
            int capitalSpecialism = 0;

            for (int i = 0; i < specialisedCamps.Length; i++)
            {
                specialisedCamps[i] = new List<Set_OrcCamp>();
            }

            foreach (Set_OrcCamp specialisedCamp in orcCulture.specializedCamps)
            {
                if (specialisedCamp == orcCulture.seat.settlement)
                {
                    capitalSpecialism = specialisedCamp.specialism;
                    continue;
                }

                specialisedCamps[specialisedCamp.specialism].Add(specialisedCamp);
            }

            for (int i = 0; i < specialisedCamps.Length; i++)
            {
                if (specialisedCamps[i].Count == 0)
                {
                    continue;
                }

                int targetCount = 1;
                if (i == capitalSpecialism)
                {
                    targetCount--;
                }

                List<Set_OrcCamp> targetSpecialisedCamps = new List<Set_OrcCamp>();
                targetSpecialisedCamps.AddRange(specialisedCamps[i]);
                while (targetCount > 0)
                {
                    targetSpecialisedCamps.RemoveAt(Eleven.random.Next(targetSpecialisedCamps.Count));
                    targetCount--;
                }

                foreach (Set_OrcCamp specialisedCamp in targetSpecialisedCamps)
                {
                    if (!stepDistances.ContainsKey(specialisedCamp.location))
                    {
                        continue;
                    }

                    if (Eleven.random.Next(100) < 0.5 * stepDistances[specialisedCamp.location])
                    {
                        specialisedCamps[specialisedCamp.specialism].Remove(specialisedCamp);
                        specialisedCamp.specialism = 0;
                        if (specialisedCamp.army != null)
                        {
                            specialisedCamp.army.die(map, "Desertion");
                            specialisedCamp.army = null;
                        }
                        Sub_Temple temple = (Sub_Temple)specialisedCamp.subs.FirstOrDefault(sub => sub is Sub_OrcTemple);
                        specialisedCamp.subs.Remove(temple);
                    }
                }
            }

            foreach (KeyValuePair<Location, int> pair in stepDistances)
            {
                Location loc = pair.Key;
                int dist = pair.Value;

                if (!loc.getNeighbours().Any(n => stepDistances.TryGetValue(n, out int i) && i > dist))
                {
                    if (Eleven.random.Next(100) < 0.5 * stepDistances[loc])
                    {
                        if (loc.settlement is Set_OrcCamp camp && camp.specialism == 0)
                        {
                            camp.fallIntoRuin("Decayed over time");
                        }
                        
                        if (loc.settlement != null)
                        {
                            Sub_OrcWaystation waystation = (Sub_OrcWaystation)loc.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation way && way.orcSociety == orcSociety);
                            if (waystation != null)
                            {
                                loc.settlement.subs.Remove(waystation);
                            }
                        }

                        if (loc.soc == orcSociety && !(loc.settlement is Set_OrcCamp))
                        {
                            Sub_OrcWaystation waystation = (Sub_OrcWaystation)loc.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation way && way.orcSociety != orcSociety);
                            if (waystation != null)
                            {
                                loc.soc = waystation.orcSociety;
                            }
                            else
                            {
                                loc.soc = null;
                            }
                        }
                    }
                }
            }

            if (!contiguous)
            {
                orcCulture.triggerCivilWar();
            }
        }

        public void onBrokenMakerSleep_EndOfSleep(Map map)
        {
            manageAISleepRestrictions(true);
        }

        public void manageAISleepRestrictions(bool unlock = false)
        {
            AIChallenge secretsOfDeath = core.comLibAI.GetAIChallengeFromAgentType(typeof(UAEN_OrcShaman), typeof(Ch_SecretsOfDeath));
            forbidAIChallenge(secretsOfDeath, unlock);

            AIChallenge learnArcaneSecret = core.comLibAI.GetAIChallengeFromAgentType(typeof(UAEN_OrcShaman), typeof(Ch_LearnSecret));
            forbidAIChallenge(learnArcaneSecret, unlock);

            AIChallenge enslaveDead = core.comLibAI.GetAIChallengeFromAgentType(typeof(UAEN_OrcShaman), typeof(Mg_EnslaveTheDead));
            forbidAIChallenge(enslaveDead, unlock);

            AIChallenge ravenousDead = core.comLibAI.GetAIChallengeFromAgentType(typeof(UAEN_OrcShaman), typeof(Mg_RavenousDead));
            forbidAIChallenge(ravenousDead, unlock);

            AIChallenge warFestival = core.comLibAI.GetAIChallengeFromAgentType(typeof(UAEN_OrcShaman), typeof(Ch_Orcs_WarFestival));
            forbidAIChallenge(warFestival, unlock);

            AIChallenge deathFestival = core.comLibAI.GetAIChallengeFromAgentType(typeof(UAEN_OrcShaman), typeof(Ch_Orcs_DeathFestival));
            forbidAIChallenge(deathFestival, unlock);
        }

        public void forbidAIChallenge(AIChallenge aiChallenge, bool unlock = false)
        {
            if (aiChallenge != null)
            {
                if (!unlock)
                {
                    if (aiChallenge.tags.Contains(AIChallenge.ChallengeTags.Forbidden))
                    {
                        core.data.forbiddenChallenges.Add(aiChallenge);
                    }
                    else
                    {
                        aiChallenge.tags.Add(AIChallenge.ChallengeTags.Forbidden);
                    }
                }
                else if (!core.data.forbiddenChallenges.Contains(aiChallenge))
                {
                    aiChallenge.tags.Remove(AIChallenge.ChallengeTags.Forbidden);
                }
            }
        }

        public override float hexHabitability(Hex hex, float hab)
        {
            if (hex.location != null)
            {
                if (hex.settlement is Set_OrcCamp && hex.location.properties.Any(pr => pr is Pr_Vinerva_LifeBoon))
                {
                    //Console.WriteLine("OrcsPlus: Adding effect of vinerva life boon");
                    hab += (float)(hex.map.opt_orcHabMult * hex.map.param.orc_habRequirement);
                }

                if (core.data.orcGeoMageHabitabilityBonus.TryGetValue(hex.location.index, out float geoMageHab))
                {
                    Console.WriteLine("OrcsPlus: Got geo mage data for " + hex.location.getName() + " with modifier value of " + geoMageHab);
                    hab += geoMageHab;
                }
            }

            return hab;
        }

        public override void onCheatEntered(string command)
        {
            Location selectedLocation = GraphicalMap.selectedHex?.location;
            if (command == "civilWar" && selectedLocation?.soc is SG_Orc orcSociety && core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                orcCulture.triggerCivilWar();
            }
        }

        public override void onChallengeComplete(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            OnChallengeComplete.processChallenge(challenge, ua, task_PerformChallenge);
        }

        public override double sovereignAI(Map map, AN actionNational, Person ruler, List<ReasonMsg> reasons, double initialUtility)
        {
            if (actionNational is AN_WarOnThreat threatWar && threatWar.target is SG_Orc orcSociety)
            {
                if (reasons != null)
                {
                    ReasonMsg distanceReason = reasons.FirstOrDefault(r => r.msg == "Distance between");
                    if (distanceReason != null)
                    {
                        initialUtility += distanceReason.value;
                        distanceReason.value *= 2;
                    }

                    if (map.locations[ruler.rulerOf].soc is Society society && (society.isDarkEmpire || society.isOphanimControlled))
                    {
                        if (core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                        {
                            if (orcCulture.tenet_alignment.status < 1 && orcCulture.tenet_intolerance.status < 0)
                            {
                                initialUtility -= 1000;
                                reasons.Add(new ReasonMsg("Mutual interests", -1000));
                            }
                        }
                    }
                }
                else
                {
                    double val = map.getStepDist(ruler.society, threatWar.target) - 1;
                    if (val > 0)
                    {
                        val = Math.Max(val * map.param.utility_soc_warDistancePenaltyPerStep, map.param.utility_soc_warDistancePenaltyCap);
                        initialUtility += val;
                    }

                    if (map.locations[ruler.rulerOf].soc is Society society && (society.isDarkEmpire || society.isOphanimControlled))
                    {
                        if (core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                        {
                            if (orcCulture.tenet_alignment.status < 1 && orcCulture.tenet_intolerance.status < 0)
                            {
                                initialUtility -= 1000;
                            }
                        }
                    }
                }
            }

            if (actionNational is AN_DeclareWar war && war.target is SG_Orc orcSociety2)
            {
                if (reasons != null)
                {
                    ReasonMsg distanceReason = reasons.FirstOrDefault(r => r.msg == "Distance between");
                    if (distanceReason != null)
                    {
                        initialUtility += distanceReason.value;
                        distanceReason.value *= 2;
                    }

                    if (map.locations[ruler.rulerOf].soc is Society society && (society.isDarkEmpire || society.isOphanimControlled))
                    {
                        if (core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                        {
                            if (orcCulture.tenet_alignment.status < 1 && orcCulture.tenet_intolerance.status < 0)
                            {
                                initialUtility -= 1000;
                                reasons.Add(new ReasonMsg("Mutual interests", -1000));
                            }
                        }
                    }
                }
                else
                {
                    double val = map.getStepDist(ruler.society, war.target) - 1;
                    if (val > 0)
                    {
                        val = Math.Max(val * map.param.utility_soc_warDistancePenaltyPerStep, map.param.utility_soc_warDistancePenaltyCap);
                        initialUtility += val;
                    }

                    if (map.locations[ruler.rulerOf].soc is Society society && (society.isDarkEmpire || society.isOphanimControlled))
                    {
                        if (core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                        {
                            if (orcCulture.tenet_alignment.status < 1 && orcCulture.tenet_intolerance.status < 0)
                            {
                                initialUtility -= 1000;
                            }
                        }
                    }
                }
            }

            return initialUtility;
        }

        public override int adjustHolyInfluenceDark(HolyOrder order, int inf, List<ReasonMsg> msgs)
        {
            HolyOrder_Orcs orcCulture = order as HolyOrder_Orcs;
            int result = 0;

            List<ReasonMsg> influenceGain;
            if (orcCulture == null || !core.data.influenceGainElder.TryGetValue(orcCulture, out influenceGain))
            {
                return result;
            }

            if (orcCulture.isGone())
            {
                core.data.influenceGainElder.Remove(orcCulture);
                return result;
            }

            foreach (ReasonMsg msg in influenceGain)
            { 
                msgs?.Add(msg);
                result += (int)msg.value;
            }

            return result;
        }

        public override int adjustHolyInfluenceGood(HolyOrder order, int inf, List<ReasonMsg> msgs)
        {
            int result = 0;
            if (!(order is HolyOrder_Orcs orcCulture) || !core.data.influenceGainHuman.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain) || influenceGain?.Count == 0)
            {
                return result;
            }

            if (orcCulture.isGone())
            {
                core.data.influenceGainHuman.Remove(orcCulture);
                return result;
            }

            foreach (ReasonMsg msg in influenceGain)
            {
                msgs?.Add(msg);
                result += (int)msg.value;
            }

            return result;
        }

        public bool TryAddInfluenceGain(HolyOrder_Orcs orcCulture, ReasonMsg msg, bool isElder = false)
        {
            if (orcCulture?.isGone() ?? true || msg?.value == 0)
            {
                return false;
            }

            if (isElder)
            {
                AddInfluenceGainElder(orcCulture, msg);

                if (core.data.isPlayerTurn)
                {
                    orcCulture.influenceElder += (int)Math.Floor(msg.value);

                    if (orcCulture.influenceElder > orcCulture.influenceElderReq)
                    {
                        orcCulture.influenceElder = orcCulture.influenceElderReq;
                    }
                    else if (orcCulture.influenceElder < 0)
                    {
                        orcCulture.influenceElder = 0;
                    }
                }

                return true;
            }

            AddInfluenceGainHuman(orcCulture, msg);

            if (core.data.isPlayerTurn)
            {
                orcCulture.influenceHuman += (int)Math.Floor(msg.value);

                if (orcCulture.influenceHuman > orcCulture.influenceHumanReq)
                {
                    orcCulture.influenceHuman = orcCulture.influenceHumanReq;
                }
                else if (orcCulture.influenceHuman < 0)
                {
                    orcCulture.influenceHuman = 0;
                }
            }

            return true;
        }

        public bool TryAddInfluenceGain(SG_Orc orcSociety, ReasonMsg msg, bool isElder = false)
        {
            if (orcSociety?.isGone() ?? true || msg?.value == 0)
            {
                return false;
            }

            if (core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (isElder)
                {
                    AddInfluenceGainElder(orcCulture, msg);

                    if (core.data.isPlayerTurn)
                    {
                        orcCulture.influenceElder += (int)Math.Floor(msg.value);
                    }

                    return true;
                }

                AddInfluenceGainHuman(orcCulture, msg);

                if (core.data.isPlayerTurn)
                {
                    orcCulture.influenceHuman += (int)Math.Floor(msg.value);
                }

                return true;
            }

            return false;
        }

        private void AddInfluenceGainElder(HolyOrder_Orcs orcCulture, ReasonMsg msg)
        {
            if (!core.data.influenceGainElder.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain) || influenceGain == null)
            {
                influenceGain = new List<ReasonMsg> ();
                core.data.influenceGainElder.Add(orcCulture, influenceGain);
            }

            bool flag = false;

            foreach (ReasonMsg gainMsg in influenceGain)
            {
                if (gainMsg.msg == msg.msg)
                {
                    gainMsg.value += msg.value;
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                influenceGain.Add(msg);
            }
        }

        private void AddInfluenceGainHuman(HolyOrder_Orcs orcCulture, ReasonMsg msg)
        {
            if (!core.data.influenceGainHuman.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain))
            {
                influenceGain = new List<ReasonMsg> ();
                core.data.influenceGainHuman.Add(orcCulture, influenceGain);
            }

            bool flag = false;

            foreach (ReasonMsg gainMsg in influenceGain)
            {
                if (gainMsg.msg == msg.msg)
                {
                    gainMsg.value += msg.value;
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                influenceGain.Add(msg);
            }
        }

        public override void onPeaceDeclared(SocialGroup att, SocialGroup def)
        {
            SG_Orc attOrcSociety = att as SG_Orc;
            SG_Orc defOrcSociety = def as SG_Orc;

            if (attOrcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(attOrcSociety, out HolyOrder_Orcs attOrcCulture) && attOrcCulture != null)
            {
                att.map.declarePeace(attOrcCulture, def);
            }

            if (defOrcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(defOrcSociety, out HolyOrder_Orcs defOrcCulture) && defOrcCulture != null)
            {
                att.map.declarePeace(att, defOrcCulture);
            }
        }

        public override void onWarDeclared(SocialGroup attacker, SocialGroup target, List<ReasonMsg> reasons, War war)
        {
            SG_Orc attOrcSociety = attacker as SG_Orc;
            SG_Orc defOrcSociety = target as SG_Orc;

            HolyOrder_Orcs attOrcCulture = null;
            HolyOrder_Orcs defOrcCulture = null;

            if (attOrcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(attOrcSociety, out attOrcCulture) && attOrcCulture != null)
            {
                attacker.map.declareWar(attOrcCulture, target, true, reasons, war.attackerObjective);
            }

            if (defOrcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(defOrcSociety, out defOrcCulture) && defOrcCulture != null)
            {
                attacker.map.declareWar(attacker, defOrcCulture, true, reasons, war.attackerObjective);
            }
        }

        public override int onAgentAttackAboutToBePerformed(AgentCombatInterface attacker, UA me, UA them, PopupBattleAgent battle, int dmg, int row)
        {
            return base.onAgentAttackAboutToBePerformed(attacker, me, them, battle, dmg, row);
        }

        public override void onAgentBattleTerminate(BattleAgents battleAgents)
        {
            if (core.data.godTenetTypes.TryGetValue(battleAgents.att.map.overmind.god.GetType(), out Type tenetType) && tenetType != null && tenetType == typeof(H_Orcs_HarbingersMadness))
            {
                UA att = battleAgents.att;
                UA def = battleAgents.def;

                if (att.society is SG_Orc orcSociety && core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_HarbingersMadness harbringers && harbringers.status < -1)
                {
                    if (!def.isDead && def.hp > 0 && !def.isCommandable() && (def.person.species is Species_Human || def.person.species is Species_Elf))
                    {
                        def.person.sanity -= 2;

                        if (def.person.sanity < 1.0)
                        {
                            def.person.goInsane(-1);
                        }
                    }
                }
                else if (att.society is HolyOrder_Orcs orcCulture2 && orcCulture2.tenet_god is H_Orcs_HarbingersMadness harbringers2 && harbringers2.status < -1)
                {
                    if (!def.isDead && def.hp > 0 && !def.isCommandable() && (def.person.species is Species_Human || def.person.species is Species_Elf))
                    {
                        def.person.sanity -= 2;

                        if (def.person.sanity < 1.0)
                        {
                            def.person.goInsane(-1);
                        }
                    }
                }

                if (def.society is SG_Orc orcSociety3 && core.data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null && orcCulture3.tenet_god is H_Orcs_HarbingersMadness harbringers3 && harbringers3.status < -1)
                {
                    if (!att.isDead && att.hp > 0 && !att.isCommandable() && (att.person.species is Species_Human || att.person.species is Species_Elf))
                    {
                        att.person.sanity -= 2;

                        if (att.person.sanity < 1.0)
                        {
                            att.person.goInsane(-1);
                        }
                    }
                }
                else if (def.society is HolyOrder_Orcs orcCulture4 && orcCulture4.tenet_god is H_Orcs_HarbingersMadness harbringers4 && harbringers4.status < -1)
                {
                    if (!att.isDead && att.hp > 0 && !def.isCommandable() && (att.person.species is Species_Human || att.person.species is Species_Elf))
                    {
                        att.person.sanity -= 2;

                        if (att.person.sanity < 1.0)
                        {
                            att.person.goInsane(-1);
                        }
                    }
                }
            }
        }

        /*public override bool interceptDeath(Person person, string v, object killer)
        {
            if (person.items.Any(i => i is I_BloodGourd))
            {
                int index = -1;
                for (int i = 0; i < person.items.Length; i++)
                {
                    if (person.items[i] is I_BloodGourd)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    person.items[index] = null;
                    person.map.addUnifiedMessage();
                }
            }

            return false;
        }*/

        public override void onPersonDeath_StartOfProcess(Person person, string v, object killer)
        {
            if (comLib == null)
            {
                return;
            }

            if (person == null)
            {
                return;
            }

            // Person Data
            Unit uPerson = person.unit;

            if (uPerson == null || uPerson.location == null || uPerson.homeLocation == -1)
            {
                return;
            }

            //Console.WriteLine("Orcs_Plus: Person with Unit has died");

            onPersonDeath_InfluenceGain(person, v, killer);

            UA uaPerson = uPerson as UA;
            SG_Orc orcSociety = uPerson.society as SG_Orc;
            HolyOrder_Orcs orcCulture = uPerson.society as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                core.data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }
            else if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
            }

            //Console.WriteLine("Orcs_Plus: Person with Agent has died");

            // Cordyceps Symbiosis
            if (orcCulture != null && orcCulture.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < -1)
            {
                //Console.WriteLine("Orcs_Plus: Orc agent is subject to Insectile Symbiosis");
                if (core.data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null && intDataCord.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null && intDataCord.typeDict.TryGetValue("Swarm", out Type swarmType) && swarmType != null && intDataCord.typeDict.TryGetValue("Hive", out Type hiveType) && hiveType != null)
                {
                    if (uaPerson.map.locations.Any(l => l.settlement != null && (l.settlement.GetType() == hiveType || l.settlement.GetType().IsSubclassOf(hiveType))))
                    {
                        //Console.WriteLine("Orcs_Plus: Spawning Drone");
                        Location loc = uaPerson.location;
                        SocialGroup soc = uaPerson.map.soc_dark;
                        if (loc.soc.GetType() == swarmType || loc.soc.GetType().IsSubclassOf(swarmType))
                        {
                            soc = loc.soc;
                        }

                        object[] args = new object[] {
                        loc,
                        soc,
                        Person_Nonunique.getNonuniquePerson(uaPerson.map.soc_dark)
                        };

                        UAEN drone = (UAEN)Activator.CreateInstance(droneType, args);
                        uaPerson.map.units.Add(drone);
                        loc.units.Add(drone);
                    }
                }
            }

            if (killer != null)
            {
                // Killer Data
                Person pKiller = killer as Person;
                Unit uKiller = killer as Unit;
                UA uaKiller = killer as UA;

                if (pKiller != null)
                {
                    //Console.WriteLine("Orcs_Plus: Person was killed by another person.");
                    uKiller = pKiller.unit;
                    uaKiller = pKiller.unit as UA;
                }
                else if (uKiller != null)
                {
                    pKiller = uKiller.person;
                }

                
                if (pKiller != null)
                {
                    // Blood Fued
                    //Console.WriteLine("Orcs_Plus: Killer has Unit");
                    if (uaPerson is UAEN_OrcElder && !pKiller.traits.Any(t => t is T_BloodFeud fued && fued.orcSociety == orcSociety))
                    {
                        //Console.WriteLine("Orcs_Plus: Killer gained blood fued by killing orc elder");
                        pKiller.receiveTrait(new T_BloodFeud(orcSociety));

                        person.map.addUnifiedMessage(uaPerson, pKiller.unit ?? pKiller as object, "Blood Feud", pKiller.getName() + " has become the target of a blood fued by killing an orc elder of the " + uaPerson.society.getName() + ". They must now spend the rest of their days looking over their shoulder for the sudden the appearance of an avenging orc upstart.", "Orc Blood Feud");
                    }

                    if (pKiller.items.Any(i => i is I_IdolOfMadness))
                    {
                        foreach (KeyValuePair<int, RelObj> pair in person.relations)
                        {
                            Person them = person.map.persons[pair.Key];
                            if (them != null && !them.isDead)
                            {
                                if (pair.Value.isCloseFamily())
                                {
                                    them.sanity -= 5;

                                    if (them.sanity <= 0)
                                    {
                                        them.goInsane();
                                    }
                                }
                            }
                        }
                    }

                    // Blood Stains
                    if (orcCulture != null && (uKiller == null || !uKiller.isCommandable()))
                    {
                        if (orcCulture.tenet_god is H_Orcs_BloodOffering blood && blood.status < 0)
                        {
                            if (core.data.tryGetModAssembly("Kishi", out ModData.ModIntegrationData intDataKishi) && intDataKishi.assembly != null && intDataKishi.methodInfoDict.TryGetValue("addBloodstain", out MethodInfo MI_AddBloodstain) && MI_AddBloodstain != null)
                            {
                                object[] parameters = new object[] { pKiller, Math.Abs(blood.status) };
                                MI_AddBloodstain.Invoke(null, parameters);
                            }
                        }
                    }
                    else if (uKiller != null && !uKiller.isCommandable())
                    {
                        SG_Orc killerOrcSociety = uKiller.society as SG_Orc;
                        HolyOrder_Orcs killerOrcCulture = uKiller.society as HolyOrder_Orcs;

                        if (killerOrcSociety != null)
                        {
                            core.data.orcSGCultureMap.TryGetValue(killerOrcSociety, out killerOrcCulture);
                        }
                        else if (killerOrcCulture != null)
                        {
                            killerOrcSociety = killerOrcCulture.orcSociety;
                        }

                        if (killerOrcCulture != null && killerOrcCulture.tenet_god is H_Orcs_BloodOffering blood && blood.status < 0 && person.hasSoul)
                        {
                            int gainAmount = Math.Abs(blood.status) - 1;
                            if (gainAmount > 0 && core.data.tryGetModAssembly("Kishi", out ModData.ModIntegrationData intDataKishi) && intDataKishi.assembly != null && intDataKishi.methodInfoDict.TryGetValue("addBloodstain", out MethodInfo MI_AddBloodstain) && MI_AddBloodstain != null)
                            {
                                object[] parameters = new object[] { pKiller, gainAmount };
                                MI_AddBloodstain.Invoke(null, parameters);
                            }
                        }
                    }
                }
            }
        }

        public void onPersonDeath_InfluenceGain(Person person, string v, object killer)
        {
            Unit uPerson = person.unit;
            UA uaPerson = person.unit as UA;

            Person pKiller = killer as Person;
            Unit uKiller = killer as Unit;
            UA uaKiller = killer as UA;

            if (pKiller != null)
            {
                uKiller = pKiller.unit;
                uaKiller = pKiller.unit as UA;
            }

            //Console.WriteLine("Orcs_Plus: Person with unit has died");
            bool isNature = false;
            bool isVampire = false;

            SG_Orc orcSociety = null;
            HolyOrder_Orcs orcCulture = null;

            HashSet<SG_Orc> influencedOrcSocietyHashSet = new HashSet<SG_Orc>();
            HolyOrder_Orcs influencedOrcCulture_Direct = null;
            List<HolyOrder_Orcs> influencedOrcCultures_Warring = new List<HolyOrder_Orcs>();
            List<HolyOrder_Orcs> influencedOrcCultures_Regional = new List<HolyOrder_Orcs>();

            if (uPerson != null)
            {
                if (core.data.tryGetModAssembly("LivingWilds", out ModData.ModIntegrationData intDataLW) && intDataLW.assembly != null && intDataLW.typeDict.TryGetValue("NatureCritter", out Type natureType) && natureType != null)
                {
                    if (uPerson.GetType() == natureType || uPerson.GetType().IsSubclassOf(natureType))
                    {
                        //Console.WriteLine("Orcs_Plus: Person is nature critter");
                        isNature = true;
                    }
                }

                if (uaPerson != null)
                {
                    isVampire = checkIsVampire(uaPerson);
                    /*if (isVampire)
                    {
                        Console.WriteLine("Orcs_Plus: Person isVampire");
                    }*/
                }

                orcSociety = uPerson.society as SG_Orc;
                orcCulture = uPerson.society as HolyOrder_Orcs;

                if (orcSociety != null)
                {
                    core.data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
                }
                else if (orcCulture != null)
                {
                    orcSociety = orcCulture.orcSociety;
                }

                if (orcCulture != null)
                {
                    //Console.WriteLine("Orcs_Plus: Person is member of orcSociety");
                    influencedOrcCulture_Direct = orcCulture;
                    influencedOrcSocietyHashSet.Add(orcCulture.orcSociety);
                }

                if (uPerson.society != null)
                {
                    foreach (SocialGroup sg in person.map.socialGroups)
                    {
                        if (sg is SG_Orc orcSociety2 && sg.getRel(uPerson.society).state == DipRel.dipState.war && !influencedOrcSocietyHashSet.Contains(orcSociety2) && core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
                        {
                            //Console.WriteLine("Orcs_Plus: Person is at war with the " + sg.getName());
                            influencedOrcCultures_Warring.Add(orcCulture2);
                            influencedOrcSocietyHashSet.Add(orcSociety2);
                        }
                    }
                }

                if (uPerson.location.soc is SG_Orc orcSociety3 && !influencedOrcSocietyHashSet.Contains(orcSociety3) && core.data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null)
                {
                    //Console.WriteLine("Orcs_Plus: Person is trespassing in the lands of the " + orcSociety3.getName());
                    influencedOrcCultures_Regional.Add(orcCulture3);
                    influencedOrcSocietyHashSet.Add(orcSociety3);
                }
            }
            else
            {
                if (person.society != null)
                {
                    foreach (SocialGroup sg in person.map.socialGroups)
                    {
                        if (sg is SG_Orc orcSociety2 && sg.getRel(person.society).state == DipRel.dipState.war && !influencedOrcSocietyHashSet.Contains(orcSociety2) && core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
                        {
                            //Console.WriteLine("Orcs_Plus: Person is at war with the " + sg.getName());
                            influencedOrcCultures_Warring.Add(orcCulture2);
                            influencedOrcSocietyHashSet.Add(orcSociety2);
                        }
                    }
                }

                if (person.rulerOf != -1)
                {
                    List<Location> neighbours = person.map.locations[person.rulerOf].getNeighbours().FindAll(n => n.soc is SG_Orc);

                    foreach (Location neighbour in neighbours)
                    {
                        if (neighbour.soc is SG_Orc orcSociety3 && !influencedOrcSocietyHashSet.Contains(orcSociety3) && core.data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null)
                        {
                            //Console.WriteLine("Orcs_Plus: Person is trespassing in the lands of the " + orcSociety3.getName());
                            influencedOrcCultures_Regional.Add(orcCulture3);
                            influencedOrcSocietyHashSet.Add(orcSociety3);
                        }
                    }
                }
            }

            if (uKiller != null && v == "Killed in battle with " + uKiller.getName())
            {
                //Console.WriteLine("Orcs_Plus: Person was killed in battle with " + uKiller.getName());
                if (isVampire)
                {
                    if (uKiller.isCommandable())
                    {
                        if (influencedOrcCulture_Direct != null)
                        {
                            core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Slew orc vampire in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2), true);
                        }

                        foreach (HolyOrder_Orcs orcs in core.data.orcSGCultureMap.Values)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Slew vampire in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2), true);
                        }
                    }
                    else if (uKiller.society != null && !uKiller.society.isDark())
                    {
                        if (influencedOrcCulture_Direct != null)
                        {
                            core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Slew orc vampire in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2));
                        }

                        foreach (HolyOrder_Orcs orcs in core.data.orcSGCultureMap.Values)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Slew vampire in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2));
                        }
                    }
                }
                else if (!isNature)
                {
                    if (uKiller.isCommandable())
                    {
                        if (influencedOrcCulture_Direct != null)
                        {
                            core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Killed orc agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Killed enemy agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Killed trespassing agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }
                    }
                    else if (uKiller.society != null && !uKiller.society.isDark() && influencedOrcCulture_Direct != null)
                    {
                        core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Killed orc agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Killed enemy agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Killed trespassing agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }

                    if (uaPerson != null && uaPerson.society != null && !uaPerson.society.isDark() && (uKiller.society is SG_Orc || uKiller.society is HolyOrder_Orcs))
                    {
                        SG_Orc orcSociety2 = uKiller.society as SG_Orc;
                        HolyOrder_Orcs orcCulture2 = uKiller.society as HolyOrder_Orcs;

                        if (orcSociety2 != null)
                        {
                            core.data.orcSGCultureMap.TryGetValue(orcSociety2, out orcCulture2);
                        }

                        if (orcCulture2 != null && !influencedOrcSocietyHashSet.Contains(orcCulture2.orcSociety))
                        {
                            core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Orcs killed hero in battle", -1 * core.data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Orcs killed hero in battle", -1 * core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Orcs killed hero in battle", -1 * core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
            }
            else if (uKiller is UM && v == "Killed by " + uKiller.getName())
            {
                if (isVampire)
                {
                    if (uKiller.isCommandable())
                    {
                        if (influencedOrcCulture_Direct != null)
                        {
                            core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Intercepted and slew orc vampire", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2), true);
                        }

                        foreach (HolyOrder_Orcs orcs in core.data.orcSGCultureMap.Values)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Intercepted and slew vampire", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2), true);
                        }
                    }
                    else if (uKiller.society != null && !uKiller.society.isDark())
                    {
                        if (influencedOrcCulture_Direct != null)
                        {
                            core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Intercepted and slew orc vampire", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2));
                        }

                        foreach (HolyOrder_Orcs orcs in core.data.orcSGCultureMap.Values)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Intercepted and slew vampire", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2));
                        }
                    }
                }
                else if (!isNature)
                {
                    if (uKiller.isCommandable())
                    {
                        if (influencedOrcCulture_Direct != null)
                        {
                            core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Intercepted and killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Intercepted and killed enemy agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Intercepted and killed trespassing agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }
                    }
                    else if (uKiller.society != null && !uKiller.society.isDark() && influencedOrcCulture_Direct != null)
                    {
                        core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Intercepted and killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Intercepted and killed enemy agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Intercepted and killed trespassing agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                    
                    if (uaPerson != null && uaPerson.society != null && !uaPerson.society.isDark() && (uKiller.society is SG_Orc || uKiller.society is HolyOrder_Orcs))
                    {
                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Orcs killed hero in battle", -core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            core.TryAddInfluenceGain(orcs, new ReasonMsg("Orcs killed hero in battle", -core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
            }
            else if (v == "Killed by a volcano")
            {
                if (uKiller != null)
                {
                    if (isVampire)
                    {
                        if (uKiller.isCommandable())
                        {
                            if (influencedOrcCulture_Direct != null)
                            {
                                core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to slay orc vampire (volcanic eruption)", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2), true);
                            }

                            foreach (HolyOrder_Orcs orcs in core.data.orcSGCultureMap.Values)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to slay vampire (volcanic eruption)", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2), true);
                            }
                        }
                        else if (uKiller.society != null && !uKiller.society.isDark())
                        {
                            if (influencedOrcCulture_Direct != null)
                            {
                                core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to slay orc vampire (volcanic eruption)", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2));
                            }

                            foreach (HolyOrder_Orcs orcs in core.data.orcSGCultureMap.Values)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to slay vampire (volcanic eruption)", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2));
                            }
                        }
                    }
                    else if (!isNature)
                    {
                        if (uKiller.isCommandable())
                        {
                            if (influencedOrcCulture_Direct != null)
                            {
                                core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to kill orc agent (volcanic eruption)", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                            }

                            foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to kill enemy agent (volcanic eruption)", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                            }

                            foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to kill trespassing agent (volcanic eruption)", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                            }
                        }
                        else if (uKiller.society != null && !uKiller.society.isDark() && influencedOrcCulture_Direct != null)
                        {
                            core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to kill orc agent (volcanic eruption)", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));

                            foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to kill enemy agent (volcanic eruption)", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                            }

                            foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to kill trespassing agent (volcanic eruption)", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                            }
                        }
                    }
                }
                else
                {
                    if (influencedOrcCulture_Direct != null)
                    {
                        core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Awakening killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        core.TryAddInfluenceGain(orcs, new ReasonMsg("Awakening killed enemy agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        core.TryAddInfluenceGain(orcs, new ReasonMsg("Awakening killed trespassing agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                    }
                }
            }
            else if (v == "Killed by Ophanim's Smite")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Smote orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    core.TryAddInfluenceGain(orcs, new ReasonMsg("Smote enemy agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    core.TryAddInfluenceGain(orcs, new ReasonMsg("Smote trespassing agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }
            }
            else if (v == "Killed by a card")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Orc agent died in Death's games", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    core.TryAddInfluenceGain(orcs, new ReasonMsg("Enemy agent died in death's games", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    core.TryAddInfluenceGain(orcs, new ReasonMsg("Trespassing agent died in death's games", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }
            }
            else if (v == "Warped Into a Demon")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Orc agent warped into a demon", core.data.influenceGain[ModData.influenceGainAction.RecieveGift]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    core.TryAddInfluenceGain(orcs, new ReasonMsg("Enemy agent warped into a demon", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    core.TryAddInfluenceGain(orcs, new ReasonMsg("Trespassing agent warped into a demon", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }
            }
            else if (uaPerson != null && uaPerson.task is Task_PerformChallenge challengeTask)
            {
                Ch_SkirmishAttacking skirmishAtt = challengeTask.challenge as Ch_SkirmishAttacking;
                Ch_SkirmishDefending skirmishDef = challengeTask.challenge as Ch_SkirmishDefending;

                if (skirmishAtt != null || skirmishDef != null)
                {
                    List<UM> enemies;
                    List<UA> enemyComs;

                    if (skirmishAtt != null)
                    {
                        enemies = skirmishAtt.battle.defenders;
                        enemyComs = skirmishAtt.battle.defComs;
                    }
                    else
                    {
                        enemies = skirmishDef.battle.attackers;
                        enemyComs = skirmishDef.battle.attComs;
                    }

                    bool influenceElder = false;
                    bool influenceHuman = false;

                    foreach (UM enemy in enemies)
                    {
                        if (enemy.isCommandable())
                        {
                            influenceElder = true;
                        }
                        else if (enemy.society != null && !enemy.society.isDark())
                        {
                            influenceHuman = true;
                        }
                    }

                    foreach (UA com in enemyComs)
                    {
                        if (com.isCommandable())
                        {
                            influenceElder = true;
                        }
                        else if (com.society != null && !com.society.isDark())
                        {
                            influenceHuman = true;
                        }
                    }

                    if (influenceElder)
                    {
                        if (isVampire)
                        {
                            if (influencedOrcCulture_Direct != null)
                            {
                                core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Slew orc vampire skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2), true);
                            }

                            foreach (HolyOrder_Orcs orcs in core.data.orcSGCultureMap.Values)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Slew vampire skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2), true);
                            }
                        }
                        else if (!isNature)
                        {
                            if (influencedOrcCulture_Direct != null)
                            {
                                core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Killed orc agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                            }

                            foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Killed enemy agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                            }

                            foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Killed trespassong agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                            }
                        }
                    }

                    if (influenceHuman)
                    {
                        if (isVampire)
                        {
                            if (influencedOrcCulture_Direct != null)
                            {
                                core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Slew orc vampire skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2));
                            }

                            foreach (HolyOrder_Orcs orcs in core.data.orcSGCultureMap.Values)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Slew vampire skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2));
                            }
                        }
                        else if (!isNature && influencedOrcCulture_Direct != null)
                        {
                            core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Killed orc agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));

                            foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Killed enemy agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                            }

                            foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                            {
                                core.TryAddInfluenceGain(orcs, new ReasonMsg("Killed trespassing agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                            }
                        }
                    }
                }
            }
            else if (v== "killed by cheat" || v == "console" || v == "Killed by console")
            {
                if (isVampire)
                {
                    if (influencedOrcCulture_Direct != null)
                    {
                        core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Slew orc vampire (console command)", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2), true);
                    }

                    foreach (HolyOrder_Orcs orcs in core.data.orcSGCultureMap.Values)
                    {
                        core.TryAddInfluenceGain(orcs, new ReasonMsg("Slew vampire (console command)", core.data.influenceGain[ModData.influenceGainAction.AgentKill] * 2), true);
                    }
                }
                else if (!isNature)
                {
                    if (influencedOrcCulture_Direct != null)
                    {
                        core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Killed orc agent (console command)", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        core.TryAddInfluenceGain(orcs, new ReasonMsg("Killed enemy agent (console command)", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        core.TryAddInfluenceGain(orcs, new ReasonMsg("Killed trespassong agent (console command)", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                    }
                }
            }
        }

        public override void onUnitDies(Unit unit, string v)
        {
            if (unit is UM_OrcRaiders raiders && raiders.person != null && raiders.subsumedUnit != null && v != "Gone")
            {
                if (GraphicalMap.selectedUnit == raiders)
                {
                    GraphicalMap.selectedUnit = raiders.subsumedUnit;
                }

                Unit sub = raiders.subsumedUnit;
                unit.map.units.Add(sub);
                sub.isDead = false;
                sub.location = raiders.location;
                raiders.location.units.Add(sub);
                sub.person = raiders.person;
                sub.addMenace(raiders.menace);
                sub.person.unit = sub;
                sub.hp = 1;
            }
        }

        public override void onGraphicalHexUpdated(GraphicalHex graphicalHex)
        {
            if (graphicalHex.map.masker.mask == MapMaskManager.maskType.RELIGION)
            {
                Location location = graphicalHex.hex.location;
                if (location != null && location.settlement is Set_OrcCamp camp && location.soc is SG_Orc orcSociety)
                {
                    if (core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture))
                    {
                        HolyOrder targetOrder = graphicalHex.map.world.ui.uiScrollables.scrollable_threats.targetOrder;
                        if (targetOrder == null || targetOrder == orcCulture)
                        {
                            UnityEngine.Color colour = orcCulture.color;
                            if (colour.a > 0f && colour.r > 0f && colour.g > 0f && colour.b > 0f)
                            {
                                graphicalHex.terrainLayer.color = colour;
                                graphicalHex.locLayer.color = colour;
                                graphicalHex.mask.enabled = false;
                            }
                        }
                        else
                        {
                            graphicalHex.mask.color = new Color(0f, 0f, 0f, 0.75f);
                        }
                    }
                }
            }
        }

        public bool checkIsVampire(UA ua)
        {
            if (ua is UAE_Baroness)
            {
                return true;
            }

            if (ua is UAEN_Vampire)
            {
                return true;
            }

            if (core.data.tryGetModAssembly("LivingCharacters", out ModData.ModIntegrationData intDataLC) && intDataLC.assembly != null && intDataLC.typeDict.TryGetValue("Vampire", out Type vampireNobleType) && vampireNobleType != null)
            {
                if (ua.GetType() == vampireNobleType || ua.GetType().IsSubclassOf(vampireNobleType))
                {
                    return true;
                }
            }

            return false;
        }

        public bool checkAlignment(SG_Orc orcSociety, Location loc)
        {
            if (orcSociety == null)
            {
                return false;
            }

            bool result = true;
            if (orcSociety != null && core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && loc.soc != null)
            {
                if (orcCulture.tenet_intolerance.status == -2)
                {
                    if (loc.soc.isDark() || (loc.soc is Society society && (society.isDarkEmpire || society.isOphanimControlled)))
                    {
                        result = false;
                    }
                }
                else if (orcCulture.tenet_intolerance.status == 2)
                {
                    if (!loc.soc.isDark() || (loc.soc is Society society && (!society.isDarkEmpire || !society.isOphanimControlled)))
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        public Item getOrcItem(Map map, SG_Orc orcSociety, Location location)
        {
            Pr_OrcishIndustry industry = (Pr_OrcishIndustry)location.properties.FirstOrDefault(pr => pr is Pr_OrcishIndustry);
            double industryValue = 0;
            if (industry != null)
            {
                industryValue = industry.charge / 100;
            }

            double roll = Eleven.random.NextDouble();
            double result = industryValue - roll;
            if (result > 0.8)
            {
                return Items.getOrcItemFromPool3(map, location.soc as SG_Orc, location);
            }
            else if (result > 0.5)
            {
                return Items.getOrcItemFromPool2(map, location.soc as SG_Orc, location);
            }
            else
            {
                roll = Math.Min(Eleven.random.Next(10), Eleven.random.Next(10));
                if (roll > 7)
                {
                    return Items.getOrcItemFromPool3(map, location.soc as SG_Orc, location);
                }
                else if (roll > 5)
                {
                    return Items.getOrcItemFromPool2(map, location.soc as SG_Orc, location);
                }
                else
                {
                    return Item.getItemFromPool1(map, -1);
                }
            }
        }

        public bool checkAlignment(SG_Orc orcSociety, SocialGroup sg)
        {
            if (orcSociety == null)
            {
                return false;
            }

            bool result = true;
            if (orcSociety != null && core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (orcCulture.tenet_intolerance.status == -2)
                {
                    if (sg.isDark() || (sg is Society society && (society.isDarkEmpire || society.isOphanimControlled)))
                    {
                        result = false;
                    }
                }
                else if (orcCulture.tenet_intolerance.status == 2)
                {
                    if (!sg.isDark() || (sg is Society society && (!society.isDarkEmpire || !society.isOphanimControlled)))
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        public bool registerGodTenet(Type godType, Type tenetType)
        {
            if (godType.IsSubclassOf(typeof(God)) && tenetType.IsSubclassOf(typeof(HolyTenet)))
            {
                if (!core.data.godTenetTypes.ContainsKey(godType))
                {
                    core.data.godTenetTypes.Add(godType, tenetType);
                    return true;
                }

                if (core.data.godTenetTypes[godType] == null)
                {
                    core.data.godTenetTypes[godType] = tenetType;
                    return true;
                }
            }

            return false;
        }

        public bool registerSettlementTypeForOrcWaystation(Type setType)
        {
            if (!setType.IsSubclassOf(typeof(Settlement)))
            {
                return false;
            }

            if (core.data.tryAddSettlementTypeForWaystation(setType))
            {
                return true;
            }

            return false;
        }
    }
}
