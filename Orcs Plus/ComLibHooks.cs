using Assets.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using UnityEngine;

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

            SG_Orc orcs = u.society as SG_Orc;
            HolyOrder_Orcs orcCulture = u.society as HolyOrder_Orcs;

            if (u is UM um && (orcs != null || orcCulture != null))
            {
                SG_Orc orcSociety = orcs;
                HolyOrder_Orcs orcCulture2 = orcCulture;

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
                    if (ModCore.core.data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null && intDataCord.typeDict.TryGetValue("VespidicSwarm", out Type vSwarmType) && vSwarmType != null && intDataCord.typeDict.TryGetValue("Swarm", out Type swarmType) && swarmType != null)
                    {
                        Location loc = um.location;
                        SocialGroup soc = um.map.soc_dark;
                        if (loc.soc.GetType() == swarmType || loc.soc.GetType().IsSubclassOf(swarmType))
                        {
                            soc = loc.soc;
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

            Unit uKiller = killer?.unit;

            bool infGainElder = false;
            bool infGainHuman = false;

            if (v == "Destroyed in battle")
            {
                BattleArmy battle = (u.task as Task_InBattle)?.battle;

                if (battle == null)
                {
                    return;
                }

                ModCore.core.data.getBattleArmyEnemies(battle, u, out List<UM> enemies, out List<UA> enemyComs);

                if (enemies.Count == 0)
                {
                    return;
                }

                if (enemies.Count > 0)
                {
                    foreach (UM enemy in enemies)
                    {
                        if (enemy.isCommandable())
                        {
                            infGainElder = true;
                        }
                        else if (!enemy.society.isDark())
                        {
                            infGainHuman = true;
                        }

                        if (infGainElder && infGainHuman)
                        {
                            break;
                        }
                    }
                }

                if (enemyComs.Count > 0 && !(infGainElder && infGainHuman))
                {
                    foreach (UA com in enemyComs)
                    {
                        if (com.isCommandable())
                        {
                            infGainElder = true;
                        }
                        else if (!com.society.isDark())
                        {
                            infGainHuman = true;
                        }

                        if (infGainElder && infGainHuman)
                        {
                            break;
                        }
                    }
                }

                if (infGainElder)
                {
                    if (orcCulture != null)
                    {
                        ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg("Destroyed orc army in batle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    if (orcs != null)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed orc army in batle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    List<SG_Orc> orcSocieties = ModCore.core.data.getOrcSocieties(map);

                    if (orcSocieties?.Count > 0 && u.society != null)
                    {
                        foreach (SG_Orc orcSociety in orcSocieties)
                        {
                            if (orcs != null && orcs == orcSociety)
                            {
                                continue;
                            }

                            if (orcSociety.getRel(u.society)?.state == DipRel.dipState.war)
                            {
                                ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Destroyed enemy army in battle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                            }
                        }
                    }

                    orcSocieties.Clear();
                    List<HolyOrder_Orcs> enemyCultures = new List<HolyOrder_Orcs>();
                    foreach (UM enemy in enemies)
                    {
                        if (enemy.society is SG_Orc enemySociety && !orcSocieties.Contains(enemySociety))
                        {
                            orcSocieties.Add(enemySociety);
                        }
                        else if (enemy.society is HolyOrder_Orcs enemyCulture && !enemyCultures.Contains(enemyCulture))
                        {
                            enemyCultures.Add(enemyCulture);
                        }
                    }

                    if (orcSocieties.Count > 0 || enemyCultures.Count > 0)
                    {
                        foreach (HolyOrder_Orcs enemyCulture in enemyCultures)
                        {
                            ModCore.core.TryAddInfluenceGain(enemyCulture, new ReasonMsg("Destroyed enemy army in battle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);

                            orcSocieties.Remove(enemyCulture.orcSociety);
                        }

                        foreach (SG_Orc enemySociety in orcSocieties)
                        {
                            ModCore.core.TryAddInfluenceGain(enemySociety, new ReasonMsg("Destroyed enemy army in battle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }
                    }
                }

                if (infGainHuman)
                {
                    if (orcCulture != null)
                    {
                        ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg("Destroyed orc army in batle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                    }

                    if (orcs != null)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Destroyed orc army in batle", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                    }
                }
            }
            else if (v == "Killed by Ophanim's Smite")
            {
                if (orcCulture != null)
                {
                    ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg("Smote orc army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                if (orcs != null)
                {
                    ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Smote orc army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                List<SG_Orc> orcSocieties = ModCore.core.data.getOrcSocieties(map);

                if (orcSocieties?.Count > 0 && u.society != null)
                {
                    foreach (SG_Orc orcSociety in orcSocieties)
                    {
                        if (orcs != null && orcs == orcSociety)
                        {
                            continue;
                        }

                        if (orcSociety.getRel(u.society).state == DipRel.dipState.war)
                        {
                            ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Smote enemy army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }
                    }
                }
            }
            else if (v == "Killed by a volcano")
            {
                if (uKiller != null)
                {
                    if (uKiller.isCommandable())
                    {
                        if (orcCulture != null)
                        {
                            ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg("Volcanic eruption (geomancy) destroyed orc army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }

                        if (orcs != null)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Volcanic eruption (geomancy) destroyed orc army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }

                        List<SG_Orc> orcSocieties = ModCore.core.data.getOrcSocieties(map);

                        if (orcSocieties?.Count > 0 && u.society != null)
                        {
                            foreach (SG_Orc orcSociety in orcSocieties)
                            {
                                if (orcs != null && orcSociety == orcs)
                                {
                                    continue;
                                }

                                if (orcSociety.getRel(u.society).state == DipRel.dipState.war)
                                {
                                    ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Volcanic eruption (geomancy) destroyed enemy army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                                }
                            }
                        }
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcCulture != null)
                        {
                            ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg("Volcanic eruption (geomancy) destroyed orc army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                        }

                        if (orcs != null)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Volcanic eruption (geomancy) destroyed orc army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]));
                        }
                    }
                }
                else
                {
                    if (orcCulture != null)
                    {
                        ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg("Awakening destroyed orc army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    if (orcs != null)
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Awakening destroyed orc army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                    }

                    List<SG_Orc> orcSocieties = ModCore.core.data.getOrcSocieties(map);

                    if (orcSocieties?.Count > 0 && u.society != null)
                    {
                        foreach (SG_Orc orcSociety in orcSocieties)
                        {
                            if (orcs != null && orcs == orcSociety)
                            {
                                continue;
                            }

                            if (orcSociety.getRel(u.society).state == DipRel.dipState.war)
                            {
                                ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Awakening destroyed enemy army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                            }
                        }
                    }
                }
            }
        }

        public override void onRazeLocation_StartOfProcess(UM um)
        {
            Settlement set = um.location?.settlement;
            SG_Orc orcSociety = um.location.soc as SG_Orc;

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

                if (orcSociety != null)
                {
                    if (um.isCommandable())
                    {
                        ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Razing Orc Camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                    }
                    else if (!um.society.isDark())
                    {
                        ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Razing Orc Camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                    }
                }
            }

            if (um.isCommandable() && um.location.soc != null)
            {
                List<SG_Orc> orcSocieties = ModCore.core.data.getOrcSocieties(um.map);

                if (orcSocieties.Count > 0 && um.society != null)
                {
                    foreach (SG_Orc orcs in orcSocieties)
                    {
                        if (orcs == orcSociety)
                        {
                            continue;
                        }

                        if (orcs.getRel(um.location.soc)?.state == DipRel.dipState.war)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razing Emeny Settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation] * 2), true);
                        }
                    }
                }
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

        public override int onArmyBattleCycle_DamageCalculated(BattleArmy batle, int dmg, UM unit, UM target)
        {
            if (unit is UM_OrcArmy orcArmy && orcArmy.society is SG_Orc orcSociety && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (orcCulture.tenet_god is H_Orcs_ShadowWarriors shadowWariors && shadowWariors.status < 0)
                {
                    if (orcArmy.homeLocation != -1)
                    {
                        Location home = map.locations[orcArmy.homeLocation];
                        if (home.settlement != null)
                        {
                            if (home.settlement.shadow >= 1.0)
                            {
                                dmg += 2;
                            }
                            else if (home.settlement.shadow >= 0.5)
                            {
                                dmg += 1;
                            }
                        }
                        else
                        {
                            if (home.hex.purity <= 0.0f)
                            {
                                dmg += 2;
                            }
                            else if (home.hex.purity <= 0.5f)
                            {
                                dmg += 1;

                            }
                        }
                    }
                }
            }

            if (target is UM_OrcArmy orcArmy2 && orcArmy2.society is SG_Orc orcSociety2 && ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety2, out HolyOrder_Orcs orcCulture2) && orcCulture2 != null)
            {
                if (orcCulture2.tenet_god is H_Orcs_ShadowWarriors shadowWariors && shadowWariors.status < -1)
                {
                    if (orcArmy2.homeLocation != -1)
                    {
                        Location home = map.locations[orcArmy2.homeLocation];
                        if (home.settlement != null)
                        {
                            if (home.settlement.shadow >= 1.0)
                            {
                                dmg = Math.Max(1, dmg - 2);
                            }
                            else if (home.settlement.shadow >= 0.5)
                            {
                                dmg = Math.Max(1, dmg - 1);
                            }
                        }
                        else
                        {
                            if (home.hex.purity <= 0.0f)
                            {
                                dmg = Math.Max(1, dmg - 2);
                            }
                            else if (home.hex.purity <= 0.5f)
                            {
                                dmg = Math.Max(1, dmg - 1);

                            }
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
            // Settlement Data
            Set_OrcCamp camp = set as Set_OrcCamp;
            SG_Orc orcSociety = camp?.location?.soc as SG_Orc;

            // Killer Data
            Person pKiller = killer as Person;
            Unit uKiller = killer as Unit;

            if (pKiller != null)
            {
                uKiller = pKiller.unit;
            }

            if (uKiller != null)
            {
                if (v == "Razed by " + uKiller.getName())
                {
                    if (uKiller.isCommandable())
                    {
                        if (orcSociety != null && !orcSociety.isGone())
                        {
                            ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Razed orc camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                        }

                        List<SG_Orc> orcSocieties = ModCore.core.data.getOrcSocieties(map);

                        if (orcSocieties.Count > 0 && set.location?.soc != null)
                        {
                            foreach (SG_Orc orcs in orcSocieties)
                            {
                                if (orcSociety != null && orcSociety == orcs)
                                {
                                    continue;
                                }

                                if (orcs.getRel(set.location.soc)?.state == DipRel.dipState.war)
                                {
                                    ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Razed emeny settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                                }
                            }
                        }
                    }
                    else if (orcSociety != null && !orcSociety.isGone() && !uKiller.society.isDark())
                    {
                        ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Razed orc camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]));
                    }
                }
                else if (v == "Destroyed by volcanic eruption")
                {
                    if (uKiller.isCommandable())
                    {
                        if (orcSociety != null && !orcSociety.isGone())
                        {
                            ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Volcanic eruption (geomancy) razed orc camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                        }

                        List<SG_Orc> orcSocieties = ModCore.core.data.getOrcSocieties(map);

                        if (orcSocieties.Count > 0 && set.location?.soc != null)
                        {
                            foreach (SG_Orc orcs in orcSocieties)
                            {
                                if (orcSociety != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcs.getRel(set.location.soc)?.state == DipRel.dipState.war)
                                {
                                    ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Volcanic eruption (geomancy) razed enemy settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                                }
                            }
                        }
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcSociety != null && !orcSociety.isGone())
                        {
                            ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Volcanic eruption (geomancy) razed orc camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]));
                        }
                    }
                }
            }
            else if (v == "Destroyed by smite")
            {
                if (orcSociety != null && !orcSociety.isGone())
                {
                    ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Smote orc camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                List<SG_Orc> orcSocieties = ModCore.core.data.getOrcSocieties(map);

                if (orcSocieties.Count > 0 && set.location?.soc != null)
                {
                    foreach (SG_Orc orcs in orcSocieties)
                    {
                        if (orcSociety != null && orcSociety == orcs)
                        {
                            continue;
                        }

                        if (orcs.getRel(set.location.soc)?.state == DipRel.dipState.war)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Smote emeny settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                        }
                    }
                }
            }
            else if (v == "Destroyed by volcanic eruption")
            {
                if (orcSociety != null && !orcSociety.isGone())
                {
                    ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Awakening razed orc camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                }

                List<SG_Orc> orcSocieties = ModCore.core.data.getOrcSocieties(map);

                if (orcSocieties.Count > 0 && set.location?.soc != null)
                {
                    foreach (SG_Orc orcs in orcSocieties)
                    {
                        if (orcSociety != null && orcSociety == orcs)
                        {
                            continue;
                        }

                        if (orcs.getRel(set.location.soc)?.state == DipRel.dipState.war)
                        {
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Awakening razed enemy settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazeLocation]), true);
                        }
                    }
                }
            }

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

        public override bool interceptAgentAI(UA ua, List<CommunityLib.AgentAI.ChallengeData> validChallengeData, CommunityLib.AgentAI.ControlParameters controlParams)
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
