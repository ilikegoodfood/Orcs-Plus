using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_DeathFestival : Challenge
    {
        public double deathRate = 1.0;

        public double devastationRate = 0.35;

        public Ch_Orcs_DeathFestival(Location location)
            : base(location)
        {
            
        }

        public override string getName()
        {
            return "Sacrificial Festival";
        }

        public override string getDesc()
        {
            return "This festival gradually creates death and devastation at this location. Increases society menace each turn.";
        }

        public override string getCastFlavour()
        {
            return "A shaman has desperate villagers captured from the nearby settlement, brought to his hidden sacrificial site, and one by one, slaughtered in the names of the ancestors.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by an orc shaman in a human or elf settlement.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = unit.getStatLore();

            if (result < 1)
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                result = 1.0;
            }
            else
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", result));

                if (location.settlement is SettlementHuman settlementHuman && settlementHuman.getSecurity(null) > 0)
                {
                    double val = 0.5 * settlementHuman.getSecurity(null);
                    if (result - val > 1.0)
                    {
                        msgs?.Add(new ReasonMsg("Security", -val));
                        result -= val;
                    }
                    else
                    {
                        val = result - 1;

                        if (val > 0)
                        {
                            msgs?.Add(new ReasonMsg("Security (penalty capped)", -val));
                            result -= val;
                        }
                    }
                }
            }

            return result;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 45.0;

            msgs?.Add(new ReasonMsg("Base", utility));

            return utility;
        }

        public override double getComplexity()
        {
            return 40.0;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.skull;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            return location.settlement is SettlementHuman;
        }

        public override bool validFor(UA ua)
        {
            return ua is UAEN_OrcShaman;
        }

        public override void turnTick(UA ua)
        {
            ua.addProfile(1);
            ua.addMenace(2);

            Property.addToProperty(getName(), Property.standardProperties.DEATH, deathRate * getProgressPerTurnInner(ua, null), location);
            Property.addToProperty(getName(), Property.standardProperties.DEVASTATION, devastationRate * getProgressPerTurnInner(ua, null), location);

            Pr_Orcs_SacrificialSite site = ua.location.properties.OfType<Pr_Orcs_SacrificialSite>().FirstOrDefault();
            if (site != null)
            {
                site.influences.Add(new ReasonMsg("In Use", -2.0));
            }

            if (ua.society is SG_Orc orcSociety)
            {
                orcSociety.menace += 1.0;
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.CRUEL,
                Tags.ORC,
                Tags.UNDEAD
            };
        }
    }
}
