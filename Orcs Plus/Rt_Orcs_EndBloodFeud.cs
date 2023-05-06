using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Rt_Orcs_EndBloodFeud : Ritual
    {
        public int cost = 100;

        public Rt_Orcs_EndBloodFeud(Location location)
            : base (location)
        {

        }

        public override string getName()
        {
            return "Appease the Tribe";
        }

        public override string getDesc()
        {
            return "Pay " + cost + " gold to the tribe to end their fued against you.";
        }

        public override string getCastFlavour()
        {
            return "A sufficiently large sack of gold will cause almost anyone to forget a past grievance... Even an orc.";
        }

        public override string getRestriction()
        {
            return "Costs one hundred gold.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override double getComplexity()
        {
            return 10.0;
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
            else
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                result = 1.0;
            }
            
            return result;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.bribe;
        }

        public override bool valid()
        {
            return true;
        }

        public override bool validFor(UA ua)
        {
            T_BloodFeud feud = ua.person.traits.OfType<T_BloodFeud>().FirstOrDefault(t => t.orcSociety == ua.location.soc);
            return ua.person.gold >= cost && feud != null && ua.location.settlement != null && ua.location.settlement is Set_OrcCamp;
        }

        public override void complete(UA u)
        {
            T_BloodFeud feud = u.person.traits.OfType<T_BloodFeud>().FirstOrDefault(t => t.orcSociety == u.location.soc);
            if (feud != null)
            {
                u.person.traits.Remove(feud);
                u.person.gold -= cost;
                u.rituals.Remove(this);

                Pr_OrcPlunder plunder = u.location.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();
                if (plunder == null)
                {
                    plunder = new Pr_OrcPlunder(u.location);
                }
                plunder.addGold(cost);
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[] {
                Tags.COOPERATION,
                Tags.ORC,
                Tags.RELIGION
            };
        }

        public override int[] buildNegativeTags()
        {
            return new int[] {
                Tags.AMBITION,
                Tags.COMBAT,
                Tags.CRUEL,
                Tags.DISCORD,
                Tags.GOLD
            };
        }
    }
}
