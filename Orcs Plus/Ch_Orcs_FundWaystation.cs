using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Orcs_Plus
{
    public class Ch_Orcs_FundWaystation : Challenge
    {
        public int cost = 40;

        public Ch_Orcs_FundWaystation(Location loc)
            :base(loc)
        {

        }

        public override string getName()
        {
            return "Fund Waystation";
        }
        public override string getDesc()
        {
            return "Dontate " + cost + " gold so that the orcs can build, guard and buy stock for a waystation in a neighbouring non-human settlement. You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.Expand] + " influence with the orc culture by completing this challenge.";
        }

        public override string getCastFlavour()
        {
            return "Hiddedn deep in the woods, on the edge of coven and ruin alike, a small unobtrusive wooden hut sits jauntily. Every so often, a travelling band of orcs might be observed sliping into and out of the hut, sometimes bearing supplies and sometimes plunder.";
        }

        public override string getRestriction()
        {
            return "Requires " + cost + " gold, and an infiltrated orc camp, or an infiltrated settlement containing an orc waystation, with a wilderness settlement (i.e. witch coven, vinerva manifestation, elder tomb) in a neighbouring location with habilitability > " + ((int)(100.0 * this.map.opt_orcHabMult * this.map.param.orc_habRequirement)).ToString() + "%";
        }

        public override double getProfile()
        {
            return map.param.ch_orcs_expand_aiProfile;
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Waystation.png");
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.COMMAND;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 0.0;

            double val = unit.getStatCommand();

            if (val > 0)
            {
                msgs?.Add(new ReasonMsg("Stat: Command", val));
                result += val;
            }
            else
            {
                msgs?.Add(new ReasonMsg("Base", 1.0));
                result = 1.0;
            }

            return result;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;

            SG_Orc orcSociety = location.soc as SG_Orc;

            Sub_OrcWaystation waystation = location.settlement.subs.OfType<Sub_OrcWaystation>().FirstOrDefault(s => s.orcSociety == orcSociety);
            if (orcSociety == null && location.settlement != null)
            {
                if (waystation == null)
                {
                    msgs?.Add(new ReasonMsg("Invalid Location", -10000.0));
                    return -10000.0;
                }

                orcSociety = waystation.orcSociety;
            }

            if (ua is UAA_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture)
            {
                msgs?.Add(new ReasonMsg("Base", 40.0));
                utility += 40.0;
            }

            return utility;
        }

        public override bool validFor(UA ua)
        {
            SG_Orc orcSociety = location.soc as SG_Orc;

            Sub_OrcWaystation waystation = location.settlement.subs.OfType<Sub_OrcWaystation>().FirstOrDefault(s => s.orcSociety == orcSociety);
            if (orcSociety == null && location.settlement != null)
            {
                if (waystation == null)
                {
                    return false;
                }

                orcSociety = waystation.orcSociety;
            }

            if (orcSociety != null && ua.person.gold >= cost && location.settlement != null && (location.settlement.infiltration == 1.0 || waystation != null || (ua is UAA_OrcElder elder && (elder.society as HolyOrder_Orcs)?.orcSociety == orcSociety)))
            {
                return true;
            }

            return false;
        }

        public override bool valid()
        {
            if (location.settlement != null && (location.soc is SG_Orc || location.settlement.subs.OfType<Sub_OrcWaystation>().FirstOrDefault() != null))
            {
                foreach (Location neighbour in location.getNeighbours())
                {
                    if (neighbour.settlement != null && neighbour.hex.getHabilitability() >= map.opt_orcHabMult * map.param.orc_habRequirement)
                    {
                        if (ModCore.core.data.getSettlementTypesForWaystation().Contains(neighbour.settlement.GetType()) && neighbour.settlement.subs.OfType<Sub_OrcWaystation>().FirstOrDefault(s => s.orcSociety == location.soc) == null)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override int getCompletionMenace()
        {
            return map.param.ch_orcs_expand_parameterValue1 / 2;
        }

        public override int getCompletionProfile()
        {
            return map.param.ch_orcs_expand_parameterValue2 / 2;
        }

        public override double getComplexity()
        {
            return map.param.ch_orcs_expand_complexity;
        }

        public override void complete(UA u)
        {
            SG_Orc orcSociety = location.soc as SG_Orc;

            if (orcSociety == null && location.settlement != null)
            {
                Sub_OrcWaystation waystation = location.settlement.subs.OfType<Sub_OrcWaystation>().FirstOrDefault();
                if (waystation != null)
                {
                    orcSociety = waystation.orcSociety;
                }
            }

            if (orcSociety != null && location.settlement != null && (location.settlement.infiltration == 1.0 || (u is UAA_OrcElder elder && (elder.society as HolyOrder_Orcs)?.orcSociety == orcSociety)))
            {
                List<Settlement> settlements = new List<Settlement>();

                foreach (Location neighbour in location.getNeighbours())
                {
                    if (neighbour.settlement != null && neighbour.hex.getHabilitability() >= map.opt_orcHabMult * map.param.orc_habRequirement)
                    {
                        if (ModCore.core.data.getSettlementTypesForWaystation().Contains(neighbour.settlement.GetType()) && neighbour.settlement.subs.OfType<Sub_OrcWaystation>().FirstOrDefault(s => s.orcSociety == location.soc) == null)
                        {
                            settlements.Add(neighbour.settlement);
                        }
                    }
                }

                if (settlements.Count > 0)
                {
                    Settlement targetSettlement = settlements[Eleven.random.Next(settlements.Count)];

                    if (targetSettlement != null)
                    {
                        Sub_OrcWaystation waystation = new Sub_OrcWaystation(targetSettlement, orcSociety);
                        targetSettlement.subs.Add(waystation);
                        u.person.gold -= cost;
                    }

                    if (u.isCommandable() && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                    {
                        ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Expand]), true);
                    }
                }
            }
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
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
