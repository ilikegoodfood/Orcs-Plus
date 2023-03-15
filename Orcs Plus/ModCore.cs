using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code;
using Assets.Code.Modding;
using CommunityLib;
using HarmonyLib;
using UnityEngine;

namespace Orcs_Plus
{
    public class ModCore : ModKernel
    {
        public static CommunityLib.ModCore comLib;

        public static ComLibHooks comLibHooks;

        public static ModCore core;

        public static ModData data;

        public static AgentAIs agentAI;

        public static bool modLivingWilds = false;

        private static bool patched = false;

        public override void onModsInitiallyLoaded()
        {
            core = this;

            if (!patched)
            {
                patched = true;
                HarmonyPatches.PatchingInit(this);
            }
        }

        public override void beforeMapGen(Map map)
        {
            foreach (ModKernel core in map.mods)
            {
                switch (core.GetType().Namespace)
                {
                    case "CommunityLib":
                        ModCore.comLib = core as CommunityLib.ModCore;
                        break;
                    case "LivingWilds":
                        modLivingWilds = true;
                        break;
                    default:
                        break;
                }
            }

            if (comLib == null)
            {
                throw new Exception("OrcsPlus: This mod REQUIRES the Community Library mod to be installed and enabled in order to operate.");
            }

            data = new ModData();
            agentAI = new AgentAIs(map);
        }

        public override void afterMapGenBeforeHistorical(Map map)
        {
            comLibHooks = new ComLibHooks(this, map);
            comLib?.RegisterHooks(comLibHooks);
        }

        public override void afterLoading(Map map)
        {
            foreach (ModKernel core in map.mods)
            {
                switch (core.GetType().Namespace)
                {
                    case "CommunityLib":
                        ModCore.comLib = core as CommunityLib.ModCore;
                        comLib?.RegisterHooks(comLibHooks);
                        break;
                    default:
                        break;
                }
            }

            if (comLib == null)
            {
                Console.WriteLine("OrcsPlus :: ERROR: Failed to find Community Library");
                return;
            }

            if (data == null)
            {
                data = new ModData();
            }

            UpdateOrcSGCultureMap(map);
        }

        public override void onTurnStart(Map map)
        {
            if (comLib == null)
            {
                return;
            }

            data.isPlayerTurn = true;

            //Console.WriteLine("OrcsPlus: Start Populate Challenges Singleton");
            UpdateOrcSGCultureMap(map);
            
            foreach (KeyValuePair<SG_Orc, HolyOrder_OrcsPlus_Orcs> pair in data.orcSGCultureMap)
            {
                if (pair.Value == null)
                {
                    CreateOrcHolyOrder(map, pair.Key);
                }
            }
        }

        private void UpdateOrcSGCultureMap(Map map)
        {
            data.getOrcSocietiesAndCultures(map, out List<SG_Orc> orcSocieties, out List<HolyOrder_OrcsPlus_Orcs> orcCultures);

            if (orcSocieties?.Count > 0)
            {
                foreach (SG_Orc orcSociety in orcSocieties)
                {
                    if (orcSociety.isGone())
                    {
                        if (data.orcSGCultureMap.ContainsKey(orcSociety))
                        {
                            data.orcSGCultureMap[orcSociety]?.checkIsGone();
                            data.orcSGCultureMap.Remove(orcSociety);
                        }
                        continue;
                    }

                    if (!data.orcSGCultureMap.ContainsKey(orcSociety))
                    {
                        data.orcSGCultureMap.Add(orcSociety, null);
                    }
                }
            }

            if (orcCultures?.Count > 0)
            {
                foreach (HolyOrder_OrcsPlus_Orcs orcCulture in orcCultures)
                {
                    if (orcCulture.isGone())
                    {
                        continue;
                    }

                    if (orcCulture.orcSociety?.isGone() ?? true || !data.orcSGCultureMap.ContainsKey(orcCulture.orcSociety))
                    {
                        orcCulture?.checkIsGone();
                        continue;
                    }

                    if (data.orcSGCultureMap[orcCulture.orcSociety] == null)
                    {
                        data.orcSGCultureMap[orcCulture.orcSociety] = orcCulture;
                    }
                }
            }
        }

        public void CreateOrcHolyOrder(Map map, SG_Orc orcSociety)
        {
            data.getOrcCamps(map, orcSociety, out List<Set_OrcCamp> orcCamps, out List<Set_OrcCamp> specializedOrcCamps);

            if (orcCamps != null)
            {
                //Console.WriteLine("OrcsPlus: Choosing seat of orc culture.");
                Location location;
                if (specializedOrcCamps.Count == 0)
                {
                    if (orcCamps.Count == 0)
                    {
                        orcSociety.checkIsGone();
                        return;
                    }

                    if (orcCamps.Count == 1)
                    {
                        location = orcCamps[0].location;
                    }
                    else
                    {
                        location = orcCamps[Eleven.random.Next(orcCamps.Count())].location;
                    }
                }
                else if (specializedOrcCamps.Count == 1)
                {
                    location = specializedOrcCamps[0].location;
                }
                else
                {
                    location = specializedOrcCamps[Eleven.random.Next(specializedOrcCamps.Count())].location;
                }

                if (location != null)
                {
                    new HolyOrder_OrcsPlus_Orcs(map, location, orcSociety);
                }
            }
        }

        public override void onTurnEnd(Map map)
        {
            if (comLib == null)
            {
                return;
            }

            data.isPlayerTurn = false;

            data.influenceGainElder.Clear();
            data.influenceGainHuman.Clear();
        }

        public override int adjustHolyInfluenceDark(HolyOrder order, int inf, List<ReasonMsg> msgs)
        {
            HolyOrder_OrcsPlus_Orcs orcCulture = order as HolyOrder_OrcsPlus_Orcs;

            List<ReasonMsg> influenceGain;
            if (orcCulture == null || !data.influenceGainElder.TryGetValue(orcCulture, out influenceGain))
            {
                return inf;
            }

            if (orcCulture.isGone())
            {
                data.influenceGainElder.Remove(orcCulture);
                return inf;
            }

            foreach (ReasonMsg msg in influenceGain)
            { 
                msgs?.Add(msg);
                inf += (int)msg.value;
            }

            return inf;
        }

        public override int adjustHolyInfluenceGood(HolyOrder order, int inf, List<ReasonMsg> msgs)
        {
            if (!(order is HolyOrder_OrcsPlus_Orcs orcCulture) || !data.influenceGainHuman.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain) || influenceGain?.Count == 0)
            {
                return inf;
            }

            if (orcCulture.isGone())
            {
                data.influenceGainHuman.Remove(orcCulture);
                return inf;
            }

            foreach (ReasonMsg msg in influenceGain)
            {
                msgs?.Add(msg);
                inf += (int)msg.value;
            }

            return inf;
        }

        public bool TryAddInfluenceGain(HolyOrder_OrcsPlus_Orcs orcCulture, ReasonMsg msg, bool isElder = false)
        {
            if (orcCulture?.isGone() ?? true || msg?.value == 0)
            {
                return false;
            }

            if (isElder)
            {
                AddInfluenceGainElder(orcCulture, msg);

                if (data.isPlayerTurn)
                {
                    orcCulture.influenceElder += (int)Math.Floor(msg.value);
                }

                return true;
            }

            AddInfluenceGainHuman(orcCulture, msg);

            if (data.isPlayerTurn)
            {
                orcCulture.influenceHuman += (int)Math.Floor(msg.value);
            }

            return true;
        }

        public bool TryAddInfluenceGain(SG_Orc orcSociety, ReasonMsg msg, bool isElder = false)
        {
            if (orcSociety?.isGone() ?? true || msg?.value == 0)
            {
                return false;
            }

            if (data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_OrcsPlus_Orcs orcCulture) && orcCulture != null)
            {
                orcCulture = data.orcSGCultureMap[orcSociety];
            }

            if (isElder)
            {
                AddInfluenceGainElder(orcCulture, msg);

                if (data.isPlayerTurn)
                {
                    orcCulture.influenceElder += (int)Math.Floor(msg.value);
                }

                return true;
            }

            AddInfluenceGainHuman(orcCulture, msg);

            if (data.isPlayerTurn)
            {
                orcCulture.influenceHuman += (int)Math.Floor(msg.value);
            }

            return true;
        }

        private void AddInfluenceGainElder(HolyOrder_OrcsPlus_Orcs orcCulture, ReasonMsg msg)
        {
            if (!data.influenceGainElder.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain))
            {
                influenceGain = new List<ReasonMsg> ();
                data.influenceGainElder.Add(orcCulture, influenceGain);
            }

            bool flag = false;

            foreach (ReasonMsg gainMsg in influenceGain)
            {
                if (gainMsg.msg == msg.msg)
                {
                    gainMsg.value += msg.value;
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                influenceGain.Add(msg);
            }
        }

        private void AddInfluenceGainHuman(HolyOrder_OrcsPlus_Orcs orcCulture, ReasonMsg msg)
        {
            if (!data.influenceGainHuman.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain))
            {
                influenceGain = new List<ReasonMsg> ();
                data.influenceGainHuman.Add(orcCulture, influenceGain);
            }

            bool flag = false;

            foreach (ReasonMsg gainMsg in influenceGain)
            {
                if (gainMsg.msg == msg.msg)
                {
                    gainMsg.value += msg.value;
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                influenceGain.Add(msg);
            }
        }

        public override void onPersonDeath_StartOfProcess(Person person, string v, object killer)
        {
            if (comLib == null)
            {
                return;
            }

            // Test message
            /*string deathMsg =  "Person " + person.getFullName() + " was killed at " + person.getLocation().getName() + ".";

            if (v.Length == 0)
            {
                deathMsg += " No reason was given for their death.";
            }
            else
            {
                deathMsg += " The cause of their death was " + v + ".";
            }
            if (killer == null)
            {
                deathMsg += " No object was held responsible for their death.";
            }
            else
            {
                deathMsg += " They were killed by an object of type " + killer.GetType().Name + ".";

                if (killer is Person)
                {
                    deathMsg += " The killer's name is " + (killer as Person).getFullName();
                }
                else if (killer is Unit)
                {
                    deathMsg += " The killer's unit name is " + (killer as Unit).getName();
                }
            }

            person.map.addUnifiedMessage(person, person.getLocation(), "Person Killed", deathMsg, "Person Killed");*/

            // Person Data
            Unit uPerson = person.unit;

            if (uPerson == null)
            {
                return;
            }

            UA uaPerson = uPerson as UA;
            SG_Orc orcs = uPerson.society as SG_Orc;
            HolyOrder_OrcsPlus_Orcs orcCulture = uPerson.society as HolyOrder_OrcsPlus_Orcs;

            // Person Activity Data
            Task_PerformChallenge performChallenge = uaPerson?.task as Task_PerformChallenge;
            Challenge challenge = performChallenge?.challenge;

            Ch_SkirmishAttacking skirmishAtt = null;
            Ch_SkirmishDefending skirmishDef = null;

            if (challenge != null)
            {
                skirmishAtt = challenge as Ch_SkirmishAttacking;
                skirmishDef = challenge as Ch_SkirmishDefending;
            }

            // Killer Data
            Person pKiller = killer as Person;
            Unit uKiller = killer as Unit;

            if (pKiller != null)
            {
                uKiller = pKiller.unit;
            }

            if (uKiller != null)
            {
                if (v == "Killed in battle with " + uKiller.getName())
                {
                    if (uKiller.isCommandable())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent in battle", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcs != null)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Killed orc agent in battle", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        List<SG_Orc> orcSocieties = data.getOrcSocieties(person.map);

                        if (orcSocieties?.Count > 0 && uPerson.society != null)
                        {
                            foreach (SG_Orc orcSociety in orcSocieties)
                            {
                                if (orcs != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcSociety.getRel(uPerson.society)?.state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcSociety, new ReasonMsg("Killed enemy agent in battle", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent in battle", data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcs != null)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Killed orc agent in battle", data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
                else if (v == "Killed by " + uKiller.getName())
                {
                    if (uKiller.isCommandable())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Intercepted and killed orc agent", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcs != null)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Intercepted and killed orc agent", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        List<SG_Orc> orcSocieties = data.getOrcSocieties(person.map);

                        if (orcSocieties?.Count > 0 && uPerson.society != null)
                        {
                            foreach (SG_Orc orcSociety in orcSocieties)
                            {
                                if (orcs != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcSociety.getRel(uPerson.society)?.state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcSociety, new ReasonMsg("Intercepted and killed enemy agent", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Intercepted and killed orc agent", data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcs != null)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Intercepted and killed orc agent", data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
                else if (v == "Killed by a volcano")
                {
                    if (uKiller.isCommandable())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcs != null)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        List<SG_Orc> orcSocieties = data.getOrcSocieties(person.map);

                        if (orcSocieties?.Count > 0 && uPerson.society != null)
                        {
                            foreach (SG_Orc orcSociety in orcSocieties)
                            {
                                if (orcs != null && orcSociety == orcs)
                                {
                                    continue;
                                }

                                if (orcSociety.getRel(uPerson.society)?.state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcSociety, new ReasonMsg("Volcanic eruption (geomancy) killed enemy agent", ModCore.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcs != null)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
            }
            else if (v == "Killed by Ophanim's Smite")
            {
                if (orcCulture != null)
                {
                    TryAddInfluenceGain(orcCulture, new ReasonMsg("Smote orc agent", ModCore.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                if (orcs != null)
                {
                    TryAddInfluenceGain(orcs, new ReasonMsg("Smote orc agent", ModCore.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                List<SG_Orc> orcSocieties = data.getOrcSocieties(person.map);

                if (orcSocieties?.Count > 0 && uPerson.society != null)
                {
                    foreach (SG_Orc orcSociety in orcSocieties)
                    {
                        if (orcs != null && orcs == orcSociety)
                        {
                            continue;
                        }

                        if (orcSociety.getRel(uPerson.society)?.state == DipRel.dipState.war)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Smote enemy agent", ModCore.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                        }
                    }
                }
            }
            else if ((skirmishAtt != null && v == "Killed by dangers during challenge: " + skirmishAtt.getName()) || (skirmishDef != null && v == "Killed by dangers during challenge: " + skirmishDef.getName()))
            {
                bool infGainElder = false;
                bool infGainHuman = false;

                if (skirmishAtt != null || skirmishDef != null)
                {
                    List<UM> enemies = new List<UM>();
                    List<UA> enemyComs = new List<UA>();

                    if (skirmishAtt != null)
                    {
                        data.getBattleArmyEnemies(skirmishAtt.battle, uaPerson, out enemies, out enemyComs);
                    }

                    if (skirmishDef != null)
                    {
                        data.getBattleArmyEnemies(skirmishDef.battle, uaPerson, out enemies, out enemyComs);
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
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent skirmishing a battle", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcs != null)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Killed orc agent skirmishing a battle", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        List<SG_Orc> orcSocieties = data.getOrcSocieties(person.map);

                        if (orcSocieties?.Count > 0)
                        {
                            foreach (SG_Orc orcSociety in orcSocieties)
                            {
                                if (orcs != null && orcSociety == orcs)
                                {
                                    continue;
                                }

                                if (orcSociety.getRel(uKiller.society)?.state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcSociety, new ReasonMsg("Killed enemy agent skirmishing a battle", ModCore.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                    }

                    if (infGainHuman)
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent skirmishing a battle", data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcs != null)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Killed orc agent skirmishing a battle", data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
            }
            else if (v == "Killed by a volcano")
            {

                if (orcCulture != null)
                {
                    TryAddInfluenceGain(orcCulture, new ReasonMsg("Awakening killed orc agent", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                if (orcs != null)
                {
                    TryAddInfluenceGain(orcs, new ReasonMsg("Awakening killed orc agent", data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                List<SG_Orc> orcSocieties = data.getOrcSocieties(person.map);

                if (orcSocieties?.Count > 0 && uPerson.society != null)
                {
                    foreach (SG_Orc orcSociety in orcSocieties)
                    {
                        if (orcs != null && orcSociety == orcs)
                        {
                            continue;
                        }

                        if (orcSociety.getRel(uPerson.society)?.state == DipRel.dipState.war)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Awakening killed enemy agent", ModCore.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }
                    }
                }
            }
        }
    }
}
