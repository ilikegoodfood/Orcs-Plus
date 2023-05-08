﻿using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class UM_OrcCorsair : UM_OrcArmy
    {
        public double strengthFactor = 0.5;

        public double goldGainFactor = 1.5;

        public Rt_Orcs_PirateTrade pirateTrade;

        public Rt_Orcs_PillageSettlement pillageSettlement;

        public UM_OrcCorsair(Location loc, SocialGroup sg, Set_OrcCamp camp)
            : base(loc, sg, camp)
        {
            moveType = MoveType.AQUAPHIBIOUS;

            pirateTrade = new Rt_Orcs_PirateTrade(location);
            pillageSettlement = new Rt_Orcs_PillageSettlement(location);

            rituals.Add(pirateTrade);
            rituals.Add(pillageSettlement);
        }

        public override Sprite getPortraitForeground()
        {
            return EventManager.getImg("OrcsPlus.Foreground_OrcCorsair.png");
        }

        public override string getName()
        {
            return "Corsairs";
        }

        public override void turnTickInner(Map map)
        {
            updateMaxHP();

            if (task is Task_RazeLocation razeTask && location.settlement is SettlementHuman settlementHuman)
            {
                Location homeBase;

                if (homeLocation != -1)
                {
                    homeBase = map.locations[homeLocation];
                }
                else
                {
                    homeBase = society.getCapitalHex().location;
                }

                if (homeBase != null)
                {
                    Pr_OrcPlunder plunder = homeBase.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();

                    if (plunder == null)
                    {
                        plunder = new Pr_OrcPlunder(map.locations[homeLocation]);
                        homeBase.properties.Add(plunder);
                    }

                    plunder.addGold((int)(map.param.ch_orcRazeGoldGainArmy * settlementHuman.prosperity * settlementHuman.population * goldGainFactor));
                }
            }

            return;
        }

        new void updateMaxHP()
        {
            if (homeLocation != -1 && map.locations[homeLocation].soc is SG_Orc)
            {
                Pr_OrcishIndustry industry = map.locations[homeLocation].properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();
                double totalIndustry = 0;

                if (industry != null)
                {
                    totalIndustry += industry.charge;
                }

                foreach (Location neighbour in map.locations[this.homeLocation].getNeighbours())
                {
                    if (neighbour.soc == society)
                    {
                        industry = neighbour.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();

                        if (industry != null)
                        {
                            totalIndustry += industry.charge;
                        }
                    }
                }

                maxHp = (int)(totalIndustry * map.param.minor_orcMilitaryScaling * map.difficultyMult_shrinkWithDifficulty * map.opt_orcStrMult);
                maxHp = (int)Math.Ceiling(maxHp * strengthFactor);

                if (maxHp < 10 * strengthFactor)
                {
                    maxHp = (int)Math.Ceiling(10 * strengthFactor);
                }
            }
            else
            {
                maxHp = (int)Math.Ceiling(10 * strengthFactor);
            }
        }

        public override void turnTickAI()
        {
            //Console.WriteLine("OrcsPlus: Start Corsair AI");
            if (hp < maxHp * 0.3)
            {
                //Console.WriteLine("OrcsPlus: Health under ecruitment threshold");
                if (location.index == homeLocation)
                {
                    task = new Task_Recruit();
                    return;
                }
                else
                {
                    if (homeLocation != -1)
                    {
                        if (!checkPath(map.locations[homeLocation]))
                        {
                            die(map, "Lost at Sea");
                            return;
                        }

                        task = new Task_GoToLocation(map.locations[homeLocation]);
                    }
                    else
                    {
                        die(map, "Lost at Sea");
                        return;
                    }
                }
            }

            if (location.soc != null && location.soc != society && society.getRel(location.soc).state == DipRel.dipState.war && location.settlement != null)
            {
                //Console.WriteLine("OrcsPlus: Raze current location");
                task = new Task_RazeLocation();
                return;
            }

            Pr_HumanOutpost outpost = location.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
            if (outpost != null && outpost.parent != null && outpost.parent != society && society.getRel(outpost.parent).state == DipRel.dipState.war)
            {
                //Console.WriteLine("OrcsPlus: Raze outpost at current location");
                task = new Task_RazeOutpost();
                return;
            }

            if (!society.isAtWar() && location.isOcean)
            {
                //Console.WriteLine("OrcsPlus: Orc society is at peace");
                // Insert Piracy at location
                if (pirateTrade.valid() && pirateTrade.validFor(this))
                {
                    task = new Task_PerformChallenge(pirateTrade);
                    return;
                }

                // Insert Pillaging at location
                if (pillageSettlement.valid() && pillageSettlement.validFor(this))
                {
                    task = new Task_PerformChallenge(pillageSettlement);
                    return;
                }
            }

            // Get Locations
            double score = 0;
            List<Location> warLocations = new List<Location>();
            List<Location> tradeLocations = new List<Location>();
            List<Location> pillageLocations = new List<Location>();
            Location targetLocation = null;
            
            //Console.WriteLine("OrcsPlus: Iterate Locations");
            foreach (Location loc in map.locations)
            {
                if (loc.isCoastal || loc.isOcean)
                {
                    //Console.WriteLine("OrcsPlus: Iterateing " + loc.getName());

                    if (society.isAtWar())
                    {
                        if (loc.soc == null)
                        {
                            //Console.WriteLine("OrcsPlus: Loc is wilderness");
                            Pr_HumanOutpost targetOutpost = loc.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
                            if (targetOutpost != null && targetOutpost.parent != null && targetOutpost.parent != society && society.getRel(targetOutpost.parent).state == DipRel.dipState.war)
                            {
                                if (checkPath(loc) && warLocations.Count == 0 || score <= map.getStepDist(location, loc))
                                {
                                    if (score < map.getStepDist(location, loc))
                                    {
                                        warLocations.Clear();
                                    }

                                    warLocations.Add(loc);
                                }
                            }
                        }
                        else if (loc.soc != society && society.getRel(loc.soc).state == DipRel.dipState.war)
                        {
                            //Console.WriteLine("OrcsPlus: loc is owned by " + loc.soc.getName());
                            if (checkPath(loc) && warLocations.Count == 0 || score <= map.getStepDist(location, loc))
                            {
                                if (score < map.getStepDist(location, loc))
                                {
                                    warLocations.Clear();
                                }

                                warLocations.Add(loc);
                            }
                        }
                    }

                    if (warLocations.Count == 0)
                    {
                        //Console.WriteLine("OrcsPlus: No war locations yet");
                        if (loc.soc != null && loc.settlement is SettlementHuman settlementHuman)
                        {
                            //Console.WriteLine("OrcsPlus: loc is settlement human");
                            if (checkAlignment(loc))
                            {
                                //Console.WriteLine("OrcsPlus: loc alignment is valid");
                                if (checkPath(loc))
                                {
                                    Pr_Devastation devastation = loc.properties.OfType<Pr_Devastation>().FirstOrDefault();
                                    if (devastation == null || devastation.charge < 150)
                                    {
                                        int gold = settlementHuman.ruler?.gold ?? 0;
                                        if ((pillageLocations.Count == 0 || score <= gold))
                                        {
                                            if (score < gold)
                                            {
                                                pillageLocations.Clear();
                                            }

                                            pillageLocations.Add(loc);
                                            score = gold;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (warLocations.Count == 0)
            {
                //Console.WriteLine("OrcsPlus: No war locations");
                //Console.WriteLine("OrcsPlus: Iterating trade routes");
                foreach (TradeRoute tradeRoute in map.tradeManager.routes)
                {
                    if (tradeRoute.raidingCooldown == 0)
                    {
                        if (!checkAlignment(tradeRoute.start()) && !checkAlignment(tradeRoute.end()))
                        {
                            //Console.WriteLine("OrcsPlus: Trade route alignment is valid");
                            foreach (Location loc in tradeRoute.path)
                            {
                                if (loc.isOcean)
                                {
                                    if (checkPath(loc))
                                    {
                                        double prosperity = 0;
                                        if (tradeRoute.start().settlement is SettlementHuman start)
                                        {
                                            prosperity += start.prosperity;
                                        }
                                        if (tradeRoute.end().settlement is SettlementHuman end)
                                        {
                                            prosperity += end.prosperity;
                                        }

                                        if (tradeLocations.Count == 0 || (prosperity * 100) - map.getStepDist(location, loc) >= score)
                                        {
                                            if ((prosperity * 100) - map.getStepDist(location, loc) > score)
                                            {
                                                tradeLocations.Clear();
                                            }

                                            tradeLocations.Add(loc);
                                            score = (prosperity * 100) - map.getStepDist(location, loc);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (warLocations.Count > 0)
                {
                    //Console.WriteLine("OrcsPlus: Getting target war location");
                    if (warLocations.Count == 1)
                    {
                        targetLocation = warLocations[0];
                    }
                    else
                    {
                        targetLocation = warLocations[Eleven.random.Next(warLocations.Count)];
                    }
                }
                else if (location.isOcean)
                {
                    //Console.WriteLine("OrcsPlus: Checking for actions at location");
                    // Insert Piracy at location
                    if (pirateTrade.valid() && pirateTrade.validFor(this))
                    {
                        task = new Task_PerformChallenge(pirateTrade);
                        return;
                    }

                    // Insert Pillaging at location
                    if (pillageSettlement.valid() && pillageSettlement.validFor(this))
                    {
                        task = new Task_PerformChallenge(pillageSettlement);
                        return;
                    }
                }

                if (targetLocation == null && tradeLocations.Count > 0)
                {
                    //Console.WriteLine("OrcsPlus: Getting target trade piracy location");
                    if (tradeLocations.Count == 1)
                    {
                        targetLocation = tradeLocations[0];
                    }
                    else
                    {
                        targetLocation = tradeLocations[Eleven.random.Next(tradeLocations.Count)];
                    }
                }

                if (targetLocation == null && pillageLocations.Count > 0)
                {
                    //Console.WriteLine("OrcsPlus: Getting target pillage location");
                    Location loc;
                    if (pillageLocations.Count == 1)
                    {
                        loc = pillageLocations[0];
                    }
                    else
                    {
                        loc = pillageLocations[Eleven.random.Next(pillageLocations.Count)];
                    }

                    List<Location> oceanLocations = new List<Location>();
                    foreach (Location neighbour in loc.getNeighbours())
                    {
                        if (neighbour.isOcean)
                        {
                            oceanLocations.Add(neighbour);
                        }
                    }

                    if (oceanLocations.Count > 0)
                    {
                        //Console.WriteLine("OrcsPlus: Getting ocean location");
                        if (oceanLocations.Count == 1)
                        {
                            targetLocation = oceanLocations[0];
                        }
                        else
                        {
                            targetLocation = oceanLocations[Eleven.random.Next(oceanLocations.Count)];
                        }
                    }
                }
            }

            if (targetLocation != null && checkPath(targetLocation))
            {
                //Console.WriteLine("OrcsPlus: Going to target location");
                task = new Task_GoToLocation(targetLocation);
                return;
            }

            if (location.index == homeLocation)
            {
                if (hp < maxHp)
                {
                    //Console.WriteLine("OrcsPlus: Recruiting Idle");
                    task = new Task_Recruit();
                    return;
                }
                //Console.WriteLine("OrcsPlus: Idle");
            }
            else
            {
                //Console.WriteLine("OrcsPlus: Going Home");
                task = new Task_GoToLocation(map.locations[homeLocation]);
                return;
            }
        }

        public bool checkAlignment(Location loc)
        {
            bool result = true;
            SG_Orc orcSociety = society as SG_Orc;
            if (orcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (orcCulture.tenet_intolerance.status == -2)
                {
                    if (loc.soc.isDark() || (loc.soc is Society society && (society.isDarkEmpire || society.isOphanimControlled)))
                    {
                        result = false;
                    }
                }
                else if (orcCulture.tenet_intolerance.status == 2)
                {
                    if (!loc.soc.isDark() || (loc.soc is Society society && (!society.isDarkEmpire || !society.isOphanimControlled)))
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        public bool checkPath(Location loc, bool safeMove = false)
        {
            bool result = true;
            if (loc != location)
            {
                Location[] pathTo = location.map.getPathTo(location, loc, this, safeMove);

                if (pathTo == null || pathTo.Length < 2)
                {
                    result = false;
                }
            }

            return result;
        }
    }
}