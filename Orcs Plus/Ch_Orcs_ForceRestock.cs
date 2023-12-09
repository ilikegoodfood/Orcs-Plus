using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_ForceRestock : Challenge
    {
        public Sub_Temple sub;

        public Ch_Orcs_ForceRestock(Location loc, Sub_Temple temple)
            : base (loc)
        {
            sub = temple;
        }

        public override string getName()
        {
            return "Force Restock " + sub.getName();
        }

        public override string getDesc()
        {
            return "Pay the orc traders and hagglers to view their alternative stock, changing the items that are on sale. The greater the orcish industry at this camp, the more likely it is that rare items might come on sale";
        }

        public override string getCastFlavour()
        {
            return "Whats you see on offer is not all that is available";
        }

        public override string getRestriction()
        {
            return "Requires " + map.param.ch_forceRestockCost + " <b>gold</b> and the orc camp to be infiltrated";
        }

        public override double getProfile()
        {
            return map.param.ch_forcerestock_aiProfile;
        }

        public override double getMenace()
        {
            return map.param.ch_forcerestock_aiMenace;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.market;
        }

        public override Challenge.challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override bool valid()
        {
            return sub.settlement.infiltration == 1.0;
        }

        public override bool validFor(UA ua)
        {
            return ua.person.gold >= map.param.ch_forceRestockCost;
        }

        public override double getComplexity()
        {
            return map.param.ch_forcerestock_complexity;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double utility = unit.getStatCommand();
            if (utility < 1)
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                utility = 1.0;
            }
            else
            {
                msgs?.Add(new ReasonMsg("Stat: Command", utility));
            }
            return utility;
        }

        public override int getCompletionMenace()
        {
            return 2;
        }

        public override int getCompletionProfile()
        {
            return 0;
        }

        public override void complete(UA u)
        {
            u.person.gold -= map.param.ch_forceRestockCost;
            if (sub is Sub_OrcCultureCapital capital)
            {
                foreach (Ch_Orcs_BuyItem ch_BuyItem in capital.buyChallenges)
                {
                    ch_BuyItem.restock();
                }

                capital.restockTimer = 25;
            }
            else if (sub is Sub_OrcTemple temple)
            {
                foreach (Ch_Orcs_BuyItem ch_BuyItem in temple.buyChallenges)
                {
                    ch_BuyItem.restock();
                }

                temple.restockTimer = 25;
            }
        }
    }
}
