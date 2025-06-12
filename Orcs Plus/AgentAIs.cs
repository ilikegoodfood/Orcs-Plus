using Assets.Code;
using CommunityLib;
using HarmonyLib;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orcs_Plus
{
    public class AgentAIs
    {
        private Map map;

        private AgentAI comLibAI;

        private List<AIChallenge> aiChallenges_Upstart;

        private List<AIChallenge> aiChallenges_Elder;

        private List<AIChallenge> aiChallenges_Shaman;

        public AgentAIs(Map map)
        {
            this.map = map;
            comLibAI = ModCore.Get().comLibAI;

            populateOrcUpstarts();

            populateOrcElders();

            populateOrcShamans();
        }

        public void populateConditional()
        {
            if (ModCore.Get().data.godTenetTypes.TryGetValue(map.overmind.god.GetType(), out Type tenet) && tenet != null)
            {
                switch (tenet.Name)
                {
                    case nameof(H_Orcs_Curseweaving):
                        AIChallenge spreadCurseBroken = new AIChallenge(typeof(Rt_H_Orcs_SpreadCurseBrokenSpirit), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.PreferLocalRandomized });
                        spreadCurseBroken.delegates_ValidFor.Add(delegate_ValidFor_SpreadCurseBrokenSpirit);
                        spreadCurseBroken.delegates_Utility.Add(delegate_Utility_SpreadCurseBrokenSpirit);
                        ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), spreadCurseBroken);
                        break;
                    case nameof(H_Orcs_DeathMastery):
                        ModCore.GetComLib().GetAgentAI().AddChallengeToAgentType(typeof(UAEN_OrcShaman), new AIChallenge(typeof(Mg_Orcs_ReviveImmortal), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                        break;
                    case nameof(H_Orcs_HarbingersMadness):
                        ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_MadnessFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                        break;
                    case nameof(H_Orcs_LifeMother):
                        ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_NurtureOrchard), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                        ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_HarvestGourd), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                        break;
                    case nameof(H_Orcs_Perfection):
                        ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(typeof(Ch_H_Orcs_PerfectionFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }));
                        break;
                    case nameof(H_Orcs_GlorySeeker):
                        AIChallenge spreadCurseGlory = new AIChallenge(typeof(Rt_H_Orcs_SpreadCurseOfGlory), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.PreferLocalRandomized });
                        spreadCurseGlory.delegates_ValidFor.Add(delegate_ValidFor_SpreadCurseOfGlory);
                        spreadCurseGlory.delegates_Utility.Add(delegate_Utility_SpreadCurseOfGlory);
                        ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), spreadCurseGlory);
                        break;
                    default:
                        break;
                }
            }

            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
            {
                if (intDataCCC.typeDict.TryGetValue("CallHordes", out Type callHordesType))
                {
                    AIChallenge CCC_CallHordes = new AIChallenge(callHordesType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden });
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcUpstart), CCC_CallHordes);
                    CCC_CallHordes = new AIChallenge(callHordesType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden });
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), CCC_CallHordes);
                    CCC_CallHordes = new AIChallenge(callHordesType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden });
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcShaman), CCC_CallHordes);
                }

                if (intDataCCC.typeDict.TryGetValue("StudyCurseweaving", out Type studyMagicType))
                {
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcShaman), new AIChallenge(studyMagicType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }));
                }
            }

            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCuriosRecast", out ModIntegrationData intDataCCCR))
            {
                if (intDataCCCR.typeDict.TryGetValue("CallHordes", out Type callHordesType))
                {
                    AIChallenge CCC_CallHordes = new AIChallenge(callHordesType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden });
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcUpstart), CCC_CallHordes);
                    CCC_CallHordes = new AIChallenge(callHordesType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden });
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), CCC_CallHordes);
                    CCC_CallHordes = new AIChallenge(callHordesType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden });
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcShaman), CCC_CallHordes);
                }

                if (intDataCCCR.typeDict.TryGetValue("StudyCurseweaving", out Type studyMagicType))
                {
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcShaman), new AIChallenge(studyMagicType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }));
                }

                if (intDataCCCR.typeDict.TryGetValue("BuySoulstone", out Type buySoulstoneType) && buySoulstoneType != null)
                {
                    comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcUpstart), new AIChallenge(buySoulstoneType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true));
                    comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(buySoulstoneType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true));
                }
                if (intDataCCCR.typeDict.TryGetValue("BuyCraftList", out Type buyCraftListType) && buySoulstoneType != null)
                {
                    comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcUpstart), new AIChallenge(buyCraftListType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true));
                    comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), new AIChallenge(buyCraftListType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true));
                }
            }

            if (ModCore.Get().data.tryGetModIntegrationData("Escamrak", out ModIntegrationData intDataEscam))
            {
                if (intDataEscam.typeDict.TryGetValue("StudyFleshcrafting", out Type studyMagicType))
                {
                    AIChallenge Escam_StudyMagic = new AIChallenge(studyMagicType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden });
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcUpstart), Escam_StudyMagic);
                    Escam_StudyMagic = new AIChallenge(studyMagicType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden });
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcElder), Escam_StudyMagic);
                    Escam_StudyMagic = new AIChallenge(studyMagicType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden });
                    ModCore.Get().comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcShaman), Escam_StudyMagic);
                }
            }
        }

        // Universal Delegates
        private bool delegate_ValidFor_BuyItem(AgentAI.ChallengeData challengeData, UA ua)
        {
            bool result = false;

            foreach (Item item in ua.person.items)
            {
                if (item == null)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private bool delegate_ValidFor_Underground(AgentAI.ChallengeData challengeData, UA ua)
        {
            if (ua.society is SG_Orc orcs)
            {
                if (!orcs.canGoUnderground())
                {
                    if (challengeData.location.hex.z == 0 || challengeData.location.hex.z == 1)
                    {
                        if (!CommunityLib.HarmonyPatches.OrcMapLayers(orcs).Item2.Contains(challengeData.location.hex.z))
                        {
                            return false;
                        }
                    }
                }
            }
            else if (ua.society is HolyOrder_Orcs orcCulture)
            {
                if (!orcCulture.orcSociety.canGoUnderground())
                {
                    if (challengeData.location.hex.z == 0 || challengeData.location.hex.z == 1)
                    {
                        if (!CommunityLib.HarmonyPatches.OrcMapLayers(orcCulture.orcSociety).Item2.Contains(challengeData.location.hex.z))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // Orc Upstart
        private void populateOrcUpstarts()
        {
            if (comLibAI.TryGetAgentType(typeof(UAEN_OrcUpstart), out CommunityLib.AgentAI.AIData aiData))
            {
                aiData.controlParameters.canAttack = true;

                aiChallenges_Upstart = new List<AIChallenge>
                {
                    new AIChallenge(typeof(Rti_RecruitWarband), 0.0, null, false, true),
                    new AIChallenge(typeof(Ch_Orcs_FundHorde), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.RequiresOwnSociety }, false, true),
                    new AIChallenge(typeof(Ch_Orcs_RaidOutpost), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.RequiresOwnSociety }, false, true),
                    new AIChallenge(typeof(Ch_BuyItem), 0.0, new List<AIChallenge.ChallengeTags> {  AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocalRandomized }, false, true),
                    new AIChallenge(typeof(Ch_Orcs_DrinkGrott), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                    new AIChallenge(typeof(Ch_Orcs_RefillDrinkingHorns), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                    new AIChallenge(typeof(Ch_DrinkPrimalWaters), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocal }, false, true),
                    new AIChallenge(typeof(Ch_Orcs_RecruitCorsair), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.RequiresOwnSociety }, false, true),
                    new AIChallenge(typeof(Rt_Orcs_ReclaimHordeBanner), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocal }, false, true),
                    new AIChallenge(typeof(Rti_RouseHorde), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true)
                };

                aiChallenges_Upstart[0].delegates_ValidFor.Add(delegate_ValidFor_RecruitWarband);
                aiChallenges_Upstart[0].delegates_Utility.Add(delegate_Utility_RecruitWarband);

                aiChallenges_Upstart[1].delegates_ValidFor.Add(delegate_ValidFor_FundHorde);
                aiChallenges_Upstart[1].delegates_Utility.Add(delegate_Utility_FundHorde);

                aiChallenges_Upstart[3].delegates_ValidFor.Add(delegate_ValidFor_BuyItem);

                aiChallenges_Upstart[8].delegates_ValidFor.Add(delegate_ValidFor_ReclaimHordeBanner);

                comLibAI.AddChallengesToAgentType(typeof(UAEN_OrcUpstart), aiChallenges_Upstart);

                AIChallenge raiding = comLibAI.GetAIChallengeFromAgentType(typeof(UAEN_OrcUpstart), typeof(Ch_OrcRaiding));
                if (raiding != null)
                {
                    MethodInfo MI_ComLibDelegate = AccessTools.Method(typeof(UAENOverrideAI), "delegate_Utility_Ch_OrcRaiding");

                    Func<AgentAI.ChallengeData, UA, double, List<ReasonMsg>, double> comLibDelegate = raiding.delegates_Utility.FirstOrDefault(del => del.GetMethodInfo() == MI_ComLibDelegate);

                    if (comLibDelegate != null)
                    {
                        raiding.delegates_Utility.Remove(comLibDelegate);
                        raiding.delegates_Utility.Add(delegate_Utility_Ch_OrcRaiding);
                    }
                }
            }
        }

        // Orc Upstarts Delegates
        private bool delegate_ValidFor_RecruitWarband(AgentAI.ChallengeData challengeData, UA ua)
        {
            bool hasFullMinions = ua.getStatCommandLimit() - ua.getCurrentlyUsedCommand() <= 0;
            Set_OrcCamp camp = ua.location.settlement as Set_OrcCamp;

            if (camp == null || (ua.isCommandable() && camp.infiltration < 1.0))
            {
                return false;
            }

            if (!hasFullMinions)
            {
                int minionCount = 0;

                foreach (Minion minion in ua.minions)
                {
                    if (minion != null)
                    {
                        minionCount++;
                    }
                }

                if (minionCount == ua.minions.Length)
                {
                    hasFullMinions = true;
                }
            }

            I_HordeBanner banner = (I_HordeBanner)ua.person.items.FirstOrDefault(i => i is I_HordeBanner);

            return !hasFullMinions && banner != null && challengeData.location.soc == banner.orcs && challengeData.location.settlement is Set_OrcCamp;
        }

        private double delegate_Utility_RecruitWarband(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Rti_RecruitWarband recruitWarband = challengeData.challenge as Rti_RecruitWarband;
            int availableCommand = ua.getStatCommandLimit() - ua.getCurrentlyUsedCommand();
            int availableSlots = 0;

            if (availableCommand > 0)
            {
                foreach (Minion minion in ua.minions)
                {
                    if (minion == null)
                    {
                        availableSlots++;
                    }
                }
            }

            if (availableSlots > 0)
            {
                int count = Math.Min(availableCommand, availableSlots);
                double val = count * recruitWarband.getMinionUtility(ua, recruitWarband.exemplar);

                string reason = "Would gain " + count + " " + recruitWarband.exemplar.getName();
                if (count > 1)
                {
                    reason += "s";
                }

                reasonMsgs?.Add(new ReasonMsg(reason, val));
                utility += val;
            }

            return utility;
        }

        private bool delegate_ValidFor_FundHorde(AgentAI.ChallengeData challengeDatae, UA ua)
        {
            if (ua.person.gold > 150)
            {
                return true;
            }

            return false;
        }

        private double delegate_Utility_FundHorde(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val = ua.person.gold - 100;
            reasonMsgs?.Add(new ReasonMsg("Excess Gold", val));
            utility += val;

            return utility;
        }

        private double delegate_Utility_Ch_OrcRaiding(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            double prosperity = 0.0;
            foreach (Location loc in challengeData.location.getNeighbours())
            {
                if (loc.settlement is SettlementHuman settlementHuman && settlementHuman.prosperity > prosperity)
                {
                    prosperity = settlementHuman.prosperity;
                }
            }

            if (prosperity > 0)
            {
                double val = prosperity * 50;
                reasonMsgs?.Add(new ReasonMsg("Prosperity", val));
                utility += val;
            }

            return utility;
        }

        private bool delegate_ValidFor_ReclaimHordeBanner(AgentAI.ChallengeData challengeData, UA ua)
        {
            if (ua.society is SG_Orc orcSociety && !ua.person.items.Any(i => i is I_HordeBanner banner && banner.orcs == orcSociety))
            {
                if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                {
                    if (challengeData.location.soc == orcSociety && challengeData.location.settlement is Set_OrcCamp camp && camp.specialism > 0 && camp.subs.Any(sub => sub is Sub_OrcCultureCapital || sub is Sub_OrcTemple))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void populateOrcElders()
        {
            AgentAI.ControlParameters controlParams = new AgentAI.ControlParameters(true)
            {
                canAttack = true,
                canDisrupt = true
            };

            aiChallenges_Elder = new List<AIChallenge>
            {
                /*0*/new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.Rest }, false, true),
                /*1*/new AIChallenge(typeof(Ch_LayLowWilderness), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.ManageMenaceProfile }, false, true),
                /*2*/new AIChallenge(typeof(Ch_H_Orcs_ReprimandUpstart), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                /*3*/new AIChallenge(typeof(Ch_Orcs_RetreatToTheHills), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor }, false, true),
                /*4*/new AIChallenge(typeof(Ch_Orcs_AccessPlunder), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor }, false, true),
                /*5*/new AIChallenge(typeof(Ch_H_Orcs_CleansingFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                /*6*/new AIChallenge(typeof(Ch_H_Orcs_DarkFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                /*7*/new AIChallenge(typeof(Rt_H_Orcs_GiftGold), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.PreferLocalRandomized, AIChallenge.ChallengeTags.ForbidWar }, false, true),
                /*8*/new AIChallenge(typeof(Ch_Orcs_FundWaystation), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                /*9*/new AIChallenge(typeof(Ch_BuyItem), 0.0, new List<AIChallenge.ChallengeTags> {  AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocalRandomized}, false, true),
                /*10*/new AIChallenge(typeof(Ch_Orcs_OrganiseTheHorde), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferLocalRandomized }, false, true),
                /*11*/new AIChallenge(typeof(Ch_Orcs_BloodMoney), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                /*12*/new AIChallenge(typeof(Ch_Orcs_DrinkGrott), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                /*13*/new AIChallenge(typeof(Ch_Orcs_RefillDrinkingHorns), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                /*14*/new AIChallenge(typeof(Ch_DrinkPrimalWaters), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocal }, false, true),
                /*15*/new AIChallenge(typeof(Rti_RouseHorde), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true),
                /*16*/new AIChallenge(typeof(Rti_Orc_CeaseWar), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true)
            };

            aiChallenges_Elder[0].delegates_ValidFor.Add(delegate_ValidFor_OwnCulture);

            aiChallenges_Elder[1].delegates_ValidFor.Add(delegate_ValidFor_OwnCulture);

            aiChallenges_Elder[2].delegates_ValidFor.Add(delegate_ValidFor_OwnCulture);

            aiChallenges_Elder[3].delegates_Valid.Add(delegate_Valid_Retreat);
            aiChallenges_Elder[3].delegates_ValidFor.Add(delegate_ValidFor_OwnCulture);
            aiChallenges_Elder[3].delegates_Utility.Add(delegate_Utility_Retreat);

            aiChallenges_Elder[4].delegates_ValidFor.Add(delegate_ValidFor_OwnCulture);
            aiChallenges_Elder[4].delegates_ValidFor.Add(delegate_ValidFor_AccessPlunder);
            aiChallenges_Elder[4].delegates_Utility.Add(delegate_Utility_AccessPlunder);

            aiChallenges_Elder[7].delegates_Valid.Add(delegate_Valid_OrcGift);
            aiChallenges_Elder[7].delegates_ValidFor.Add(delegate_ValidFor_OrcGift);
            aiChallenges_Elder[7].delegates_Utility.Add(delegate_Utility_OrcGift);

            aiChallenges_Elder[9].delegates_ValidFor.Add(delegate_ValidFor_BuyItem);

            aiChallenges_Elder[10].delegates_Utility.Add(delegate_Utility_Organise);

            comLibAI.RegisterAgentType(typeof(UAEN_OrcElder), controlParams);
            comLibAI.AddChallengesToAgentType(typeof(UAEN_OrcElder), aiChallenges_Elder);

            if (comLibAI.TryGetAgentType(typeof(UAEN_OrcElder), out AgentAI.AIData aiData) && aiData != null)
            {
                aiData.aiChallenges_UniversalDelegates_ValidFor.Add(delegate_ValidFor_Underground);
            }
        }

        private bool delegate_ValidFor_OwnCulture(AgentAI.ChallengeData challengeData, UA ua)
        {
            if (ua is UAEN_OrcElder elder && (elder.society as HolyOrder_Orcs)?.orcSociety == challengeData.location.soc)
            {
                return true;
            }

            return false;
        }

        private bool delegate_Valid_Retreat(AgentAI.ChallengeData challengeData)
        {
            Set_OrcCamp camp = challengeData.location.settlement as Set_OrcCamp;
            SG_Orc orcSociety = challengeData.location.soc as SG_Orc;

            if (camp != null && orcSociety != null)
            {
                foreach (Unit unit in challengeData.location.units)
                {
                    if (unit is UM && unit.task is Task_RazeLocation)
                    {
                        return true;
                    }
                }

                foreach (Location neighbour in challengeData.location.getNeighbours())
                {
                    if (neighbour.soc == orcSociety && neighbour.settlement is Set_OrcCamp)
                    {
                        foreach (Unit unit in neighbour.units)
                        {
                            if (unit is UM && unit.task is Task_RazeLocation)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private double delegate_Utility_Retreat(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Set_OrcCamp camp = challengeData.location.settlement as Set_OrcCamp;
            SG_Orc orcSociety = challengeData.location.soc as SG_Orc;

            if (camp != null && orcSociety != null && orcSociety.isAtWar())
            {
                int attackerCount = 0;
                List<Pr_OrcishIndustry> targetIndustry = new List<Pr_OrcishIndustry>();
                double defenceDelta = 0.0;

                foreach (Unit unit in challengeData.location.units)
                {
                    if (unit is UM && unit.task is Task_RazeLocation)
                    {
                        Pr_OrcishIndustry industry = (Pr_OrcishIndustry)challengeData.location.properties.FirstOrDefault(pr => pr is Pr_OrcishIndustry);
                        if (industry?.charge > 10)
                        {
                            attackerCount++;
                            if (!targetIndustry.Contains(industry))
                            {
                                targetIndustry.Add(industry);
                                defenceDelta += industry.charge / 2;
                            }
                        }
                    }
                }

                foreach (Location neighbour in challengeData.location.getNeighbours())
                {
                    if (neighbour.soc == orcSociety && neighbour.settlement is Set_OrcCamp)
                    {
                        foreach (Unit unit in neighbour.units)
                        {
                            if (unit is UM && unit.task is Task_RazeLocation)
                            {
                                Pr_OrcishIndustry industry = (Pr_OrcishIndustry)neighbour.properties.FirstOrDefault(pr => pr is Pr_OrcishIndustry);
                                if (industry?.charge > 10)
                                {
                                    attackerCount++;
                                    if (!targetIndustry.Contains(industry))
                                    {
                                        targetIndustry.Contains(industry);
                                        defenceDelta += industry.charge / 2;
                                    }
                                }
                            }
                        }
                    }
                }

                double val = (defenceDelta / 2);
                reasonMsgs?.Add(new ReasonMsg("Defence Gain", val));
                utility += val;

                val = 10 * attackerCount;
                reasonMsgs?.Add(new ReasonMsg("Attacking Armies", val));
                utility += val;
            }

            return utility;
        }

        private bool delegate_ValidFor_AccessPlunder(AgentAI.ChallengeData challengeData, UA ua)
        {
            bool result = false;

            if (ua.person.gold < 100)
            {
                result = true;
            }

            return result;
        }

        private double delegate_Utility_AccessPlunder(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Pr_OrcPlunder plunder = (challengeData.challenge as Ch_Orcs_AccessPlunder)?.cache;
            int gold;

            if (plunder.gold <= 50)
            {
                gold = plunder.gold;
            }
            else if (plunder.gold / 2.0 <= 50.0)
            {
                gold = 50;
            }
            else
            {
                gold = (int)Math.Floor(plunder.gold / 2.0);
            }

            int goldTarget = 150;
            if (ua.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_MammonClient client && client.status < 0)
            {
                goldTarget += 50;
            }

            double val = Math.Min(gold, goldTarget - ua.person.gold);
            reasonMsgs?.Add(new ReasonMsg("Gold", val));
            utility += val;

            return utility;
        }

        private bool delegate_Valid_OrcGift(AgentAI.ChallengeData challengeData)
        {
            bool result = false;

            if (challengeData.location.settlement is SettlementHuman settlementHuman)
            {
                result = true;

                if (settlementHuman.ruler != null)
                {
                    List<int> allHates = new List<int>();
                    allHates.AddRange(settlementHuman.ruler.hates);
                    allHates.AddRange(settlementHuman.ruler.extremeHates);
                    if (allHates.Contains(Tags.ORC) || allHates.Contains(Tags.GOLD))
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        private bool delegate_ValidFor_OrcGift(AgentAI.ChallengeData challengeData, UA ua)
        {
            Rt_H_Orcs_GiftGold ritual = challengeData.challenge as Rt_H_Orcs_GiftGold;
            return ua is UAEN_OrcElder elder && elder.person.gold >= ritual.bribeCost && elder.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety.menace > ritual.bribeEffect && challengeData.location.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null;
        }

        private double delegate_Utility_OrcGift(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            utility += ((ua.society as HolyOrder_Orcs)?.orcSociety.menace ?? 0) * 4;

            if (utility > 0)
            {
                reasonMsgs?.Add(new ReasonMsg("Society Menace", utility));

                if (challengeData.location.settlement != null && challengeData.location.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
                {
                    Person ruler = settlementHuman.ruler;
                    double val;
                    if (ruler.likes.Contains(Tags.ORC))
                    {
                        val = 10;
                        reasonMsgs?.Add(new ReasonMsg("Local ruler likes orcs", val));
                        utility += val;
                    }
                    else if (ruler.extremeLikes.Contains(Tags.ORC))
                    {
                        val = 20;
                        reasonMsgs?.Add(new ReasonMsg("Local ruler loves orcs", val));
                        utility += val;
                    }

                    if (ruler.likes.Contains(Tags.GOLD))
                    {
                        val = 10;
                        reasonMsgs?.Add(new ReasonMsg("Local ruler likes gold", val));
                        utility += val;
                    }
                    else if (ruler.extremeLikes.Contains(Tags.GOLD))
                    {
                        val = 20;
                        reasonMsgs?.Add(new ReasonMsg("Local ruler loves gold", val));
                        utility += val;
                    }
                }
            }

            return utility;
        }

        private double delegate_Utility_Organise(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && challengeData.location.settlement != null)
            {
                Sub_OrcTemple hall = (Sub_OrcTemple)challengeData.location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcTemple);
                if (hall != null && hall.order == orcCulture)
                {
                    Pr_OrcishIndustry industry = (Pr_OrcishIndustry)challengeData.location.properties.FirstOrDefault(pr => pr is Pr_OrcishIndustry);
                    double charge = 0.0;
                    if (industry != null)
                    {
                        charge = industry.charge;
                    }

                    if (orcCulture.tenet_industrious.status == -1)
                    {
                        double val = ua.map.param.ch_orcs_organisethehorde_parameterValue5 - charge;
                        reasonMsgs?.Add(new ReasonMsg("Failing Industry", val));
                        utility += val;
                    }
                    else if (orcCulture.tenet_industrious.status == -2)
                    {
                        double val = ua.map.param.ch_orcs_organisethehorde_parameterValue1 - charge;
                        reasonMsgs?.Add(new ReasonMsg("Failing Industry", val));
                        utility += val;
                    }
                }
            }

            return utility;
        }

        private bool delegate_ValidFor_SpreadCurseOfGlory(AgentAI.ChallengeData challengeData, UA ua)
        {
            bool result = false;

            if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_GlorySeeker glory && glory.status < -1 && !glory.cursed)
            {
                if (!orcCulture.acolytes.Any(a => a != ua && ((a.task is Task_PerformChallenge performChallenge && performChallenge.challenge is Rt_H_Orcs_SpreadCurseOfGlory) || (a.task is Task_GoToPerformChallenge goPerformChallenge && goPerformChallenge.challenge is Rt_H_Orcs_SpreadCurseOfGlory))))
                {
                    if (challengeData.location.soc != null && orcCulture.getRel(challengeData.location.soc).state == DipRel.dipState.war)
                    {
                        if (challengeData.location.settlement is SettlementHuman humanSettlement && humanSettlement.ruler != null && !humanSettlement.ruler.house.curses.Any(curse => curse is Curse_EGlory))
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        private double delegate_Utility_SpreadCurseOfGlory(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            if (challengeData.location.soc is Society society && challengeData.location.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
            {
                if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_GlorySeeker glory && !glory.cursed && glory.status < -1)
                {
                    double val = 100.0;
                    reasonMsgs?.Add(new ReasonMsg("Base", val));
                    utility += val;

                    if (challengeData.location == society.getCapital())
                    {
                        val = 30;
                        reasonMsgs?.Add(new ReasonMsg("Target is Soverign", val));
                        utility += val;
                    }

                    if (!elder.minions.Any(m => m is M_OrcChampion))
                    {
                        val = -200.0;
                        reasonMsgs?.Add(new ReasonMsg("Unguarded by Champion", val));
                        utility += val;
                    }
                }
            }

            return utility;
        }

        private bool delegate_ValidFor_SpreadCurseBrokenSpirit(AgentAI.ChallengeData challengeData, UA ua)
        {
            bool result = false;

            if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_Curseweaving curse && curse.status < 0 && curse.usedCount < Math.Abs(curse.status))
            {
                if (!orcCulture.acolytes.Any(a => a != ua && ((a.task is Task_PerformChallenge performChallenge && performChallenge.challenge is Rt_H_Orcs_SpreadCurseBrokenSpirit) || (a.task is Task_GoToPerformChallenge goPerformChallenge && goPerformChallenge.challenge is Rt_H_Orcs_SpreadCurseBrokenSpirit))))
                {
                    if (challengeData.location.soc != null && orcCulture.getRel(challengeData.location.soc).state == DipRel.dipState.war)
                    {
                        if (challengeData.location.settlement is SettlementHuman humanSettlement && humanSettlement.ruler != null && !humanSettlement.ruler.house.curses.Any(curse2 => curse2 is Curse_BrokenSpirit))
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        private double delegate_Utility_SpreadCurseBrokenSpirit(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            if (challengeData.location.soc is Society society && challengeData.location.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
            {
                if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_Curseweaving curse && curse.usedCount < Math.Abs(curse.getMaxNegativeInfluence()))
                {
                    double val = 100.0;
                    reasonMsgs?.Add(new ReasonMsg("Base", val));
                    utility += val;

                    if (challengeData.location == society.getCapital())
                    {
                        val = 30;
                        reasonMsgs?.Add(new ReasonMsg("Target is Soverign", val));
                        utility += val;
                    }

                    if (!elder.minions.Any(m => m is M_OrcChampion))
                    {
                        val = -200.0;
                        reasonMsgs?.Add(new ReasonMsg("Unguarded by Champion", val));
                        utility += val;
                    }
                }
            }

            return utility;
        }

        private void populateOrcShamans()
        {
            aiChallenges_Shaman = new List<AIChallenge>
            {
                new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.Rest, AIChallenge.ChallengeTags.RequiresOwnSociety }, false, true),
                new AIChallenge(typeof(Ch_LayLowWilderness), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.ManageMenaceProfile, AIChallenge.ChallengeTags.RequiresOwnSociety }, false, true),
                new AIChallenge(typeof(Ch_SecretsOfDeath), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal, AIChallenge.ChallengeTags.ForbidWar }, false, true),
                new AIChallenge(typeof(Ch_LearnSecret), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal, AIChallenge.ChallengeTags.ForbidWar }, false, true),
                new AIChallenge(typeof(Mg_SkeletalServitor), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal }, false, true),
                new AIChallenge(typeof(Mg_FacelessServitor), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal }, false, true),
                new AIChallenge(typeof(Mg_EnslaveTheDead), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal }, false, true),
                new AIChallenge(typeof(Mg_RavenousDead), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal }, false, true),
                new AIChallenge(typeof(Ch_DeathsShadow), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor }),
                new AIChallenge(typeof(Ch_Orcs_WarFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.RequiresOwnSociety }, false, true),
                new AIChallenge(typeof(Rt_Orcs_SacrificialSite), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.RequiresSociety, AIChallenge.ChallengeTags.ForbidWar, AIChallenge.ChallengeTags.PreferLocal }, false, true),
                new AIChallenge(typeof(Ch_Orcs_DeathFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.RequiresSociety, AIChallenge.ChallengeTags.PreferLocal, AIChallenge.ChallengeTags.ForbidWar }, false, true),
                new AIChallenge(typeof(Ch_Orcs_AccessPlunder), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor }, false, true),
                new AIChallenge(typeof(Ch_BuyItem), 0.0, new List<AIChallenge.ChallengeTags> {  AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocalRandomized}, false, true),
                new AIChallenge(typeof(Rt_StudyDeath), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.RequiresOwnSociety}, false, true),
                new AIChallenge(typeof(Ch_Orcs_DrinkGrott), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                new AIChallenge(typeof(Ch_Orcs_RefillDrinkingHorns), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }, false, true),
                new AIChallenge(typeof(Ch_DrinkPrimalWaters), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocal }, false, true),
                new AIChallenge(typeof(Rti_RouseHorde), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true),
                new AIChallenge(typeof(Rt_StudyBlood), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true),
                new AIChallenge(typeof(Rt_StudyGeomancy), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true),
                new AIChallenge(typeof(Rti_Orc_CeaseWar), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }, false, true)
            };

            aiChallenges_Shaman[2].delegates_ValidFor.Add(delegate_ValidFor_SecretsOfDeath);
            aiChallenges_Shaman[2].delegates_Utility.Add(delegate_Utility_SecretsOfDeath);
            aiChallenges_Shaman[3].delegates_ValidFor.Add(delegate_ValidFor_LearnArcaneSecret);
            aiChallenges_Shaman[3].delegates_Utility.Add(delegate_Utility_LearnSecret);
            aiChallenges_Shaman[4].delegates_ValidFor.Add(delegate_ValidFor_SkeletalServitor);
            aiChallenges_Shaman[4].delegates_Utility.Add(delegate_Utility_SkeletalServitor);
            aiChallenges_Shaman[5].delegates_ValidFor.Add(delegate_ValidFor_FacelessServitor);
            aiChallenges_Shaman[5].delegates_Utility.Add(delegate_Utility_FacelessServitor);
            aiChallenges_Shaman[6].delegates_ValidFor.Add(delegate_ValidFor_EnslaveDead);
            aiChallenges_Shaman[6].delegates_Utility.Add(delegate_Utility_EnslaveDead);
            aiChallenges_Shaman[7].delegates_ValidFor.Add(delegate_ValidFor_RavenousDead);
            aiChallenges_Shaman[7].delegates_Utility.Add(delegate_Utility_RavenousDead);
            aiChallenges_Shaman[8].delegates_ValidFor.Add(delegate_ValidFor_DeathsShadow);
            aiChallenges_Shaman[8].delegates_Utility.Add(delegate_Utility_DeathsShadow);
            aiChallenges_Shaman[10].delegates_ValidFor.Add(delegate_ValidFor_SacrificialSite);
            aiChallenges_Shaman[10].delegates_Utility.Add(delegate_Utility_SacrificialSite);
            aiChallenges_Shaman[12].delegates_Utility.Add(delegate_Utility_AccessPlunder_Shaman);

            comLibAI.RegisterAgentType(typeof(UAEN_OrcShaman), new AgentAI.ControlParameters(true));
            comLibAI.AddChallengesToAgentType(typeof(UAEN_OrcShaman), aiChallenges_Shaman);

            if (comLibAI.TryGetAgentType(typeof(UAEN_OrcShaman), out AgentAI.AIData aiData) && aiData != null)
            {
                aiData.aiChallenges_UniversalDelegates_ValidFor.Add(delegate_ValidFor_Underground);
            }
        }

        private bool delegate_ValidFor_SecretsOfDeath(AgentAI.ChallengeData challengeData, UA ua)
        {
            T_MasteryDeath deathMastery = (T_MasteryDeath)ua.person.traits.FirstOrDefault(t => t is T_MasteryDeath);

            if (deathMastery == null)
            {
                deathMastery = new T_MasteryDeath();
                deathMastery.level = 2;
                ua.person.receiveTrait(deathMastery);
            }

            if (deathMastery.level < deathMastery.getMaxLevel())
            {
                return true;
            }

            return false;
        }

        private bool delegate_ValidFor_SkeletalServitor(AgentAI.ChallengeData challengeData, UA ua)
        {
            if (ua.minions.All(m => m != null) || ua.getStatCommandLimit() <= ua.getCurrentlyUsedCommand())
            {
                return false;
            }

            Pr_Death death = (Pr_Death)challengeData.location.properties.FirstOrDefault(pr => pr is Pr_Death);
            if (death == null)
            {
                return false;
            }

            int turns = ua.map.getStepDist(ua.location, challengeData.location) + (int)Math.Ceiling(challengeData.challenge.getComplexity() / challengeData.challenge.getProgressPerTurnInner(ua, null)) + 2;
            double decay = 0.5;

            int catacombCount = 0;
            foreach (Location neighbour in challengeData.location.getNeighbours())
            {
                if (neighbour.settlement != null)
                {
                    foreach (Subsettlement sub in neighbour.settlement.subs)
                    {
                        if (sub is Sub_Catacombs)
                        {
                            catacombCount++;
                        }
                    }
                }
            }
            decay += catacombCount * 5;

            if (death.charge - (decay * turns) < ua.map.param.mg_skeletalServitorCost)
            {
                return false;
            }

            return true;
        }

        private double delegate_Utility_SkeletalServitor(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            if (challengeData.challenge is Mg_SkeletalServitor skel)
            {
                double val = skel.exemplar.getCommandCost() * map.param.utility_UA_recruitPerPoint;
                if (reasonMsgs != null)
                {
                    ReasonMsg msg = reasonMsgs.FirstOrDefault(rm => rm.msg.Contains("Would gain "));
                    if (msg != null)
                    {
                        msg.value += val;
                    }
                }
                utility += val;
            }

            return utility;
        }

        private bool delegate_ValidFor_FacelessServitor(AgentAI.ChallengeData challengeData, UA ua)
        {
            if (ua.minions.All(m => m != null) || ua.getStatCommandLimit() <= ua.getCurrentlyUsedCommand())
            {
                return false;
            }

            Pr_Death death = (Pr_Death)challengeData.location.properties.FirstOrDefault(pr => pr is Pr_Death);
            if (death == null)
            {
                return false;
            }

            int turns = ua.map.getStepDist(ua.location, challengeData.location) + (int)Math.Ceiling(challengeData.challenge.getComplexity() / challengeData.challenge.getProgressPerTurnInner(ua, null)) + 2;
            double decay = 0.5;

            int catacombCount = 0;
            foreach (Location neighbour in challengeData.location.getNeighbours())
            {
                if (neighbour.settlement != null)
                {
                    foreach (Subsettlement sub in neighbour.settlement.subs)
                    {
                        if (sub is Sub_Catacombs)
                        {
                            catacombCount++;
                        }
                    }
                }
            }
            decay += catacombCount * 5;

            if (death.charge - (decay * turns) < ua.map.param.mg_facelessServitorCost)
            {
                return false;
            }

            return true;
        }

        private double delegate_Utility_FacelessServitor(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            if (challengeData.challenge is Mg_FacelessServitor faceless)
            {
                double val = faceless.exemplar.getCommandCost() * map.param.utility_UA_recruitPerPoint;
                if (reasonMsgs != null)
                {
                    ReasonMsg msg = reasonMsgs.FirstOrDefault(rm => rm.msg.Contains("Would gain "));
                    if (msg != null)
                    {
                        msg.value += val;
                    }
                }
                utility += val;

                val = 10;
                reasonMsgs?.Add(new ReasonMsg("Superior Undead", val));
                utility += val;
            }

            return utility;
        }

        private double delegate_Utility_SecretsOfDeath(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            List<Trait> traits = ua.person.traits;
            T_ArcaneKnowledge arcaneKnowledge = (T_ArcaneKnowledge)traits.FirstOrDefault(t => t is T_ArcaneKnowledge);
            T_MasteryDeath deathMastery = (T_MasteryDeath)traits.FirstOrDefault(t => t is T_MasteryDeath);

            if (arcaneKnowledge == null)
            {
                arcaneKnowledge = new T_ArcaneKnowledge();
                arcaneKnowledge.level = 0;
                ua.person.receiveTrait(arcaneKnowledge);
            }

            if (deathMastery == null)
            {
                deathMastery = new T_MasteryDeath();
                deathMastery.level = 2;
                ua.person.receiveTrait(deathMastery);
            }

            if (deathMastery.level < deathMastery.getMaxLevel())
            {
                Rt_StudyDeath studyDeath = (Rt_StudyDeath)ua.rituals.FirstOrDefault(rt => rt is Rt_StudyDeath);
                if (studyDeath != null && arcaneKnowledge.level < studyDeath.getReq(deathMastery.level))
                {
                    reasonMsgs?.Add(new ReasonMsg("Requires Arcane Knowledge", 60));
                    utility += 60;
                }
            }

            return utility;
        }

        private bool delegate_ValidFor_LearnArcaneSecret(AgentAI.ChallengeData challengeData, UA ua)
        {
            T_MasteryDeath deathMastery = (T_MasteryDeath)ua.person.traits.FirstOrDefault(t => t is T_MasteryDeath);
            if (deathMastery == null)
            {
                deathMastery = new T_MasteryDeath();
                deathMastery.level = 2;
                ua.person.receiveTrait(deathMastery);
            }

            if (deathMastery.level < deathMastery.getMaxLevel())
            {
                return true;
            }

            return false;
        }

        private double delegate_Utility_LearnSecret(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            List<Trait> traits = ua.person.traits;
            T_ArcaneKnowledge arcaneKnowledge = (T_ArcaneKnowledge)traits.FirstOrDefault(t => t is T_ArcaneKnowledge);
            T_MasteryDeath deathMastery = (T_MasteryDeath)traits.FirstOrDefault(t => t is T_MasteryDeath);

            if (arcaneKnowledge == null)
            {
                arcaneKnowledge = new T_ArcaneKnowledge();
                arcaneKnowledge.level = 0;
                ua.person.receiveTrait(arcaneKnowledge);
            }

            if (deathMastery == null)
            {
                deathMastery = new T_MasteryDeath();
                deathMastery.level = 2;
                ua.person.receiveTrait(deathMastery);
            }

            if (deathMastery.level < deathMastery.getMaxLevel())
            {
                Rt_StudyDeath studyDeath = (Rt_StudyDeath)ua.rituals.FirstOrDefault(rt => rt is Rt_StudyDeath);
                if (studyDeath != null && arcaneKnowledge.level < studyDeath.getReq(deathMastery.level))
                {
                    reasonMsgs?.Add(new ReasonMsg("Requires Arcane Knowledge", 80));
                    utility += 60;
                }
            }

            return utility;
        }

        private bool delegate_ValidFor_EnslaveDead(AgentAI.ChallengeData challengeData, UA ua)
        {
            T_MasteryDeath deathMastery = (T_MasteryDeath)ua.person.traits.FirstOrDefault(t => t is T_MasteryDeath);

            if (deathMastery == null)
            {
                deathMastery = new T_MasteryDeath();
                deathMastery.level = 2;
                ua.person.receiveTrait(deathMastery);
            }

            if (deathMastery.level >= map.param.mg_ravenousDeadReq)
            {
                return false;
            }

            if (ua.getCurrentlyUsedCommand() < ua.getStatCommandLimit() && ua.minions.Any(m => m == null))
            {
                return false;
            }

            Pr_Death death = (Pr_Death)challengeData.location.properties.FirstOrDefault(pr => pr is Pr_Death);
            if (death == null)
            {
                return false;
            }

            int turns = ua.map.getStepDist(ua.location, challengeData.location) + (int)Math.Ceiling(challengeData.challenge.getComplexity() / challengeData.challenge.getProgressPerTurnInner(ua, null)) + 2;
            double decay = 0.5;

            int catacombCount = 0;
            foreach(Location neighbour in challengeData.location.getNeighbours())
            {
                if (neighbour.settlement != null)
                {
                    foreach (Subsettlement sub in neighbour.settlement.subs)
                    {
                        if (sub is Sub_Catacombs)
                        {
                            catacombCount++;
                        }
                    }
                }
            }
            decay += catacombCount * 5;

            if (death.charge - (decay * turns) <= 0.0)
            {
                return false;
            }

            return true;
        }

        private double delegate_Utility_EnslaveDead(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Pr_Death death = (Pr_Death)challengeData.location.properties.FirstOrDefault(pr => pr is Pr_Death);
            if (death != null)
            {
                double val = Math.Min(death.charge, map.param.ch_releaseFromDeathMax);
                reasonMsgs?.Add(new ReasonMsg("Potential Streangth of Undead", val));
                utility += val;
            }
            else
            {
                reasonMsgs?.Add(new ReasonMsg("Requires Death", -10000));
                utility -= 10000;
            }

            return utility;
        }

        private bool delegate_ValidFor_RavenousDead(AgentAI.ChallengeData challengeData, UA ua)
        {
            if (ua.getCurrentlyUsedCommand() < ua.getStatCommandLimit() && ua.minions.Any(m => m == null))
            {
                return false;
            }

            Pr_Death death = (Pr_Death)challengeData.location.properties.FirstOrDefault(pr => pr is Pr_Death);
            if (death == null)
            {
                return false;
            }

            int turns = ua.map.getStepDist(ua.location, challengeData.location) + (int)Math.Ceiling(challengeData.challenge.getComplexity() / challengeData.challenge.getProgressPerTurnInner(ua, null)) + 2;
            double decay = 0.5;

            int catacombCount = 0;
            foreach (Location neighbour in challengeData.location.getNeighbours())
            {
                if (neighbour.settlement != null)
                {
                    foreach (Subsettlement sub in neighbour.settlement.subs)
                    {
                        if (sub is Sub_Catacombs)
                        {
                            catacombCount++;
                        }
                    }
                }
            }
            decay += catacombCount * 5;

            if (death.charge - (decay * turns) <= 0.0)
            {
                return false;
            }

            return true;
        }

        private double delegate_Utility_RavenousDead(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Location location = challengeData.challenge.location;
            if (challengeData.challenge is Ritual)
            {
                location = challengeData.location;
            }

            Pr_Death death = (Pr_Death)location.properties.FirstOrDefault(pr => pr is Pr_Death);
            if (death != null)
            {
                double val = Math.Min(death.charge, map.param.mg_ravenousDeadMax);
                reasonMsgs?.Add(new ReasonMsg("Potential Streangth of Undead", val));
                utility += val;
            }
            else
            {
                reasonMsgs?.Add(new ReasonMsg("Requires Death", -10000));
                utility -= 10000;
            }

            return utility;
        }

        private bool delegate_ValidFor_DeathsShadow(AgentAI.ChallengeData challengeData, UA ua)
        {
            bool result = false;
            if (ua.society is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && ((orcCulture.tenet_shadowWeaving.status < 0 && ua.person.shadow >= 50) || orcCulture.tenet_shadowWeaving.status == -2))
            {
                result = true;
            }

            if (result)
            {
                Pr_Death death = (Pr_Death)challengeData.location.properties.FirstOrDefault(pr => pr is Pr_Death);
                if (death == null)
                {
                    return false;
                }

                int turns = ua.map.getStepDist(ua.location, challengeData.location) + (int)Math.Ceiling(challengeData.challenge.getComplexity() / challengeData.challenge.getProgressPerTurnInner(ua, null)) + 2;
                double decay = 0.5;

                int catacombCount = 0;
                foreach (Location neighbour in challengeData.location.getNeighbours())
                {
                    if (neighbour.settlement != null)
                    {
                        foreach (Subsettlement sub in neighbour.settlement.subs)
                        {
                            if (sub is Sub_Catacombs)
                            {
                                catacombCount++;
                            }
                        }
                    }
                }
                decay += catacombCount * 5;

                if (death.charge - (decay * turns) < ua.map.param.mg_deathsShadowDeathModifierReq)
                {
                    return false;
                }
            }

            return result;
        }

        private double delegate_Utility_DeathsShadow(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Location location = challengeData.challenge.location;
            if (challengeData.challenge is Ritual)
            {
                location = challengeData.location;
            }

            Pr_Death death = (Pr_Death)location.properties.FirstOrDefault(pr => pr is Pr_Death);
            if (death != null)
            {
                double val = Math.Min(Math.Min(death.charge, 1), 1 - location.settlement.shadow) * 100;
                reasonMsgs?.Add(new ReasonMsg("Potential Shadow", val));
                utility += val;
            }
            else
            {
                reasonMsgs?.Add(new ReasonMsg("Requires Death", -10000));
                utility -= 10000;
            }

            return utility;
        }

        private bool delegate_ValidFor_SacrificialSite(AgentAI.ChallengeData challengeData, UA ua)
        {
            return challengeData.location.settlement is SettlementHuman && challengeData.location.properties.FirstOrDefault(pr => pr is Pr_Orcs_SacrificialSite) == null && challengeData.location.properties.FirstOrDefault(pr => pr is Pr_Devastation)?.charge >= (challengeData.challenge as Rt_Orcs_SacrificialSite)?.minDevastation;
        }

        private double delegate_Utility_SacrificialSite(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            utility += 55.0;
            reasonMsgs?.Add(new ReasonMsg("Base", 55.0));

            if (challengeData.location.settlement is SettlementHuman settlementHuman)
            {
                if (settlementHuman.getSecurity(null) > 0)
                {
                    double val = settlementHuman.getSecurity(null) * -5.0;
                    reasonMsgs?.Add(new ReasonMsg("Security", val));
                    utility += val;
                }
            }
            else
            {
                reasonMsgs?.Add(new ReasonMsg("Invalid Location", -10000.0));
                utility -= 10000.0;
            }

            return utility;
        }

        private double delegate_Utility_AccessPlunder_Shaman(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Pr_OrcPlunder plunder = (challengeData.challenge as Ch_Orcs_AccessPlunder)?.cache;
            int gold;

            if (plunder.gold <= 50)
            {
                gold = plunder.gold;
            }
            else if (plunder.gold / 2.0 <= 50.0)
            {
                gold = 50;
            }
            else
            {
                gold = (int)Math.Floor(plunder.gold / 2.0);
            }

            int goldTarget = 100;
            if (ua.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_MammonClient client && client.status < 0)
            {
                goldTarget += 25;
            }

            double val = Math.Min(gold, goldTarget - ua.person.gold);
            reasonMsgs?.Add(new ReasonMsg("Gold", val));
            utility += val;

            return utility;
        }
    }
}
