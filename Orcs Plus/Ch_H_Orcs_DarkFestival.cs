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
        double shadowPush = 0.02;

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
            return "Gradually enshadows this location and nearby locations and units. Increaces society menace by 1 each turn.";
        }

        public override string getCastFlavour()
        {
            return "An elder leads this camp in a great festival celebrating and worshiping the elder god, exposing all those present to its great and terrible influence.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by a member of an Orc Culture within their own borders. The Orc Culture's Shadow Weaving tenet status must be -2.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            msgs?.Add(new ReasonMsg("Base", 1.0));

            return 1.0;
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

            return utility;
        }

        public override double getComplexity()
        {
            return 5;
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

            if (location.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenets.OfType<H_ShadowWeaving>().FirstOrDefault()?.status == -2)
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
            return ua is UAA_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety == location.soc;
        }

        public override void turnTick(UA ua)
        {
            ua.addProfile(1);
            ua.addMenace(2);

            location.settlement.shadow += 0.1;

            foreach (Unit unit in location.units)
            {
                if (unit is UA agent)
                {
                    agent.person.shadow += shadowPush;
                }
            }

            if (location.soc is SG_Orc orcSociety)
            {
                if (orcSociety.upstart != null && orcSociety.upstart.homeLocation == location.index)
                {
                    orcSociety.upstart.person.shadow += shadowPush;
                }

                orcSociety.menace += 1;
            }

            foreach (Location neighbour in location.getNeighbours())
            {
                if (location.getShadow() > neighbour.getShadow())
                {
                    float deltaShadow = (float)Math.Min(1.0 - location.getShadow(), shadowPush);

                    if (neighbour.settlement != null)
                    {
                        neighbour.settlement.shadow += deltaShadow;
                    }
                    else
                    {
                        neighbour.hex.purity -= deltaShadow;
                    }

                    location.settlement.shadow -= deltaShadow;
                }

                foreach (Unit unit in location.units)
                {
                    if (unit is UA agent)
                    {
                        agent.person.shadow += shadowPush;
                    }
                }

                if (neighbour.soc is SG_Orc orcs)
                {
                    if (orcs.upstart != null && orcs.upstart.homeLocation == location.index)
                    {
                        orcs.upstart.person.shadow += shadowPush;
                    }
                }
            }
        }

        public override bool isChannelled()
        {
            return true;
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
