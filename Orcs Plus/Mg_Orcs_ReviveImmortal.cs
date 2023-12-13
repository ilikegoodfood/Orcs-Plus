using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Mg_Orcs_ReviveImmortal : Challenge
    {
        public Pr_Orcs_ImmortalRemains immortalRemains;

        public Mg_Orcs_ReviveImmortal(Location location, Pr_Orcs_ImmortalRemains remains)
            : base(location)
        {
            immortalRemains = remains;
        }

        public override string getName()
        {
            return "Revive " + immortalRemains.person.getFullName();
        }

        public override string getDesc()
        {
            return "Return " + immortalRemains.person.getFullName() + " to life.";
        }

        public override string getCastFlavour()
        {
            return "Ixthus, King of Cups, has granted " + immortalRemains.person.getFullName() + " a taste of immortality. With a mastery of death, their mortal remains can be used to bring them back, again and again, until they are no longer of use.";
        }

        public override string getRestriction()
        {
            if (immortalRemains.ua.isCommandable())
            {
                return "Requires Mastery of Death Level 3, or an orc spirit caller of an orc culture who's Death Mastery tenet is fully elder aligned (-2), and an agent slot.";
            }

            return "Requires Mastery of Death Level 3, or an orc spirit caller of an orc culture who's Death Mastery tenet is fully elder aligned (-2).";
        }

        public override Sprite getSprite()
        {
            if (immortalRemains.person.shadow >= 50.0)
            {
                return immortalRemains.ua.getPortraitForegroundAlt();
            }

            return immortalRemains.ua.getPortraitForeground();
        }

        public override double getComplexity()
        {
            return 40;
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 0.0;

            double val = unit.getStatLore();
            if (val >= 1.0)
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", val));
                result += val;
            }
            else
            {
                val = 1.0;
                msgs?.Add(new ReasonMsg("Base", val));
                result = val;
            }

            return result;
        }

        public override bool isChannelled()
        {
            return true;
        }

        public override double getProfile()
        {
            return immortalRemains.ua.inner_profileMin + 10.0;
        }

        public override bool valid()
        {
            bool valid = true;

            if (immortalRemains.ua.isCommandable() && map.overmind.nEnthralled >= map.overmind.getAgentCap())
            {
                valid = false;
            }

            return valid;
        }

        public override bool validFor(UA ua)
        {
            bool valid = false;
            if (Math.Ceiling(getComplexity() / getProgressPerTurnInner(ua, null)) <= immortalRemains.charge)
            {
                if (ua.getCurrentlyUsedCommand() >= ua.getStatCommandLimit() || !ua.minions.Any(m => m == null) || ua.isCommandable())
                {
                    if (ua is UAEN_OrcShaman)
                    {
                        if (ua.society is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                        {
                            if (immortalRemains.ua.society == orcSociety || immortalRemains.ua.society == orcCulture)
                            {
                                if (orcCulture.tenet_god is H_Orcs_DeathMastery && orcCulture.tenet_god.status < -1)
                                {
                                    if (ua.person.traits.Any(t => t is T_MasteryDeath && t.level >= 2))
                                    {
                                        valid = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (ua.person.traits.Any(t => t is T_MasteryDeath && t.level >= 3))
                    {
                        valid = true;
                    }
                }
            }

            return valid;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 100.0;
            msgs?.Add(new ReasonMsg("Base", utility));

            double excessCharge = immortalRemains.charge - Math.Ceiling(getComplexity() / getProgressPerTurnInner(ua, null));
            if (excessCharge >= 0.0 && excessCharge < 40.0)
            {
                double val = (40 - excessCharge) * 1.25;
                msgs?.Add(new ReasonMsg("Remains are degrading", val));
                utility += val;
            }

            return utility;
        }

        public override int getCompletionProfile()
        {
            return 6;
        }

        public override int getCompletionMenace()
        {
            return 8;
        }

        public override void complete(UA u)
        {
            immortalRemains.ua.isDead = false;
            map.units.Add(immortalRemains.ua);
            location.units.Add(immortalRemains.ua);
            immortalRemains.ua.location = location;

            immortalRemains.person.isDead = false;
            immortalRemains.person.society.people.Add(immortalRemains.person.index);

            if (immortalRemains.ua.isCommandable())
            {
                map.overmind.calculateAgentsUsed();
            }

            immortalRemains.ua.setMenace(immortalRemains.ua.inner_menaceMin);
            immortalRemains.ua.setProfile(immortalRemains.ua.inner_profileMin);

            location.properties.Remove(immortalRemains);
        }
    }
}
