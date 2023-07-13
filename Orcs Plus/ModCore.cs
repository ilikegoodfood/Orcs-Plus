using Assets.Code;
using Assets.Code.Modding;
using CommunityLib;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orcs_Plus
{
    public class ModCore : ModKernel
    {
        public static CommunityLib.ModCore comLib;

        public AgentAI comLibAI;

        private ComLibHooks comLibHooks;

        public static ModCore core;

        public ModData data;

        public List<Power> godPowers1 = new List<Power>();

        public List<Power> godPowers2 = new List<Power>();

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

            if (core.data.godTenetTypes.TryGetValue(map.overmind.god.GetType(), out Type tenetType) && tenetType != null)
            {
                switch (tenetType.Name)
                {
                    case nameof(H_Orcs_LifeMother):
                        godPowers1 = new List<Power>()
                        {
                            new P_Vinerva_Life(map),
                            new P_Vinerva_Health(map)
                        };

                        godPowers2 = new List<Power>()
                        {
                            new P_Vinerva_Thorns(map)
                        };
                        break;
                    case nameof(H_Orcs_Perfection):
                        godPowers2 = new List<Power>()
                        {
                            new P_Ophanim_PerfectHorde(map)
                        };
                        break;
                    default:
                        break;
                }
            }

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
                map.hintSystem.popCustomHint("Orc Cultures", "Orcs Plus introduces cultures to the orc hordes. These cultures can be found in the holy order UI, and use many of the same mechanics, such as tenets, acolytes, and temples, however you gain influence with them in a very different way to other holy orders. Many challenges and rituals that benefit orcs, or that can only be done in orc camps, provide influence upon completion. These challenges specify this in their tooltips. In addition to this, killing an orc agent, or being involved in an army battle in which an opposed orc military unit is destroyed, grants influence over that orc culture. Similarly, killing agents, or being involved in army battles in which military units are destroyed, grants influence over any orc cultures that are at war with those agents or military units. Razing an orc camp, or razing a location belonging to a society that an orc society is at war with also grants influence over the orc culture.");
            }
        }

        public override void afterLoading(Map map)
        {
            core = this;
            if (core.data == null)
            {
                core.data = new ModData();
                core.data.isPlayerTurn = true;
                updateOrcSGCultureMap(map);
            }

            if (comLib == null)
            {
                core.comLibHooks = new ComLibHooks(map);
            }

            getModKernels(map);
            HarmonyPatches_Conditional.PatchingInit();

            if (comLib == null)
            {
                throw new Exception("OrcsPlus: This mod REQUIRES the Community Library mod to be installed and enabled in order to operate.");
            }
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
                        comLib.RegisterHooks(comLibHooks);
                        core.comLibAI = comLib.GetAgentAI();
                        
                        new AgentAIs(map);
                        if (core.data.godTenetTypes.TryGetValue(map.overmind.god.GetType(), out Type tenetType) && tenetType != null)
                        {
                            switch (tenetType.Name)
                            {
                                case nameof(H_Orcs_HarbringersMadness):
                                    core.comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_MadnessFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                                    break;
                                case nameof(H_Orcs_LifeMother):
                                    core.comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_NurtureOrchard), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                                    core.comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_HarvestGourd), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                                    break;
                                case nameof(H_Orcs_Perfection):
                                    core.comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_PerfectionFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
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

                            Type doomedType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.UAEN_Doomed", false);
                            if (doomedType != null)
                            {
                                intDataCord.typeDict.Add("Doomed", doomedType);
                            }

                            Type droneType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Drone", false);
                            if (droneType != null)
                            {
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
                                intDataCord.typeDict.Add("Swarm", swarmType);
                            }
                        }
                        break;
                    case "CovenExpansion":
                        ModData.ModIntegrationData intDataCCC = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("CovensCursesCurios", intDataCCC);

                        if (core.data.tryGetModAssembly("CovensCursesCurios", out intDataCCC) && intDataCCC.assembly != null)
                        {
                            Type dominionBanner = intDataCCC.assembly.GetType("CovenExpansion.I_BarbDominion", false);
                            intDataCCC.typeDict.Add("Banner", dominionBanner);
                        }
                        break;
                    case "LivingWilds":
                        ModData.ModIntegrationData intDataLW = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("LivingWilds", intDataLW);

                        if (core.data.tryGetModAssembly("LivingWilds", out intDataLW))
                        {
                            Type natureCritter = intDataLW.assembly.GetType("LivingWilds.UAEN_Nature_Critter", false);
                            if (natureCritter != null)
                            {
                                intDataLW.typeDict.Add("NatureCritter", natureCritter);
                            }

                            Type natureSanctuary = intDataLW.assembly.GetType("LivingWilds.Set_Nature_NatureSanctuary", false);
                            if (natureSanctuary != null)
                            {
                                intDataLW.typeDict.Add("NatureSanctuary", natureSanctuary);
                                comLib.registerSettlementTypeForOrcExpansion(natureSanctuary);
                            }

                            Type wolfRun = intDataLW.assembly.GetType("LivingWilds.Set_Nature_WolfRun", false);
                            if (wolfRun != null)
                            {
                                intDataLW.typeDict.Add("WolfRun", wolfRun);
                                core.data.tryAddSettlementTypeForWaystation(wolfRun);
                            }
                        }
                        break;
                    case "ShadowsLib":
                        ModData.ModIntegrationData intDataIx = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("Ixthus", intDataIx);

                        if (core.data.tryGetModAssembly("Ixthus", out intDataIx))
                        {
                            Type tenet = intDataIx.assembly.GetType("ShadowsLib.H_expeditionPatrons", false);
                            if (tenet != null)
                            {
                                intDataIx.typeDict.Add("Tenet", tenet);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public override void onTurnStart(Map map)
        {
            if (comLib == null)
            {
                return;
            }

            core.data.isPlayerTurn = true;

            updateOrcSGCultureMap(map);

            if (core.godPowers1.Count > 0 || core.godPowers2.Count > 0)
            {
                updateGodPowers(map);
            }
        }

        private void updateOrcSGCultureMap(Map map)
        {
            //Console.WriteLine("OrcsPlus: updating orcSGCultureMap");
            core.data.orcSGCultureMap.Clear();
            List<HolyOrder_Orcs> orcCultures = core.data.getOrcCultures(map);

            if (orcCultures?.Count > 0)
            {
                foreach (HolyOrder_Orcs orcCulture in orcCultures)
                {
                    if (orcCulture.orcSociety == null || orcCulture.checkIsGone())
                    {
                        continue;
                    }

                    core.data.orcSGCultureMap.Add(orcCulture.orcSociety, orcCulture);
                }
            }

            //Console.WriteLine("OrcsPlus: orcSGCultureMap updated");
        }

        public void updateGodPowers(Map map)
        {
            int status = 0;

            foreach (HolyOrder_Orcs orcCulture in core.data.orcSGCultureMap.Values)
            {
                if (orcCulture?.tenet_god.status < status)
                {
                    status = orcCulture.tenet_god.status;
                }

                if (status == -2)
                {
                    break;
                }
            }

            if (status == 0)
            {
                foreach (Power p in godPowers1)
                {
                    int index = map.overmind.god.powers.FindIndex(pow => pow == p);
                    if (index != -1)
                    {
                        map.overmind.god.powers.RemoveAt(index);
                        map.overmind.god.powerLevelReqs.RemoveAt(index);
                    }
                }

                foreach (Power p in godPowers2)
                {
                    int index = map.overmind.god.powers.FindIndex(pow => pow == p);
                    if (index != -1)
                    {
                        map.overmind.god.powers.RemoveAt(index);
                        map.overmind.god.powerLevelReqs.RemoveAt(index);
                    }
                }
            }
            else if (status == -1)
            {
                foreach (Power p in godPowers1)
                {
                    if (!map.overmind.god.powers.Contains(p))
                    {
                        map.overmind.god.powers.Add(p);
                        map.overmind.god.powerLevelReqs.Add(p.getCost() - 1);
                    }
                }

                foreach (Power p in godPowers2)
                {
                    int index = map.overmind.god.powers.FindIndex(pow => pow == p);
                    if (index != -1)
                    {
                        map.overmind.god.powers.RemoveAt(index);
                        map.overmind.god.powerLevelReqs.RemoveAt(index);
                    }
                }
            }
            else if (status == -2)
            {
                foreach (Power p in godPowers1)
                {
                    if (!map.overmind.god.powers.Contains(p))
                    {
                        map.overmind.god.powers.Add(p);
                        map.overmind.god.powerLevelReqs.Add(p.getCost() - 1);
                    }
                }

                foreach (Power p in godPowers2)
                {
                    if (!map.overmind.god.powers.Contains(p))
                    {
                        map.overmind.god.powers.Add(p);
                        map.overmind.god.powerLevelReqs.Add(p.getCost() - 1);
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

            core.data.isPlayerTurn = false;

            core.data.influenceGainElder.Clear();
            core.data.influenceGainHuman.Clear();
        }

        public override float hexHabitability(Hex hex, float hab)
        {
            if (core.data.godTenetTypes.TryGetValue(hex.map.overmind.god.GetType(), out Type tenet) && tenet != null)
            {
                if (tenet == typeof(H_Orcs_LifeMother))
                {
                    if (hex.location != null && hex.location.settlement is Set_OrcCamp && hex.location.properties.OfType<Pr_Vinerva_LifeBoon>().FirstOrDefault() != null)
                    {
                        hab += (float)(hex.map.opt_orcHabMult * hex.map.param.orc_habRequirement);
                    }
                }
            }

            return hab;
        }

        public override double sovereignAI(Map map, AN actionNational, Person ruler, List<ReasonMsg> reasons, double initialUtility)
        {
            if (actionNational is AN_WarOnThreat threatWar && threatWar.target is SG_Orc)
            {
                if (reasons != null)
                {
                    ReasonMsg distanceReason = reasons.FirstOrDefault(r => r.msg == "Distance between");
                    if (distanceReason != null)
                    {
                        initialUtility += distanceReason.value;
                        distanceReason.value *= 2;
                    }
                }
                else
                {
                    double val = map.getStepDist(ruler.society, threatWar.target) - 1;
                    if (val > 0)
                    {
                        val *= Math.Min(map.param.utility_soc_warDistancePenaltyPerStep, map.param.utility_soc_warDistancePenaltyCap);
                        initialUtility += val;
                    }
                }
            }

            if (actionNational is AN_DeclareWar war && war.target is SG_Orc)
            {
                if (reasons != null)
                {
                    ReasonMsg distanceReason = reasons.FirstOrDefault(r => r.msg == "Distance between");
                    if (distanceReason != null)
                    {
                        initialUtility += distanceReason.value;
                        distanceReason.value *= 2;
                    }
                }
                else
                {
                    double val = map.getStepDist(ruler.society, war.target) - 1;
                    if (val > 0)
                    {
                        val *= Math.Min(map.param.utility_soc_warDistancePenaltyPerStep, map.param.utility_soc_warDistancePenaltyCap);
                        initialUtility += val;
                    }
                }
            }

            return initialUtility;
        }

        public override int adjustHolyInfluenceDark(HolyOrder order, int inf, List<ReasonMsg> msgs)
        {
            HolyOrder_Orcs orcCulture = order as HolyOrder_Orcs;

            List<ReasonMsg> influenceGain;
            if (orcCulture == null || !core.data.influenceGainElder.TryGetValue(orcCulture, out influenceGain))
            {
                return inf;
            }

            if (orcCulture.isGone())
            {
                core.data.influenceGainElder.Remove(orcCulture);
                return inf;
            }

            foreach (ReasonMsg msg in influenceGain)
            { 
                msgs?.Add(msg);
                inf += (int)msg.value;
            }

            return inf;
        }

        public override int adjustHolyInfluenceGood(HolyOrder order, int inf, List<ReasonMsg> msgs)
        {
            if (!(order is HolyOrder_Orcs orcCulture) || !core.data.influenceGainHuman.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain) || influenceGain?.Count == 0)
            {
                return inf;
            }

            if (orcCulture.isGone())
            {
                core.data.influenceGainHuman.Remove(orcCulture);
                return inf;
            }

            foreach (ReasonMsg msg in influenceGain)
            {
                msgs?.Add(msg);
                inf += (int)msg.value;
            }

            return inf;
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
            if (!core.data.influenceGainElder.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain))
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

        public override void onAgentBattleTerminate(BattleAgents battleAgents)
        {
            if (core.data.godTenetTypes.TryGetValue(battleAgents.att.map.overmind.god.GetType(), out Type tenetType) && tenetType != null && tenetType == typeof(H_Orcs_HarbringersMadness))
            {
                UA att = battleAgents.att;
                UA def = battleAgents.def;

                if (att.society is SG_Orc orcSociety && core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_HarbringersMadness harbringers && harbringers.status < -1)
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
                else if (att.society is HolyOrder_Orcs orcCulture2 && orcCulture2.tenet_god is H_Orcs_HarbringersMadness harbringers2 && harbringers2.status < -1)
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

                if (def.society is SG_Orc orcSociety3 && core.data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null && orcCulture3.tenet_god is H_Orcs_HarbringersMadness harbringers3 && harbringers3.status < -1)
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
                else if (def.society is HolyOrder_Orcs orcCulture4 && orcCulture4.tenet_god is H_Orcs_HarbringersMadness harbringers4 && harbringers4.status < -1)
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

        public override void onPersonDeath_StartOfProcess(Person person, string v, object killer)
        {
            if (comLib == null)
            {
                return;
            }

            // Person Data
            Unit uPerson = person.unit;

            if (uPerson == null)
            {
                return;
            }

            //Console.WriteLine("Orcs_Plus: Person with Unit has died.");
            UA uaPerson = uPerson as UA;
            SG_Orc orcSociety = uPerson.society as SG_Orc;
            HolyOrder_Orcs orcCulture = uPerson.society as HolyOrder_Orcs;

            // Person Activity Data
            Task_PerformChallenge performChallenge = uaPerson?.task as Task_PerformChallenge;
            Challenge challenge = performChallenge?.challenge;

            Ch_SkirmishAttacking skirmishAtt = null;
            Ch_SkirmishDefending skirmishDef = null;

            if (challenge != null)
            {
                //Console.WriteLine("Orcs_Plus: Person died performing challenge");
                skirmishAtt = challenge as Ch_SkirmishAttacking;
                skirmishDef = challenge as Ch_SkirmishDefending;
            }

            if (uaPerson is UAEN_OrcUpstart || uaPerson is UAEN_OrcElder || uaPerson is UAEN_OrcShaman)
            {
                SG_Orc orcSociety2 = uaPerson.society as SG_Orc;
                HolyOrder_Orcs orcCulture2 = uaPerson.society as HolyOrder_Orcs;

                if (orcSociety2 != null)
                {
                    core.data.orcSGCultureMap.TryGetValue(orcSociety2, out orcCulture2);
                }
                else if (orcCulture2 != null)
                {
                    orcSociety2 = orcCulture2.orcSociety;
                }

                if (orcCulture2 != null && orcCulture2.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < -1)
                {
                    if (core.data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null && intDataCord.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null && intDataCord.typeDict.TryGetValue("Swarm", out Type swarmType) && swarmType != null)
                    {
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
                        UA drone = (UA)Activator.CreateInstance(droneType, args);
                        uaPerson.map.units.Add(drone);
                        loc.units.Add(drone);
                    }
                }
            }

            // Killer Data
            Person pKiller = killer as Person;
            Unit uKiller = killer as Unit;

            if (pKiller != null)
            {
                //Console.WriteLine("Orcs_Plus: Person was killed by another person.");
                uKiller = pKiller.unit;
            }

            if (uKiller != null)
            {
                if (uaPerson is UAEN_OrcElder && uKiller is UA uaKiller && !uaKiller.person.traits.Any(t => t is T_BloodFeud fued && fued.orcSociety == orcSociety))
                {
                    uaKiller.person.receiveTrait(new T_BloodFeud(orcCulture.orcSociety));

                    person.map.addUnifiedMessage(uaPerson, uaKiller, "Blood Feud", uaKiller.getName() + " has become the target of a blood fued by killing an orc elder of the " + uaPerson.society.getName() + ". They must now spend the rest of their days looking over their shoulder for the sudden the appearance of an avenging orc upstart.", "Orc Blood Feud");
                }

                //Console.WriteLine("OrcsPlus: killer is " + uKiller.getName());
                if (v == "Killed in battle with " + uKiller.getName())
                {
                    //Console.WriteLine("OrcsPlus: Died in battle");
                    if (uKiller.isCommandable())
                    {
                        //Console.WriteLine("OrcsPlus: killer is commandable");
                        if (orcCulture != null)
                        {
                            //Console.WriteLine("OrcsPlus: victim is of orc culture");
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Killed orc agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }
                        //Console.WriteLine("OrcsPlus: Direct influence added");
                        List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                        if (orcSocieties.Count > 0 && uPerson.society != null)
                        {
                            //Console.WriteLine("OrcsPlus: Got active orc societies");
                            foreach (SG_Orc orcs in orcSocieties)
                            {
                                if (orcSociety != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcs.getRel(uPerson.society).state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcs, new ReasonMsg("Killed enemy agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                        //Console.WriteLine("OrcsPlus: End");
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Killed orc agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }

                    if (core.data.godTenetTypes.TryGetValue(person.map.overmind.god.GetType(), out Type tenetType) && tenetType != null && tenetType == typeof(H_Orcs_HarbringersMadness))
                    {
                        if (uKiller.society is SG_Orc orcSociety2 && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null && orcCulture2.tenet_god is H_Orcs_HarbringersMadness harbringers && harbringers.status < -1)
                        {
                            foreach (RelObj relation in person.relations.Values)
                            {
                                if (relation.relation == RelObj.relType.PARENT || relation.relation == RelObj.relType.SPOUSE || relation.relation == RelObj.relType.CHILD)
                                {
                                    Person relative = person.map.persons[relation.them];

                                    if (!relative.isDead && relative.unit != null && relative.unit.hp > 0 && !relative.unit.isCommandable())
                                    {
                                        person.map.persons[relation.them].sanity -= 2;

                                        if (person.map.persons[relation.them].sanity < 1.0)
                                        {
                                            person.map.persons[relation.them].goInsane(-1);
                                        }
                                    }
                                }
                            }
                        }
                        else if (uKiller.society is HolyOrder_Orcs orcCulture3 && orcCulture3.tenet_god is H_Orcs_HarbringersMadness harbringers2 && harbringers2.status < -1)
                        {
                            foreach (RelObj relation in person.relations.Values)
                            {
                                Person relative = person.map.persons[relation.them];

                                if (!relative.isDead && relative.unit != null && relative.unit.hp > 0 && !relative.unit.isCommandable())
                                {
                                    person.map.persons[relation.them].sanity -= 2;

                                    if (person.map.persons[relation.them].sanity < 1.0)
                                    {
                                        person.map.persons[relation.them].goInsane(-1);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (v == "Killed by " + uKiller.getName())
                {
                    if (uKiller.isCommandable())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Intercepted and killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Intercepted and killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                        if (orcSocieties?.Count > 0 && uPerson.society != null)
                        {
                            foreach (SG_Orc orcs in orcSocieties)
                            {
                                if (orcSociety != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcs.getRel(uPerson.society).state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcs, new ReasonMsg("Intercepted and killed enemy agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Intercepted and killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Intercepted and killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
                else if (v == "Killed by a volcano")
                {
                    if (uKiller.isCommandable())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                        if (orcSocieties?.Count > 0 && uPerson.society != null)
                        {
                            foreach (SG_Orc orcs in orcSocieties)
                            {
                                if (orcSociety != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcs.getRel(uPerson.society).state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcs, new ReasonMsg("Volcanic eruption (geomancy) killed enemy agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
            }
            else if (v == "Killed by Ophanim's Smite")
            {
                if (orcCulture != null)
                {
                    TryAddInfluenceGain(orcCulture, new ReasonMsg("Smote orc agent", core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                if (orcSociety != null)
                {
                    TryAddInfluenceGain(orcSociety, new ReasonMsg("Smote orc agent", core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                if (orcSocieties?.Count > 0 && uPerson.society != null)
                {
                    foreach (SG_Orc orcs in orcSocieties)
                    {
                        if (orcSociety != null && orcs == orcSociety)
                        {
                            continue;
                        }

                        if (orcs.getRel(uPerson.society).state == DipRel.dipState.war)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Smote enemy agent", core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }
                    }
                }
            }
            else if ((skirmishAtt != null && v == "Killed by dangers during challenge: " + skirmishAtt.getName()) || (skirmishDef != null && v == "Killed by dangers during challenge: " + skirmishDef.getName()))
            {
                bool infGainElder = false;
                bool infGainHuman = false;

                if (skirmishAtt != null || skirmishDef != null)
                {
                    List<UM> enemies = new List<UM>();
                    List<UA> enemyComs = new List<UA>();

                    if (skirmishAtt != null)
                    {
                        core.data.getBattleArmyEnemies(skirmishAtt.battle, uaPerson, out enemies, out enemyComs);
                    }

                    if (skirmishDef != null)
                    {
                        core.data.getBattleArmyEnemies(skirmishDef.battle, uaPerson, out enemies, out enemyComs);
                    }

                    if (enemies.Count > 0)
                    {
                        foreach (UM enemy in enemies)
                        {
                            if (enemy.isCommandable())
                            {
                                infGainElder = true;
                            }
                            else if (!enemy.society.isDark())
                            {
                                infGainHuman = true;
                            }

                            if (infGainElder && infGainHuman)
                            {
                                break;
                            }
                        }
                    }

                    if (enemyComs.Count > 0 && !(infGainElder && infGainHuman))
                    {
                        foreach (UA com in enemyComs)
                        {
                            if (com.isCommandable())
                            {
                                infGainElder = true;
                            }
                            else if (!com.society.isDark())
                            {
                                infGainHuman = true;
                            }

                            if (infGainElder && infGainHuman)
                            {
                                break;
                            }
                        }
                    }

                    if (infGainElder)
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Killed orc agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                        if (orcSocieties?.Count > 0)
                        {
                            foreach (SG_Orc orcs in orcSocieties)
                            {
                                if (orcSociety != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcs.getRel(uKiller.society).state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcs, new ReasonMsg("Killed enemy agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                    }

                    if (infGainHuman)
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Killed orc agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
            }
            else if (v == "Killed by a volcano")
            {

                if (orcCulture != null)
                {
                    TryAddInfluenceGain(orcCulture, new ReasonMsg("Awakening killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                if (orcSociety != null)
                {
                    TryAddInfluenceGain(orcSociety, new ReasonMsg("Awakening killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                if (orcSocieties?.Count > 0 && uPerson.society != null)
                {
                    foreach (SG_Orc orcs in orcSocieties)
                    {
                        if (orcSociety != null && orcs == orcSociety)
                        {
                            continue;
                        }

                        if (orcs.getRel(uPerson.society).state == DipRel.dipState.war)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Awakening killed enemy agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }
                    }
                }
            }
        }

        public bool checkAlignment(SG_Orc orcSociety, Location loc)
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
