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

        public AgentAIs(Map map)
        {
            this.map = map;
            comLibAI = ModCore.comLibAI;

            populateOrcUpstarts();

            populateOrcElders();
        }

        private void populateOrcUpstarts()
        {
            if (comLibAI.TryGetAgentType(typeof(UAEN_OrcUpstart)))
            {
                aiChallenges_Upstart = new List<AIChallenge>();

                AIChallenge challenge = new AIChallenge(typeof(Rti_RecruitWarband), 0.0);
                challenge.delegates_ValidFor.Add(delegate_ValidFor_RecruitWarband);
                challenge.delegates_Utility.Add(delegate_Utility_RecruitWarband);
                aiChallenges_Upstart.Add(challenge);

                AIChallenge challenge1 = new AIChallenge(typeof(Ch_Orcs_FundHorde), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.RequiresOwnSociety });
                challenge1.delegates_ValidFor.Add(delegate_ValidFor_FundHorde);
                challenge1.delegates_Utility.Add(delegate_Utility_FundHorde);
                aiChallenges_Upstart.Add(challenge1);

                aiChallenges_Upstart.Add(new AIChallenge(typeof(Ch_Orcs_RaidOutpost), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.RequiresOwnSociety }));

                comLibAI.AddChallengesToAgentType(typeof(UAEN_OrcUpstart), aiChallenges_Upstart);
            }
        }

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
            if (ua.person.gold > 200)
            {
                return true;
            }

            return false;
        }

        private double delegate_Utility_FundHorde(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val = ua.person.gold - 200;
            reasonMsgs.Add(new ReasonMsg("Stash Gold", val));
            utility += val;

            return utility;
        }

        private void populateOrcElders()
        {
            aiChallenges_Elder = new List<AIChallenge>();

            AIChallenge challenge = new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.Rest });
            challenge.delegates_ValidFor.Add(delegate_ValidFor_OwnCulture);
            aiChallenges_Elder.Add(challenge);

            AIChallenge challenge1 = new AIChallenge(typeof(Ch_LayLowWilderness), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.ManageMenaceProfile });
            challenge1.delegates_ValidFor.Add(delegate_ValidFor_OwnCulture);
            aiChallenges_Elder.Add(challenge1);

            AIChallenge challenge2 = new AIChallenge(typeof(Ch_H_Orcs_ReprimandUpstart), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility });
            challenge2.delegates_ValidFor.Add(delegate_ValidFor_OwnCulture);
            aiChallenges_Elder.Add(challenge2);

            AIChallenge challenge3 = new AIChallenge(typeof(Ch_Orcs_RetreatToTheHills), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor });
            challenge3.delegates_Valid.Add(delegate_Valid_Retreat);
            challenge3.delegates_ValidFor.Add(delegate_ValidFor_OwnCulture);
            challenge3.delegates_Utility.Add(delegate_Utility_Retreat);
            aiChallenges_Elder.Add(challenge3);

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

            if (camp != null && orcSociety != null && orcSociety.isAtWar())
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
            double result = 0;
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

                result = (defenceDelta / 2) + (10 * attackerCount);
            }

            return result;
        }
    }
}
