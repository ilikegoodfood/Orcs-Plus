using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Rti_DrinkGrott : Ritual
    {
        public int damage = 2;

        public I_DrinkingHorn caster;

        public Rti_DrinkGrott(Location location, I_DrinkingHorn caster)
            :base(location)
        {
            this.caster = caster;
        }

        public override string getName()
        {
            return "Drink Orc Grott";
        }

        public override string getDesc()
        {
            return "Drinking grott temporarily boosts an agent's might and command stats by 1, but if the agent is not an orc, they suffer " + damage + " damage when drinking it. If the agent has a drinking horn, completing this challenge also fills it.";
        }

        public override string getCastFlavour()
        {
            return "Grott is a thick orc beverage that is served scalding hot from a fire. Its taste is reportedly foul beyond measure, but it is said to fortify the body of those who drink it. When travelling beyond their homeland, orcs will sometimes transport grott in drinking horns, such as this one, for later consumption.";
        }

        public override string getRestriction()
        {
            return "Requires a drinking horn full of grott.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Foreground_DrinkingHorn_Full.png");
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 100.0;
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

        public override bool valid()
        {
            return caster.full;
        }

        public override bool validFor(UM ua)
        {
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

            caster.full = false;
            msgString = u.getName() + " feels envigorated after their drink. The Grott is hot and tasty.";

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
