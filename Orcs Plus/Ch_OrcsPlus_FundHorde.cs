﻿using Assets.Code;
using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_OrcsPlus_FundHorde : Challenge
    {
        public Set_OrcCamp camp;

        public SG_Orc orcSociety;

        public HolyOrder_OrcsPlus_Orcs orcCulture;

        public Ch_OrcsPlus_FundHorde (Location location, Set_OrcCamp camp) : base(location)
        {
            this.camp = camp;
        }

        public HolyOrder_OrcsPlus_Orcs getOrcCulture()
        {
            orcSociety = camp.location.soc as SG_Orc;

            if (ModCore.data.orcSGCultureMap.ContainsKey(orcSociety))
            {
                orcCulture = ModCore.data.orcSGCultureMap[orcSociety];
            }

            return orcCulture;
        }

        public override string getName()
        {
            return "Fund the " + getOrcCulture().getName();
        }

        public override string getDesc()
        {
            return "Allows you to donate gold to the " + getOrcCulture().getName() + ". You will gain influence over their culture equalt to half of the gold donated.";
        }

        public override string getCastFlavour()
        {
            return "The glitter of gold hold sway even among the clans of the orcs.";
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
            if (u.isCommandable() && !map.automatic)
            {
                ItemToOrcCulture interB = new ItemToOrcCulture(map, getOrcCulture(), u.person);
                map.world.prefabStore.popItemTrade(u.person, interB, "Donate To Orc Horde", 0, -1);
            }
            else
            {
                if (u.isCommandable() && map.automatic)
                {
                    getOrcCulture()?.receiveFunding(u.person, u.person.gold);
                    ModCore.core.TryAddInfluenceGain(getOrcCulture(), new ReasonMsg("Gifted gold", u.person.gold / 2), true);
                    u.person.gold = 0;
                }
                else if (u is UAEN_OrcUpstart)
                {
                    getOrcCulture()?.receiveFunding(u.person, u.person.gold / 2);

                    if (u.society != null && !u.society.isDark() && !u.corrupted)
                    {
                        ModCore.core.TryAddInfluenceGain(getOrcCulture(), new ReasonMsg("Gifted gold", u.person.gold / 4));
                    }

                    u.person.gold /= 2;
                }
                else
                {
                    getOrcCulture()?.receiveFunding(u.person, u.person.gold);
                    
                    if (u.society != null && !u.society.isDark() && !u.corrupted)
                    {
                        ModCore.core.TryAddInfluenceGain(getOrcCulture(), new ReasonMsg("Gifted gold", u.person.gold / 2));
                    }

                    u.person.gold = 0;
                }
            }
        }

        public override bool valid()
        {
            return true;
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
                Tags.ORC
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
