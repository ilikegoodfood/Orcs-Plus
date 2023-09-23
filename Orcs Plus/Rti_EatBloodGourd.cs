using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace Orcs_Plus
{
    public class Rti_EatBloodGourd : Ritual
    {
        public I_BloodGourd caster;

        public Rti_EatBloodGourd(Location loc, I_BloodGourd caster)
            : base(loc)
        {
            this.caster = caster;
        }

        public override string getName()
        {
            return "Eat " + caster.getName();
        }

        public override string getDesc()
        {
            return "Eating the " + caster.getName() + " immediately restores an agent's health to their maximum health, consuming the Gourd of Blood.";
        }

        public override string getCastFlavour()
        {
            return "The thick, meat-like flesh of the gourd is hard to swallow, and the iron-rich blood that fills your mouth is overwhelming, but with each bite consumed, your flesh stitches itself back together, and an immense physical relief floods through you.";
        }

        public override string getRestriction()
        {
            return "Requires the agent to be holding a " + caster.getName();
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = -60;
            msgs?.Add(new ReasonMsg("Gourds of Blood are expensive", utility));

            double val = (1 - (ua.hp / ua.maxHp)) * ua.map.param.utility_UA_heal;
            msgs?.Add(new ReasonMsg("HP Losses", val));
            utility += val;

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

        public override Sprite getSprite()
        {
            return caster.getIconFore();
        }

        public override bool validFor(UM ua)
        {
            return false;
        }

        public override bool validFor(UA ua)
        {
            return ua.hp < ua.maxHp && ua.person != null && ua.person.items.Contains(caster);
        }

        public override void complete(UA u)
        {
            if (u.person != null)
            {
                for (int i = 0; i < u.person.items.Length; i++)
                {
                    if (u.person.items[i] == caster)
                    {
                        u.person.items[i] = null;
                        break;
                    }
                }

                msgString = u.getName() + " regained " + (u.maxHp - u.hp) + " hp.";
                u.hp = u.maxHp;
            }
        }

        public override int isGoodTernary()
        {
            return 0;
        }

        public override int[] buildNegativeTags()
        {
            return new int[] {
                Tags.DANGER,
                Tags.GOLD
            };
        }
    }
}
