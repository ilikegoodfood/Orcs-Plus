using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_H_Orcs_NurtureOrchard : ChallengeHoly
    {
        public Ch_H_Orcs_NurtureOrchard (Location location)
            : base (location)
        {

        }

        public override string getName()
        {
            return "Holy: Nurture Sapling of Life";
        }

        public override string getDesc()
        {
            return "The Sapling of Life can be nurtured and grown into a great orchard, which provides water, food and a stable habitat for settlement. When this challenge is completed, Vinerva gains " + ModCore.core.data.influenceGain[ModData.influenceGainAction.RecieveGift] + " influence over the orc culture.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by an Orc Elder adjacent to lands belonging to their own culture. The Orc Culture's Life Mother tenet status must be elder aligned.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Foreground_SaplingOak.png");
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            bool result = false;

            foreach (Location neighbour in location.getNeighbours())
            {
                if (neighbour.soc is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_LifeMother life && life.status < 0)
                {
                    result = true;
                    break;
                }

                if (neighbour.soc is HolyOrder_Orcs orcCulture2 && orcCulture2.tenet_god is H_Orcs_LifeMother life2 && life2.status < 0)
                {
                    result = true;
                    break;
                }

                Sub_OrcWaystation waystation = neighbour.settlement?.subs.OfType<Sub_OrcWaystation>().FirstOrDefault();
                if (waystation != null && ModCore.core.data.orcSGCultureMap.TryGetValue(waystation.orcSociety, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null && orcCulture3.tenet_god is H_Orcs_LifeMother life3 && life3.status < 0)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public override bool validFor(UA ua)
        {
            bool result = false;

            if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_LifeMother life && life.status < 0)
            {
                foreach (Location neighbour in location.getNeighbours())
                {
                    if (neighbour.soc == orcCulture || neighbour.soc == orcCulture.orcSociety || neighbour.settlement?.subs.OfType<Sub_OrcWaystation>().FirstOrDefault()?.orcSociety == orcCulture.orcSociety)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double val = 1.0;
            msgs?.Add(new ReasonMsg("Base", val));
            
            return val;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 40.0;
            msgs?.Add(new ReasonMsg("Base", utility));

            return utility;
        }

        public override double getComplexity()
        {
            return 10.0;
        }

        public override void turnTick(UA ua)
        {
            ua.addProfile(1);
            ua.addMenace(2);
            

            SG_Orc orcSociety = ua.society as SG_Orc;
            HolyOrder_Orcs orcCulture = ua.society as HolyOrder_Orcs;

            if (orcSociety == null && orcCulture == null)
            {
                return;
            }

            if (orcSociety == null)
            {
                orcSociety = orcCulture.orcSociety;
            }

            orcSociety.menace += 0.5;

            Pr_Vinerva_Life life = location.properties.OfType<Pr_Vinerva_Life>().FirstOrDefault();
            if (life != null)
            {
                life.influences.Add(new ReasonMsg("Tended by " + ua.getName(), 25.0));

                if (life.charge >= 300.0)
                {
                    life.crises(orcSociety);
                    ua.task = null;
                }
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.ORC
            };
        }
    }
}
