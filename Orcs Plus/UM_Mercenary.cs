using Assets.Code;
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
    public class UM_Mercenary : UM
    {
        public SocialGroup source;

        public SocialGroup buyer;

        public int duration;

        public UM_Mercenary(Location location, SocialGroup source, SocialGroup buyer, int armySize, int duration = 25)
            : base (location, source)
        {
            this.source = source;
            this.buyer = buyer;
            maxHp = armySize;
            hp = maxHp;
            this.duration = duration;
        }

        public override string getName()
        {
            switch (source)
            {
                case SG_Orc orcs:
                    if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcs, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.ophanim_PerfectSociety)
                    {
                        return "Perfect " + source.getCapitalHex().location.shortName + " Mercenary Horde";
                    }
                    return source.getCapitalHex().location.shortName + " Mercenary Horde";
                case Soc_Elven elves:
                    if (elves.isDarkEmpire)
                    {
                        return "Dark Elven Mercenaries of " + elves.name;
                    }
                    return "Elven Mercenaries of " + elves.name;
                case Society society:
                    if (society.isOphanimControlled)
                    {
                        return "Perfect Mercenary Company of " + society.getName();
                    }
                    else if (society.isDarkEmpire)
                    {
                        return "Dark Empire Mercenary Company";
                    }
                    return "Mercenary Company of the " + society.getName();
                default:
                    return "Mercenary Company";
            }
        }

        public override Sprite getPortraitForeground()
        {
            switch(source)
            {
                case SG_Orc orcs:
                    if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcs, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.ophanim_PerfectSociety)
                    {
                        return EventManager.getImg("OrcsPlus.Foreground_PerfectHorde.png");
                    }
                    return map.world.textureStore.unit_orc;
                case Soc_Elven _:
                    return map.world.textureStore.unit_elfArmy;
                case Society society:
                    if (society.isOphanimControlled)
                    {
                        return map.world.textureStore.unit_humanArmy_ophanim;
                    }
                    else if (society.isDarkEmpire)
                    {
                        return map.world.textureStore.unit_humanArmy_elites;
                    }
                    else
                    {
                        return map.world.textureStore.unit_humanArmy;
                    }
                default:
                    return map.world.textureStore.unit_humanArmy;
            }
        }

        public override void turnTickInner(Map map)
        {
            if ((duration == 1 && society == buyer) || buyer.isGone() || buyer.lastTurnLocs.Count == 0)
            {
                duration = 0;
                society = source;
                task = null;
            }

            if (society == buyer && duration > 0)
            {
                duration--;
            }

            if (source.getRel(buyer).state == DipRel.dipState.war)
            {
                disband(map, "Contract Terminated");
                return;
            }
        }

        public override void turnTickAI()
        {
            if (duration <= 0)
            {
                if (source.isGone() || source.lastTurnLocs.Count == 0)
                {
                    if (location.soc == null)
                    {
                        disband(map, "Contract Ended");
                        return;
                    }
                    
                    task = new CommunityLib.Task_GoToWilderness(true);
                    return;
                }

                if (location.soc == source)
                {
                    disband(map, "Contract Ended");
                    return;
                }

                task = new Task_GoToSocialGroup(source);
                return;
            }

            if (hp < maxHp * 0.3 && turnTickAI_tryGoHeal())
            {
                return;
            }

            if (duration > 0 && society != buyer)
            {
                if (location.soc != buyer)
                {
                    task = new Task_GoToSocialGroup(buyer);
                    return;
                }
                else
                {
                    society = buyer;
                    homeLocation = location.index;
                }
            }

            if (source is SG_Orc && turnTickAI_OrcArmy())
            {
                return;
            }

            if (turnTickAI_humanArmy())
            {
                return;
            }

            if (hp < maxHp && turnTickAI_tryGoHeal())
            {
                return;
            }

            if (location.index != homeLocation)
            {
                task = new Task_GoToLocation(map.locations[homeLocation]);
                return;
            }
        }

        public bool turnTickAI_tryGoHeal()
        {
            if (!source.isGone() && source.lastTurnLocs.Count > 0)
            {
                bool canRecruit = false;

                if (source is SG_Orc)
                {
                    if (map.locations.Any(l => l.soc == source && l.settlement is Set_OrcCamp))
                    {
                        canRecruit = true;
                    }
                }
                else
                {
                    canRecruit = true;
                }

                if (canRecruit)
                {
                    if (location.soc == source)
                    {
                        if (source is SG_Orc && !(location.settlement is Set_OrcCamp))
                        {
                            int steps = -1;
                            Location targetLocation = null;
                            List<Location> targetLocations = new List<Location>();
                            foreach (Location loc in map.locations)
                            {
                                if (loc.soc == source && loc.settlement is Set_OrcCamp)
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

                            if (targetLocation != null)
                            {
                                task = new Task_GoToLocation(targetLocation);
                                return true;
                            }

                            canRecruit = false;
                        }

                        if (canRecruit)
                        {
                            task = new Task_Recruit();
                            return true;
                        }
                    }

                    if (canRecruit)
                    {
                        task = new Task_GoToSocialGroup(source);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool turnTickAI_humanArmy()
        {
            int steps = -1;
            UM target = null;
            List<UM> targets = new List<UM>();
            Location targetLocation = null;
            List<Location> targetLocations = new List<Location>();
            DipRel rel = null;

            foreach (Unit unit in location.map.units)
            {
                // Will not attack non militray units or units of this society.
                if (unit is UM um && um.society != null && um.society != society && !(um is UM_Refugees))
                {
                    if (!(um.location.soc == um.society && um.hp < um.maxHp / 3))
                    {
                        rel = society.getRel(um.society);
                        if (rel.state == DipRel.dipState.war || (rel.state == DipRel.dipState.hostile && um.location.soc == society))
                        {
                            int engagedHP = 0;
                            foreach (Unit unit2 in map.units)
                            {
                                if (unit2.society == society && unit2 is UM um2)
                                {
                                    bool engaging = false;
                                    Task_AttackArmy attackTask = um2.task as Task_AttackArmy;

                                    if (attackTask == null)
                                    {
                                        Task_InBattle battleTask = um2.task as Task_InBattle;
                                        if (battleTask != null && (battleTask.battle.attackers.Contains(um) || battleTask.battle.defenders.Contains(um)))
                                        {
                                            engaging = true;
                                        }
                                    }
                                    else
                                    {
                                        engaging = true;
                                    }

                                    if (engaging)
                                    {
                                        engagedHP += um2.hp;
                                    }
                                }
                            }

                            int dist = map.getStepDist(location, um.location);
                            if (engagedHP < um.hp * 2 && !fightingMutualEnemy(um) && (steps == -1 || dist <= steps))
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
                task = new Task_AttackArmy(target, this);
                return true;
            }

            if (society.isAtWar())
            {
                if (location.soc != null && location.soc != society && location.settlement != null)
                {
                    rel = society.getRel(location.soc);
                    if (rel.state == DipRel.dipState.war && rel.war != null)
                    {
                        if (rel.war.attackerObjective == War.warType.INVASION)
                        {
                            if (!(society is Society) || (!location.settlement.isHuman && !(location.settlement is Set_CityRuins) && !(location.settlement is Set_TombOfGods)))
                            {
                                task = new Task_RazeLocation();
                                return true;
                            }

                            task = new Task_CaptureLocation();
                            return true;
                        }
                    }
                    task = new Task_RazeLocation();
                    return true;
                }

                int siegingHP = 0;

                foreach (Unit unit3 in map.units)
                {
                    if (unit3 is UM um3 && um3.society == society && (um3.task is Task_GoToLocation || um3.task is Task_RaidLocation || um3.task is Task_RazeLocation || um3.task is Task_CaptureLocation))
                    {
                        siegingHP += um3.hp;
                    }
                }

                steps = -1;
                targetLocations.Clear();
                targetLocation = null;

                foreach (Location loc in map.locations)
                {
                    if (loc.soc != null && loc.soc != society && society.getRel(loc.soc).state == DipRel.dipState.war)
                    {
                        if (siegingHP < society.currentMilitary || siegingHP < loc.soc.currentMilitary * 2.0)
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
                }

                if (targetLocations.Count == 1)
                {
                    targetLocation = targetLocations[0];
                }
                else if (targetLocations.Count > 1)
                {
                    targetLocation = targetLocations[Eleven.random.Next(targetLocations.Count)];
                }

                if (targetLocation != null)
                {
                    task = new Task_GoToLocation(targetLocation);
                    return true;
                }
            }

            return false;
        }

        public bool turnTickAI_OrcArmy()
        {
            SG_Orc orcSociety = source as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            int steps = -1;
            UM target = null;
            List<UM> targets = new List<UM>();

            if (location.soc != null && location.soc != society && society.getRel(location.soc).state == DipRel.dipState.war && location.settlement != null)
            {
                task = new Task_RazeLocation();
                return true;
            }

            Pr_HumanOutpost outpost = location.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
            if (outpost != null && outpost.parent != null && society.getRel(outpost.parent).state == DipRel.dipState.war)
            {
                task = new Task_RazeOutpost();
                return true;
            }

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
                                DipRel rel = null;
                                if (um.society != null)
                                {
                                    rel = um.society.getRel(um.society);
                                }
                                // Will attack military units that they are at war with, or that they are hostile with and are within this army's territory.
                                if (um.society == null || rel.state == DipRel.dipState.war || (rel.state == DipRel.dipState.hostile && unit.location.soc == um.society && ModCore.Get().isHostileAlignment(um.society as SG_Orc, um.society)))
                                {
                                    // Ignore units that are already in combat with armies that the orcs are also at war with.
                                    if (!fightingMutualEnemy(um) && !checkIsCordyceps(um, orcCulture))
                                    {
                                        if (targets.Count == 0 || dist <= steps)
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
                task = new Task_AttackArmy(target, this);
                return true;
            }

            if (society.isAtWar())
            {
                steps = -1;
                Location targetLocation = null;
                List<Location> targetLocations = new List<Location>();

                if (orcCulture != null && orcCulture.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < 0)
                {
                    if (ModCore.Get().data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null && intDataCord.typeDict.TryGetValue("God", out Type t) && t != null)
                    {
                        if (map.overmind.god.GetType() == t || map.overmind.god.GetType().IsSubclassOf(t))
                        {
                            FieldInfo FI_VespidicAttack = AccessTools.Field(t, "God_Insect.vespidSwarmTarget");
                            if (FI_VespidicAttack != null)
                            {
                                Location vespidicTarget = (Location)FI_VespidicAttack.GetValue(map.overmind.god);
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
                    return true;
                }
            }

            return false;
        }

        private static bool checkIsCordyceps(UM um, HolyOrder_Orcs orcCulture)
        {
            bool cordyceps = false;
            if (orcCulture != null && orcCulture.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < 0)
            {
                if (ModCore.Get().data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null)
                {
                    if (um is UM_Refugees refugee)
                    {
                        if (refugee.task != null)
                        {
                            if (intDataCord.typeDict.TryGetValue("Doomed", out Type doomedType) && doomedType != null && (refugee.task.GetType() == doomedType || refugee.task.GetType().IsSubclassOf(doomedType)))
                            {
                                if (intDataCord.typeDict.TryGetValue("Hive", out Type hiveType) && hiveType != null)
                                {
                                    if (um.map.locations.Any(l => l.settlement != null && (l.settlement.GetType() == hiveType || l.settlement.GetType().IsSubclassOf(hiveType))))
                                    {
                                        cordyceps = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (intDataCord.typeDict.TryGetValue("VespidicSwarm", out Type vSwarmType) && vSwarmType != null && (um.GetType() == vSwarmType || um.GetType().IsSubclassOf(vSwarmType)))
                    {
                        cordyceps = true;
                    }
                }
            }

            return cordyceps;
        }

        public override int[] getPositiveTags()
        {
            return new int[]
            {
                source.index + 20000,
                buyer.index + 20000
            };
        }
    }
}
