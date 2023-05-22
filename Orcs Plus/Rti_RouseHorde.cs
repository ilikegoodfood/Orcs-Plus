using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            Type tBanner = null;
            if (ModCore.core.data.tryGetModAssembly("CovensCursesCurios", out Assembly asmCCC))
            {
                tBanner = asmCCC.GetType("CovenExpansion.I_BarbDominion");
            }

            if ((banner is I_HordeBanner) || banner.GetType() == tBanner)
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
                Pr_OrcishIndustry industry = camp.location.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();
                if (industry != null)
                {
                    UM_OrcRabble rabble = new UM_OrcRabble(camp.location, camp.location.soc, camp, (int)Math.Ceiling(industry.charge));
                    camp.location.units.Add(rabble);
                    map.units.Add(rabble);
                    camp.fallIntoRuin("Camp Abandonned");
                }
            }
        }
    }
}
