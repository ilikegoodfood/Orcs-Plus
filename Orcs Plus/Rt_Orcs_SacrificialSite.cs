using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Rt_Orcs_SacrificialSite : Ritual
    {
        public UA parent = null;

        public double minDevastation = 100.0;

        public double initValue = 50.0;

        public Rt_Orcs_SacrificialSite(Location location, UA parent)
            : base(location)
        {
            this.parent = parent;
        }

        public override string getName()
        {
            return "Create Sacrificial Site";
        }

        public override string getDesc()
        {
            return "Create a site to sacrifice the living to the spirits of the ancestors.";
        }

        public override string getCastFlavour()
        {
            return "Carefully crafted from the flesh and bones of the dead, this site is tucked away on the outskirts of a vulnerable settlement, silently wearing thin the barriers between the living and the dead.";
        }

        public override string getRestriction()
        {
            return "Requires to be in a human settlement with " + minDevastation + ", or greater, devastation.";
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.skull;
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 0.0;

            double val = unit.getStatLore();

            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", val));
                result += val;
            }
            else
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                result = 1.0;
            }

            return result;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;

            if (ua is UAEN_OrcShaman shaman && ua.location.settlement is SettlementHuman && ua.location.properties.OfType<Pr_Devastation>().FirstOrDefault()?.charge >= minDevastation)
            {
                msgs?.Add(new ReasonMsg("Base", 25.0));
                utility += 25.0;
            }

            return utility;
        }

        public override bool valid()
        {
            return true;
        }

        public override bool validFor(UA ua)
        {
            return ua is UAEN_OrcShaman && ua.location.settlement is SettlementHuman && ua.location.properties.OfType<Pr_Devastation>().FirstOrDefault()?.charge >= minDevastation && ua.location.properties.OfType<Pr_Orcs_SacrificialSite>().FirstOrDefault() == null;
        }

        public override int getCompletionMenace()
        {
            return 8;
        }

        public override int getCompletionProfile()
        {
            return 3;
        }

        public override double getComplexity()
        {
            return 20.0;
        }

        public override void complete(UA u)
        {
            Pr_Orcs_SacrificialSite site = u.location.properties.OfType<Pr_Orcs_SacrificialSite>().FirstOrDefault();
            if (site == null)
            {
                site = new Pr_Orcs_SacrificialSite(u.location);
                site.charge = initValue;
                u.location.properties.Add(site);
            }
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override int[] buildPositiveTags()
        {
            return new int[] {
                Tags.CRUEL,
                Tags.ORC,
                Tags.UNDEAD
            };
        }

        public override int[] buildNegativeTags()
        {
            return new int[] {
                Tags.COOPERATION
            };
        }
    }
}
