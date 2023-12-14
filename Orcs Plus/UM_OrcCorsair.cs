using Assets.Code;
using CommunityLib;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            return EventManager.getImg("OrcsPlus.Icon_OrcCorsair.png");
        }

        public override string getName()
        {
            return "Orc Corsairs";
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

                Pr_Ophanim_Perfection perfection = map.locations[this.homeLocation].properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
                double perfectionMult = 1.0;
                if (perfection != null)
                {
                    perfectionMult += (perfection.charge / 1200);
                }

                maxHp = (int)(totalIndustry * perfectionMult * map.param.minor_orcMilitaryScaling * map.difficultyMult_shrinkWithDifficulty * map.opt_orcStrMult * strengthFactor);

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
            SG_Orc orcSociety = society as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            //Console.WriteLine("OrcsPlus: Start Corsair AI");
            if (location.index == homeLocation && hp < maxHp)
            {
                //Console.WriteLine("OrcsPlus: Is At home and not at maxHp");
                task = new Task_Recruit();
                return;
            }
            else if (hp < maxHp * 0.3)
            {
                //Console.WriteLine("OrcsPlus: Health under ecruitment threshold");
                if (homeLocation != -1)
                {
                    if (!checkPath(map.locations[homeLocation], out _))
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

            if (orcCulture?.tenet_expansionism?.status < -1)
            {
                Rt_Orcs_BuildCamp cBuildCamp = (Rt_Orcs_BuildCamp)rituals.FirstOrDefault(rt => rt is Rt_Orcs_BuildCamp);
                if (cBuildCamp != null && cBuildCamp.validFor(this))
                {
                    task = new Task_PerformChallenge(cBuildCamp);
                }
            }

            bool atWar = society.isAtWar();
            if (!atWar && location.isOcean)
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
            
            if (atWar)
            {
                if (orcCulture != null && orcCulture.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < 0)
                {
                    if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.typeDict.TryGetValue("God", out Type cordycepsType))
                    {
                        if (map.overmind.god.GetType() == cordycepsType || map.overmind.god.GetType().IsSubclassOf(cordycepsType))
                        {
                            if (intDataCord.fieldInfoDict.TryGetValue("VespidicSwarmTarget", out FieldInfo FI_vSwarmTarget))
                            {
                                Location vespidicTarget = (Location)FI_vSwarmTarget.GetValue(map.overmind.god);
                                if (vespidicTarget != null && vespidicTarget != location)
                                {
                                    if ((vespidicTarget.isCoastal || vespidicTarget.isOcean) && checkPath(vespidicTarget, out int dist) && dist < 3)
                                    {
                                        if (vespidicTarget.soc != null && vespidicTarget.soc != orcSociety && vespidicTarget.soc != orcCulture && orcSociety.getRel(vespidicTarget.soc).state == DipRel.dipState.war)
                                        {
                                            if (vespidicTarget.settlement != null && !(vespidicTarget.settlement is Set_TombOfGods) && !(vespidicTarget.settlement is Set_CityRuins))
                                            {
                                                targetLocation = vespidicTarget;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Console.WriteLine("OrcsPlus: Iterate Locations");
            if (targetLocation == null)
            {
                foreach (Location loc in map.locations)
                {
                    if (loc.isCoastal || loc.isOcean)
                    {
                        //Console.WriteLine("OrcsPlus: Iterateing " + loc.getName());
                        if (atWar)
                        {
                            if (loc.soc == null)
                            {
                                //Console.WriteLine("OrcsPlus: Loc is wilderness");
                                Pr_HumanOutpost targetOutpost = loc.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
                                if (targetOutpost != null && targetOutpost.parent != null && targetOutpost.parent != society && society.getRel(targetOutpost.parent).state == DipRel.dipState.war)
                                {
                                    if (checkPath(loc, out int dist) && warLocations.Count == 0 || dist >= score)
                                    {
                                        if (dist > score)
                                        {
                                            warLocations.Clear();
                                        }

                                        warLocations.Add(loc);
                                        score = dist;
                                    }
                                }
                            }
                            else if (loc.soc != society && society.getRel(loc.soc).state == DipRel.dipState.war)
                            {
                                //Console.WriteLine("OrcsPlus: loc is owned by " + loc.soc.getName());
                                if (checkPath(loc, out int dist) && warLocations.Count == 0 || dist >= score)
                                {
                                    if (dist > score)
                                    {
                                        warLocations.Clear();
                                    }

                                    warLocations.Add(loc);
                                    score = dist;
                                }
                            }
                        }

                        if (warLocations.Count == 0)
                        {
                            //Console.WriteLine("OrcsPlus: No war locations yet");
                            if (loc.soc != null && loc.settlement is SettlementHuman settlementHuman)
                            {
                                //Console.WriteLine("OrcsPlus: loc is settlement human");
                                if (ModCore.Get().isHostileAlignment(society as SG_Orc, loc))
                                {
                                    //Console.WriteLine("OrcsPlus: loc alignment is valid");
                                    if (checkPath(loc, out _))
                                    {
                                        Pr_Devastation devastation = loc.properties.OfType<Pr_Devastation>().FirstOrDefault();
                                        if (devastation == null || devastation.charge < 150)
                                        {
                                            if ((pillageLocations.Count == 0 || score <= settlementHuman.prosperity))
                                            {
                                                if (score < settlementHuman.prosperity)
                                                {
                                                    pillageLocations.Clear();
                                                }

                                                pillageLocations.Add(loc);
                                                score = settlementHuman.prosperity;
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
                            if (!ModCore.Get().isHostileAlignment(society as SG_Orc, tradeRoute.start()) && !ModCore.Get().isHostileAlignment(society as SG_Orc, tradeRoute.end()))
                            {
                                //Console.WriteLine("OrcsPlus: Trade route alignment is valid");
                                foreach (Location loc in tradeRoute.path)
                                {
                                    if (loc.isOcean)
                                    {
                                        if (checkPath(loc, out int dist))
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

                                            if (tradeLocations.Count == 0 || (prosperity * 100) - dist >= score)
                                            {
                                                if ((prosperity * 100) - dist > score)
                                                {
                                                    tradeLocations.Clear();
                                                }

                                                tradeLocations.Add(loc);
                                                score = (prosperity * 100) - dist;
                                            }
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

            if (targetLocation != null && checkPath(targetLocation, out _))
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

        public bool checkPath(Location loc, out int steps, bool safeMove = false)
        {
            steps = 0;
            if (loc != location)
            {
                Location[] pathTo = location.map.getPathTo(location, loc, this, safeMove);

                if (pathTo == null || pathTo.Length < 2)
                {
                    return false;
                }

                steps = pathTo.Length;
            }

            return true;
        }
    }
}
