using Assets.Code;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orcs_Plus
{
    public class UM_OrcBeastArmy : UM_OrcArmy
    {
        public double strengthFactor = 0.75;

        public UM_OrcBeastArmy(Location loc, SocialGroup sg, Set_OrcCamp camp)
            : base(loc, sg, camp)
        {

        }

        public override Sprite getPortraitForeground()
        {
            return EventManager.getImg("OrcsPlus.Foreground_ArmouredBeast.png");
        }

        public override string getName()
        {
            return "Bestial Horde";
        }

        public override void turnTickInner(Map map)
        {
            updateMaxHP();

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

        public override void inBattle(BattleArmy battleArmy)
        {
            return;
        }

        public override void turnTickAI()
        {
            if (hp < maxHp * 0.3)
            {
                if (location.index == homeLocation)
                {
                    task = new Task_Recruit();
                }
                else
                {
                    task = new Task_GoToLocation(map.locations[homeLocation]);
                }
                return;
            }
            
            if (location.soc != null && location.soc != society && society.getRel(location.soc).state == DipRel.dipState.war && location.settlement != null)
            {
                if (location.settlement is SettlementHuman)
                {
                    task = new Task_DevourLocation();
                }
                else
                {
                    task = new Task_RazeLocation();
                }

                return;
            }

            Pr_HumanOutpost outpost = location.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
            if (outpost != null && outpost.parent != null && society.getRel(outpost.parent).state == DipRel.dipState.war)
            {
                task = new Task_RazeOutpost();
                return;
            }

            int distance = 0;
            List<UM> targets = new List<UM>();
            UM target = null;

            foreach (Unit unit in location.map.units)
            {
                // Will not attack non militray units or units of this society.
                if (unit is UM um && um.society != society)
                {
                    int dist = map.getStepDist(location, um.location);
                    // Will not attack refugees that are further than 1 step distance.
                    if (!(um is UM_Refugees && dist > 1))
                    {
                        // Will attack military units in own societ's territory or at a distance up to 4 step distance away.
                        if (um.location.soc == society || dist <= 4)
                        {
                            // Will only attack military units in own society if they are above recruitment threshold.
                            if (um.location.soc != um.society || um.hp >= um.maxHp / 3)
                            {
                                if (um.society != null)
                                {
                                    DipRel rel = society.getRel(um.society);
                                    // Will attack military units that they are at war with, or that they are hostile with and are within this army's territory.
                                    if (rel.state == DipRel.dipState.war || (rel.state == DipRel.dipState.hostile && unit.location.soc == society))
                                    {
                                        if (targets.Count == 0 || distance >= dist)
                                        {
                                            if (distance > dist)
                                            {
                                                targets.Clear();
                                            }

                                            targets.Add(um);
                                        }
                                    }
                                }
                                else
                                {
                                    if (targets.Count == 0 || distance >= dist)
                                    {
                                        if (distance > dist)
                                        {
                                            targets.Clear();
                                        }

                                        targets.Add(um);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (targets.Count > 0)
            {
                target = targets[Eleven.random.Next(targets.Count)];
            }

            if (target != null)
            {
                task = new Task_DevourArmy(target, this);
                return;
            }

            distance = 0;
            List<Location> targetLocations = new List<Location>();
            Location targetLocation = null;

            foreach(Location loc in map.locations)
            {
                if (loc.soc == null)
                {
                    Pr_HumanOutpost targetOutpost = loc.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
                    if (targetOutpost != null && targetOutpost.parent != null && targetOutpost.parent != society && society.getRel(targetOutpost.parent).state == DipRel.dipState.war)
                    {
                        if (targetLocations.Count == 0 || distance >= map.getStepDist(location, loc))
                        {
                            if (distance > map.getStepDist(location, loc))
                            {
                                targetLocations.Clear();
                            }

                            targetLocations.Add(loc);
                        }
                    }
                }
                else if (loc.soc != society && society.getRel(loc.soc).state == DipRel.dipState.war)
                {
                    if (targetLocations.Count == 0 || distance >= map.getStepDist(location, loc))
                    {
                        if (distance > map.getStepDist(location, loc))
                        {
                            targetLocations.Clear();
                        }

                        targetLocations.Add(loc);
                    }
                }
            }

            if (targetLocations.Count > 0)
            {
                targetLocation = targetLocations[Eleven.random.Next(targetLocations.Count)];
            }

            if (targetLocation != null)
            {
                task = new Task_GoToLocation(targetLocation);
                return;
            }

            if (location.index == homeLocation)
            {
                if (hp < maxHp)
                {
                    task = new Task_Recruit();
                    return;
                }
            }
            else
            {
                task = new Task_GoToLocation(map.locations[homeLocation]);
                return;
            }
        }
    }
}
