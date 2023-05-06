using Assets.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public class ComLibHooks : CommunityLib.Hooks
    {
        public ComLibHooks(Map map)
            : base(map)
        {

        }

        public override void onUnitDeath_StartOfProcess(Unit u, string v, Person killer)
        {
            if (u?.society == null || u is UA)
            {
                return;
            }

            SG_Orc orcs = u.society as SG_Orc;
            HolyOrder_Orcs orcCulture = u.society as HolyOrder_Orcs;

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

                        if (orcSociety.getRel(u.society)?.state == DipRel.dipState.war)
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

                                if (orcSociety.getRel(u.society)?.state == DipRel.dipState.war)
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

                            if (orcSociety.getRel(u.society)?.state == DipRel.dipState.war)
                            {
                                ModCore.core.TryAddInfluenceGain(orcSociety, new ReasonMsg("Awakening destroyed enemy army", ModCore.core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                            }
                        }
                    }
                }
            }
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
                case UAA_OrcElder elder:
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
                case UAA_OrcElder elder:
                    return interceptOrcElder(elder);
                case UAEN_OrcShaman shaman:
                    return interceptOrcShaman(shaman);
                default:
                    break;
            }

            return false;
        }

        private bool interceptOrcElder(UAA_OrcElder elder)
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

                    if (actionsToRemove.Count > 0)
                    {
                        //Console.WriteLine("OrcsPlus: Removing unwanted actions");
                        foreach (MonsterAction action in actionsToRemove)
                        {
                            actions.Remove(action);
                        }
                    }
                    //Console.WriteLine("OrcsPlus: Finished Processing Actions");
                }
                //Console.WriteLine("OrcsPlus: Finished processing orcSociety");
            }
        }
    }
}
