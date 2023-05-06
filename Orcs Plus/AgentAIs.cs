using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;

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
            comLibAI = ModCore.core.comLibAI;

            populateOrcUpstarts();

            populateOrcElders();

            populateOrcShamans();
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

        // Orc Upstart
        private void populateOrcUpstarts()
        {
            if (comLibAI.TryGetAgentType(typeof(UAEN_OrcUpstart)))
            {
                aiChallenges_Upstart = new List<AIChallenge>
                {
                    new AIChallenge(typeof(Rti_RecruitWarband), 0.0),
                    new AIChallenge(typeof(Ch_Orcs_FundHorde), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.RequiresOwnSociety }),
                    new AIChallenge(typeof(Ch_Orcs_RaidOutpost), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.RequiresOwnSociety }),
                    new AIChallenge(typeof(Ch_BuyItem), 0.0, new List<AIChallenge.ChallengeTags> {  AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocalRandomized})
                };

                aiChallenges_Upstart[0].delegates_ValidFor.Add(delegate_ValidFor_RecruitWarband);
                aiChallenges_Upstart[0].delegates_Utility.Add(delegate_Utility_RecruitWarband);

                aiChallenges_Upstart[1].delegates_ValidFor.Add(delegate_ValidFor_FundHorde);
                aiChallenges_Upstart[1].delegates_Utility.Add(delegate_Utility_FundHorde);

                aiChallenges_Upstart[3].delegates_ValidFor.Add(delegate_ValidFor_BuyItem);

                comLibAI.AddChallengesToAgentType(typeof(UAEN_OrcUpstart), aiChallenges_Upstart);
            }
        }


        // Orc Upstarts Delegates
        private bool delegate_ValidFor_RecruitWarband(AgentAI.ChallengeData challengeData, UA ua)
        {
            bool hasFullMinions = ua.getStatCommandLimit() - ua.getCurrentlyUsedCommand() <= 0;
            Set_OrcCamp camp = ua.location.settlement as Set_OrcCamp;

            if (ua.isCommandable() && !(camp?.isInfiltrated ?? false))
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

                if (minionCount == 3)
                {
                    hasFullMinions = true;
                }
            }

            I_HordeBanner banner = ua.person.items.OfType<I_HordeBanner>().FirstOrDefault();

            return !hasFullMinions && challengeData.location.soc != null && banner != null && challengeData.location.soc == banner.orcs && challengeData.location.settlement is Set_OrcCamp;
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

                reasonMsgs.Add(new ReasonMsg(reason, val));
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
            reasonMsgs.Add(new ReasonMsg("Excess Gold", val));
            utility += val;

            return utility;
        }

        private void populateOrcElders()
        {
            aiChallenges_Elder = new List<AIChallenge>
            {
                new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.Rest }),
                new AIChallenge(typeof(Ch_LayLowWilderness), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.ManageMenaceProfile }),
                new AIChallenge(typeof(Ch_H_Orcs_ReprimandUpstart), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }),
                new AIChallenge(typeof(Ch_Orcs_RetreatToTheHills), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor }),
                new AIChallenge(typeof(Ch_Orcs_AccessPlunder), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor }),
                new AIChallenge(typeof(Ch_H_Orcs_CleansingFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }),
                new AIChallenge(typeof(Ch_H_Orcs_DarkFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }),
                new AIChallenge(typeof(Rt_H_Orcs_GiftGold), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocal, AIChallenge.ChallengeTags.ForbidWar }),
                new AIChallenge(typeof(Ch_Orcs_FundWaystation), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility }),
                new AIChallenge(typeof(Ch_BuyItem), 0.0, new List<AIChallenge.ChallengeTags> {  AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocalRandomized})
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

            aiChallenges_Elder[9].delegates_ValidFor.Add(delegate_ValidFor_BuyItem);

            comLibAI.RegisterAgentType(typeof(UAA_OrcElder), AgentAI.ControlParameters.newDefault());
            comLibAI.AddChallengesToAgentType(typeof(UAA_OrcElder), aiChallenges_Elder);
        }

        private bool delegate_ValidFor_OwnCulture(AgentAI.ChallengeData challengeData, UA ua)
        {
            if (ua is UAA_OrcElder elder && (elder.society as HolyOrder_Orcs)?.orcSociety == challengeData.location.soc)
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
                        Pr_OrcishIndustry industry = challengeData.location.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();
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
                                Pr_OrcishIndustry industry = neighbour.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();
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
            int gold = 0;

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

            double val = Math.Min(gold, 150 - ua.person.gold);
            reasonMsgs?.Add(new ReasonMsg("Gold", val));
            utility += val;

            return utility;
        }

        private void populateOrcShamans()
        {
            aiChallenges_Shaman = new List<AIChallenge>
            {
                new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.Rest, AIChallenge.ChallengeTags.RequiresOwnSociety }),
                new AIChallenge(typeof(Ch_LayLowWilderness), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.ManageMenaceProfile, AIChallenge.ChallengeTags.RequiresOwnSociety }),
                new AIChallenge(typeof(Ch_SecretsOfDeath), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal, AIChallenge.ChallengeTags.ForbidWar }),
                new AIChallenge(typeof(Ch_LearnSecret), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal, AIChallenge.ChallengeTags.ForbidWar }),
                new AIChallenge(typeof(Mg_SkeletalServitor), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal }),
                new AIChallenge(typeof(Mg_FacelessServitor), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal }),
                new AIChallenge(typeof(Mg_EnslaveTheDead), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal }),
                new AIChallenge(typeof(Mg_RavenousDead), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferOwnSociety, AIChallenge.ChallengeTags.PreferLocal }),
                new AIChallenge(typeof(Ch_DeathsShadow), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor }),
                new AIChallenge(typeof(Ch_Orcs_WarFestival), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.RequiresOwnSociety })
            };

            aiChallenges_Shaman[2].delegates_Utility.Add(delegate_Utility_SecretsOfDeath);
            aiChallenges_Shaman[3].delegates_Utility.Add(delegate_Utility_LearnSecret);
            aiChallenges_Shaman[6].delegates_ValidFor.Add(delegate_ValidFor_EnslaveDead);
            aiChallenges_Shaman[6].delegates_Utility.Add(delegate_Utility_EnslaveDead);
            aiChallenges_Shaman[7].delegates_Utility.Add(delegate_Utility_RavenousDead);
            aiChallenges_Shaman[8].delegates_ValidFor.Add(delegate_ValidFor_DeathsShadow);
            aiChallenges_Shaman[8].delegates_Utility.Add(delegate_Utility_DeathsShadow);

            comLibAI.RegisterAgentType(typeof(UAEN_OrcShaman), AgentAI.ControlParameters.newDefault());
            comLibAI.AddChallengesToAgentType(typeof(UAEN_OrcShaman), aiChallenges_Shaman);
        }

        private double delegate_Utility_SecretsOfDeath(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            List<Trait> traits = ua.person.traits;
            T_ArcaneKnowledge arcaneKnowledge = traits.OfType<T_ArcaneKnowledge>().FirstOrDefault();
            T_MasteryDeath deathMastery = traits.OfType<T_MasteryDeath>().FirstOrDefault();

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
                Rt_StudyDeath studyDeath = ua.rituals.OfType<Rt_StudyDeath>().FirstOrDefault();
                if (studyDeath != null && arcaneKnowledge.level < studyDeath.getReq(deathMastery.level))
                {
                    reasonMsgs?.Add(new ReasonMsg("Requires Arcane Knowledge", 60));
                    utility += 100;
                }
            }

            return utility;
        }

        private double delegate_Utility_LearnSecret(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            List<Trait> traits = ua.person.traits;
            T_ArcaneKnowledge arcaneKnowledge = traits.OfType<T_ArcaneKnowledge>().FirstOrDefault();
            T_MasteryDeath deathMastery = traits.OfType<T_MasteryDeath>().FirstOrDefault();

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
                Rt_StudyDeath studyDeath = ua.rituals.OfType<Rt_StudyDeath>().FirstOrDefault();
                if (studyDeath != null && arcaneKnowledge.level < studyDeath.getReq(deathMastery.level))
                {
                    reasonMsgs?.Add(new ReasonMsg("Requires Arcane Knowledge", 100));
                    utility += 100;
                }
            }

            return utility;
        }

        private bool delegate_ValidFor_EnslaveDead(AgentAI.ChallengeData challengeData, UA ua)
        {
            T_MasteryDeath deathMastery = ua.person.traits.OfType<T_MasteryDeath>().FirstOrDefault();

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

            return true;
        }

        private double delegate_Utility_EnslaveDead(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Location location = challengeData.challenge.location;
            if (challengeData.challenge is Ritual)
            {
                location = challengeData.location;
            }

            Pr_Death death = location.properties.OfType<Pr_Death>().FirstOrDefault();
            if (death != null)
            {
                double val = Math.Min(death.charge * 100, map.param.ch_releaseFromDeathMax) * 2;
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

        private double delegate_Utility_RavenousDead(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Location location = challengeData.challenge.location;
            if (challengeData.challenge is Ritual)
            {
                location = challengeData.location;
            }

            Pr_Death death = location.properties.OfType<Pr_Death>().FirstOrDefault();
            if (death != null)
            {
                double val = Math.Min(death.charge * 100, map.param.mg_ravenousDeadMax);
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
            if (ua.society is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && ((orcCulture.tenet_shadowWeaving.status < 0 && ua.person.shadow >= 50) || orcCulture.tenet_shadowWeaving.status == -2))
            {
                return true;
            }

            return false;
        }

        private double delegate_Utility_DeathsShadow(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Location location = challengeData.challenge.location;
            if (challengeData.challenge is Ritual)
            {
                location = challengeData.location;
            }

            Pr_Death death = location.properties.OfType<Pr_Death>().FirstOrDefault();
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
    }
}
