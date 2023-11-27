using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_H_Orcs_CleansingFestival : Challenge
    {
        public double shadowPurge = 0.025;

        public double shadowPull = 0.005;

        public Ch_H_Orcs_CleansingFestival(Location loc)
            : base(loc)
        {
            
        }

        public override string getName()
        {
            return "Holy: Cleansing Festival";
        }

        public override string getDesc()
        {
            return "This festival gradually draws shadow, from nearby locations and units, to this location, and simultaneously cleanses shadow at this location. The Elder gains profile (1) and loses menace (-4) each turn while performing this challenge.";
        }

        public override string getCastFlavour()
        {
            return "An elder leads the camp in a festival of light and purity. Warriors and children dance in the twilight, inhaling the purifying smoke from the great bonfire that stands at the very heart of the camp, driving the shadows away.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by an Orc Elder in a camp belonging to their culture.\r\n The Orc Culture's Shadow Weaving tenet status must be greater than zero.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
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
            double utility = 0.0;
            double shadow = location.getShadow();
            double pullShadow = 0.0;

            foreach (Location loc in location.getNeighbours())
            {
                pullShadow += Math.Min(loc.getShadow(), shadowPull * getComplexity());
            }

            double val = Math.Min(1, shadow + pullShadow) * 100;
            val *= 1 - ua.person.shadow;
            msgs?.Add(new ReasonMsg("Potential Shadow Reduction (reduced by shadow)", val));
            utility += val;

            val = ua.person.shadow * ua.map.param.ch_cleanseOwnSoul_Menace;
            val *= 1 - ua.person.shadow;
            msgs?.Add(new ReasonMsg("Own Shadow (reduced by shadow)", val));
            utility += val;

            return utility;
        }

        public override double getComplexity()
        {
            return 20;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.festival;
        }

        public override int isGoodTernary()
        {
            return 1;
        }

        public override bool valid()
        {
            bool result = false;

            if (location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenets.OfType<H_Orcs_ShadowWeaving>().FirstOrDefault()?.status > 0)
            {
                double shadow = location.getShadow();

                if (shadow > 0.05)
                {
                    result = true;
                }
            }

            return result;
        }

        public override bool validFor(UA ua)
        {
            return ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety == location.soc;
        }

        public override void turnTick(UA ua)
        {
            ua.addProfile(1);
            ua.addMenace(-4);

            double deltaShadow = shadowPull * getProgressPerTurnInner(ua, null);
            double deltaPurge = shadowPurge * getProgressPerTurnInner(ua, null);

            foreach (Location neighbour in location.getNeighbours())
            {
                if (location.settlement.shadow + deltaShadow < 1.0 + deltaPurge)
                {
                    if (neighbour.settlement != null && neighbour.settlement.shadow > 0.0)
                    {
                        neighbour.settlement.shadow -= deltaShadow;

                        if (neighbour.settlement.shadow < 0.0)
                        {
                            neighbour.settlement.shadow = 0.0;
                        }
                    }
                    else if (neighbour.hex.purity < 1.0f)
                    {
                        neighbour.hex.purity += (float)deltaShadow;

                        if (neighbour.hex.purity > 1.0f)
                        {
                            neighbour.hex.purity = 1.0f;
                        }
                    }

                    location.settlement.shadow += deltaShadow;
                }

                foreach (Unit unit in neighbour.units)
                {
                    if (unit is UA agent && !agent.isCommandable() && agent.person.shadow > 0.0)
                    {
                        agent.person.shadow -= deltaShadow;

                        if (agent.person.shadow < 0.0)
                        {
                            agent.person.shadow = 0.0;
                        }
                    }
                }
            }

            location.settlement.shadow -= deltaPurge;

            if (location.settlement.shadow < 0.0)
            {
                location.settlement.shadow = 0.0;
            }

            foreach (Unit unit in location.units)
            {
                if (unit is UA agent && !agent.isCommandable() && agent.person.shadow > 0.0)
                {
                    agent.person.shadow -= shadowPull * getProgressPerTurnInner(ua, null);

                    if (agent.person.shadow < 0.0)
                    {
                        agent.person.shadow = 0.0;
                    }
                }
            }

            if (location.soc is SG_Orc orcSociety)
            {
                foreach(Unit unit in map.units)
                {
                    if (unit.homeLocation == location.index && unit is UA agent && !agent.isCommandable() && agent.person.shadow > 0.0)
                    {
                        agent.person.shadow -= shadowPull * getProgressPerTurnInner(ua, null);

                        if (agent.person.shadow < 0.0)
                        {
                            agent.person.shadow = 0.0;
                        }
                    }
                }

                if (orcSociety.menace > 0.0)
                {
                    orcSociety.menace -= 1;

                    if (orcSociety.menace < 0)
                    {
                        orcSociety.menace = 0;
                    }
                }
            }
        }

        public override int[] buildNegativeTags()
        {
            return new int[]
            {
                Tags.SHADOW
            };
        }
    }
}
