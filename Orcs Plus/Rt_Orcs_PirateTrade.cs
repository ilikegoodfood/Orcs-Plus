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
    public class Rt_Orcs_PirateTrade : Ritual
    {
        public Rt_Orcs_PirateTrade(Location location)
            : base(location)
        {

        }

        public override string getName()
        {
            return "Pirate Trade Route";
        }

        public override string getDesc()
        {
            return "Conduct pirate operations against trade ships passing through the area, sending vast sums of plunder back to the army's home port. The army gains 15 profile and menace by completing this raid.";
        }

        public override string getRestriction()
        {
            return "Requires at least one active trade route to pass through this location.";
        }

        public override string getCastFlavour()
        {
            return "Unmarked black ships haunt the waves, striking without warning. Arows fly, grappling hooks are thrown, orc warriors leap from hull to hull. Empty the hold, sink the ship, leave none alive.";
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.raidShipping;
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
            foreach (TradeRoute tradeRoute in um.map.tradeManager.routes)
            {
                if (tradeRoute.raidingCooldown == 0 && tradeRoute.path.Contains(um.location))
                {
                    List<Location> endPoints = new List<Location> { tradeRoute.start(), tradeRoute.end() };
                    List<bool> endAlignments = new List<bool>();
                    foreach (Location endPoint in endPoints)
                    {
                        if (endPoint.soc != null)
                        {
                            SG_Orc orcSociety = um.society as SG_Orc;
                            if (orcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                            {
                                if (orcCulture.tenet_intolerance.status == -2)
                                {
                                    if (endPoint.soc.isDark() || (endPoint.soc is Society society && (society.isDarkEmpire || society.isOphanimControlled)))
                                    {
                                        endAlignments.Add(false);
                                    }
                                }
                                else if (orcCulture.tenet_intolerance.status == 2)
                                {
                                    if (!endPoint.soc.isDark() || (endPoint.soc is Society society && (!society.isDarkEmpire || !society.isOphanimControlled)))
                                    {
                                        endAlignments.Add(false);
                                    }
                                }
                            }
                        }
                    }

                    if (endAlignments.Count < 2)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override double getComplexity()
        {
            return 30;
        }

        public override int getCompletionMenace()
        {
            return 15;
        }

        public override int getCompletionProfile()
        {
            return 15;
        }

        public override double getProgressPerTurnInner(UM unit, List<ReasonMsg> msgs)
        {
            double val = unit.hp / 5;
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
            List<TradeRoute> targetRoutes = new List<TradeRoute>();
            foreach (TradeRoute tradeRoute in map.tradeManager.routes)
            {
                if (tradeRoute.raidingCooldown == 0 && tradeRoute.path.Contains(u.location))
                {
                    List<Location> endPoints = new List<Location> { tradeRoute.start(), tradeRoute.end() };
                    List<bool> endAlignments = new List<bool>();
                    foreach (Location endPoint in endPoints)
                    {
                        if (ModCore.core.checkAlignment(u.society as SG_Orc, endPoint))
                        {
                            endAlignments.Add(true);
                        }
                    }

                    if (endAlignments.Count > 0)
                    {
                        targetRoutes.Add(tradeRoute);
                    }
                }
            }

            if (targetRoutes.Count > 0)
            {
                double prosperity = 0;
                int gold = 0;

                foreach(TradeRoute target in targetRoutes)
                {
                    // Place Shipwreck
                    int wreckRoll = Eleven.random.Next(10);

                    if (wreckRoll < 1)
                    {
                        List<Location> oceanLocations = target.path.FindAll(l => l.isOcean).ToList();

                        if (oceanLocations.Count > 0)
                        {
                            Location wreckLocation = oceanLocations[Eleven.random.Next(oceanLocations.Count)];
                            Pr_Shipwreck wreck = wreckLocation.properties.OfType<Pr_Shipwreck>().FirstOrDefault();

                            if (wreck == null)
                            {
                                wreckLocation.properties.Add(new Pr_Shipwreck(wreckLocation));
                            }
                            else
                            {
                                wreck.charge += 25.0;
                            }
                        }
                    }

                    // Gather Propsperity
                    if (target.start().settlement is SettlementHuman startSettlement)
                    {
                        prosperity += startSettlement.prosperity;
                        
                        if (startSettlement.ruler != null)
                        {
                            gold += (int)Math.Ceiling(startSettlement.ruler.gold * map.param.ch_orcRaidingGoldGain);
                            startSettlement.ruler.gold *= (int)Math.Floor(1 - map.param.ch_orcRaidingGoldGain);
                        }
                    }

                    if (target.end().settlement is SettlementHuman endSettlement)
                    {
                        prosperity += endSettlement.prosperity;

                        if (endSettlement.ruler != null)
                        {
                            gold += (int)Math.Ceiling(endSettlement.ruler.gold * map.param.ch_orcRaidingGoldGain);
                            endSettlement.ruler.gold *= (int)Math.Floor(1 - map.param.ch_orcRaidingGoldGain);
                        }
                    }

                    target.raidingCooldown = map.param.ch_raidShippingCooldown / 2;
                }

                gold += (int)Math.Ceiling(prosperity * map.param.ch_raidShippingMult);

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

                //u.society.menace += map.param.ch_orcishRaidingMenaceGain * 2;
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.GOLD,
                Tags.COMBAT,
                Tags.DANGER
            };
        }
    }
}
