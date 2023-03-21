using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Orcs_Plus
{
    internal class Rti_RecruitWarband : Ritual
    {
        public I_HordeBanner banner;

        public Minion exemplar;

        public Rti_RecruitWarband (Location loc, I_HordeBanner banner)
            : base (loc)
        {
            this.banner = banner;
            exemplar = new M_Goblin(loc.map);
        }

        public override string getName()
        {
            return "Recruit Warband";
        }

        public override string getDesc()
        {
            return "Fills every empty minion slot with a Goblin.";
        }

        public override string getCastFlavour()
        {
            return "War is at hand, and the banner-bearer demands aid.";
        }

        public override string getRestriction()
        {
            return "Requires an orc warbanner and a subjugated orc camp of the same culture as the banner.";
        }

        public override double getProfile()
        {
            return 0.0;
        }

        public override double getMenace()
        {
            return 50.0;
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            int num = 0;
            num += unit.getStatMight();
            msgs?.Add(new ReasonMsg("Stat: Might", unit.getStatMight()));
            num += unit.getStatCommand();
            msgs?.Add(new ReasonMsg("Stat: Command", unit.getStatCommand()));
            if (num < 1)
            {
                num++;
                msgs?.Add(new ReasonMsg("Base", 1.0));
            }

            return num;
        }

        public override double getComplexity()
        {
            return 15.0;
        }

        public override int getCompletionMenace()
        {
            return 0;
        }

        public override int getCompletionProfile()
        {
            return 0;
        }

        public override bool validFor(UA ua)
        {
            bool hasFullMinions = ua.getStatCommandLimit() - ua.getCurrentlyUsedCommand() <= 0;
            Set_OrcCamp camp = ua.location.settlement as Set_OrcCamp;

            if (ua.isCommandable() && !(camp?.isInfiltrated ?? false))
            {
                return false;
            }

            if (!hasFullMinions)
            {
                int minionCount = 0;

                foreach (Minion minion in ua.minions)
                {
                    if (minion != null)
                    {
                        minionCount++;
                    }
                }

                if (minionCount == 3)
                {
                    hasFullMinions = true;
                }
            }

            return !hasFullMinions && ua.location.soc != null && ua.location.soc == banner.orcs && ua.location.settlement is Set_OrcCamp;
        }

        public override bool valid()
        {
            return true;
        }

        public override Sprite getSprite()
        {
            return map.world.textureStore.minion_goblin;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override void complete(UA u)
        {
            int availableCommandLimit = u.getStatCommandLimit() - u.getCurrentlyUsedCommand();
            if (availableCommandLimit <= 0)
            {
                return;
            }

            for (int i = 0; i < u.minions.Length; i++)
            {
                if (availableCommandLimit <= 0)
                {
                    break;
                }

                if (u.minions[i] == null)
                {
                    u.minions[i] = new M_Goblin(map);
                    availableCommandLimit--;
                }
            }

            return;
        }

        public double getMinionUtility(UA ua, Minion m)
        {
            int val = m.getMaxHP() + m.getMaxDefence() + m.getAttack();
            return (double)(val * this.map.param.utility_UA_recruitPerPoint);
        }

        public override int[] buildPositiveTags()
        {
            if (exemplar == null)
            {
                return new int[0];
            }

            return exemplar.getTags();
        }

        public override int[] buildNegativeTags()
        {
            return new int[0];
        }
    }
}
