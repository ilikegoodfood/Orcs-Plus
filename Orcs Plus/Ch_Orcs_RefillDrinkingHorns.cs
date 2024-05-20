using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_RefillDrinkingHorns : Challenge
    {
        public Ch_Orcs_RefillDrinkingHorns(Location location)
            :base (location)
        {
            
        }

        public override string getName()
        {
            return "Refill Drinking Horns";
        }

        public override string getDesc()
        {
            return "Refills your drinking horns with orc grott, a thick orc beverage that temporarily boosts an agents might and command stats by 1, but if the agent is not an orc, they suffer 2 damage when drinking it.";
        }

        public override string getCastFlavour()
        {
            return "Refill your drinking horns with scalding hot grott from the pot over the fire for later consumption.";
        }

        public override string getRestriction()
        {
            return "Requires an empty drinking horn, and an infiltrated orc camp.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Icon_Grott.png");
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;

            List<Item> emptyHorns = ua.person.items.Where(i => i is I_DrinkingHorn horn && !horn.full).ToList();
            if (emptyHorns.Count > 0)
            {
                double val = 30.0 * emptyHorns.Count;
                msgs?.Add(new ReasonMsg("Refills empty dirnking horns", val));
                utility += val;
            }

            return utility;
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override double getComplexity()
        {
            return 1;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            msgs?.Add(new ReasonMsg("Base", 1.0));
            return 1.0;
        }

        public override bool validFor(UM ua)
        {
            return false;
        }

        public override bool validFor(UA ua)
        {
            bool result = false;
            bool emptyHorn = ua.person.items.Any(i => i is I_DrinkingHorn horn && !horn.full);

            if (emptyHorn)
            {
                if (location.soc is SG_Orc orcScoiety)
                {
                    if (ua.society == location.soc)
                    {
                        result = true;
                    }

                    if (ua.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety == location.soc)
                    {
                        result = true;
                    }

                    if (!ua.isCommandable() && !ua.society.isDark() && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcScoiety, out HolyOrder_Orcs orcCulture2) && orcCulture2.tenet_intolerance.status > 0)
                    {
                        result = true;
                    }
                }

                if (ua.isCommandable())
                {
                    if (ua is UAE_Shaman)
                    {
                        return true;
                    }

                    if (location.settlement is Set_OrcCamp && location.settlement.infiltration == 1.0)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        public override void complete(UA u)
        {
            List<Item> emptyHorns = u.person.items.Where(i => i is I_DrinkingHorn horn && !horn.full).ToList();
            foreach (I_DrinkingHorn emptyHorn in emptyHorns)
            {
                emptyHorn.full = true;
            }
        }
    }
}
