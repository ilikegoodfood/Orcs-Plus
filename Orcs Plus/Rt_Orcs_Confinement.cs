using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Rt_Orcs_Confinement : Ritual
    {
        public Rt_Orcs_Confinement(Location location)
            : base(location)
        {

        }

        public override string getName()
        {
            return "Confined to Camp";
        }

        public override string getDesc()
        {
            return "The upstart is confined to the camp by the elders.";
        }

        public override string getCastFlavour()
        {
            return "The elders becon and the young come running. The veterans of conflicts passed speak, and the young listen. \"Quite!\", they cry, \"You are too loud. Quite yourself.\". Silence reigns, at least for a time.";
        }

        public override string getRestriction()
        {
            return "Cannot self-confine.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override double getComplexity()
        {
            return 10.0;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            msgs?.Add(new ReasonMsg("Base", 1.0));
            return 1.0;
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Icon_GreatHall.png");
        }

        public override bool validFor(UA ua)
        {
            SG_Orc orcSociety = ua.society as SG_Orc;
            HolyOrder_Orcs orcCulture = ua.society as HolyOrder_Orcs;

            if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
            }

            if (orcSociety != null && ua.location.soc == orcSociety && ua.location.settlement is Set_OrcCamp)
            { 
                if ((ua.task is Task_PerformChallenge challenge && challenge.challenge == this) || (ua.task is Task_GoToPerformChallenge goChallenge && goChallenge.challenge == this) || (ua.task is Task_GoToPerformChallengeAtLocation goLocChallenge && goLocChallenge.challenge == this))
                {
                    return true;
                }
            }

            return false;
        }

        public override void turnTick(UA ua)
        {
            bool profile = ua.inner_profile > ua.inner_profileMin;
            bool menace = ua.inner_menace > ua.inner_menaceMin;
            double progress = getProgressPerTurn(ua, null);
            if (profile)
            {
                ua.addProfile(Math.Min(-map.param.ch_layLoweReductionPerTurnNonhuman * progress, ua.inner_profile - ua.inner_profileMin));
            }
            if (menace)
            {
                ua.addMenace(Math.Min(-map.param.ch_layLoweReductionPerTurnNonhuman * progress, ua.inner_menace - ua.inner_menaceMin));
            }

            if (!profile && !menace)
            {
                ua.task = null;
            }

            if (ua.hp < ua.maxHp)
            {
                ua.hp += Math.Min(ua.maxHp, (int)Math.Floor(1 * progress));
            }

            if (ua.challengesSinceRest > 0)
            {
                ua.challengesSinceRest -= Math.Max(0, (int)Math.Floor(1 * progress));
            }

            foreach (Minion minion in ua.minions)
            {
                if (minion != null && minion.hp < minion.getMaxHP() && minion.getTags().FirstOrDefault(i => i == Tags.UNDEAD) == 0)
                {
                    minion.hp += Math.Min(minion.getMaxHP(), (int)Math.Floor(1 * progress));
                }
            }
        }

        public override bool npcOnly()
        {
            return true;
        }

        public override int[] buildPositiveTags()
        {
            return new int[] {
                Tags.COOPERATION
            };
        }

        public override int[] buildNegativeTags()
        {
            return new int[] {
                Tags.AMBITION,
                Tags.COMBAT,
                Tags.CRUEL,
                Tags.DANGER,
                Tags.DISCORD
            };
        }
    }
}
