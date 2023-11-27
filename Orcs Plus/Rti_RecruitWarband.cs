using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    internal class Rti_RecruitWarband : Ritual
    {
        public Item banner;

        public Minion exemplar;

        public Rti_RecruitWarband (Location loc, Item banner)
            : base (loc)
        {
            Type t = null;
            if (ModCore.Get().data.tryGetModAssembly("CovensCursesCurios", out ModData.ModIntegrationData intDataCCC) && intDataCCC.assembly != null)
            {
                intDataCCC.typeDict.TryGetValue("Banner", out t);
            }

            if ((banner is I_HordeBanner) || (t != null && (banner.GetType() == t || banner.GetType().IsSubclassOf(t))))
            {
                this.banner = banner;
            }
            else
            {
                this.banner = null;
            }

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
            return 4;
        }

        public override int getCompletionProfile()
        {
            return 2;
        }

        public override bool validFor(UA ua)
        {
            if (banner == null)
            {
                return false;
            }

            bool hasFullMinions = ua.getStatCommandLimit() - ua.getCurrentlyUsedCommand() <= 0;
            Set_OrcCamp camp = ua.location.settlement as Set_OrcCamp;

            if (camp == null || (ua.isCommandable() && !camp.isInfiltrated))
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

            bool result = !hasFullMinions && ua.location.soc != null && ua.location.settlement is Set_OrcCamp;
            if (result && banner is I_HordeBanner hordeBanner && ua.location.soc != hordeBanner.orcs)
            {
                result = false;
            }

            return result;
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
