using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_H_Orcs_DarkFestival : ChallengeHoly
    {
        public double shadowGen = 0.025;

        public double shadowPush = 0.005;

        public Ch_H_Orcs_DarkFestival(Location loc)
            : base(loc)
        {

        }

        public override string getName()
        {
            return "Holy: Dark Festival";
        }

        public override string getDesc()
        {
            return "This festival gradually enshadows this location and nearby locations and units. The Elder gains profile (1) and menace (2) each turn while performing this challenge.";
        }

        public override string getCastFlavour()
        {
            return "An elder leads this camp in a great festival celebrating and worshipping the elder god, exposing all those present to its great and terrible influence.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by an Orc Elder in a camp belonging to their culture. The Orc Culture's Shadow Weaving tenet status must be -2.";
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
            double purity = 1 - location.getShadow();
            double pushShadow = 0.0;

            foreach (Location loc in location.getNeighbours())
            {
                pushShadow += Math.Min(1 - loc.getShadow(), shadowPush * getComplexity());
            }

            double val = Math.Min(1, purity + pushShadow) * 100;
            msgs?.Add(new ReasonMsg("Potential Shadow Spread", val));
            utility += val;

            val = (1 - ua.person.shadow) * 50;
            msgs?.Add(new ReasonMsg("Potential Self Enshadowment", val));
            utility += val;

            SG_Orc orcSociety = ua.society as SG_Orc;
            HolyOrder_Orcs orcCulture = ua.society as HolyOrder_Orcs;
            
            if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
            }

            if (orcSociety != null && orcSociety.menace > 0)
            {
                val = orcSociety.menace * -3;
                msgs?.Add(new ReasonMsg("Society Menace", val));
                utility += val;
            }

            return utility;
        }

        public override double getComplexity()
        {
            return 20;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.enshadow;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            bool result = false;

            if (location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenets.OfType<H_Orcs_ShadowWeaving>().FirstOrDefault()?.status == -2)
            {
                if (location.getShadow() < 1)
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
            ua.addMenace(2);

            double deltaShadow = shadowPush * getProgressPerTurnInner(ua, null);

            if (location.settlement.shadow < 1.0)
            {
                location.settlement.shadow += shadowGen * getProgressPerTurnInner(ua, null);
            }

            foreach (Unit unit in location.units)
            {
                if (unit is UA agent && agent.person.shadow < 1.0)
                {
                    agent.person.shadow += deltaShadow;

                    if (agent.person.shadow > 1.0)
                    {
                        agent.person.shadow = 1.0;
                    }
                }
            }

            if (location.soc is SG_Orc orcSociety)
            {
                foreach (Unit unit in map.units)
                {
                    if (unit.homeLocation == location.index && unit is UA agent && !agent.isCommandable() && agent.person.shadow < 1.0)
                    {
                        agent.person.shadow += deltaShadow;

                        if (agent.person.shadow > 1.0)
                        {
                            agent.person.shadow = 1.0;
                        }
                    }
                }

                orcSociety.menace += 0.5;
            }

            foreach (Location neighbour in location.getNeighbours())
            {
                if (location.getShadow() > neighbour.getShadow())
                {
                    if (neighbour.settlement != null)
                    {
                        neighbour.settlement.shadow += deltaShadow;

                        if (neighbour.settlement.shadow > 1.0)
                        {
                            neighbour.settlement.shadow = 1.0;
                        }
                    }
                    else
                    {
                        neighbour.hex.purity -= (float)deltaShadow;

                        if (neighbour.hex.purity > 1.0f)
                        {
                            neighbour.hex.purity = 1.0f;
                        }
                    }

                    location.settlement.shadow -= deltaShadow;
                }

                foreach (Unit unit in location.units)
                {
                    if (unit is UA agent)
                    {
                        agent.person.shadow += deltaShadow;
                    }
                }

                if (location.settlement.shadow > 1.0)
                {
                    location.settlement.shadow = 1.0;
                }
                else if (location.settlement.shadow < 0.0)
                {
                    location.settlement.shadow = 0.0;
                }
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.SHADOW
            };
        }
    }
}
