using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Rt_H_Orcs_GiftGold : Ritual
    {
        public int bribeCost = 40;

        public double bribeEffect = 4;

        public Rt_H_Orcs_GiftGold(Location loc)
            : base (loc)
        {

        }

        public override string getName()
        {
            return "Holy: Orcish Gift";
        }

        public override string getDesc()
        {
            return "The elder gives a handsom gift of " + bribeCost + " gold to a human noble in hopes of improving relations. Reduces society menace by " + bribeEffect + ".";
        }

        public override string getCastFlavour()
        {
            return "Weary of war and violence, wise to their consecuences, and all to aware of the frailty of orc might, the elder approaches the human nobility with deference and an offering few could ignore.";
        }

        public override string getRestriction()
        {
            return "Costs " + bribeCost + " gold.";
        }

        public override Sprite getSprite()
        {
            return this.map.world.iconStore.bribe;
        }

        public override double getComplexity()
        {
            return 40;
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 0.0;
            double val = unit.getStatCommand();

            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Stat: Command", val));
                result += val;
            }

            val = unit.getStatIntrigue();
            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Stat: Intruige", val));
                result += val;
            }

            if (result < 1)
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                result = 1.0;
            }
            

            return result;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = ((ua.society as HolyOrder_Orcs)?.orcSociety.menace ?? -1) * 4;

            if (utility >= 0)
            {
                msgs?.Add(new ReasonMsg("Society Menace", utility));
            }
            else
            {
                utility = 0;
            }

            return utility;
        }

        public override bool validFor(UA ua)
        {
            return ua is UAEN_OrcElder elder && elder.person.gold >= bribeCost && elder.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety.menace > 5 && elder.location.settlement is SettlementHuman;
        }

        public override int getCompletionProfile()
        {
            return 5;
        }

        public override void complete(UA u)
        {
            HolyOrder_Orcs orcCulture = u.society as HolyOrder_Orcs;
            if (orcCulture != null && orcCulture.orcSociety.menace > 5)
            {
                orcCulture.orcSociety.menace -= bribeEffect;
                u.person.gold -= bribeCost;
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.COOPERATION
            };
        }

        public override int[] buildNegativeTags()
        {
            return new int[]
            {
                Tags.COMBAT,
                Tags.CRUEL,
                Tags.DANGER,
                Tags.DISCORD,
                Tags.GOLD
            };
        }
    }
}
