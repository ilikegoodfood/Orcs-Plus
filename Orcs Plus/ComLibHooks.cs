﻿using Assets.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using UnityEngine;
using static HarmonyLib.Code;

namespace Orcs_Plus
{
    public class ComLibHooks : CommunityLib.Hooks
    {
        public ComLibHooks(Map map)
            : base(map)
        {

        }

        public override void onPlayerInfluenceTenet(HolyOrder order, HolyTenet tenet)
        {
            if (ModCore.core.godPowers1.Count > 0 || ModCore.core.godPowers2.Count > 0)
            {
                ModCore.core.updateGodPowers(order.map);
            }
        }

        public override void onUnitDeath_StartOfProcess(Unit u, string v, Person killer)
        {
            if (u is UA)
            {
                return;
            }
            // Shipwreck Logic
            if (u.location.isOcean)
            {
                int wreckRoll = Eleven.random.Next(10);

                if (wreckRoll == 0)
                {
                    Pr_Shipwreck wreck = u.location.properties.OfType<Pr_Shipwreck>().FirstOrDefault();
                    if (wreck == null)
                    {
                        wreck = new Pr_Shipwreck(u.location);
                        u.location.properties.Add(wreck);
                    }
                    else
                    {
                        wreck.charge += 20.0;
                    }
                }
            }

            // Orc Logic
            SG_Orc orcs = u.society as SG_Orc;
            HolyOrder_Orcs orcCulture = u.society as HolyOrder_Orcs;

            if (orcs != null)
            {
                ModCore.core.data.orcSGCultureMap.TryGetValue(orcs, out orcCulture);
            }

            if (u is UM um && orcCulture != null)
            {
                SG_Orc orcSociety = um.society as SG_Orc;
                HolyOrder_Orcs orcCulture2 = um.society as HolyOrder_Orcs;

                if (um is UM_Mercenary mercenary)
                {
                    orcSociety = mercenary.source as SG_Orc;
                    orcCulture2 = mercenary.source as HolyOrder_Orcs;
                }

                if (orcSociety != null)
                {
                    ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture2);
                }

                if (orcCulture2 != null && orcCulture2.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < -1)
                {
                    if (ModCore.core.data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null && intDataCord.typeDict.TryGetValue("VespidicSwarm", out Type vSwarmType) && vSwarmType != null)
                    {
                        Location loc = um.location;
                        SocialGroup soc = um.map.soc_dark;
                        if (intDataCord.typeDict.TryGetValue("Swarm", out Type swarmType) && swarmType != null)
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
            SG_Orc orcSociety = u.society as SG_Orc;
            HolyOrder_Orcs orcCulture = u.society as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            HashSet<SG_Orc> influencedOrcSocietyHasSet = new HashSet<SG_Orc>();
            HolyOrder_Orcs influencedOrcCulture_Direct = null;
            List<HolyOrder_Orcs> influencedOrcCultures_Warring = new List<HolyOrder_Orcs>();
            List<HolyOrder_Orcs> influencedOrcCultures_Regional = new List<HolyOrder_Orcs>();

            if (orcCulture != null)
            {
                influencedOrcCulture_Direct = orcCulture;
                influencedOrcSocietyHasSet.Add(orcCulture.orcSociety);
            }

            if (u.society != null)
            {
                foreach (SocialGroup sg in u.map.socialGroups)
                {
                    if (sg is SG_Orc orcSociety2 && sg.getRel(u.society).state == DipRel.dipState.war && !influencedOrcSocietyHasSet.Contains(orcSociety2) && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
                    {
                        influencedOrcCultures_Warring.Add(orcCulture2);
                        influencedOrcSocietyHasSet.Add(orcSociety2);

                    }
                }
            }

            if (u.task is Task_InBattle battleTask)
            {
                ModCore.core.data.getBattleArmyEnemies(battleTask.battle, u, out List<UM> enemies, out List<UA> enemyComs);

                foreach (UM enemy in enemies)
                {
                    if (enemy.society is SG_Orc orcSociety3 && !influencedOrcSocietyHasSet.Contains(orcSociety3) && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null)
                    {
                        influencedOrcCultures_Warring.Add(orcCulture3);
                        influencedOrcSocietyHasSet.Add(orcSociety3);
                    }
                }

                foreach (UA com in enemyComs)
                {
                    if (com.society is SG_Orc orcSociety3 && !influencedOrcSocietyHasSet.Contains(orcSociety3) && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3) && orcCulture3 != null)
                    {
                        influencedOrcCultures_Warring.Add(orcCulture3);
                        influencedOrcSocietyHasSet.Add(orcSociety3);
                    }
                }
            }

            if (u.location.soc is SG_Orc orcSociety4 && !influencedOrcSocietyHasSet.Contains(orcSociety4) && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety4, out HolyOrder_Orcs orcCulture4) && orcCulture4 != null)
            {
                influencedOrcCultures_Regional.Add(orcCulture4);
                influencedOrcSocietyHasSet.Add(orcSociety4);
            }

            if (v == "Destroyed in battle" && u.task is Task_InBattle battleTask2)
            {
                bool influenceElder = false;
                bool influenceHuman = false;

                ModCore.core.data.getBattleArmyEnemies(battleTask2.battle, u, out List<UM> enemies, out List<UA> enemyComs);

                foreach (UM enemy in enemies)
                {
                    if (enemy.isCommandable())
                    {
                        influenceElder = true;
                    }
                    else if (enemy.society != null && !enemy.society.isDark())
                    {
                        influenceHuman = true;
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
                }

                if (influenceElder)
                {
                    if (influencedOrcCulture_Direct != null)
                    {
                        ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Destroyed orc army in battle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed enemy army in battle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed tresspassing army in battle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }
                }

                if (influenceHuman && influencedOrcCulture_Direct != null)
                {
                    ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Destroyed orc army in battle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]));

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed enemy army in battle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed tresspassing army in battle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                    }
                }
            }
            else if (v == "Killed by Ophanim's Smite")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Smote orc army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Smote enemy army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Smote trespassing army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }
            }
            else if (v == "Killed by a volcano")
            {
                if (killer != null && killer.unit != null)
                {
                    if (killer.unit.isCommandable())
                    {
                        if (influencedOrcCulture_Direct != null)
                        {
                            ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to destroy orc army (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy enemy army (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy trespassing army (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }
                    }
                    else if (killer.unit.society != null && !killer.unit.society.isDark() && influencedOrcCulture_Direct != null)
                    {
                        ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to destroy orc army (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]));

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy enemy army (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy trespassing army (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                        }
                    }
                }
                else
                {
                    if (influencedOrcCulture_Direct != null)
                    {
                        ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Awakening destroyed orc army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Awakening destroyed enemy army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Awakening destroyed trespassing army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }
                }
            }
        }

        public override void onRazeLocation_StartOfProcess(UM um)
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

            if (ModCore.core.data.godTenetTypes.TryGetValue(map.overmind.god.GetType(), out Type tenetType) && tenetType != null && tenetType == typeof(H_Orcs_HarbringersMadness))
            {
                if (um.society is SG_Orc orcSociety2 && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null && orcCulture2.tenet_god is H_Orcs_HarbringersMadness harbringers && harbringers.status < -1)
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
                        if (unit.homeLocation == um.location.index && unit is UA agent && !agent.isCommandable() && (agent.person.species is Species_Human || agent.person.species is Species_Elf))
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
        }

        public void onRazeLocation_InfluenceGain(UM um)
        {
            SG_Orc orcSociety = um.location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = um.location.soc as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            HashSet<SG_Orc> influencedOrcSocietyHasSet = new HashSet<SG_Orc>();
            HolyOrder_Orcs influencedOrcCulture_Direct = null;
            List<HolyOrder_Orcs> influencedOrcCultures_Warring = new List<HolyOrder_Orcs>();
            List<HolyOrder_Orcs> influencedOrcCultures_Regional = new List<HolyOrder_Orcs>();

            if (orcCulture != null)
            {
                influencedOrcCulture_Direct = orcCulture;
                influencedOrcSocietyHasSet.Add(orcCulture.orcSociety);
            }

            if (um.location.soc != null)
            {
                foreach (SocialGroup sg in um.map.socialGroups)
                {
                    if (sg is SG_Orc orcSociety2 && sg.getRel(um.location.soc).state == DipRel.dipState.war && !influencedOrcSocietyHasSet.Contains(orcSociety2) && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
                    {
                        influencedOrcCultures_Warring.Add(orcCulture2);
                        influencedOrcSocietyHasSet.Add(orcSociety2);
                    }
                }
            }

            foreach (Location neighbour in um.location.getNeighbours())
            {
                if (neighbour.soc is SG_Orc orcSociety3 && !influencedOrcSocietyHasSet.Contains(orcSociety3) && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3))
                {
                    influencedOrcCultures_Regional.Add(orcCulture3);
                    influencedOrcSocietyHasSet.Add(orcSociety3);
                }
            }

            if (um.isCommandable())
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Razing Orc Camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation] * 2), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razing enemy settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation] * 2), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razing encroaching settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation] * 2), true);
                }
            }
            else if (um.society != null && !um.society.isDark() && influencedOrcCulture_Direct != null)
            {
                ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Razing Orc Camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razing enemy settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razing encroaching settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                }
            }
        }

        public override void onRazeLocation_EndOfProcess(UM um)
        {
            SG_Orc orcSociety = um.society as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (um is UM_Mercenary mercenary)
            {
                orcSociety = mercenary.source as SG_Orc;
            }

            if (orcSociety != null)
            {
                ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcCulture != null && orcCulture.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < 0)
            {
                if (ModCore.core.data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null)
                {
                    intDataCord.typeDict.TryGetValue("Hive", out Type hiveType);
                    intDataCord.typeDict.TryGetValue("Doomed", out Type doomedType);
                    List<UM_Refugees> refugees = new List<UM_Refugees>();

                    if (doomedType != null && hiveType != null)
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

        public override int onArmyBattleCycle_DamageCalculated(BattleArmy battle, int dmg, UM unit, UM target)
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
            if (unit.society is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (orcCulture.tenet_god is H_Orcs_ShadowWarriors shadowWariors && shadowWariors.status < 0)
                {
                    if (unit.homeLocation != -1)
                    {
                        Location home = map.locations[unit.homeLocation];
                        double shadow = shadow = 1.0 - home.hex.purity;

                        int bonus = 0;
                        if (shadow >= 0.5)
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
            if (target.society is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (orcCulture.tenet_god is H_Orcs_ShadowWarriors shadowWariors && shadowWariors.status < -1)
                {
                    if (target.homeLocation != -1)
                    {
                        Location home = map.locations[target.homeLocation];
                        double shadow = 1.0 - home.hex.purity;

                        int bonus = 0;
                        if (shadow >= 0.5)
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
                                battle.messages.Add("Elder Influence allowed " + target.getName() + " to shrug off" + bonus + " damage (home location shadow at " + Math.Floor(shadow * 100) + "%)");
                            }
                            dmg = Math.Max(1, dmg - bonus);
                        }
                    }
                }
            }

            return dmg;
        }

        public override string onPopupHolyOrder_DisplayInfluenceElder(HolyOrder order, string s, int infGain)
        {
            if (order is HolyOrder_Orcs)
            {
                s = s.Remove(s.LastIndexOf(" (+"));
                s += " (+" + infGain + " This Turn)";
            }

            return s;
        }

        public override string onPopupHolyOrder_DisplayInfluenceHuman(HolyOrder order, string s, int infGain)
        {
            if (order is HolyOrder_Orcs)
            {
                s = s.Remove(s.LastIndexOf(" (+"));
                s += " (+" + infGain + " This Turn)";
            }

            return s;
        }

        public override void onPopupHolyOrder_DisplayBudget(HolyOrder order, List<ReasonMsg> msgs)
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

        public override void onPopupHolyOrder_DisplayStats(HolyOrder order, List<ReasonMsg> msgs)
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

        public override void onSettlementFallIntoRuin_StartOfProcess(Settlement set, string v, object killer = null)
        {
            onSettlementFallIntoRuin_InfluenceGain(set, v, killer);

            // Adds death to witches covens when they are raised.
            if (set is Set_MinorOther)
            {
                foreach (Subsettlement sub in set.subs)
                {
                    if (sub is Sub_WitchCoven)
                    {
                        Pr_Death death = null;

                        foreach (Property p in set.location.properties)
                        {
                            death = p as Pr_Death;

                            if (death != null)
                            {
                                break;
                            }
                        }

                        if (death == null)
                        {
                            death = new Pr_Death(set.location);
                            death.charge = 0;
                        }

                        Property.addToPropertySingleShot("Militray Action", Property.standardProperties.DEATH, 25.0, set.location);
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
                ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            HashSet<SG_Orc> influencedOrcSocietyHasSet = new HashSet<SG_Orc>();
            HolyOrder_Orcs influencedOrcCulture_Direct = null;
            List<HolyOrder_Orcs> influencedOrcCultures_Warring = new List<HolyOrder_Orcs>();
            List<HolyOrder_Orcs> influencedOrcCultures_Regional = new List<HolyOrder_Orcs>();

            if (orcCulture != null)
            {
                influencedOrcCulture_Direct = orcCulture;
                influencedOrcSocietyHasSet.Add(orcCulture.orcSociety);
            }

            if (set.location.soc != null)
            {
                foreach (SocialGroup sg in set.map.socialGroups)
                {
                    if (sg is SG_Orc orcSociety2 && sg.getRel(set.location.soc).state == DipRel.dipState.war && !influencedOrcSocietyHasSet.Contains(orcSociety2) && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
                    {
                        influencedOrcCultures_Warring.Add(orcCulture2);
                        influencedOrcSocietyHasSet.Add(orcSociety2);
                    }
                }
            }

            foreach (Location neighbour in set.location.getNeighbours())
            {
                if (neighbour.soc is SG_Orc orcSociety3 && !influencedOrcSocietyHasSet.Contains(orcSociety3) && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety3, out HolyOrder_Orcs orcCulture3))
                {
                    influencedOrcCultures_Regional.Add(orcCulture3);
                    influencedOrcSocietyHasSet.Add(orcSociety3);
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
                if (uKiller.isCommandable())
                {
                    if (influencedOrcCulture_Direct != null)
                    {
                        ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Razed orc camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razed enemy settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razed encroaching settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }
                }
                else if (uKiller.society != null && !uKiller.society.isDark() && influencedOrcCulture_Direct != null)
                {
                    ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Razed orc camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]));

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razed enemy settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razed encroaching settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }
                }
            }
            else if (v == "Killed by Ophanim's Smite")
            {
                if (influencedOrcCulture_Direct != null)
                {
                    ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Smote orc camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                {
                    ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Smote enemy settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                {
                    ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Smote encroaching settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }
            }
            else if (v == "Killed by a volcano")
            {
                if (uKiller != null)
                {
                    if (uKiller.isCommandable())
                    {
                        if (influencedOrcCulture_Direct != null)
                        {
                            ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to destroy orc camp (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy enemy settlement (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy encroaching settlement (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                        }
                    }
                    else if (uKiller.society != null && !uKiller.society.isDark() && influencedOrcCulture_Direct != null)
                    {
                        ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Used geomancy to destroy orc camp (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]));

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy enemy settlement (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]));
                        }

                        foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Used geomancy to destroy encroaching settlement (volcanic eruption)", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]));
                        }
                    }
                }
                else
                {
                    if (influencedOrcCulture_Direct != null)
                    {
                        ModCore.core.TryAddInfluenceGain(influencedOrcCulture_Direct, new ReasonMsg("Awakening destroyed orc camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Warring)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Awakening destroyed enemy settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }

                    foreach (HolyOrder_Orcs orcs in influencedOrcCultures_Regional)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Awakening destroyed encroaching settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                    }
                }
            }
        }

        public override void onSettlementFallIntoRuin_EndOfProcess(Settlement set, string v, object killer = null)
        {
            //Console.WriteLine("OrcsPlus: Settlement fallen into ruin");
            if (set is SettlementHuman settlementHuman && settlementHuman.subs.Count > 0 && settlementHuman.location.settlement is Set_CityRuins ruins)
            {
                //Console.WriteLine("OrcsPlus: settlment is human settlment and is now city ruin");
                bool hasDock = false;

                foreach (Subsettlement sub in settlementHuman.subs)
                {
                    if (sub is Sub_Docks)
                    {
                        hasDock = true;
                        break;
                    }
                }

                if (hasDock)
                {
                    //Console.WriteLine("OrcsPlus: settlement has dock");
                    ModCore.core.shipwreckLocations.Add(set.location);
                }
            }
        }

        public override bool interceptGetVisibleUnits(UA ua, List<Unit> visibleUnits)
        {
            switch (ua)
            {
                case UAEN_OrcElder elder:
                    visibleUnits = elder.getVisibleUnits();
                    return true;
                default:
                    break;
            }

            return false;
        }

        public override bool interceptAgentAI(UA ua, CommunityLib.AgentAI.AIData aiData, List<CommunityLib.AgentAI.ChallengeData> validChallengeData, List<CommunityLib.AgentAI.TaskData> taskData, List<Unit> visibleUnits)
        {
            switch(ua)
            {
                case UAEN_OrcElder elder:
                    return interceptOrcElder(elder);
                case UAEN_OrcShaman shaman:
                    return interceptOrcShaman(shaman);
                default:
                    break;
            }

            return false;
        }

        private bool interceptOrcElder(UAEN_OrcElder elder)
        {
            if (elder.society.isGone())
            {
                elder.die(map, "Died in the wilderness", null);
                return true;
            }
            return false;
        }

        private bool interceptOrcShaman(UAEN_OrcShaman shaman)
        {
            if (shaman.society.isGone())
            {
                shaman.die(map, "Died in the wilderness", null);
                return true;
            }
            return false;
        }

        public override List<TaskData> onUIScroll_Unit_populateUM(UM um)
        {
            List<TaskData> tasks = new List<TaskData>();

            if (um.location.properties.OfType<Pr_HumanOutpost>().FirstOrDefault() != null)
            {
                TaskData task_RazeOutpost = new TaskData();
                task_RazeOutpost.onClick = UMTaskDelegates.onClick_RazeOutpost;
                task_RazeOutpost.title = "Raze Outpost at " + um.location.getName();
                task_RazeOutpost.icon = um.map.world.iconStore.raze;
                task_RazeOutpost.menaceGain = um.map.param.um_menaceGainFromRaze;
                task_RazeOutpost.backColor = new Color(0.8f, 0f, 0f);
                task_RazeOutpost.enabled = true;

                tasks.Add(task_RazeOutpost);
            }

            if (um is UM_OrcRaiders orcRaiders)
            {
                Rt_Orcs_BuildCamp challenge = orcRaiders.rituals.OfType<Rt_Orcs_BuildCamp>().FirstOrDefault();

                if (challenge != null)
                {
                    TaskData task_Orcs_BuildCamp = new TaskData();
                    task_Orcs_BuildCamp.challenge = challenge;
                }
            }

            return tasks;
        }

        public override bool interceptChallengePopout(UM um, TaskData taskData, ref TaskData_Popout popoutData)
        {
            if (um != null && taskData.title == "Raze Outpost at " + um.location.getName())
            {
                popoutData.description = "This army is razing an outpost at this location, preventing positive growth and causing negative growth equal to 10 times the army's hp (" + um.hp * -10 + ").";
                popoutData.menaceGain = um.map.param.um_menaceGainFromRaze;
                popoutData.backColor = new Color(0.6f, 0.0f, 0.0f);

                return true;
            }

            return false;
        }

        public override bool interceptReplaceItem(Person person, Item item, Item newItem, bool obligateHold)
        {
            if (person.unit is UAEN_OrcUpstart upstart && item is I_HordeBanner banner && banner.orcs == upstart.society)
            {
                return true;
            }

            return false;
        }

        public override void populatingMonsterActions(SG_ActionTakingMonster atMonster, List<MonsterAction> actions)
        {
            //Console.WriteLine("OrcsPlus: populatingMonsterActions");
            if (atMonster is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
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

                if (orcCulture.tenet_god is H_Orcs_MammonClient client && client.status < 0)
                {
                    foreach (SocialGroup neighbour in orcSociety.getNeighbours())
                    {
                        if (neighbour is Society society && !(society is HolyOrder) && orcSociety.getRel(society).state != DipRel.dipState.war && society.capital != -1)
                        {
                            bool present = false;
                            foreach(MonsterAction action in orcCulture.monsterActions)
                            {
                                if (action is MA_Orc_HireMercenaries hire && hire.target == society)
                                {
                                    present = true;
                                    actions.Add(action);
                                    break;
                                }
                            }

                            if (!present)
                            {
                                MonsterAction action = new MA_Orc_HireMercenaries(orcSociety, society);
                                orcCulture.monsterActions.Add(action);
                                actions.Add(action);
                            }
                        }

                        if (neighbour is SG_Orc orcs && orcSociety.getRel(orcs).state != DipRel.dipState.war && orcs.capital != -1)
                        {
                            bool present = false;
                            foreach (MonsterAction action in orcCulture.monsterActions)
                            {
                                if (action is MA_Orc_HireMercenaries hire && hire.target == orcs)
                                {
                                    present = true;
                                    actions.Add(action);
                                    break;
                                }
                            }

                            if (!present)
                            {
                                MonsterAction action = new MA_Orc_HireMercenaries(orcSociety, orcs);
                                orcCulture.monsterActions.Add(action);
                                actions.Add(action);
                            }
                        }
                    }
                }
            }
        }
    }
}
