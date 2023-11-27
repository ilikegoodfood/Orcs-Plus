using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_H_Orcs_PerfectionFestival : ChallengeHoly
    {
        public double perfectionRate = 7.5;

        public Ch_H_Orcs_PerfectionFestival (Location location)
            : base (location)
        {

        }

        public override string getName()
        {
            return "Holy: Festival of Perfection";
        }

        public override string getDesc()
        {
            return "This festival gradually raises perfection in this camp. The Elder gains profile (2) and menace (4) each turn while performing this challenge.";
        }

        public override string getCastFlavour()
        {
            return "An elder leads this camp in a celebration of Ophanim's perfect will, inspiring and driving all those who see it to aspire towards that goal with a near-mindless zeal.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by an Orc Elder in a camp belonging to their culture. The Orc Culture's Perfection tenet status must be elder aligned (-1 or lower).";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double val = 1.0;
            msgs?.Add(new ReasonMsg("Base", val));

            return val;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;

            SG_Orc orcSociety = ua.society as SG_Orc;
            HolyOrder_Orcs orcCulture = ua.society as HolyOrder_Orcs;

            if (orcSociety != null || orcCulture != null)
            {
                if (orcSociety == null)
                {
                    orcSociety = orcCulture.orcSociety;
                }
                else if (orcCulture == null)
                {
                    ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
                }
            }

            if (orcCulture != null && orcCulture.tenet_god is H_Orcs_Perfection perfection && perfection.status < 0)
            {
                utility = 20.0;
                msgs?.Add(new ReasonMsg("Base", utility));

                double val = 0.0;
                Pr_Ophanim_Perfection perfectionLocal = location.properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
                if (perfectionLocal == null)
                {
                    if (perfection.status < -1)
                    {
                        val = perfection.thresholdMajor / 3;
                        msgs?.Add(new ReasonMsg("Potential Perfection", val));
                        utility += val;
                    }
                    else
                    {
                        val = perfection.thresholdMinor / 3;
                        msgs?.Add(new ReasonMsg("Potential Perfection", val));
                        utility += val;
                    }
                }
                else
                {
                    if (perfection.status < -1)
                    {
                        val = (perfection.thresholdMajor - perfectionLocal.charge) / 3;
                        msgs?.Add(new ReasonMsg("Potential Perfection", val));
                        utility += val;
                    }
                    else
                    {
                        val = (perfection.thresholdMinor - perfectionLocal.charge) / 3;
                        msgs?.Add(new ReasonMsg("Potential Perfection", val));
                        utility += val;
                    }
                }
            }
            else
            {
                utility = -1000.0;
                msgs?.Add(new ReasonMsg("Doubt", utility));
            }

            return utility;
        }

        public override double getComplexity()
        {
            return 10.0;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.ophanimLight;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            if (location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_Perfection perfection && perfection.status < 0)
            {
                if (location.settlement.subs.Any(sub => sub is Sub_OrcCultureCapital || sub is Sub_OrcTemple))
                {
                    Pr_Ophanim_Perfection perfectionLocal = location.properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
                    if (perfection.status < -1)
                    {
                        if (perfectionLocal == null || perfectionLocal.charge < perfection.thresholdMajor)
                        {
                            return true;
                        }
                    }
                    else if (perfectionLocal == null || perfectionLocal.charge < perfection.thresholdMinor)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool validFor(UA ua)
        {
            return ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety == location.soc;
        }

        public override void turnTick(UA ua)
        {
            ua.addProfile(2);
            ua.addMenace(4);

            Pr_Ophanim_Perfection perfection = location.properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
            if (perfection == null)
            {
                perfection = new Pr_Ophanim_Perfection(location);
                location.properties.Add(perfection);
            }

            perfection.influences.Add(new ReasonMsg("Festival of Perfection", perfectionRate * getProgressPerTurnInner(ua, null)));

            if (location.soc is SG_Orc orcSociety)
            {
                orcSociety.menace += 0.5;
            }

            ModCore.Get().TryAddInfluenceGain(location.soc as SG_Orc, new ReasonMsg(getName(), (ModCore.Get().data.influenceGain[ModData.influenceGainAction.RecieveGift] / 20) * getProgressPerTurnInner(ua, null)), true);
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.AMBITION,
                Tags.COMBAT,
                Tags.ORC,
                Tags.RELIGION
            };
        }
    }
}
