using Assets.Code;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Reflection;
using static UnityEngine.GraphicsBuffer;

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
            return EventManager.getImg("OrcsPlus.Icon_ArmouredBeast.png");
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

        public override void inBattle(BattleArmy battleArmy)
        {
            return;
        }

        public override void turnTickAI()
        {
            SG_Orc orcSociety = society as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

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
            
            if (location.soc != null && location.soc != society && society.getRel(location.soc).state == DipRel.dipState.war && location.settlement != null && !(location.settlement is Set_CityRuins) && !(location.settlement is Set_TombOfGods))
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

            int steps = -1;
            List<UM> targets = new List<UM>();
            UM target = null;

            foreach (Unit unit in location.map.units)
            {
                // Will not attack non militray units or units of this society.
                if (unit is UM um && um.society != society)
                {
                    int dist = map.getStepDist(location, um.location);

                    if (ModCore.Get().isAttackingSociety(this, um))
                    {
                        if (steps == -1 || dist <= steps)
                        {
                            if (dist < steps)
                            {
                                targets.Clear();
                            }

                            targets.Add(um);
                            steps = dist;
                        }
                        continue;
                    }

                    // Will not attack refugees that are further than 1 step distance.
                    if (!(um is UM_Refugees && dist > 1))
                    {
                        // Will attack military units in own societ's territory or at a distance up to 4 step distance away.
                        if (um.location.soc == society || dist <= 4)
                        {
                            // Will only attack military units in own society if they are above recruitment threshold.
                            if (um.location.soc != um.society || um.hp >= um.maxHp / 3)
                            {
                                DipRel rel = null;
                                if (um.society != null)
                                {
                                    rel = society.getRel(um.society);
                                }
                                // Will attack military units that they are at war with, or that they are hostile with and are within this army's territory.
                                if (um.society == null
                                    || rel.state == DipRel.dipState.war
                                    || (rel.state == DipRel.dipState.hostile && unit.location.soc == society && ModCore.Get().isHostileAlignment(society as SG_Orc, um.society)))
                                {
                                    // Ignore units that are already in combat with armies that the orcs are also at war with, and ignore armies that are of elder design based on tenets.
                                    if (!fightingMutualEnemy(um) && ModCore.Get().isHostileAlignment(this, um))
                                    {
                                        if (steps == -1 || dist <= steps)
                                        {
                                            if (dist < steps)
                                            {
                                                targets.Clear();
                                            }

                                            targets.Add(um);
                                            steps = dist;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (targets.Count == 1)
            {
                target = targets[0];
            }
            else if (targets.Count > 1)
            {
                target = targets[Eleven.random.Next(targets.Count)];
            }

            if (target != null)
            {
                task = new Task_DevourArmy(target, this);
                return;
            }

            if (society.isAtWar())
            {
                steps = -1;
                List<Location> targetLocations = new List<Location>();
                Location targetLocation = null;

                if (orcCulture != null && orcCulture.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < 0)
                {
                    if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.typeDict.TryGetValue("God", out Type cordycepsType))
                    {
                        if (map.overmind.god.GetType() == cordycepsType || map.overmind.god.GetType().IsSubclassOf(cordycepsType))
                        {
                            if (intDataCord.fieldInfoDict.TryGetValue("VespidicSwarmTarget", out FieldInfo FI_vSwarmTarget))
                            {
                                Location vespidicTarget = (Location)FI_vSwarmTarget.GetValue(map.overmind.god);
                                if (vespidicTarget != null && vespidicTarget != location && map.getStepDist(location, vespidicTarget) < 3)
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

                if (targetLocation == null)
                {
                    foreach (Location loc in map.locations)
                    {
                        if (loc.soc == null)
                        {
                            Pr_HumanOutpost targetOutpost = loc.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
                            if (targetOutpost != null && targetOutpost.parent != null && targetOutpost.parent != society && society.getRel(targetOutpost.parent).state == DipRel.dipState.war)
                            {
                                int dist = map.getStepDist(location, loc);
                                if (steps == -1 || dist <= steps)
                                {
                                    if (dist < steps)
                                    {
                                        targetLocations.Clear();
                                    }

                                    targetLocations.Add(loc);
                                    steps = dist;
                                }
                            }
                        }
                        else if (loc.soc != society && society.getRel(loc.soc).state == DipRel.dipState.war)
                        {
                            int dist = map.getStepDist(location, loc);
                            if (steps == -1 || dist <= steps)
                            {
                                if (dist < steps)
                                {
                                    targetLocations.Clear();
                                }

                                targetLocations.Add(loc);
                                steps = dist;
                            }
                        }
                    }

                    if (targetLocations.Count == 1)
                    {
                        targetLocation = targetLocations[0];
                    }
                    else if (targetLocations.Count > 1)
                    {
                        targetLocation = targetLocations[Eleven.random.Next(targetLocations.Count)];
                    }
                }

                if (targetLocation != null)
                {
                    task = new Task_GoToLocation(targetLocation);
                    return;
                }
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
