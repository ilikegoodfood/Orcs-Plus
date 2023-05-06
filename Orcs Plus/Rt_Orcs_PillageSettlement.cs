using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class Rt_Orcs_PillageSettlement : Ritual
    {
        public Rt_Orcs_PillageSettlement(Location loc)
            : base(loc)
        {

        }

        public override string getName()
        {
            return "Pillage Settlement";
        }

        public override string getDesc()
        {
            return "Conducts a naval raid against a neighbouring settlement, causing some devastation and stealing anything that not's nailed down.";
        }

        public override string getRestriction()
        {
            return "Requires an ocean location adjacent to a settlement with a ruler.";
        }

        public override string getCastFlavour()
        {
            return "Unmarked black ships emerge silently from the early morning fog, rowboats filled with orc warriors, arrows falling all across the docks. They surge into the sleeping settlement, grabbing everything worth carrying, and disappear as quickly and slently as they come, leaving only destruction and death.";
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.raidPort;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override bool valid()
        {
            return true;
        }

        public override bool validFor(UM um)
        {
            if (um.location.isOcean)
            {
                foreach (Location neighbour in um.location.getNeighbours())
                {
                    if (neighbour.settlement is SettlementHuman settlementHuman && neighbour.soc != null)
                    {
                        bool validAlignment = true;
                        SG_Orc orcSociety = um.society as SG_Orc;
                        if (orcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                        {
                            if (orcCulture.tenet_intolerance.status == -2)
                            {
                                if (neighbour.soc.isDark() || (neighbour.soc is Society society && (society.isDarkEmpire || society.isOphanimControlled)))
                                {
                                    validAlignment = false;
                                }
                            }
                            else if (orcCulture.tenet_intolerance.status == 2)
                            {
                                if (!neighbour.soc.isDark() || (neighbour.soc is Society society && (!society.isDarkEmpire || !society.isOphanimControlled)))
                                {
                                    validAlignment = false;
                                }
                            }
                        }

                        Pr_Devastation devastation = settlementHuman.location.properties.OfType<Pr_Devastation>().FirstOrDefault();
                        if (validAlignment && devastation == null || devastation.charge < 150)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override double getComplexity()
        {
            return 10;
        }

        public override int getCompletionMenace()
        {
            return map.param.ch_orcs_raiding_parameterValue2;
        }

        public override int getCompletionProfile()
        {
            return map.param.ch_orcs_raiding_parameterValue3;
        }

        public override double getProgressPerTurnInner(UM unit, List<ReasonMsg> msgs)
        {
            double val = unit.hp / 10;
            if (val < 1)
            {
                val = 1.0;
                msgs?.Add(new ReasonMsg("Base", val));
            }
            else
            {
                msgs?.Add(new ReasonMsg("Army Size", val));
            }
            return val;
        }

        public override void complete(UM u)
        {
            if (u.location.isOcean)
            {
                List<SettlementHuman> targetSettlements = new List<SettlementHuman>();
                int gold = 0;
                SettlementHuman target = null;

                foreach (Location neighbour in u.location.getNeighbours())
                {
                    if (neighbour.settlement is SettlementHuman settlementHuman && neighbour.soc != null)
                    {
                        bool validAlignment = true;
                        SG_Orc orcSociety = u.society as SG_Orc;
                        if (orcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                        {
                            if (orcCulture.tenet_intolerance.status == -2)
                            {
                                if (neighbour.soc.isDark() || (neighbour.soc is Society society && (society.isDarkEmpire || society.isOphanimControlled)))
                                {
                                    validAlignment = false;
                                }
                            }
                            else if (orcCulture.tenet_intolerance.status == 2)
                            {
                                if (!neighbour.soc.isDark() || (neighbour.soc is Society society && (!society.isDarkEmpire || !society.isOphanimControlled)))
                                {
                                    validAlignment = false;
                                }
                            }
                        }

                        if (validAlignment && (targetSettlements.Count == 0 || (settlementHuman.ruler?.gold ?? 0) >= gold))
                        {
                            if ((settlementHuman.ruler?.gold ?? 0) > gold)
                            {
                                targetSettlements.Clear();
                            }

                            targetSettlements.Add(settlementHuman);
                        }
                    }
                }

                if (targetSettlements.Count > 0)
                {
                    target = targetSettlements[Eleven.random.Next(targetSettlements.Count)];
                }

                if (target != null)
                {
                    gold = (int)Math.Ceiling(target.prosperity * 10);

                    if (target.ruler != null)
                    {
                        target.ruler.gold *= (int)Math.Floor(1 - map.param.ch_orcRaidingGoldGain);
                        gold += (int)Math.Ceiling(target.ruler.gold * map.param.ch_orcRaidingGoldGain);
                    }

                    Property.addToPropertySingleShot("Pillaged by " + u.getName(), Property.standardProperties.DEVASTATION, 100.0, target.location);

                    Location homeBase;

                    if (u.homeLocation != -1)
                    {
                        homeBase = map.locations[u.homeLocation];
                    }
                    else
                    {
                        homeBase = u.society.getCapitalHex().location;
                    }

                    if (homeBase != null)
                    {
                        Pr_OrcPlunder plunder = homeBase.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();

                        if (plunder == null)
                        {
                            plunder = new Pr_OrcPlunder(map.locations[u.homeLocation]);
                            homeBase.properties.Add(plunder);
                        }

                        plunder.addGold(gold);
                    }

                    //u.society.menace += map.param.ch_orcishRaidingMenaceGain;
                }
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.GOLD,
                Tags.COMBAT,
                Tags.DANGER,
                Tags.CRUEL
            };
        }
    }
}
