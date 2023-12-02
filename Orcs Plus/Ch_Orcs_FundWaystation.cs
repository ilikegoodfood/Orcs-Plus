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
            bool ambigous = false;
            SG_Orc orcSocity = location.soc as SG_Orc;

            if (location.settlement != null && location.settlement.subs.Count > 0)
            {
                List<Sub_OrcWaystation> waystations = location.settlement.subs.OfType<Sub_OrcWaystation>().ToList();
                if (waystations.Count > 0)
                {
                    Sub_OrcWaystation waystation = waystations.FirstOrDefault(sub => sub.getChallenges().Contains(this));
                    if (waystation != null)
                    {
                        orcSocity = waystation.orcSociety;
                    }

                    if (waystations.Count > 1)
                    {
                        ambigous = true;
                    }
                }
            }

            if (ambigous && orcSocity != null)
            {
                return "Fund " + orcSocity.getName() + " Waystation";
            }

            return "Fund Waystation";
        }

        public override string getDesc()
        {
            return "Donate " + cost + " gold so that the orcs can build, guard and buy stock for a waystation in a neighbouring non-human settlement. You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildWaystation] + " influence with the orc culture by completing this challenge.";
        }

        public override string getCastFlavour()
        {
            return "Hidden deep in the woods, on the edge of coven and ruin alike, a small unobtrusive wooden hut sits jauntily. Every so often, a travelling band of orcs might be observed slipping into and out of the hut, sometimes bearing supplies and sometimes plunder.";
        }

        public override string getRestriction()
        {
            return "Costs " + cost + " gold, and, unless performed by an Orc Elder, requires an infiltrated orc camp, or an orc waystation, with a wilderness settlement (i.e. witch coven, Vinerva manifestation, elder tomb) in a neighbouring location, with habitability > " + ((int)(100.0 * this.map.opt_orcHabMult * this.map.param.orc_habRequirement)).ToString() + "%";
        }

        public override double getProfile()
        {
            return map.param.ch_orcs_expand_aiProfile;
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("OrcsPlus.Icon_Waystation.png");
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

            Sub_OrcWaystation waystation = (Sub_OrcWaystation)location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(this));
            if (waystation != null)
            {
                orcSociety = waystation.orcSociety;
            }

            if (ua is UAEN_OrcElder elder && elder.society is HolyOrder_Orcs orcCulture)
            {
                msgs?.Add(new ReasonMsg("Base", 40.0));
                utility += 40.0;
            }

            return utility;
        }

        public override bool validFor(UA ua)
        {
            SG_Orc orcSociety = location.soc as SG_Orc;

            Sub_OrcWaystation waystation = null;
            if (location.settlement != null && location.settlement.subs.Count > 0)
            {
                waystation = (Sub_OrcWaystation)location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(this));
                if (waystation != null)
                {
                    orcSociety = waystation.orcSociety;
                }
            }

            if (orcSociety != null && ua.person.gold >= cost && location.settlement != null)
            {
                if (ua is UAEN_OrcElder elder && (elder.society as HolyOrder_Orcs)?.orcSociety == orcSociety)
                {
                    return true;
                }

                if (ua.isCommandable() && (location.settlement.infiltration == 1.0 || waystation != null))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool valid()
        {
            SG_Orc orcSociety = location.soc as SG_Orc;

            if (location.settlement != null)
            {
                Sub_OrcWaystation waystaion = (Sub_OrcWaystation)location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation way && way.getChallenges().Contains(this));
                if (waystaion != null)
                {
                    orcSociety = waystaion.orcSociety;
                }

                if (orcSociety != null && ((location.soc == orcSociety && location.settlement is Set_OrcCamp) || waystaion != null))
                {
                    foreach (Location neighbour in location.getNeighbours())
                    {
                        if (!neighbour.isOcean && neighbour.settlement != null && neighbour.hex.getHabilitability() >= map.opt_orcHabMult * map.param.orc_habRequirement)
                        {
                            if (ModCore.Get().data.getSettlementTypesForWaystation().TryGetValue(neighbour.settlement.GetType(), out HashSet<Type> blacklist) && !neighbour.settlement.subs.Any(sub => (sub is Sub_OrcWaystation way && way.orcSociety == orcSociety) || blacklist.Contains(sub.GetType())))
                            {
                                return true;
                            }
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

            if (location.settlement != null && location.settlement.subs.Count > 0)
            {
                Sub_OrcWaystation waystation = (Sub_OrcWaystation)location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(this));
                if (waystation != null)
                {
                    orcSociety = waystation.orcSociety;
                }
            }

            if (location.settlement != null)
            {
                bool infiltrated = true;
                bool infiltratable = (location.settlement is Set_OrcCamp || location.settlement.subs.Any(sub => sub.canBeInfiltrated()));
                if (infiltratable)
                {
                    infiltrated = location.settlement.isInfiltrated;
                }

                if (orcSociety != null && (u.isCommandable() && infiltrated) || (u is UAEN_OrcElder elder && (elder.society as HolyOrder_Orcs)?.orcSociety == orcSociety))
                {
                    List<Settlement> settlements = new List<Settlement>();

                    foreach (Location neighbour in location.getNeighbours())
                    {
                        if (neighbour.settlement != null && neighbour.hex.getHabilitability() >= map.opt_orcHabMult * map.param.orc_habRequirement)
                        {
                            if (ModCore.Get().data.getSettlementTypesForWaystation().TryGetValue(neighbour.settlement.GetType(), out HashSet<Type> blacklist) && !neighbour.settlement.subs.Any(sub => (sub is Sub_OrcWaystation way && way.orcSociety == orcSociety) || blacklist.Contains(sub.GetType())))
                            {
                                settlements.Add(neighbour.settlement);
                            }
                        }
                    }

                    if (settlements.Count > 0)
                    {
                        Settlement targetSettlement = settlements[0];
                        if (settlements.Count > 1)
                        {
                            targetSettlement = settlements[Eleven.random.Next(settlements.Count)];
                        }

                        Sub_OrcWaystation waystation = new Sub_OrcWaystation(targetSettlement, orcSociety);
                        targetSettlement.subs.Add(waystation);
                        if (targetSettlement.location.soc == null)
                        {
                            targetSettlement.location.soc = orcSociety;
                        }
                        u.person.gold -= cost;

                        if (u.isCommandable() && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                        {
                            ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.Expand]), true);
                        }
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
