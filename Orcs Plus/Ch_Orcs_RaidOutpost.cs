using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_RaidOutpost : ChallengeHoly
    {
        public SG_Orc orcSociety;

        public HolyOrder_Orcs orcCulture;

        public Ch_Orcs_RaidOutpost(Location location, SG_Orc orcSociety)
            : base(location)
        {
            this.orcSociety = orcSociety;
        }

        public override string getName()
        {
            bool ambigous = location.settlement?.subs.OfType<Sub_OrcWaystation>().Count() > 1;

            if (ambigous)
            {
                return "Raid Outpost (" + orcSociety.getName() + ")";
            }

            return "Raid Outpost";
        }

        public override string getDesc()
        {
            return "Raids a nearby human outpost, taking vast sums of gold that the humans had invested in its success, and potentially razing it to the ground. Adds " + map.param.ch_orcishRaidingMenaceGain + " menace to the orcish horde. You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.Raiding] + " influence with the orc culture by completing this challenge.";
        }

        public override string getCastFlavour()
        {
            return "Unruly and disorganised, the orcish raiders are motivated by bloodlust, greed, remembered grudges and the need for personal glory. As a military force their poor discipline is offset by their extreme personal strength and reckless bravery.";
        }

        public override string getRestriction()
        {
            return "Requires a human outpost in a location neighbouring this orc camp.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 0.0;

            int val = unit.getStatCommand();
            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Stat: Command", val));
                result += val;
            }

            val = unit.getStatMight();
            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Stat: Might", val));
                result += val;
            }

            if (result < 1.0)
            {
                msgs?.Add(new ReasonMsg("Base", val));
                result = 1.0;
            }

            return result;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double result = 0.0;

            Pr_HumanOutpost targetOutpost = null;
            double charge = 0.0;

            foreach (Location loc in location.getNeighbours())
            {
                Pr_HumanOutpost outpost = loc.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
                if (outpost != null && outpost.charge > charge)
                {
                    targetOutpost = outpost;
                    charge = outpost.charge;
                }
            }

            if (targetOutpost != null)
            {
                if (ua is UAEN_OrcUpstart || ua is UAEN_OrcElder)
                {
                    result = Math.Min(map.param.ch_raidOutpostDmg, charge);
                    msgs?.Add(new ReasonMsg("Potential Damage", result));
                }
                else if (ua is UAG uag && uag.society != null)
                {
                    if (targetOutpost.parent != uag.society && targetOutpost.parent.getRel(uag.society).state == DipRel.dipState.war)
                    {
                        result = Math.Min(map.param.ch_raidOutpostDmg, charge);
                        msgs?.Add(new ReasonMsg("Potential Damage to Enemy Outpost", result));
                    }
                }
            }

            return result;
        }

        public override double getComplexity()
        {
            return map.param.ch_raidoutpost_complexity;
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.raid;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            foreach (Location loc in location.getNeighbours())
            {
                if (loc.properties.OfType<Pr_HumanOutpost>().FirstOrDefault() != null)
                {
                    return true;
                }
            }

            return false;
        }

        public override void complete(UA u)
        {
            List<Pr_HumanOutpost> outposts = new List<Pr_HumanOutpost>();
            Pr_HumanOutpost targetOutpost = null;

            foreach (Location loc in location.getNeighbours())
            {
                Pr_HumanOutpost outpost = loc.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
                if (outpost != null)
                {
                    outposts.Add(outpost);
                }
            }

            if (outposts.Count == 1)
            {
                targetOutpost = outposts[0];
            }
            else if (outposts.Count > 1)
            {
                double charge = 0.0;
                foreach (Pr_HumanOutpost outpost in outposts)
                {
                    if (outpost.charge > charge)
                    {
                        charge = outpost.charge;
                        targetOutpost = outpost;
                    }
                }
            }

            if (targetOutpost != null)
            {
                double initCharge = targetOutpost.charge;
                targetOutpost.charge -= map.param.ch_raidOutpostDmg;
                if (targetOutpost.charge <= 0.0)
                {
                    targetOutpost.charge = 0.0;
                    targetOutpost.location.properties.Remove(targetOutpost);
                    u.person.gold += targetOutpost.funding;
                    msgString = u.getName() + " lead a raid against the outpost of the " + targetOutpost.parent.getName() + ", completely destoying it, and recovering loot worth " + targetOutpost.funding + " gold.";
                    targetOutpost.funding = 0;
                }
                else
                {
                    double dmgPercent = (1.0 - (targetOutpost.charge / initCharge));
                    int goldStolen = (int)Math.Ceiling(targetOutpost.funding * dmgPercent);
                    msgString = u.getName() + " lead a raid against the outpost of the " + targetOutpost.parent.getName() + ", recovering loot worth " + goldStolen + " gold.";
                    u.person.gold += goldStolen;
                    targetOutpost.funding -= goldStolen;
                }
            }

            if (u.isCommandable())
            {
                ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg(getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Raiding]), true);
            }
            else if (!u.society.isDark())
            {
                ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg(getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Raiding]));
            }
        }

        public override int getCompletionMenace()
        {
            return map.param.ch_orcs_raiding_parameterValue2;
        }

        public override int getCompletionProfile()
        {
            return map.param.ch_orcs_raiding_parameterValue3;
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.COMBAT,
                Tags.CRUEL,
                Tags.DANGER,
                Tags.DISCORD,
                Tags.GOLD,
                Tags.ORC
            };
        }
    }
}
