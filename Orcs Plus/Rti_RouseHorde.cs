using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Rti_RouseHorde : Ritual
    {
        public Item banner;

        public Rti_RouseHorde (Location loc, Item banner)
            : base(loc)
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
        }

        public override string getName()
        {
            return "Rouse the Horde";
        }

        public override string getDesc()
        {
            return "Raise an orc rabble from the population of this camp, destroying it. The rabble will have hp equal to the orcish industry in the camp, and will disperse over time.";
        }

        public override string getCastFlavour()
        {
            return "The orc living in this camp are driven to a brilliant frenzy. Weapons and possessions are gathered, armour donned, warcries raised to the sky. Within days, nothing remains but the horde itself.";
        }

        public override string getRestriction()
        {
            return "Requires an orc warbanner and a subjugated orc camp, of the same culture as the banner, with at least 10 orcish industry.";
        }

        public override Sprite getSprite()
        {
            return map.world.textureStore.unit_orc;
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            int num = 0;
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
            return 20.0;
        }

        public override int getCompletionMenace()
        {
            return 25;
        }

        public override int getCompletionProfile()
        {
            return 15;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool validFor(UA ua)
        {
            if (banner == null)
            {
                return false;
            }

            Set_OrcCamp camp = ua.location.settlement as Set_OrcCamp;

            if (camp == null || (ua.isCommandable() && !camp.isInfiltrated))
            {
                return false;
            }

            Pr_OrcishIndustry industry = camp.location.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();
            if (industry != null && industry.charge >= 10.0)
            {
                if (banner is I_HordeBanner hordeBanner && ua.location.soc != hordeBanner.orcs)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public override bool valid()
        {
            return true;
        }

        public override void complete(UA u)
        {
            if (u.location.settlement is Set_OrcCamp camp)
            {
                Pr_OrcishIndustry industry = (Pr_OrcishIndustry)camp.location.properties.FirstOrDefault(pr => pr is Pr_OrcishIndustry);
                if (industry != null)
                {
                    double initHp = industry.charge;

                    Pr_Ophanim_Perfection perfection = (Pr_Ophanim_Perfection)u.location.properties.FirstOrDefault(pr => pr is Pr_Ophanim_Perfection);
                    if (perfection != null)
                    {
                        initHp *= 1 + (perfection.charge / 1200);
                    }

                    UM_OrcRabble rabble = new UM_OrcRabble(camp.location, camp.location.soc, camp, (int)Math.Ceiling(initHp));
                    camp.location.units.Add(rabble);
                    map.units.Add(rabble);
                    msgString = u.getName() + " has roused a rabble of " + rabble.maxHp + " orcs.";
                    camp.fallIntoRuin("Camp Abandonned");
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
