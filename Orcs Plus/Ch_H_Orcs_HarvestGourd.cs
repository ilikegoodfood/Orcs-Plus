using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_H_Orcs_HarvestGourd : ChallengeHoly
    {
        public Ch_H_Orcs_HarvestGourd(Location location)
            : base(location)
        {

        }

        public override string getName()
        {
            return "Holy: Harvest Gourd of Blood";
        }

        public override string getDesc()
        {
            return "The Gourd of Blood grants orc warriors the ability to survive and rapidly heal from even the most gruesome of wounds, at least for a time. When this challenge is completed, Vinerva gains " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.RecieveGift] + " influence over the orc culture.";
        }

        public override string getRestriction()
        {
            return "Can only be performed by an Orc Elder in a camp belonging to their culture. The Orc Culture's Life Mother tenet status must be elder aligned.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Foreground_BloodGourd.png");
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override int getInherentDanger()
        {
            Pr_Vinerva_Health giftHealth = (Pr_Vinerva_Health)location.properties.FirstOrDefault(pr => pr is Pr_Vinerva_Health);
            if (giftHealth != null)
            {
                return Math.Max(2, (int)Math.Ceiling(giftHealth.charge / 25.0));
            }

            return 2;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool validFor(UA ua)
        {
            return ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture && orcCulture.tenet_god is H_Orcs_LifeMother life && life.status < 0 && (location.soc == orcCulture.orcSociety || location.soc == orcCulture);
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

            SG_Orc orcSociety = ua.society as SG_Orc;
            HolyOrder_Orcs orcCulture = ua.society as HolyOrder_Orcs;

            if (orcSociety != null || orcCulture != null)
            {
                if (orcSociety == null)
                {
                    orcSociety = orcCulture.orcSociety;
                }

                if (orcCulture == null)
                {
                    ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
                }

                if (orcSociety != null && orcSociety.isAtWar())
                {
                    double val = 50.0;
                    msgs?.Add(new ReasonMsg("Society is at War", val));
                    utility += val;
                }

                if (orcCulture != null && orcCulture.vinerva_HealthDuration > 10)
                {
                    double val = (orcCulture.vinerva_HealthDuration - 10) * -2;
                    msgs?.Add(new ReasonMsg("Already benefitting from gourd of blood", val));
                    utility += val;
                }
            }

            return utility;
        }

        public override double getComplexity()
        {
            return 10.0;
        }

        public override void complete(UA u)
        {
            HolyOrder_Orcs orcCulture = u.society as HolyOrder_Orcs;
            if (orcCulture == null && u.society is SG_Orc orcSociety)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            Pr_Vinerva_Health gourd = u.location.properties.OfType<Pr_Vinerva_Health>().FirstOrDefault();
            if (orcCulture != null && gourd != null)
            {
                gourd.charge -= 50;
                if (gourd.charge <= 0.0)
                {
                    gourd.charge = 0.0;
                    gourd.location.properties.Remove(gourd);
                }

                orcCulture.vinerva_HealthDuration += 10;
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg("Feasted on Gourd of Blood", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RecieveGift]), true);
                orcCulture.orcSociety.menace += 5;
            }
        }

        public override int getCompletionProfile()
        {
            return 6;
        }

        public override int getCompletionMenace()
        {
            return 8;
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.DANGER,
                Tags.ORC
            };
        }
    }
}
