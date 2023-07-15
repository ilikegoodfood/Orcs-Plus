using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_H_Orcs_ReprimandUpstart : ChallengeHoly
    {
        public HolyOrder_Orcs orcCulture;

        public Sub_Temple sub;

        public Ch_H_Orcs_ReprimandUpstart(Sub_Temple sub, Location location)
            : base (location)
        {
            this.sub = sub;
            orcCulture = sub.order as HolyOrder_Orcs;
        }

        public override string getName()
        {
            return "Holy: Reprimand Orc Upstart";
        }

        public override string getDesc()
        {
            return "An elder recalls the upstart to the seat of the elders and reprimands him for his actions. The upstart will rapidly loose menace and profile, and will heal and rest, while they are confined to the camp.";
        }

        public override string getCastFlavour()
        {
            return "The elders beckon and the young come running. The veterans of conflicts passed speak, and the young listen. \"Quite!\", they cry, \"You are too loud. Quite yourself.\". Silence reigns, at least for a time.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by an Orc Elder, in a camp containing a Great Hall or a Seat of the Elders of their culture.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override double getComplexity()
        {
            return orcCulture.orcSociety.upstart?.getStatCommand() * 2 ?? 10;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double result = 0.0;
            double val = -10000.0;

            UAEN_OrcUpstart upstart = null;
            double excessMenaceProfile = 0;

            if (orcCulture != null)
            {
                foreach (UA agent in orcCulture.agents)
                {
                    if (agent is UAEN_OrcUpstart upstart2 && (upstart2.inner_profile > upstart2.inner_profileMin + (10 * map.param.ch_layLoweReductionPerTurnNonhuman) || upstart2.inner_menace > upstart2.inner_menaceMin + (10 * map.param.ch_layLoweReductionPerTurnNonhuman)))
                    {
                        if (!((upstart2.task is Task_PerformChallenge task && task.challenge is Rt_Orcs_Confinement) || (upstart2.task is CommunityLib.Task_GoToPerformChallengeAtLocation task2 && task2.challenge is Rt_Orcs_Confinement) || upstart2.task is Task_Disrupted || upstart2.task is Task_DisruptUA))
                        {
                            if (upstart2.inner_profile - upstart2.inner_profileMin + upstart2.inner_menace - upstart2.inner_menaceMin > excessMenaceProfile)
                            {
                                excessMenaceProfile = upstart2.inner_profile - upstart2.inner_profileMin + upstart2.inner_menace - upstart2.inner_menaceMin;
                                upstart = upstart2;
                            }
                        }
                    }
                }
            }

            if (upstart == null)
            {
                msgs?.Add(new ReasonMsg("Requires Upstart", val));
                return val;
            }

            val = Math.Max(0.0, upstart.inner_profile - upstart.inner_profileMin) / 2;
            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Potential Profile Reduction", val));
                result += val;
            }

            val = Math.Max(0.0, upstart.inner_menace - upstart.inner_menaceMin) / 2;
            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Potential Menace Reduction", val));
                result += val;
            }

            if (upstart.task is Task_PerformChallenge currentTask)
            {
                Challenge challenge = currentTask.challenge;
                val = (currentTask.progress / challenge.getProgressPerTurn(upstart, null)) * 10;
                msgs?.Add(new ReasonMsg("Wasted Challenge Progress", -val));
                result -= val;
            }

            return result;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            msgs?.Add(new ReasonMsg("Stat: Command", unit.getStatCommand()));
            return Math.Max(1.0, unit.getStatCommand());
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Icon_GreatHall.png");
        }

        public override bool validFor(UA ua)
        {
            return ua is UAEN_OrcElder elder && elder.society == orcCulture;
        }

        public override bool valid()
        {
            bool result = false;
            if (location != sub.settlement.location || location.settlement != sub.settlement || location.settlement.subs.OfType<Sub_Temple>().FirstOrDefault() != sub)
            {
                return result;
            }

            if (orcCulture != null)
            {
                foreach (UA agent in orcCulture.agents)
                {
                    if (agent is UAEN_OrcUpstart upstart && (upstart.inner_profile > upstart.inner_profileMin + (10 * map.param.ch_layLoweReductionPerTurnNonhuman) || upstart.inner_menace > upstart.inner_menaceMin + (10 * map.param.ch_layLoweReductionPerTurnNonhuman)))
                    {
                        if (!((upstart.task is Task_PerformChallenge task && task.challenge is Rt_Orcs_Confinement) || (upstart.task is CommunityLib.Task_GoToPerformChallengeAtLocation task2 && task2.challenge is Rt_Orcs_Confinement) || upstart.task is Task_Disrupted || upstart.task is Task_DisruptUA))
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public override void complete(UA u)
        {
            UAEN_OrcUpstart upstart = null;
            double excessMenaceProfile = 0;

            if (orcCulture != null)
            {
                foreach (UA agent in orcCulture.agents)
                {
                    if (agent is UAEN_OrcUpstart upstart2 && (upstart2.inner_profile > upstart2.inner_profileMin + (10 * map.param.ch_layLoweReductionPerTurnNonhuman) || upstart2.inner_menace > upstart2.inner_menaceMin + (10 * map.param.ch_layLoweReductionPerTurnNonhuman)))
                    {
                        if (!((upstart2.task is Task_PerformChallenge task && task.challenge is Rt_Orcs_Confinement) || (upstart2.task is CommunityLib.Task_GoToPerformChallengeAtLocation task2 && task2.challenge is Rt_Orcs_Confinement) || upstart2.task is Task_Disrupted || upstart2.task is Task_DisruptUA))
                        {
                            if (upstart2.inner_profile - upstart2.inner_profileMin + upstart2.inner_menace - upstart2.inner_menaceMin > excessMenaceProfile)
                            {
                                excessMenaceProfile = upstart2.inner_profile - upstart2.inner_profileMin + upstart2.inner_menace - upstart2.inner_menaceMin;
                                upstart = upstart2;
                            }
                        }
                    }
                }
            }

            if (upstart != null)
            {
                Rt_Orcs_Confinement confine = upstart.rituals.OfType<Rt_Orcs_Confinement>().FirstOrDefault();

                if (confine != null)
                {
                    upstart.task = new CommunityLib.Task_GoToPerformChallengeAtLocation(confine, location, true);
                }
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[] { 
                Tags.COOPERATION,
                Tags.RELIGION
            };
        }

        public override int[] buildNegativeTags()
        {
            return new int[] {
                Tags.AMBITION,
                Tags.COMBAT,
                Tags.CRUEL,
                Tags.DISCORD
            };
        }
    }
}
