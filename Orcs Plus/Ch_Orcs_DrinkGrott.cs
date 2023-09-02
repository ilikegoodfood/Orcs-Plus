using Assets.Code;
using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_DrinkGrott : Challenge
    {

        public int damage = 2;

        public Ch_Orcs_DrinkGrott(Location location)
            : base(location)
        {

        }

        public override string getName()
        {
            return "Drink Orc Grott";
        }

        public override string getDesc()
        {
            return "Drinking grott temporarily boosts an agents might and command stats by 1, but if the agent is not an orc, they suffer " + damage + " damage when drinking it. If the agent has a drinking horn, completing this challenge also fills it.";
        }

        public override string getCastFlavour()
        {
            return "Grott is a thick orc beverage that is served scalding hot from a fire. It's taste is reportedly foul beyond measure, but it is said to fortify the body of those who drink it. The drinking of Grott has become a right of passage for young orc warriors.";
        }

        public override string getRestriction()
        {
            return "Requires and infiltrated orc camp, or an agent of the local orc horde. The camp must contain the Seat of the Elders or a Great Hall.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Icon_Grott.png");
        }

        public override double getProfile()
        {
            return 30;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 50.0;
            msgs?.Add(new ReasonMsg("Base", utility));

            T_Grott grott = (T_Grott)ua.person.traits.FirstOrDefault(t => t is T_Grott);
            if (grott != null)
            {
                double val = -10 * grott.duration;
                msgs?.Add(new ReasonMsg("Already affected by Orc Grott", val));
                utility += val;

                if (ua.getStatCommandLimit() <= ua.getCurrentlyUsedCommand())
                {
                    val = 30.0;
                    msgs?.Add(new ReasonMsg("Current Minions Require Grott", val));
                    utility += val;
                }
            }

            if (ua.person.species != ua.map.species_orc)
            {
                if (ua.hp <= damage)
                {
                    double val = -1000;
                    msgs?.Add(new ReasonMsg("Danger (vs my HP)", val));
                    utility += val;
                }
                else
                {
                    double val = -damage * map.param.utility_ua_challengeDangerAversion;
                    msgs?.Add(new ReasonMsg("Danger (vs my HP)", val));
                    utility += val;
                }
            }

            List<Item> emptyHorns = ua.person.items.Where(i => i is I_DrinkingHorn horn && !horn.full).ToList();
            if (emptyHorns.Count > 0)
            {
                double val = 30.0 * emptyHorns.Count;
                msgs?.Add(new ReasonMsg("Refills empty dirnking horns", val));
                utility += val;
            }

            return utility;
        }

        public override bool allowMultipleUsers()
        {
            return true;
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override double getComplexity()
        {
            return 2;
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
            if (location.soc is SG_Orc orcScoiety)
            {
                if (ua.society == location.soc)
                {
                    return true;
                }

                if (ua.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety == location.soc)
                {
                    return true;
                }

                if (!ua.isCommandable() && !ua.society.isDark() && ModCore.core.data.orcSGCultureMap.TryGetValue(orcScoiety, out HolyOrder_Orcs orcCulture2) && orcCulture2.tenet_intolerance.status > 0)
                {
                    return true;
                }
            }

            if (ua.isCommandable() && location.settlement is Set_OrcCamp && location.settlement.isInfiltrated)
            {
                return true;
            }

            return false;
        }

        public override void complete(UA u)
        {
            T_Grott grott = (T_Grott)u.person.traits.FirstOrDefault(t => t is T_Grott);
            if (grott != null)
            {
                grott.duration = map.param.ch_primalWatersDur;
            }
            else
            {
                u.person.traits.Add(new T_Grott(map.param.ch_primalWatersDur));
            }

            List<I_DrinkingHorn> horns = u.person.items.OfType<I_DrinkingHorn>().ToList();
            foreach(I_DrinkingHorn horn in horns)
            {
                if (!horn.full)
                {
                    horn.full = true;
                }
            }

            msgString = u.getName() + " feels envigortated after their drink. The Grott is hot and tasty.";

            if (u.person.species != map.species_orc)
            {
                msgString = u.getName() + " forces down the hot, foul-tasting drink. They feel envirotaed by it, but suffered serious ill effects from the drinking. " + u.getName() + " suffered 2 damage (current health " + u.hp + "/" + u.maxHp + ").";

                u.hp -= damage;
                if (u.hp <= 0)
                {
                    map.addUnifiedMessage(u, null, "Hero killed by danger", u.getName() + " has been killed by the dangers encountered while performing quest " + getName() + ".", UnifiedMessage.messageType.KILLED_BY_DANGER, false);
                    u.die(map, "Killed by the danger of challenge: " + getName());
                }
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
                {
                    Tags.DANGER
                };
        }
    }
}
