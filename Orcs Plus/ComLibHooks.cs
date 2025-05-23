﻿using Assets.Code;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Orcs_Plus
{
    public class ComLibHooks
    {
        private Dictionary<UM, int> armyBattle_AttackerOrcHPDict = new Dictionary<UM, int>();
        private Dictionary<UM, int> armyBattle_DefenderOrcHPDict = new Dictionary<UM, int>();
        private Map map;

        public ComLibHooks(Map map)
        {
            this.map = map;
            HooksDelegateRegistry registry = ModCore.GetComLib().HookRegistry;

            registry.RegisterHook_onGraphicalUnitUpdated(onGraphicalUnitUpdated);
            registry.RegisterHook_onPathfinding_AllowSecondPass(onPathfinding_AllowSecondPass);
            registry.RegisterHook_onGetTradeRouteEndpoints(onGetTradeRouteEndpoints);
            registry.RegisterHook_onPlayerOpensReligionUI(onPlayerOpensReligionUI);
            registry.RegisterHook_onPlayerInfluenceTenet(onPlayerInfluenceTenet);
            registry.RegisterHook_onAgentBattle_ReinforceFromEscort(onAgentBattle_ReinforceFromEscort);
            registry.RegisterHook_onAgentBattle_ReceiveDamage(onAgentBattle_ReceiveDamage);
            registry.RegisterHook_onUnitDeath_StartOfProcess(onUnitDeath_StartOfProcess);
            registry.RegisterHook_onRazeLocation_StartOfProcess(onRazeLocation_StartOfProcess);
            registry.RegisterHook_onRazeLocation_EndOfProcess(onRazeLocation_EndOfProcess);
            registry.RegisterHook_onSettlementCalculatesShadowGain(onSettlementComputesShadowGain);
            registry.RegisterHook_onBrokenMakerSleeps_StartOfProcess(ModCore.Get().onBrokenMakerSleep_StartOfSleep);
            registry.RegisterHook_onBrokenMakerSleeps_TurnTick(ModCore.Get().onBrokenMakerSleep_TurnTick);
            registry.RegisterHook_onBrokenMakerSleeps_EndOfProcess(ModCore.Get().onBrokenMakerSleep_EndOfSleep);
            registry.RegisterHook_onArmyBattleCycle_StartOfProcess(onArmyBattleCycle_StartOfProcess);
            registry.RegisterHook_onArmyBattleCycle_DamageCalculated(onArmyBattleCycle_DamageCalculated);
            registry.RegisterHook_onUnitReceivesArmyBattleDamage(onUnitReceivesArmyBattleDamage);
            registry.RegisterHook_onArmyBattleCycle_EndOfProcess(onArmyBattleCycle_EndOfProcess);
            registry.RegisterHook_onPopupHolyOrder_DisplayInfluenceHuman(onPopupHolyOrder_DisplayInfluenceHuman);
            registry.RegisterHook_onPopupHolyOrder_DisplayInfluenceElder(onPopupHolyOrder_DisplayInfluenceElder);
            registry.RegisterHook_onPopupHolyOrder_DisplayBudget(onPopupHolyOrder_DisplayBudget);
            registry.RegisterHook_onPopupHolyOrder_DisplayStats(onPopupHolyOrder_DisplayStats);
            registry.RegisterHook_onSettlementFallIntoRuin_StartOfProcess(onSettlementFallIntoRuin_StartOfProcess);
            registry.RegisterHook_interceptAgentAI(interceptAgentAI);
            registry.RegisterHook_onAgentAI_EndOfProcess(onAgentAI_EndOfProcess);
            registry.RegisterHook_onLocationViewFaithButton_GetHolyOrder(onLocationViewFaithButton_GetHolyOrder);
            registry.RegisterHook_onUIScroll_Unit_populateUM(onUIScroll_Unit_populateUM);
            registry.RegisterHook_interceptReplaceItem(interceptReplaceItem);
            registry.RegisterHook_populatingMonsterActions(populatingMonsterActions);
            registry.RegisterHook_onActionTakingMonsterAIDecision(onActionTakingMonsterAIDecision);
            registry.RegisterHook_onBrokenMakerPowerCreatesAgent_ProcessCurse(onBrokenMakerPowerCreatesAgent_ProcessCurse);
        }

        public void onGraphicalUnitUpdated(GraphicalUnit graphicalUnit)
        {
            if (graphicalUnit.unit is UAEN_OrcElder elder)
            {
                MapMaskManager.maskType mask = graphicalUnit.unit.map.masker.mask;
                if (mask == MapMaskManager.maskType.RELIGION)
                {
                    HolyOrder targetOrder = graphicalUnit.unit.map.world.ui.uiScrollables.scrollable_threats.targetOrder;
                    if (targetOrder == null || targetOrder == elder.society)
                    {
                        graphicalUnit.portraitLayer.color = Color.white;
                        graphicalUnit.backgroundLayer.color = Color.white;
                        graphicalUnit.borderLayer1.color = Color.white;
                        graphicalUnit.borderLayer2.color = Color.white;
                        graphicalUnit.ringLayer.color = Color.white;
                    }
                }
            }
        }

        public bool onPathfinding_AllowSecondPass(Location loc, Unit u, List<int> expectedMapLayers, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates)
        {
            if (u != null && u.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety.canGoUnderground())
            {
                pathfindingDelegates.Remove(Pathfinding.delegate_LAYERBOUND);

                return true;
            }

            return false;
        }

        public void onGetTradeRouteEndpoints(Map map, List<Location> endpoints)
        {
            if (map.overmind.god is God_Mammon)
            {
                foreach (SocialGroup sg in map.socialGroups)
                {
                    if (sg is HolyOrder_Orcs orcCulture && !orcCulture.isGone() && orcCulture.capital != -1 && orcCulture.tenet_god is H_Orcs_MammonClient && orcCulture.tenet_god.status < -1)
                    {
                        endpoints.Add(map.locations[orcCulture.capital]);
                    }
                }
            }
        }

        public void onPlayerOpensReligionUI(HolyOrder order)
        {
            ModCore.Get().powers.updateOrcPowers(order.map);
        }

        public void onPlayerInfluenceTenet(HolyOrder order, HolyTenet tenet)
        {
            ModCore.Get().powers.updateOrcPowers(order.map);
        }

        public Minion onAgentBattle_ReinforceFromEscort(UA ua, UM escort)
        {
            if (escort is UM_VengenceHorde)
            {
                return new M_OrcWarrior(ua.map);
            }

            return null;
        }

        public int onAgentBattle_ReceiveDamage(PopupBattleAgent popup, BattleAgents battle, UA defender, Minion minion, int dmg, int row)
        {
            //Console.WriteLine("OrcsPlus: ReceiveDamage hook called");

            if ((minion == null || minion.isDead) && defender.person.items.Any(i => i is I_SnakeskinArmour))
            {
                dmg--;
            }

            return dmg;
        }

        public void onUnitDeath_StartOfProcess(Unit u, string v, Person killer)
        {
            //Console.WriteLine("Orcs_Plus: Unit died");

            if (u == null || u is UA)
            {
                //Console.WriteLine("Orcs_Plus: Unit is Person");
                return;
            }
            //Console.WriteLine("Orcs_Plus: Unit is not Person");

            // Orc Logic
            SG_Orc orcSociety;
            HolyOrder_Orcs orcCulture;

            if (u is UM_Mercenary mercenary)
            {
                //Console.WriteLine("Orcs_Plus: UM is Mercenary Company");
                orcSociety = mercenary.source as SG_Orc;
                orcCulture = mercenary.source as HolyOrder_Orcs;
            }
            else
            {
                orcSociety = u.society as SG_Orc;
                orcCulture = u.society as HolyOrder_Orcs;
            }

            if (orcSociety != null)
            {
                //Console.WriteLine("Orcs_Plus: UM is orc army");
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (u is UM um)
            {
                //Console.WriteLine("Orcs_Plus: Unit is UM");
                if (orcCulture != null && orcCulture.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < -1)
                {
                    //Console.WriteLine("Orcs_Plus: UM is subject to Insectile Symbiosis");
                    if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.typeDict.TryGetValue("VespidicSwarm", out Type vSwarmType))
                    {
                        Location loc = um.location;
                        SocialGroup soc = um.map.soc_dark;
                        if (intDataCord.typeDict.TryGetValue("Swarm", out Type swarmType))
                        {
                            if (loc.soc != null && (loc.soc.GetType() == swarmType || loc.soc.GetType().IsSubclassOf(swarmType)))
                            {
                                soc = loc.soc;
                            }
                        }

                        object[] args = new object[] {
                                loc,
                                soc,
                                um.maxHp / 2
                            };
                        UM vSwarm = (UM)Activator.CreateInstance(vSwarmType, args);
                        um.map.units.Add(vSwarm);
                        loc.units.Add(vSwarm);
                    }
                }
            }

            onUnitDeath_InfluenceGain(u, v, killer);
        }

        public void onUnitDeath_InfluenceGain(Unit u, string v, Person killer)
        {
            //Console.WriteLine("Orcs_Plus: UM has died");

            SG_Orc orcSociety = u.society as SG_Orc;
            HolyOrder_Orcs orcCulture = u.society as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            HashSet<SG_Orc> influencedOrcSocietyHashSet = new HashSet<SG_Orc>();
            HolyOrder_Orcs influencedOrcCulture_Direct = null;
            List<HolyOrder_Orcs> influencedOrcCultures_Warring = new List<HolyOrder_Orcs>();
            List<HolyOrder_Orcs> influencedOrcCultures_Regional = new List<HolyOrder_Orcs>();

            if (orcCulture != null)
            {
                //Console.WriteLine("Orcs_Plus: UM is orc army");
                influencedOrcCulture_Direct = orcCulture;
                influencedOrcSocietyHashSet.Add(orcCulture.orcSociety);
            }

            if (u.society != null)
            {
                foreach (SocialGroup sg in u.map.socialGroups)
                {
                    if (sg is SG_Orc orcSociety2 && sg.getRel(u.society).state == DipRel.dipState.war && !influencedOrcSocietyHashSet.Contains(orcSociety2) && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
                    {
                        //Console.WriteLine("Orcs_Plus: UM is at war with " + sg.getName());
                        influencedOrcCultures_Warring.Add(orcCulture2);
                        influencedOrcSocietyHashSet.Add(orcSociety2);
                    }
                }
            }

            if (u.task is Task_InBattle battleTask)
            {
                //Console.WriteLine("Orcs_Plus: UM died in battle");
                ModCore.Get().data.getBattleArmyEnemies(battleTask.battle, u, out List<UM> enemies, out List<UA> enemyComs);

                foreach (UM enemy in enemies)
                {
                    if (enemy.society is SG_Orc orcSociety3 && !influencedOrcSocietyHashSet.Contains(orcSociety3) && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null)
                    {
                        //Console.WriteLine("Orcs_Plus: UM was in battle against the " + orcSociety3.getName());
                        influencedOrcCultures_Warring.Add(orcCulture3);
                        influencedOrcSocietyHashSet.Add(orcSociety3);
                    }
                }

                foreach (UA com in enemyComs)
                {
                    if (com.society is SG_Orc orcSociety3 && !influencedOrcSocietyHashSet.Contains(orcSociety3) && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null)
                    {
                        //Console.WriteLine("Orcs_Plus: UM was in battle against the " + orcSociety3.getName());
                        influencedOrcCultures_Warring.Add(orcCulture3);
                        influencedOrcSocietyHashSet.Add(orcSociety3);
                    }
                }
            }

            if (u.location.soc is SG_Orc orcSociety4 && !influencedOrcSocietyHashSet.Contains(orcSociety4) && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety4, out HolyOrder_Orcs orcCulture4) && orcCulture4 != null)
            {
                //Console.WriteLine("Orcs_Plus: UM was trespassing in the lands of the " + orcSociety4.getName());
                influencedOrcCultures_Regional.Add(orcCulture4);
                influencedOrcSocietyHashSet.Add(orcSociety4);
            }

            if (v == "Destroyed in battle" && u.task is Task_InBattle battleTask2)
            {
                //Console.WriteLine("Orcs_Plus: UM died in battle");
                bool influenceElder = false;
                bool influenceHuman = false;
                bool influenceOrc = false;

                ModCore.Get().data.getBattleArmyEnemies(battleTask2.battle, u, out List<UM> enemies, out List<UA> enemyComs);

                foreach (UM enemy in enemies)
                {
                    if (enemy.isCommandable() || enemy is UM_MonsterHeart || enemy is UM_Tentacle)
                    {
                        influenceElder = true;
                    }
                    else if (enemy.society != null && !enemy.society.isDark())
                    {
                        influenceHuman = true;
                    }
                    
                    if (enemy.society is SG_Orc || enemy.society is HolyOrder_Orcs)
                    {
                        influenceOrc = true;
                    }
                }

                foreach (UA com in enemyComs)
                {
                    if (com.isCommandable())
                    {
                        influenceElder = true;
                    }
                    else if (com.society != null && !com.society.isDark())
                    {
                        influenceHuman = true;
                    }
                    
                    if (com.society is SG_Orc || com.society is HolyOrder_Orcs)
                    {
                        influenceOrc = true;
                    }
                }

                if (u.society == null || u.society.isDark())
                {
                    influenceOrc = false;
                }

                if (influenceElder)
                {
                    //Console.WriteLine("Orcs_Plus: UM's eath grant elder influence");
                    if (influencedOrcCulture_Direct != null)
                    {
                        ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Destroyed orc army in battle", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed enemy army in battle", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed tresspassing army in battle", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }
                }

                if (influenceHuman && influencedOrcCulture_Direct != null)
                {
                    //Console.WriteLine("Orcs_Plus: UM's death grants human influence");
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Destroyed orc army in battle", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]));

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed enemy army in battle", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed tresspassing army in battle", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                    }
                }

                if (influenceOrc)
                {
                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Orcs destroyed human army in battle", -1 * ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Orcs destroyed tresspassing human in battle", -1 * ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                    }
                }
            }
            else if (v == "Killed by Ophanim's Smite")
            {
                //Console.WriteLine("Orcs_Plus: UM was smote");
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Smote orc army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Smote enemy army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Smote trespassing army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }
            }
            else if (v == "Killed by a volcano")
            {
                //Console.WriteLine("Orcs_Plus: UM died to volcanic eruption");
                if (killer != null && killer.unit != null)
                {
                    if (killer.unit.isCommandable())
                    {
                        if (influencedOrcCulture_Direct != null)
                        {
                            ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to destroy orc army (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy enemy army (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy trespassing army (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }
                    }
                    else if (killer.unit.society != null && !killer.unit.society.isDark() && influencedOrcCulture_Direct != null)
                    {
                        ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to destroy orc army (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]));

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy enemy army (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy trespassing army (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                        }
                    }
                }
                else
                {
                    if (influencedOrcCulture_Direct != null)
                    {
                        ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("She Who Will Feast's awakening destroyed orc army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("She Who Will Feast's awakening destroyed enemy army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("She Who Will Feast's awakening destroyed trespassing army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }
                }
            }
            else if (v == "Dragged underwater by Tentacles")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("The Evil Beneath destroyed orc army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("The Evil Beneath destroyed enemy army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("The Evil Beneath destroyed trespassing army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }
            }
            else if (v == "Smashed by a Flesh Tentacle")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Escamrak destroyed orc army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Escamrak destroyed enemy army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Escamrak destroyed trespassing army", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }
            }
            else if (v == "killed by cheat" || v == "console" || v == "Killed by console")
            {
                //Console.WriteLine("Orcs_Plus: UM died to console command");
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Killed orc army (console command)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Killed enemy army (console command)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Killed trespassing army (console command)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }
            }
        }

        public void onRazeLocation_StartOfProcess(UM um)
        {
            Settlement set = um.location.settlement;

            onRazeLocation_InfluenceGain(um);

            if (set is Set_OrcCamp)
            {
                Pr_Death death = um.location.properties.OfType<Pr_Death>().FirstOrDefault();
                if (death == null)
                {
                    death = new Pr_Death(um.location);
                    death.charge = 0;
                    um.location.properties.Add(death);
                }

                Property.addToProperty("Militray Action", Property.standardProperties.DEATH, 2.0, set.location);
            }

            if (um.society is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
            {
                if (orcCulture2.tenet_god is H_Orcs_HarbingersMadness harbringers)
                {
                    if (harbringers.status < -1)
                    {
                        if (um.location.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
                        {
                            settlementHuman.ruler.sanity -= 1;

                            if (settlementHuman.ruler.sanity < 1.0)
                            {
                                settlementHuman.ruler.goInsane(-1);
                            }
                        }

                        foreach (Unit unit in map.units)
                        {
                            if (unit.homeLocation == um.location.index && unit is UA agent && !agent.isCommandable() && agent.person != null)
                            {
                                agent.person.sanity -= 1;

                                if (agent.person.sanity < 1.0)
                                {
                                    agent.person.goInsane(-1);
                                }
                            }
                        }
                    }
                }
                else if (orcCulture2.tenet_god is H_Orcs_SecretsOfDestruction secrets)
                {
                    if (secrets.status < 0)
                    {
                        if (set != null && set.defences > 0.0)
                        {
                            double explosivesCost = 10.0;

                            Pr_Orcs_ExplosivesStockpile explosives = (Pr_Orcs_ExplosivesStockpile)map.locations[um.homeLocation].properties.FirstOrDefault(pr => pr is Pr_Orcs_ExplosivesStockpile);
                            if (um is UM_OrcRaiders && (explosives == null || explosives.charge < explosivesCost))
                            {
                                foreach (Location location in map.locations)
                                {
                                    if (location.soc == orcSociety)
                                    {
                                        explosives = (Pr_Orcs_ExplosivesStockpile)location.properties.FirstOrDefault(pr => pr is Pr_Orcs_ExplosivesStockpile && pr.charge >=  explosivesCost);
                                        if (explosives != null)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            if (explosives != null && explosives.charge >= explosivesCost)
                            {
                                explosives.influences.Add(new ReasonMsg("Shipped to the siege of " + set.getName(), -explosivesCost));

                                set.defences -= 20.0;
                                if (set.defences < 0.0)
                                {
                                    set.defences = 0.0;
                                }

                                Property.addToPropertySingleShot("Area devestated by explosions", Property.standardProperties.DEVASTATION, 20.0, um.location);
                            }
                        }
                    }
                }
            }
        }

        public void onRazeLocation_InfluenceGain(UM um)
        {
            SG_Orc orcSociety = um.location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = um.location.soc as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            HashSet<SG_Orc> influencedOrcSocietyHashSet = new HashSet<SG_Orc>();
            HolyOrder_Orcs influencedOrcCulture_Direct = null;
            List<HolyOrder_Orcs> influencedOrcCultures_Warring = new List<HolyOrder_Orcs>();
            List<HolyOrder_Orcs> influencedOrcCultures_Regional = new List<HolyOrder_Orcs>();

            if (orcCulture != null)
            {
                influencedOrcCulture_Direct = orcCulture;
                influencedOrcSocietyHashSet.Add(orcCulture.orcSociety);
            }

            if (um.location.soc != null)
            {
                foreach (SocialGroup sg in um.map.socialGroups)
                {
                    if (sg is SG_Orc orcSociety2 && sg.getRel(um.location.soc).state == DipRel.dipState.war && !influencedOrcSocietyHashSet.Contains(orcSociety2) && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
                    {
                        influencedOrcCultures_Warring.Add(orcCulture2);
                        influencedOrcSocietyHashSet.Add(orcSociety2);
                    }
                }
            }

            foreach (Location neighbour in um.location.getNeighbours())
            {
                if (neighbour.soc is SG_Orc orcSociety3 && !influencedOrcSocietyHashSet.Contains(orcSociety3) && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null)
                {
                    influencedOrcCultures_Regional.Add(orcCulture3);
                    influencedOrcSocietyHashSet.Add(orcSociety3);
                }
            }

            if (um.isCommandable())
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Razing Orc Camp", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation] * 2), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Razing enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation] * 2), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Razing encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation] * 2), true);
                }
            }
            else if (um.society != null && !um.society.isDark() && influencedOrcCulture_Direct != null)
            {
                ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Razing Orc Camp", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]));

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Razing enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Razing encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                }
            }
            
            if ((um.society is SG_Orc || um.society is HolyOrder_Orcs) && (um.location.soc != null && !um.location.soc.isDark()))
            {
                SG_Orc orcSociety2 = um.society as SG_Orc;
                HolyOrder_Orcs orcCulture2 = um.society as HolyOrder_Orcs;

                if (orcSociety2 != null)
                {
                    ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety2, out orcCulture2);
                }

                if (orcCulture2 != null && !influencedOrcSocietyHashSet.Contains(orcCulture2.orcSociety))
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Orcs razing human settlement", -1 * ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Orcs razing human settlement", -1 * ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Orcs razing human settlement", -1 * ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                }
            }
        }

        public void onRazeLocation_EndOfProcess(UM um)
        {
            SG_Orc orcSociety;
            HolyOrder_Orcs orcCulture;

            if (um is UM_Mercenary mercenary)
            {
                //Console.WriteLine("Orcs_Plus: UM is Mercenary Company");
                orcSociety = mercenary.source as SG_Orc;
                orcCulture = mercenary.source as HolyOrder_Orcs;
            }
            else
            {
                orcSociety = um.society as SG_Orc;
                orcCulture = um.society as HolyOrder_Orcs;
            }

            if (orcSociety != null)
            {
                //Console.WriteLine("Orcs_Plus: UM is orc army");
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcCulture != null && orcCulture.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < 0)
            {
                if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord))
                {
                    List<UM_Refugees> refugees = new List<UM_Refugees>();

                    if (intDataCord.typeDict.TryGetValue("Doomed", out Type doomedType) && intDataCord.typeDict.TryGetValue("Hive", out Type hiveType))
                    {
                        foreach (Unit unit in um.location.units)
                        {
                            if (unit is UM_Refugees refugee && refugee.homeLocation == um.location.index && refugee.task == null)
                            {
                                refugees.Add(refugee);
                            }
                        }

                        if (refugees.Count > 0)
                        {
                            bool isHive = false;
                            foreach (Location loc in um.map.locations)
                            {
                                if (loc.settlement != null && (loc.settlement.GetType() == hiveType || loc.settlement.GetType().IsSubclassOf(hiveType)))
                                {
                                    isHive = true;
                                    break;
                                }
                            }

                            if (isHive)
                            {
                                foreach (UM_Refugees refugee in refugees)
                                {
                                    refugee.task = (Task)Activator.CreateInstance(doomedType, new object[] { refugee });
                                }
                            }
                        }
                    }
                }
            }
        }

        public double onSettlementComputesShadowGain(Settlement set, List<ReasonMsg> msgs, double shadowGain)
        {
            if (ModCore.Get().data.orcFestivalShadowGain_Dark.TryGetValue(set, out Pair < double, double> shadowDelta))
            {
                if (shadowDelta.Item1 != 0.0)
                {
                    msgs?.Add(new ReasonMsg("Holy: Dark Festival - Generated Shadow", shadowDelta.Item1));
                    shadowGain += shadowDelta.Item1;
                }
                if (shadowDelta.Item2 != 0.0)
                {
                    msgs?.Add(new ReasonMsg("Holy: Dark Festival - Shadow Movement", shadowDelta.Item2));
                    shadowGain += shadowDelta.Item2;
                }

                ModCore.Get().data.orcFestivalShadowGain_Dark.Remove(set);
            }

            if (ModCore.Get().data.orcFestivalShadowGain_Light.TryGetValue(set, out shadowDelta))
            {
                if (shadowDelta.Item2 != 0.0)
                {
                    msgs?.Add(new ReasonMsg("Holy: Cleansing Festival - Shadow Movement", shadowDelta.Item2));
                    shadowGain += shadowDelta.Item2;
                }
                if (shadowDelta.Item1 != 0.0)
                {
                    msgs?.Add(new ReasonMsg("Holy: Cleansing Festival - Purged Shadow", shadowDelta.Item1));
                    shadowGain += shadowDelta.Item1;
                }

                ModCore.Get().data.orcFestivalShadowGain_Light.Remove(set);
            }

            return shadowGain;
        }

        public void onArmyBattleCycle_StartOfProcess(BattleArmy battle)
        {
            bool explode = false;
            foreach (UM army in battle.attackers.ToList())
            {
                if (army == null || army.homeLocation < 0 || army.homeLocation >= map.locations.Count)
                {
                    continue;
                }

                if (checkArmyDesertion(battle, army))
                {
                    continue;
                }

                if (!explode && checkUseExplosives(battle, army))
                {
                    explode = true;
                }
            }

            if (explode && battle.defenders.Count > 0)
            {
                Property.addToPropertySingleShot("Area devestated by explosions", Property.standardProperties.DEVASTATION, 20.0, battle.defenders[0].location);

                battle.messages.Add("All defending armies have suffered 4 damage from explosive bombardment.");
                foreach (UM army in battle.defenders.ToList())
                {
                    if (army == null)
                    {
                        continue;
                    }

                    army.hp -= 4;
                    if (army.hp <= 0)
                    {
                        battle.messages.Add(army.getName() + " was destroyed by explosive bombardment.");
                        battle.defenders.Remove(army);
                        army.task = null;
                        army.die(map, "Destroyed in battle");
                    }
                }
            }

            explode = false;
            foreach (UM army in battle.defenders.ToList())
            {
                if (army == null || army.homeLocation < 0 || army.homeLocation >= map.locations.Count)
                {
                    continue;
                }

                if (checkArmyDesertion(battle, army))
                {
                    continue;
                }

                if (!explode && checkUseExplosives(battle, army))
                {
                    explode = true;
                }
            }

            if (explode && battle.attackers.Count > 0)
            {
                Property.addToPropertySingleShot("Area devestated by explosions", Property.standardProperties.DEVASTATION, 20.0, battle.attackers[0].location);

                battle.messages.Add("All attacking armies have suffered 4 damage from explosive bombardment.");
                foreach (UM army in battle.attackers.ToList())
                {
                    if (army == null)
                    {
                        continue;
                    }

                    army.hp -= 4;
                    if (army.hp <= 0)
                    {
                        battle.messages.Add(army.getName() + " was destroyed by explosive bombardment.");
                        battle.attackers.Remove(army);
                        army.task = null;
                        army.die(map, "Destroyed in battle");
                    }
                }
            }
        }

        public bool checkArmyDesertion(BattleArmy battle, UM army)
        {
            Location home = map.locations[army.homeLocation];
            if (home != null && home.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
            {
                if (settlementHuman.ruler.house.curses.Any(curse => curse is Curse_BrokenSpirit))
                {
                    army.hp -= 2;
                    if (army.hp > 0)
                    {
                        battle.messages.Add(army.getName() + " has suffered desertion (2 damage).");
                    }
                    else
                    {
                        battle.messages.Add(army.getName() + " deserts battle.");
                        if (battle.attackers.Contains(army))
                        {
                            battle.attackers.Remove(army);
                        }
                        else if (battle.defenders.Contains(army))
                        {
                            battle.defenders.Remove(army);
                        }
                        army.task = null;
                        army.disband(map, "Desertion");
                        return true;
                    }
                }
            }

            return false;
        }

        public bool checkUseExplosives(BattleArmy battle, UM army)
        {
            SG_Orc orcSociety;
            HolyOrder_Orcs orcCulture;

            if (army is UM_Mercenary mercenary)
            {
                //Console.WriteLine("Orcs_Plus: UM is Mercenary Company");
                orcSociety = mercenary.source as SG_Orc;
                orcCulture = mercenary.source as HolyOrder_Orcs;
            }
            else
            {
                orcSociety = army.society as SG_Orc;
                orcCulture = army.society as HolyOrder_Orcs;
            }

            if (orcSociety != null)
            {
                //Console.WriteLine("Orcs_Plus: UM is orc army");
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            double explosivesCost = 10.0;
            if (orcCulture != null)
            {
                if (orcCulture.tenet_god is H_Orcs_SecretsOfDestruction secret)
                {
                    if (secret.status < -1)
                    {
                        Pr_Orcs_ExplosivesStockpile explosives = (Pr_Orcs_ExplosivesStockpile)map.locations[army.homeLocation].properties.FirstOrDefault(pr => pr is Pr_Orcs_ExplosivesStockpile);
                        if (army is UM_OrcRaiders && (explosives == null || explosives.charge < explosivesCost))
                        {
                            foreach (Location location in orcSociety.lastTurnLocs)
                            {
                                if (location.soc == orcSociety)
                                {
                                    explosives = (Pr_Orcs_ExplosivesStockpile)location.properties.FirstOrDefault(pr => pr is Pr_Orcs_ExplosivesStockpile && pr.charge >= explosivesCost);
                                    if (explosives != null)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        if (explosives != null && explosives.charge >= explosivesCost)
                        {
                            explosives.influences.Add(new ReasonMsg("Shipped to a battle at " + army.location.getName(), -explosivesCost));
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public int onArmyBattleCycle_DamageCalculated(BattleArmy battle, int dmg, UM unit, UM target)
        {
            if (unit is UM_OrcArmy || unit is UM_OrcRaiders)
            {
                dmg = SWWF_boostArmyDamage(battle, dmg, unit);
            }

            if (target is UM_OrcArmy || target is UM_OrcRaiders)
            {
                dmg = SWWF_mitigateArmyDamage(battle, dmg, target);
            }

            return dmg;
        }

        public int SWWF_boostArmyDamage(BattleArmy battle, int dmg, UM unit)
        {
            SG_Orc orcSociety;
            HolyOrder_Orcs orcCulture;

            if (unit is UM_Mercenary mercenary)
            {
                //Console.WriteLine("Orcs_Plus: UM is Mercenary Company");
                orcSociety = mercenary.source as SG_Orc;
                orcCulture = mercenary.source as HolyOrder_Orcs;
            }
            else
            {
                orcSociety = unit.society as SG_Orc;
                orcCulture = unit.society as HolyOrder_Orcs;
            }

            if (orcSociety != null)
            {
                //Console.WriteLine("Orcs_Plus: UM is orc army");
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcCulture != null && orcCulture != null)
            {
                if (orcCulture.tenet_god is H_Orcs_ShadowWarriors shadowWariors && shadowWariors.status < 0)
                {
                    if (unit.homeLocation != -1)
                    {
                        Location home = map.locations[unit.homeLocation];
                        double shadow = home.getShadow();

                        int bonus = 0;
                        if (shadow >= 0.5)
                        {
                            bonus += 1;
                        }
                        if (shadow >= 1.0)
                        {
                            bonus += 1;
                        }
                        if (Eleven.random.NextDouble() <= shadow)
                        {
                            bonus += 1;
                        }

                        if (bonus > 0)
                        {
                            battle.messages.Add("Elder Influence boosted the damage dealt by " + unit.getName() + " by " + bonus + " (home location shadow at " + Math.Floor(shadow * 100) + "%)");
                            dmg += bonus;
                        }
                    }
                }
            }

            return dmg;
        }

        public int SWWF_mitigateArmyDamage(BattleArmy battle, int dmg, UM target)
        {
            SG_Orc orcSociety;
            HolyOrder_Orcs orcCulture;

            if (target is UM_Mercenary mercenary)
            {
                //Console.WriteLine("Orcs_Plus: UM is Mercenary Company");
                orcSociety = mercenary.source as SG_Orc;
                orcCulture = mercenary.source as HolyOrder_Orcs;
            }
            else
            {
                orcSociety = target.society as SG_Orc;
                orcCulture = target.society as HolyOrder_Orcs;
            }

            if (orcSociety != null)
            {
                //Console.WriteLine("Orcs_Plus: UM is orc army");
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcCulture != null && orcCulture.tenet_god is H_Orcs_ShadowWarriors shadowWariors && shadowWariors.status < -1)
            {
                if (target.homeLocation != -1)
                {
                    Location home = map.locations[target.homeLocation];
                    double shadow = home.getShadow();

                    int bonus = 0;
                    if (shadow >= 0.5)
                    {
                        bonus += 1;
                    }
                    if (shadow >= 1.0)
                    {
                        bonus += 1;
                    }
                    if (Eleven.random.NextDouble() <= shadow)
                    {
                        bonus += 1;
                    }

                    if (bonus > 0)
                    {
                        if (dmg - bonus <= 1)
                        {
                            battle.messages.Add("Elder Influence allowed " + target.getName() + " to almost entirely shrug off the attack (home location shadow at " + Math.Floor(shadow * 100) + "%)");
                        }
                        else
                        {
                            battle.messages.Add("Elder Influence allowed " + target.getName() + " to shrug off " + bonus + " damage (home location shadow at " + Math.Floor(shadow * 100) + "%)");
                        }
                        dmg = Math.Max(1, dmg - bonus);
                    }
                }
            }

            return dmg;
        }

        public int onUnitReceivesArmyBattleDamage(BattleArmy battle, UM u, int dmg)
        {
            SG_Orc orcSociety;
            HolyOrder_Orcs orcCulture;

            if (u is UM_Mercenary mercenary)
            {
                //Console.WriteLine("Orcs_Plus: UM is Mercenary Company");
                orcSociety = mercenary.source as SG_Orc;
                orcCulture = mercenary.source as HolyOrder_Orcs;
            }
            else
            {
                orcSociety = u.society as SG_Orc;
                orcCulture = u.society as HolyOrder_Orcs;
            }

            if (orcSociety != null)
            {
                //Console.WriteLine("Orcs_Plus: UM is orc army");
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcCulture != null && orcCulture.tenet_god is H_Orcs_DeathMastery deathMastery && deathMastery.status < 0)
            {
                if (battle.attackers.Contains(u))
                {
                    if (!armyBattle_AttackerOrcHPDict.ContainsKey(u))
                    {
                        armyBattle_AttackerOrcHPDict.Add(u, u.hp);
                    }
                }
                else
                {
                    if (!armyBattle_DefenderOrcHPDict.ContainsKey(u))
                    {
                        armyBattle_DefenderOrcHPDict.Add(u, u.hp);
                    }
                }
            }

            return dmg;
        }

        public void onArmyBattleCycle_EndOfProcess(BattleArmy battle)
        {
            manageUndead_Ixthus(battle);
        }

        private void manageUndead_Ixthus(BattleArmy battle = null)
        {
            Location location = null;

            int attackerDeadHP = 0;
            int defenderDeadHP = 0;
            foreach (KeyValuePair<UM, int> pair in armyBattle_AttackerOrcHPDict)
            {
                if (location == null && pair.Key.location != null)
                {
                    location = pair.Key.location;
                }

                if (pair.Key.hp < pair.Value)
                {
                    SG_Orc orcSociety;
                    HolyOrder_Orcs orcCulture;

                    if (pair.Key is UM_Mercenary mercenary)
                    {
                        //Console.WriteLine("Orcs_Plus: UM is Mercenary Company");
                        orcSociety = mercenary.source as SG_Orc;
                        orcCulture = mercenary.source as HolyOrder_Orcs;
                    }
                    else
                    {
                        orcSociety = pair.Key.society as SG_Orc;
                        orcCulture = pair.Key.society as HolyOrder_Orcs;
                    }

                    if (orcSociety != null)
                    {
                        //Console.WriteLine("Orcs_Plus: UM is orc army");
                        ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
                    }

                    if (orcCulture != null && orcCulture.tenet_god is H_Orcs_DeathMastery deathMastery && deathMastery.status < 0)
                    {
                        double hpConversion = deathMastery.status < -1 ? 1.5 * (pair.Value - pair.Key.hp) : 1 * (pair.Value - pair.Key.hp);
                        int deathHP = (int)Math.Floor(hpConversion);
                        if (Eleven.random.NextDouble() < hpConversion - deathHP)
                        {
                            deathHP++;
                        }

                        if (deathHP > 0)
                        {
                            attackerDeadHP += deathHP;
                        }
                    }
                }
            }

            foreach (KeyValuePair<UM, int> pair in armyBattle_DefenderOrcHPDict)
            {
                if (location == null && pair.Key.location != null)
                {
                    location = pair.Key.location;
                }

                if (pair.Key.hp < pair.Value)
                {
                    SG_Orc orcSociety;
                    HolyOrder_Orcs orcCulture;

                    if (pair.Key is UM_Mercenary mercenary)
                    {
                        //Console.WriteLine("Orcs_Plus: UM is Mercenary Company");
                        orcSociety = mercenary.source as SG_Orc;
                        orcCulture = mercenary.source as HolyOrder_Orcs;
                    }
                    else
                    {
                        orcSociety = pair.Key.society as SG_Orc;
                        orcCulture = pair.Key.society as HolyOrder_Orcs;
                    }

                    if (orcSociety != null)
                    {
                        //Console.WriteLine("Orcs_Plus: UM is orc army");
                        ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
                    }

                    if (orcCulture != null && orcCulture.tenet_god is H_Orcs_DeathMastery deathMastery && deathMastery.status < 0)
                    {
                        double hpConversion = deathMastery.status < -1 ? 0.75 * (pair.Value - pair.Key.hp) : 0.5 * (pair.Value - pair.Key.hp);
                        int deathHP = (int)Math.Floor(hpConversion);
                        if (Eleven.random.NextDouble() < hpConversion - deathHP)
                        {
                            deathHP++;
                        }

                        if (deathHP > 0)
                        {
                            defenderDeadHP += deathHP;
                        }
                    }
                }
            }

            if (location != null)
            {
                if (attackerDeadHP > 0)
                {
                    UM_UntamedDead dead = null;
                    foreach (Unit u in location.units)
                    {
                        if (u is UM_UntamedDead untamedDead && untamedDead.master == null)
                        {
                            if (dead == null)
                            {
                                dead = untamedDead;

                                if (battle == null)
                                {
                                    break;
                                }
                            }

                            if (untamedDead.task is Task_InBattle bTask && bTask.battle.attackers.Contains(u))
                            {
                                dead = untamedDead;
                                break;
                            }
                        }
                    }

                    if (dead == null)
                    {
                        dead = new UM_UntamedDead(location, map.soc_dark, attackerDeadHP);
                        location.units.Add(dead);
                        map.units.Add(dead);

                        dead.master = null;

                        if (!battle.done)
                        {
                            dead.task = new Task_InBattle(battle);
                            battle.attackers.Add(dead);
                        }
                    }
                    else
                    {
                        dead.hp += attackerDeadHP;
                        dead.maxHp = dead.hp;
                    }
                }

                if (defenderDeadHP > 0)
                {
                    UM_UntamedDead dead = null;
                    foreach (Unit u in location.units)
                    {
                        if (u is UM_UntamedDead untamedDead && untamedDead.master == null)
                        {
                            if (dead == null)
                            {
                                dead = untamedDead;

                                if (battle == null)
                                {
                                    break;
                                }
                            }

                            if (untamedDead.task is Task_InBattle bTask && bTask.battle.attackers.Contains(u))
                            {
                                dead = untamedDead;
                                break;
                            }
                        }
                    }

                    if (dead == null)
                    {
                        dead = new UM_UntamedDead(location, map.soc_dark, defenderDeadHP);
                        location.units.Add(dead);
                        map.units.Add(dead);

                        dead.master = null;

                        if (!battle.done)
                        {
                            dead.task = new Task_InBattle(battle);
                            battle.defenders.Add(dead);
                        }
                    }
                    else
                    {
                        dead.hp += defenderDeadHP;
                        dead.maxHp = dead.hp;
                    }
                }
            }

            armyBattle_AttackerOrcHPDict.Clear();
            armyBattle_DefenderOrcHPDict.Clear();
        }

        public string onPopupHolyOrder_DisplayInfluenceElder(HolyOrder order, string s, int infGain)
        {
            if (order is HolyOrder_Orcs)
            {
                s = s.Remove(s.LastIndexOf(" (+"));
                s += " (+" + infGain + " This Turn)";
            }

            return s;
        }

        public string onPopupHolyOrder_DisplayInfluenceHuman(HolyOrder order, string s, int infGain)
        {
            if (order is HolyOrder_Orcs)
            {
                s = s.Remove(s.LastIndexOf(" (+"));
                s += " (+" + infGain + " This Turn)";
            }

            return s;
        }

        public void onPopupHolyOrder_DisplayBudget(HolyOrder order, List<ReasonMsg> msgs)
        {
            HolyOrder_Orcs orcCulture = order as HolyOrder_Orcs;

            if (orcCulture != null)
            {
                List<ReasonMsg> msgsToRemove = new List<ReasonMsg>();

                foreach (ReasonMsg msg in msgs)
                {
                    switch (msg.msg)
                    {
                        case "Income":
                            msg.msg = "Plunder";
                            msg.value = orcCulture.plunderValue;
                            break;
                        case "Gold for Conversion":
                            msgsToRemove.Add(msg);
                            break;
                        case "Gold for Acolytes":
                            msgsToRemove.Add(msg);
                            break;
                        case "Gold for Temples":
                            msgsToRemove.Add(msg);
                            break;
                        default:
                            break;
                    }
                }

                foreach (ReasonMsg msg in msgsToRemove)
                {
                    msgs.Remove(msg);
                }
            }
        }

        public void onPopupHolyOrder_DisplayStats(HolyOrder order, List<ReasonMsg> msgs)
        {
            HolyOrder_Orcs orcCulture = order as HolyOrder_Orcs;

            if (orcCulture != null)
            {
                List<ReasonMsg> msgsToRemove = new List<ReasonMsg>();

                foreach (ReasonMsg msg in msgs)
                {
                    switch (msg.msg)
                    {
                        case "Worshippers":
                            msgsToRemove.Add(msg);
                            break;
                        case "of which Rulers":
                            msgsToRemove.Add(msg);
                            break;
                        default:
                            break;
                    }
                }

                foreach (ReasonMsg msg in msgsToRemove)
                {
                    msgs.Remove(msg);
                }

                msgs.Insert(0, new ReasonMsg("Agents", orcCulture.agents?.Count ?? 0.0));
                msgs.Add(new ReasonMsg("Camps", (orcCulture.camps?.Count ?? 0.0) + (orcCulture.specializedCamps?.Count ?? 0.0)));
                msgs.Add(new ReasonMsg("of which Specialised", orcCulture.specializedCamps?.Count ?? 0.0));
            }
        }

        public void onSettlementFallIntoRuin_StartOfProcess(Settlement set, string v, object killer = null)
        {
            onSettlementFallIntoRuin_InfluenceGain(set, v, killer);

            // Adds death to witches covens when they are raised.
            if (set is Set_MinorOther)
            {
                foreach (Subsettlement sub in set.subs)
                {
                    if (sub is Sub_WitchCoven)
                    {
                        Pr_Death death = (Pr_Death)set.location.properties.FirstOrDefault(pr => pr is Pr_Death);

                        if (death == null)
                        {
                            death = new Pr_Death(set.location)
                            {
                                charge = 0
                            };
                        }

                        death.influences.Add(new ReasonMsg("Militray Action", 25.0));
                        break;
                    }
                }
            }
        }

        public void onSettlementFallIntoRuin_InfluenceGain(Settlement set, string v, object killer = null)
        {
            SG_Orc orcSociety = set.location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = set.location.soc as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            HashSet<SG_Orc> influencedOrcSocietyHashSet = new HashSet<SG_Orc>();
            HolyOrder_Orcs influencedOrcCulture_Direct = null;
            List<HolyOrder_Orcs> influencedOrcCultures_Warring = new List<HolyOrder_Orcs>();
            List<HolyOrder_Orcs> influencedOrcCultures_Regional = new List<HolyOrder_Orcs>();

            if (orcCulture != null)
            {
                influencedOrcCulture_Direct = orcCulture;
                influencedOrcSocietyHashSet.Add(orcCulture.orcSociety);
            }

            if (set.location.soc != null)
            {
                foreach (SocialGroup sg in set.map.socialGroups)
                {
                    if (sg is SG_Orc orcSociety2 && sg.getRel(set.location.soc).state == DipRel.dipState.war && !influencedOrcSocietyHashSet.Contains(orcSociety2) && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
                    {
                        influencedOrcCultures_Warring.Add(orcCulture2);
                        influencedOrcSocietyHashSet.Add(orcSociety2);
                    }
                }
            }

            foreach (Location neighbour in set.location.getNeighbours())
            {
                if (neighbour.soc is SG_Orc orcSociety3 && !influencedOrcSocietyHashSet.Contains(orcSociety3) && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null)
                {
                    influencedOrcCultures_Regional.Add(orcCulture3);
                    influencedOrcSocietyHashSet.Add(orcSociety3);
                }
            }

            Person pKiller = killer as Person;
            Unit uKiller = killer as Unit;

            if (pKiller != null)
            {
                uKiller = pKiller.unit;
            }

            if (uKiller != null && v == "Razed by " + uKiller.getName())
            {
                if (uKiller.isCommandable() || uKiller is UM_MonsterHeart || uKiller is UM_Tentacle)
                {
                    if (influencedOrcCulture_Direct != null)
                    {
                        ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Razed orc camp", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Razed enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Razed encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }
                }
                else if (uKiller.society != null && !uKiller.society.isDark() && influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Razed orc camp", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]));

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Razed enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Razed encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }
                }

                if (uKiller.society is SG_Orc || uKiller.society is HolyOrder_Orcs)
                {
                    SG_Orc orcSociety2 = uKiller.society as SG_Orc;
                    HolyOrder_Orcs orcCulture2 = uKiller.society as HolyOrder_Orcs;

                    if (orcSociety2 != null)
                    {
                        ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety2, out orcCulture2);
                    }

                    if (orcCulture2 != null && !influencedOrcSocietyHashSet.Contains(orcCulture2.orcSociety))
                    {
                        ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Orcs razing human settlement", -1 * ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Orcs razing human settlement", -1 * ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Orcs razing human settlement", -1 * ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                    }
                }
            }
            else if (v == "Destroyed by smite")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Smote orc camp", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Smote enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Smote encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }
            }
            else if (v == "Killed by a card")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Orc agent died in Death's games", ModCore.Get().data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Enemy agent died in death's games", ModCore.Get().data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Trespassing agent died in death's games", ModCore.Get().data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }
            }
            else if (v == "Destroyed by volcanic eruption")
            {
                if (uKiller != null)
                {
                    if (uKiller.isCommandable())
                    {
                        if (influencedOrcCulture_Direct != null)
                        {
                            ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to destroy orc camp (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy enemy settlement (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy encroaching settlement (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                        }
                    }
                    else if (uKiller.society != null && !uKiller.society.isDark() && influencedOrcCulture_Direct != null)
                    {
                        ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to destroy orc camp (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]));

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy enemy settlement (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]));
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy encroaching settlement (volcanic eruption)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]));
                        }
                    }
                }
                else
                {
                    if (influencedOrcCulture_Direct != null)
                    {
                        ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("She Who Will Feast'a awakening destroyed orc camp", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("She Who Will Feast's awakening destroyed enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Ahe Who Will Feats's awakening destroyed encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }
                }
            }
            else if (v == "Consumed by Kishi's tide")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("The Evil Beneath destroyed orc camp", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RecieveGift]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("The Evil Beneath destroyed enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("The Evil Beneath destroyed encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }
            }
            else if (v == "Consumed by Kishi's tide")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Kishi's tide consumed orc camp", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RecieveGift]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Kishi's tide consumed enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Kishi's tide consumed encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }
            }
            else if (v == "Consumed by the Tide of Flesh")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Flesh tide consumed orc camp", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RecieveGift]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Flesh tide consumed enemy settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Flesh tide consumed encroaching settlement", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }
            }
            else if (v == "Population called away by Escamrak")
            {
                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Population of encroaching settlement called away", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }
            }
            else if (v == "console" || v == "Console" || v == "Cheats")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.Get().TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Destroyed orc camp (console command)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed enemy settlement (console command)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.Get().TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed encroaching settlement (console command)", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }
            }
        }

        public bool interceptAgentAI(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> validChallengeData, List<AgentAI.TaskData> taskData, List<Unit> visibleUnits)
        {
            switch(ua)
            {
                case UAEN_OrcElder elder:
                    return interceptOrc(elder);
                case UAEN_OrcShaman shaman:
                    return interceptOrc(shaman);
                default:
                    break;
            }

            return false;
        }

        private bool interceptOrc(UAEN uaen)
        {
            if (uaen.society.isGone())
            {
                uaen.die(map, "Died in the wilderness", null);
                return true;
            }
            return false;
        }

        public void onAgentAI_EndOfProcess(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> validChallengeData, List<AgentAI.TaskData> validTaskData, List<Unit> visibleUnits)
        {
            if (ua is UAEN_OrcUpstart || ua is UAEN_OrcElder || ua is UAEN_OrcShaman)
            {
                SG_Orc orcSociety = ua.society as SG_Orc;
                HolyOrder_Orcs orcCulture = ua.society as HolyOrder_Orcs;

                if (orcSociety != null)
                {
                    ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
                }
                else if (orcCulture != null)
                {
                    orcSociety = orcCulture.orcSociety;
                }

                if (ua is UAEN_OrcElder elder)
                {
                    if (elder.task is Task_DisruptUA tDisrupt)
                    {
                        elder.task = new Task_Orcs_DisruptUA(elder, tDisrupt.other);
                    }
                }
                
                if (orcSociety != null && ua.task is Task_AttackUnit tAttack && tAttack.target is UA targetUA && targetUA.person != null && targetUA.person.traits.Any(t => t is T_BloodFeud feud && feud.orcSociety == orcSociety))
                {
                    ua.task = new Task_AvengeBloodFeud(ua, targetUA);
                }

                if (ModCore.Get().data.tryGetModIntegrationData("Escamrak", out ModIntegrationData intDataEscam))
                {
                    if (intDataEscam.typeDict.TryGetValue("FleshStatBonusTrait", out Type fleshStatBonusType) && intDataEscam.fieldInfoDict.TryGetValue("FleshStatBonusTrait_BonusType", out FieldInfo FI_BonusType))
                    {
                        Trait fleshStatBonusTrait = ua.person.traits.FirstOrDefault(t => t.GetType() == fleshStatBonusType || t.GetType().IsSubclassOf(fleshStatBonusType));
                        if (fleshStatBonusTrait != null)
                        {
                            string bonusType = "Might";

                            if (ua is UAEN_OrcElder)
                            {
                                bonusType = "Command";
                            }
                            else if (ua is UAEN_OrcShaman)
                            {
                                bonusType = "Lore";
                            }

                            if (ua.task is Task_PerformChallenge challengeTask)
                            {
                                switch (challengeTask.challenge.getChallengeType())
                                {
                                    case Challenge.challengeStat.MIGHT:
                                        bonusType = "Might";
                                        break;
                                    case Challenge.challengeStat.LORE:
                                        bonusType = "Lore";
                                        break;
                                    case Challenge.challengeStat.INTRIGUE:
                                        bonusType = "Intrigue";
                                        break;
                                    case Challenge.challengeStat.COMMAND:
                                        bonusType = "Command";
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else if (ua.task is Task_GoToPerformChallenge goChallengeTask)
                            {
                                switch (goChallengeTask.challenge.getChallengeType())
                                {
                                    case Challenge.challengeStat.MIGHT:
                                        bonusType = "Might";
                                        break;
                                    case Challenge.challengeStat.LORE:
                                        bonusType = "Lore";
                                        break;
                                    case Challenge.challengeStat.INTRIGUE:
                                        bonusType = "Intrigue";
                                        break;
                                    case Challenge.challengeStat.COMMAND:
                                        bonusType = "Command";
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                if (ua.task is Task_AttackUnit || ua.task is Task_AttackUnitWithEscort || ua.task is Task_DisruptUA || ua.task is Task_Bodyguard)
                                {
                                    bonusType = "Might";
                                }
                            }

                            FI_BonusType.SetValue(fleshStatBonusTrait, bonusType);
                        }
                    }
                }
            }
        }

        public HolyOrder onLocationViewFaithButton_GetHolyOrder(Location loc)
        {
            if (loc.settlement is Set_OrcCamp && loc.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                return orcCulture;
            }

            return null;
        }

        public List<Hooks.TaskUIData> onUIScroll_Unit_populateUM(UM um)
        {
            List<Hooks.TaskUIData> tasks = new List<Hooks.TaskUIData>();

            if (um.location.properties.Any(pr => pr is Pr_HumanOutpost))
            {
                Hooks.TaskUIData task_RazeOutpost = new Hooks.TaskUIData
                {
                    onClick = UMTaskDelegates.onClick_RazeOutpost,
                    title = "Raze Outpost at " + um.location.getName(),
                    description = "This army is razing an outpost at this location, preventing positive growth and causing negative growth equal to 10 times the army's hp (" + um.hp * -10 + ").",
                    icon = um.map.world.iconStore.raze,
                    menaceGain = um.map.param.um_menaceGainFromRaze,
                    backColor = new Color(0.8f, 0f, 0f),
                    enabled = true
                };

                tasks.Add(task_RazeOutpost);
            }

            return tasks;
        }

        public bool interceptReplaceItem(Person person, Item item, Item newItem, bool obligateHold)
        {
            if (person.unit is UAEN_OrcUpstart upstart && item is I_HordeBanner banner && banner.orcs == upstart.society)
            {
                return true;
            }

            return false;
        }

        public void populatingMonsterActions(SG_ActionTakingMonster atMonster, List<MonsterAction> actions)
        {
            //Console.WriteLine("OrcsPlus: populatingMonsterActions");
            if (atMonster is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                // Remove attack actions against elder aligned societies if intolerance is at -2.
                //Console.WriteLine("OrcsPlus: Monster is orcSociety with associated orcCulture");
                List<MonsterAction> actionsToRemove = new List<MonsterAction>();

                if (actions.Count > 0)
                {
                    //Console.WriteLine("OrcsPlus: orcSociety has actions");
                    //Console.WriteLine("OrcsPlus: Iterating Actions");
                    foreach (MonsterAction action in actions)
                    {
                        //Console.WriteLine("OrcsPlus: Iterating action " + action.getName() + " of type " + action.GetType().Name);
                        if (action is MA_Orc_Attack attack && attack.target != null)
                        {
                            //Console.WriteLine("OrcsPlus: Action is MA_Orc_Attack");
                            if (orcCulture.tenet_intolerance.status == -2)
                            {
                                //Console.WriteLine("OrcsPlus: orcCulture is Dark tolerant");
                                if ((attack.target.isDark() && !(attack.target is SG_Orc || attack.target is HolyOrder_Orcs)) || (attack.target is Society society && (society.isDarkEmpire || society.isOphanimControlled)))
                                {
                                    actionsToRemove.Add(action);
                                }
                            }
                            else if (orcCulture.tenet_intolerance.status == 2)
                            {
                                //Console.WriteLine("OrcsPlus: orcCulture is Human tolerant");
                                if (!attack.target.isDark() && (!(attack.target is Society society) || !(society.isDarkEmpire || society.isOphanimControlled)))
                                {
                                    actionsToRemove.Add(action);
                                }
                            }
                            //Console.WriteLine("OrcsPlus: Attack iterated");
                        }
                    }

                    //Console.WriteLine("OrcsPlus: Removing " + actionsToRemove.Count + " unwanted actions");
                    foreach (MonsterAction action in actionsToRemove)
                    {
                        actions.Remove(action);
                    }
                    //Console.WriteLine("OrcsPlus: Finished Processing Actions");
                }
                //Console.WriteLine("OrcsPlus: Finished processing orcSociety");

                // Add Great Contruction action to orc societies
                MA_Orcs_GreatConstruction gConstruction = (MA_Orcs_GreatConstruction)orcCulture.monsterActions.FirstOrDefault(ma => ma is MA_Orcs_GreatConstruction);
                if (gConstruction == null)
                {
                    gConstruction = new MA_Orcs_GreatConstruction(orcSociety);
                    orcCulture.monsterActions.Add(gConstruction);
                    actions.Add(gConstruction);
                }
                else if (!actions.Contains(gConstruction))
                {
                    actions.Add(gConstruction);
                }

                // Add Hire Mercinaries action to orc societies if mammon god tenet is elder aligned.
                if (orcCulture.tenet_god is H_Orcs_MammonClient client && client.status < 0)
                {
                    foreach (SocialGroup neighbour in orcSociety.getNeighbours())
                    {
                        if (neighbour is Society society && !(society is HolyOrder) && orcSociety.getRel(society).state != DipRel.dipState.war && society.capital != -1)
                        {
                            MA_Orc_HireMercenaries hireMercs = (MA_Orc_HireMercenaries)orcCulture.monsterActions.FirstOrDefault(ma => ma is MA_Orc_HireMercenaries hire && hire.target == society);
                            if (hireMercs == null)
                            {
                                hireMercs = new MA_Orc_HireMercenaries(orcSociety, society);
                                orcCulture.monsterActions.Add(hireMercs);
                                actions.Add(hireMercs);
                            }
                            else if (!actions.Contains(hireMercs))
                            {
                                actions.Add(hireMercs);
                            }
                        }

                        if (neighbour is SG_Orc orcs && orcSociety.getRel(orcs).state != DipRel.dipState.war && orcs.capital != -1)
                        {
                            MA_Orc_HireMercenaries hireMercs = (MA_Orc_HireMercenaries)orcCulture.monsterActions.FirstOrDefault(ma => ma is MA_Orc_HireMercenaries hire && hire.target == orcs);
                            if (hireMercs == null)
                            {
                                hireMercs = new MA_Orc_HireMercenaries(orcSociety, orcs);
                                orcCulture.monsterActions.Add(hireMercs);
                                actions.Add(hireMercs);
                            }
                            else if (!actions.Contains(hireMercs))
                            {
                                actions.Add(hireMercs);
                            }
                        }
                    }
                }
            }
        }

        public void onActionTakingMonsterAIDecision(SG_ActionTakingMonster monster)
        {
            if (monster.actionUnderway is MA_Orcs_GreatConstruction gConstruction && gConstruction.specialism == 3)
            {
                PrWM_Manticore manticore = (PrWM_Manticore)gConstruction.target.location.properties.FirstOrDefault(pr => pr is PrWM_Manticore);

                if (manticore != null)
                {
                    gConstruction.target.location.properties.Add(new PrWM_CagedManticore(gConstruction.target.location));
                    gConstruction.target.location.properties.Remove(manticore);
                }
            }
        }

        public string onBrokenMakerPowerCreatesAgent_ProcessCurse(Curse curse, Person person, Location location, string text)
        {
            if (curse is Curse_EGlory)
            {
                text = string.Concat(new string[]
                {
                    text,
                    "\n\n",
                    person.getName(),
                    " gains the Blessing of Glory as a trait from the Curse of Glory"
                });
                person.receiveTrait(new T_Et_Glory());
            }
            return text;
        }
    }
}
