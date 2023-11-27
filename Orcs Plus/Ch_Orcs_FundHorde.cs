using Assets.Code;
using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_FundHorde : Challenge
    {
        public Set_OrcCamp camp;

        public SG_Orc orcSociety;

        public HolyOrder_Orcs orcCulture;

        public Ch_Orcs_FundHorde (Location location, Set_OrcCamp camp) : base(location)
        {
            this.camp = camp;
        }

        public HolyOrder_Orcs getOrcCulture()
        {
            orcSociety = camp.location.soc as SG_Orc;

            if (orcSociety != null && ModCore.Get().data.orcSGCultureMap.ContainsKey(orcSociety))
            {
                orcCulture = ModCore.Get().data.orcSGCultureMap[orcSociety];
            }

            return orcCulture;
        }

        public override string getName()
        {
            return "Fund the " + getOrcCulture()?.getName();
        }

        public override string getDesc()
        {
            return "Allows you to donate gold to the " + getOrcCulture()?.getName() + ". You will gain influence over their culture equal to half of the gold donated.";
        }

        public override string getCastFlavour()
        {
            return "The glitter of gold holds sway even among the clans of the orcs.";
        }

        public override string getRestriction()
        {
            return "Costs at least one gold";
        }

        public override double getProfile()
        {
            return map.param.ch_fundholyorder_aiProfile;
        }

        public override double getMenace()
        {
            return 0.0;
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

        public override double getComplexity()
        {
            return 1.0;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.bribe;
        }

        public override int isGoodTernary()
        {
            return 0;
        }

        public override bool allowMultipleUsers()
        {
            return true;
        }

        public override bool validFor(UA ua)
        {
            return ua.person.gold >= 1;
        }

        public override void complete(UA u)
        {
            if (u.isCommandable())
            {
                if (!map.automatic)
                {
                    map.world.prefabStore.popItemTrade(u.person, new ItemToOrcCulture(map, getOrcCulture(), u.person), "Donate To Orc Horde", 0, -1);
                }
                else
                {
                    getOrcCulture()?.receiveFunding(u.person, u.person.gold);
                    ModCore.Get().TryAddInfluenceGain(getOrcCulture(), new ReasonMsg("Gifted gold", u.person.gold / 2), true);
                    u.person.gold = 0;
                }
            }
            else if (u is UAEN_OrcUpstart)
            {
                u.person.gold /= 2;
                getOrcCulture()?.receiveFunding(u.person, u.person.gold);
            }
            else
            {
                getOrcCulture()?.receiveFunding(u.person, u.person.gold);

                if (u.society != null && !u.society.isDark() && !u.corrupted)
                {
                    ModCore.Get().TryAddInfluenceGain(getOrcCulture(), new ReasonMsg("Gifted gold", u.person.gold / 2));
                }

                u.person.gold = 0;
            }
        }

        public override bool valid()
        {
            return camp.location.settlement == camp && camp.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.ContainsKey(orcSociety) && ModCore.Get().data.orcSGCultureMap[orcSociety] != null;
        }

        public override int getSimplificationLevel()
        {
            return 0;
        }

        public override double getUtility(UA ua, List<ReasonMsg> reasons)
        {
            double result = 0.0;
            double val;
            if (getOrcCulture()?.tenet_alignment.status >= 0)
            {
                val = map.param.ch_fundOutpostBaseMotivation;
                reasons?.Add(new ReasonMsg("Seeks to influence orc culture", val));
                result += val;
            }
            if (getOrcCulture()?.tenet_alignment.status < 0)
            {
                if ((ua.society == null || !ua.society.isDark()) && !ua.corrupted)
                {
                    val = -map.param.ch_fundOutpostBaseMotivation;
                    reasons?.Add(new ReasonMsg("Will not fund evil orc culture", val));
                    result += val;
                }
            }
            val = getOrcCulture()?.plunderValue ?? 0.0;
            reasons?.Add(new ReasonMsg("Existing Funding", -val));
            result -= val;

            return result;
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.COOPERATION,
                Tags.ORC,
                Tags.RELIGION
            };
        }

        public override int[] buildNegativeTags()
        {
            return new int[]
            {
                Tags.GOLD
            };
        }
    }
}
