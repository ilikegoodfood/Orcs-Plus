using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_H_Orcs_BuildTemple : ChallengeHoly
    {
        public int cost = 60;

        public Ch_H_Orcs_BuildTemple(Location location)
            : base(location)
        {

        }

        public override challengeStat getChallengeType()
        {
            return Challenge.challengeStat.COMMAND;
        }

        public override string getName()
        {
            return "Holy: Build Great Hall";
        }

        public override string getDesc()
        {
            return "Builds a great hall in this orc camp.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by an Orc Elder in a specialised camp belonging to their culture. Requires " + cost + " gold.";
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double val = unit.getStatCommand();

            if (val < 1)
            {
                val = 1.0;
                msgs?.Add(new ReasonMsg("Base", val));
            }
            else
            {
                msgs?.Add(new ReasonMsg("Stat: Command", val));
            }

            return val;
        }

        public override double getComplexity()
        {
            return map.param.ch_h_buildtemple_complexity;
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Icon_GreatHall.png");
        }

        public override bool validFor(UA ua)
        {
            return location.soc is SG_Orc orcSociety && ua is UAEN_OrcElder elder && elder.person.gold >= cost && elder.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety == orcSociety;
        }

        public override bool valid()
        {
            return location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.priorityTemples.status > 0 && location.settlement is Set_OrcCamp camp && camp.specialism != 0 && !camp.subs.Any(sub => sub is Sub_Temple);
        }

        public override void complete(UA u)
        {
            HolyOrder_Orcs orcCulture = u.society as HolyOrder_Orcs;

            if (orcCulture != null && location.settlement != null)
            {
                Sub_OrcTemple hall = location.settlement.subs.OfType<Sub_OrcTemple>().FirstOrDefault();

                if (hall == null)
                {
                    hall = new Sub_OrcTemple(location.settlement, orcCulture);
                    location.settlement.subs.Add(hall);
                }
                else if (hall.order != orcCulture)
                {
                   location.settlement.subs.Remove(hall);
                    hall = new Sub_OrcTemple(location.settlement, orcCulture);
                    location.settlement.subs.Add(hall);
                }
                else
                {
                    return;
                }

                u.person.gold -= cost;
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.RELIGION,
                Tags.COOPERATION,
                Tags.ORC
            };
        }
    }
}
