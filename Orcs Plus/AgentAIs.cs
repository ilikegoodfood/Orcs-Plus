using Assets.Code;
using Assets.Code.Modding;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orcs_Plus
{
    public class AgentAIs
    {
        Map map;

        AgentAI comLibAI;

        public AgentAIs(Map map)
        {
            this.map = map;
            comLibAI = ModCore.comLib.GetAgentAI();

            populateOrcUpstarts();
        }

        private void populateOrcUpstarts()
        {
            if (comLibAI.TryGetAgentType(typeof(UAEN_OrcUpstart)))
            {
                AIChallenge challenge = new AIChallenge(typeof(Rti_OrcsPlus_RecruitWarband), 0.0);
                challenge.delegates_ValidFor.Add(delegate_ValidFor_RecruitWarband);
                challenge.delegates_Utility.Add(delegate_Utility_RecruitWarband);
                comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcUpstart), challenge);

                AIChallenge challenge1 = new AIChallenge(typeof(Ch_OrcsPlus_FundHorde), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.RequiresOwnSociety });
                challenge1.delegates_ValidFor.Add(delegate_ValidFor_FundHorde);
                challenge1.delegates_Utility.Add(delegate_Utility_FundHorde);
                comLibAI.AddChallengeToAgentType(typeof(UAEN_OrcUpstart), challenge1);
            }
        }

        private static bool delegate_ValidFor_RecruitWarband(AgentAI.ChallengeData challengeData, UA ua)
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

        private static double delegate_Utility_RecruitWarband(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Rti_OrcsPlus_RecruitWarband recruitWarband = challengeData.challenge as Rti_OrcsPlus_RecruitWarband;
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

        private static bool delegate_ValidFor_FundHorde(AgentAI.ChallengeData challengeDatae, UA ua)
        {
            if (ua.person.gold > 200)
            {
                return true;
            }

            return false;
        }

        private static double delegate_Utility_FundHorde(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val = ua.person.gold - 200;
            reasonMsgs.Add(new ReasonMsg("Stash Gold", val));
            utility += val;

            return utility;
        }
    }
}
