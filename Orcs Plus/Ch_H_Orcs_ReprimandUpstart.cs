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

        public Sub_OrcCultureCapital seat;

        public Ch_H_Orcs_ReprimandUpstart(Sub_OrcCultureCapital seat, Location location)
            : base (location)
        {
            this.seat = seat;
            orcCulture = seat.order as HolyOrder_Orcs;
        }

        public override string getName()
        {
            return "Reprimand Orc Upstart";
        }

        public override string getDesc()
        {
            return "Recalls the upstart to the seat of the elders and reprimands him for his actions. The upstart will rapidly loose menace and profile while they are confined to the camp.";
        }

        public override string getCastFlavour()
        {
            return "The elders becon and the young come running. The veterans of conflicts passed speak, and the young listen. \"Quite!\", they cry, \"You are too loud. Quite yourself.\". Silence reigns, at least for a time.";
        }

        public override string getRestriction()
        {
            return "Requires the camp hosting the seat of the elders to be infiltrated.";
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

            UAEN_OrcUpstart upstart = orcCulture.orcSociety.upstart;

            if (upstart == null)
            {
                msgs?.Add(new ReasonMsg("Requires Upstart", val));
                return val;
            }

            val = Math.Max(0.0, upstart.inner_profile - upstart.inner_profileMin);
            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Potential Profile Reduction", val));
                result += val;
            }

            val = Math.Max(0.0, upstart.inner_menace - upstart.inner_menaceMin);
            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Potential Menace Reduction", val));
                result += val;
            }

            if (upstart.task is Task_PerformChallenge task)
            {
                Challenge challenge = task.challenge;
                val = (task.progress / challenge.getProgressPerTurn(upstart, null)) * 10;
                msgs?.Add(new ReasonMsg("Wasted Weeks", -val));
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
            return (ua is UAA_OrcElder elder && elder.society == seat.order) || (ua.isCommandable() && location.settlement is Set_OrcCamp camp && camp.isInfiltrated);
        }

        public override bool valid()
        {
            if (location != seat.settlement.location || location.settlement != seat.settlement || location.settlement.subs.OfType<Sub_OrcCultureCapital>().FirstOrDefault() != seat)
            {
                return false;
            }

            if (orcCulture.orcSociety?.upstart != null)
            {
                UAEN_OrcUpstart upstart = orcCulture.orcSociety.upstart;

                if (upstart.inner_profile > upstart.inner_profileMin + 20 || upstart.inner_menace > upstart.inner_menaceMin + 15)
                {
                    if (!((upstart.task is Task_PerformChallenge task && task.challenge is Rt_Orcs_Confinement) || (upstart.task is CommunityLib.Task_GoToPerformChallengeAtLocation task2 && task2.challenge is Rt_Orcs_Confinement)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void complete(UA u)
        {
            UAEN_OrcUpstart upstart = orcCulture.orcSociety.upstart;
            Rt_Orcs_Confinement confine = upstart.rituals.OfType<Rt_Orcs_Confinement>().FirstOrDefault();

            if (confine != null)
            {
                upstart.task = new CommunityLib.Task_GoToPerformChallengeAtLocation(confine, this.location);
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
