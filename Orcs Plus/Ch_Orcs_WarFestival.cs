using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_WarFestival : Challenge
    {
        public double industryMin = 25;

        public double industryConsumption = 1.7;

        public double industryConversionFactor = 1;

        public Ch_Orcs_WarFestival(Location loc)
            : base(loc)
        {
            
        }

        public override string getName()
        {
            return "Festival of War";
        }

        public override string getDesc()
        {
            return "Gradually reduces orcish industry to produce death. Increases society menace by 1 each turn.";
        }

        public override string getCastFlavour()
        {
            return "A violant celebration of war, warfare, and martial prowess, the orcs gather in dirt rings and pit-arenas, desperate to proove their strength. Blood soaks the dirt and sand on which they fight and die for prestige.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by an orc shaman within their own borders.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
        }

        public override double getComplexity()
        {
            return 15;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double val = unit.getStatLore();

            if (val < 1)
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                val = 1.0;
            }
            else
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", val));
            }

            return val;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double val = 50;
            msgs?.Add(new ReasonMsg("Base", val));

            return val;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.death;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            Pr_Death death = location.properties.OfType<Pr_Death>().FirstOrDefault();
            if (death != null && death.charge >= 300)
            {
                return false;
            }

            double industryCharge = location.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault()?.charge - industryMin ?? 0.0;
            if (industryCharge < 0.05)
            {
                foreach (Location neighbour in location.getNeighbours())
                {
                    if (neighbour.soc == location.soc)
                    {
                        industryCharge += neighbour.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault()?.charge - industryMin ?? 0.0;

                        if (industryCharge >= 0.05)
                        {
                            break;
                        }
                    }
                }
            }

            if (industryCharge < 0.05)
            {
                return false;
            }

            return true;
        }

        public override bool validFor(UA ua)
        {
            return ua is UAEN_OrcShaman shaman && shaman.society is SG_Orc orcSociety && orcSociety == location.soc;
        }

        public override void turnTick(UA ua)
        {
            ua.addProfile(0.5);
            ua.addMenace(1.5);

            Pr_OrcishIndustry industry = location.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();
            double deltaIndustry = 0;
            double val = industryConsumption * getProgressPerTurnInner(ua, null);

            if (industry != null && industry.charge > industryMin)
            {
                if (industry.charge - val <= industryMin)
                {
                    val = industry.charge - industryMin;
                    if (val > 0)
                    {
                        Property.addToProperty(getName(), Property.standardProperties.ORCISH_INDUSTRY, -val, location);
                    }
                }

                deltaIndustry += val;
            }

            foreach (Location neighbour in location.getNeighbours())
            {
                if (neighbour.soc == location.soc)
                {
                    Pr_OrcishIndustry industry2 = neighbour.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();
                    if (industry2 != null && industry2.charge > industryMin)
                    {
                        val = industryConsumption * getProgressPerTurnInner(ua, null);
                        if (industry2.charge - val <= industryMin)
                        {
                            val = industry2.charge - industryMin;
                            Property.addToProperty(getName(), Property.standardProperties.ORCISH_INDUSTRY, -val, neighbour);
                        }
                        deltaIndustry += val;
                    }
                }
            }

            if (deltaIndustry > 0)
            {
                Property.addToProperty(getName(), Property.standardProperties.DEATH, deltaIndustry * industryConversionFactor, location);
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.UNDEAD
            };
        }
    }
}
