using Assets.Code;
using Assets.Code.Modding;
using CommunityLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orcs_Plus
{
    public class ModCore : ModKernel
    {
        public static CommunityLib.ModCore comLib;

        public AgentAI comLibAI;

        private ComLibHooks comLibHooks;

        public static ModCore core;

        public ModData data;

        private static bool patched = false;

        public override void onModsInitiallyLoaded()
        {
            core = this;

            if (!patched)
            {
                patched = true;
                HarmonyPatches.PatchingInit();
            }
        }

        public override void beforeMapGen(Map map)
        {
            data = new ModData();
            comLibHooks = new ComLibHooks(map);

            getModKernels(map);
            HarmonyPatches_Conditional.PatchingInit();

            if (comLib == null)
            {
                throw new Exception("OrcsPlus: This mod REQUIRES the Community Library mod to be installed and enabled in order to operate.");
            }

            new AgentAIs(map);
        }

        public override void afterMapGenAfterHistorical(Map map)
        {
            if (core.data.tryGetModAssembly("Ixthus", out Assembly asmIx) && asmIx != null)
            {
                Type t = asmIx.GetType("ShadowsLib.H_expeditionPatrons", false);
                if (t != null)
                {
                    foreach (HolyOrder_Orcs orcCulture in core.data.getOrcCultures(map, true))
                    {
                        for (int i = 0; i < orcCulture.tenets.Count; i++)
                        {
                            if (orcCulture.tenets[i] != null && orcCulture.tenets[i].GetType() == t)
                            {
                                orcCulture.tenets.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override void afterLoading(Map map)
        {
            core = this;
            if (data == null)
            {
                data = new ModData();
                data.isPlayerTurn = true;
                UpdateOrcSGCultureMap(map);
            }

            if (comLib == null)
            {
                comLibHooks = new ComLibHooks(map);
            }

            getModKernels(map);
            HarmonyPatches_Conditional.PatchingInit();

            if (comLib == null)
            {
                throw new Exception("OrcsPlus: This mod REQUIRES the Community Library mod to be installed and enabled in order to operate.");
            }

            new AgentAIs(map);
        }

        private void getModKernels(Map map)
        {
            foreach (ModKernel kernel in map.mods)
            {
                switch (kernel.GetType().Namespace)
                {
                    case "CommunityLib":
                        comLib = kernel as CommunityLib.ModCore;
                        if (comLib != null)
                        {
                            comLib.RegisterHooks(comLibHooks);
                            comLibAI = comLib.GetAgentAI();
                        }
                        break;
                    case "CovenExpansion":
                        core.data.addModAssembly("CovensCursesCurios", kernel.GetType().Assembly);
                        break;
                    case "LivingWilds":
                        core.data.addModAssembly("LivingWilds", kernel.GetType().Assembly);

                        if (core.data.tryGetModAssembly("LivingWilds", out Assembly asmLW) && asmLW != null)
                        {
                            Type natureSanctuary = asmLW.GetType("Set_Nature_NatureSanctuary", false);
                            if (natureSanctuary != null)
                            {
                                comLib.registerSettlementTypeForOrcExpansion(natureSanctuary);
                            }

                            Type wilderness = asmLW.GetType("LivingWilds.Set_Nature_UnoccupiedWilderness", false);
                            if (wilderness != null)
                            {
                                comLib.registerSettlementTypeForOrcExpansion(wilderness);
                            }
                        }
                        break;
                    case "ShadowsLib":
                        core.data.addModAssembly("Ixthus", kernel.GetType().Assembly);
                        break;
                    default:
                        break;
                }
            }
        }

        public override void onTurnStart(Map map)
        {
            if (comLib == null)
            {
                return;
            }

            core.data.isPlayerTurn = true;

            UpdateOrcSGCultureMap(map);

            if (core.data.waystationsToRemove.Count > 0)
            {
                foreach (Sub_OrcWaystation waystation in core.data.waystationsToRemove)
                {
                    waystation.settlement.location.settlement.subs.Remove(waystation);
                }

                core.data.waystationsToRemove.Clear();
            }
        }

        private void UpdateOrcSGCultureMap(Map map)
        {
            //Console.WriteLine("OrcsPlus: updating orcSGCultureMap");
            core.data.orcSGCultureMap.Clear();
            List<HolyOrder_Orcs> orcCultures = core.data.getOrcCultures(map);

            if (orcCultures?.Count > 0)
            {
                foreach (HolyOrder_Orcs orcCulture in orcCultures)
                {
                    if (orcCulture.orcSociety == null || orcCulture.checkIsGone())
                    {
                        continue;
                    }

                    core.data.orcSGCultureMap.Add(orcCulture.orcSociety, orcCulture);
                }
            }

            //Console.WriteLine("OrcsPlus: orcSGCultureMap updated");
        }

        public override void onTurnEnd(Map map)
        {
            if (comLib == null)
            {
                return;
            }

            core.data.isPlayerTurn = false;

            core.data.influenceGainElder.Clear();
            core.data.influenceGainHuman.Clear();
        }

        public override double sovereignAI(Map map, AN actionNational, Person ruler, List<ReasonMsg> reasons, double initialUtility)
        {
            if (actionNational is AN_WarOnThreat threatWar && threatWar.target is SG_Orc)
            {
                if (reasons != null)
                {
                    ReasonMsg distanceReason = reasons.FirstOrDefault(r => r.msg == "Distance between");
                    if (distanceReason != null)
                    {
                        initialUtility += distanceReason.value;
                        distanceReason.value *= 2;
                    }
                }
                else
                {
                    double val = map.getStepDist(ruler.society, threatWar.target) - 1;
                    if (val > 0)
                    {
                        val *= Math.Min(map.param.utility_soc_warDistancePenaltyPerStep, map.param.utility_soc_warDistancePenaltyCap);
                        initialUtility += val;
                    }
                }
            }

            if (actionNational is AN_DeclareWar war && war.target is SG_Orc)
            {
                if (reasons != null)
                {
                    ReasonMsg distanceReason = reasons.FirstOrDefault(r => r.msg == "Distance between");
                    if (distanceReason != null)
                    {
                        initialUtility += distanceReason.value;
                        distanceReason.value *= 2;
                    }
                }
                else
                {
                    double val = map.getStepDist(ruler.society, war.target) - 1;
                    if (val > 0)
                    {
                        val *= Math.Min(map.param.utility_soc_warDistancePenaltyPerStep, map.param.utility_soc_warDistancePenaltyCap);
                        initialUtility += val;
                    }
                }
            }

            return initialUtility;
        }

        public override void onChallengeComplete(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            switch (challenge)
            {
                case Mg_EnslaveTheDead _:
                    if (ua is UAEN_OrcShaman shaman && !shaman.isCommandable())
                    {
                        foreach (Unit unit in shaman.location.units)
                        {
                            if (unit is UM_UntamedDead dead && dead.master == shaman)
                            {
                                dead.master = null;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public override int adjustHolyInfluenceDark(HolyOrder order, int inf, List<ReasonMsg> msgs)
        {
            HolyOrder_Orcs orcCulture = order as HolyOrder_Orcs;

            List<ReasonMsg> influenceGain;
            if (orcCulture == null || !core.data.influenceGainElder.TryGetValue(orcCulture, out influenceGain))
            {
                return inf;
            }

            if (orcCulture.isGone())
            {
                core.data.influenceGainElder.Remove(orcCulture);
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
            if (!(order is HolyOrder_Orcs orcCulture) || !core.data.influenceGainHuman.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain) || influenceGain?.Count == 0)
            {
                return inf;
            }

            if (orcCulture.isGone())
            {
                core.data.influenceGainHuman.Remove(orcCulture);
                return inf;
            }

            foreach (ReasonMsg msg in influenceGain)
            {
                msgs?.Add(msg);
                inf += (int)msg.value;
            }

            return inf;
        }

        public bool TryAddInfluenceGain(HolyOrder_Orcs orcCulture, ReasonMsg msg, bool isElder = false)
        {
            if (orcCulture?.isGone() ?? true || msg?.value == 0)
            {
                return false;
            }

            if (isElder)
            {
                AddInfluenceGainElder(orcCulture, msg);

                if (core.data.isPlayerTurn)
                {
                    orcCulture.influenceElder += (int)Math.Floor(msg.value);

                    if (orcCulture.influenceElder > orcCulture.influenceElderReq)
                    {
                        orcCulture.influenceElder = orcCulture.influenceElderReq;
                    }
                }

                return true;
            }

            AddInfluenceGainHuman(orcCulture, msg);

            if (core.data.isPlayerTurn)
            {
                orcCulture.influenceHuman += (int)Math.Floor(msg.value);

                if (orcCulture.influenceHuman > orcCulture.influenceHumanReq)
                {
                    orcCulture.influenceHuman = orcCulture.influenceHumanReq;
                }
            }

            return true;
        }

        public bool TryAddInfluenceGain(SG_Orc orcSociety, ReasonMsg msg, bool isElder = false)
        {
            if (orcSociety?.isGone() ?? true || msg?.value == 0)
            {
                return false;
            }

            if (core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (isElder)
                {
                    AddInfluenceGainElder(orcCulture, msg);

                    if (core.data.isPlayerTurn)
                    {
                        orcCulture.influenceElder += (int)Math.Floor(msg.value);
                    }

                    return true;
                }

                AddInfluenceGainHuman(orcCulture, msg);

                if (core.data.isPlayerTurn)
                {
                    orcCulture.influenceHuman += (int)Math.Floor(msg.value);
                }

                return true;
            }

            return false;
        }

        private void AddInfluenceGainElder(HolyOrder_Orcs orcCulture, ReasonMsg msg)
        {
            if (!core.data.influenceGainElder.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain))
            {
                influenceGain = new List<ReasonMsg> ();
                core.data.influenceGainElder.Add(orcCulture, influenceGain);
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

        private void AddInfluenceGainHuman(HolyOrder_Orcs orcCulture, ReasonMsg msg)
        {
            if (!core.data.influenceGainHuman.TryGetValue(orcCulture, out List<ReasonMsg> influenceGain))
            {
                influenceGain = new List<ReasonMsg> ();
                core.data.influenceGainHuman.Add(orcCulture, influenceGain);
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

        public override void onPeaceDeclared(SocialGroup att, SocialGroup def)
        {
            SG_Orc attOrcSociety = att as SG_Orc;
            SG_Orc defOrcSociety = def as SG_Orc;

            if (attOrcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(attOrcSociety, out HolyOrder_Orcs attOrcCulture) && attOrcCulture != null)
            {
                att.map.declarePeace(attOrcCulture, def);
            }

            if (defOrcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(defOrcSociety, out HolyOrder_Orcs defOrcCulture) && defOrcCulture != null)
            {
                att.map.declarePeace(att, defOrcCulture);
            }
        }

        public override void onWarDeclared(SocialGroup attacker, SocialGroup target, List<ReasonMsg> reasons, War war)
        {
            SG_Orc attOrcSociety = attacker as SG_Orc;
            SG_Orc defOrcSociety = target as SG_Orc;

            HolyOrder_Orcs attOrcCulture = null;
            HolyOrder_Orcs defOrcCulture = null;

            if (attOrcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(attOrcSociety, out attOrcCulture) && attOrcCulture != null)
            {
                attacker.map.declareWar(attOrcCulture, target, true, reasons);
            }

            if (defOrcSociety != null && ModCore.core.data.orcSGCultureMap.TryGetValue(defOrcSociety, out defOrcCulture) && defOrcCulture != null)
            {
                attacker.map.declareWar(attacker, defOrcCulture, true, reasons);
            }
        }

        public override void onPersonDeath_StartOfProcess(Person person, string v, object killer)
        {
            if (comLib == null)
            {
                return;
            }

            // Person Data
            Unit uPerson = person.unit;

            if (uPerson == null)
            {
                return;
            }

            UA uaPerson = uPerson as UA;
            SG_Orc orcSociety = uPerson.society as SG_Orc;
            HolyOrder_Orcs orcCulture = uPerson.society as HolyOrder_Orcs;

            if ((orcSociety != null && orcSociety.isGone()) || (orcCulture != null && orcCulture.isGone()))
            {
                return;
            }

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
                if (uaPerson is UAA_OrcElder && uKiller is UA uaKiller && uaKiller.person.traits.OfType<T_BloodFeud>().FirstOrDefault(t => t.orcSociety == orcCulture.orcSociety) == null)
                {
                    uaKiller.person.receiveTrait(new T_BloodFeud(orcCulture.orcSociety));

                    person.map.addUnifiedMessage(uaPerson, uaKiller, "Blood Feud", uaKiller.getName() + " has become the target of a blood fued by killing an orc elder of the " + uaPerson.society.getName() + ". They must now spend the rest of their days looking over their shoulder for the sudden the appearance of an avenging orc upstart.", "Orc Blood Feud");
                }
                //Console.WriteLine("OrcsPlus: killer is " + uKiller.getName());
                if (v == "Killed in battle with " + uKiller.getName())
                {
                    //Console.WriteLine("OrcsPlus: Died in battle");
                    if (uKiller.isCommandable())
                    {
                        //Console.WriteLine("OrcsPlus: killer is commandable");
                        if (orcCulture != null)
                        {
                            //Console.WriteLine("OrcsPlus: victim is of orc culture");
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Killed orc agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }
                        //Console.WriteLine("OrcsPlus: Direct influence added");
                        List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                        if (orcSocieties.Count > 0 && uPerson.society != null)
                        {
                            //Console.WriteLine("OrcsPlus: Got active orc societies");
                            foreach (SG_Orc orcs in orcSocieties)
                            {
                                if (orcSociety != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcs.getRel(uPerson.society).state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcs, new ReasonMsg("Killed enemy agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                        //Console.WriteLine("OrcsPlus: End");
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Killed orc agent in battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
                else if (v == "Killed by " + uKiller.getName())
                {
                    if (uKiller.isCommandable())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Intercepted and killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Intercepted and killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                        if (orcSocieties?.Count > 0 && uPerson.society != null)
                        {
                            foreach (SG_Orc orcs in orcSocieties)
                            {
                                if (orcSociety != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcs.getRel(uPerson.society).state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcs, new ReasonMsg("Intercepted and killed enemy agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Intercepted and killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Intercepted and killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
                else if (v == "Killed by a volcano")
                {
                    if (uKiller.isCommandable())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                        if (orcSocieties?.Count > 0 && uPerson.society != null)
                        {
                            foreach (SG_Orc orcs in orcSocieties)
                            {
                                if (orcSociety != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcs.getRel(uPerson.society).state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcs, new ReasonMsg("Volcanic eruption (geomancy) killed enemy agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                    }
                    else if (!uKiller.society.isDark())
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Volcanic eruption (geomancy) killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
            }
            else if (v == "Killed by Ophanim's Smite")
            {
                if (orcCulture != null)
                {
                    TryAddInfluenceGain(orcCulture, new ReasonMsg("Smote orc agent", core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                if (orcSociety != null)
                {
                    TryAddInfluenceGain(orcSociety, new ReasonMsg("Smote orc agent", core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
                }

                List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                if (orcSocieties?.Count > 0 && uPerson.society != null)
                {
                    foreach (SG_Orc orcs in orcSocieties)
                    {
                        if (orcSociety != null && orcs == orcSociety)
                        {
                            continue;
                        }

                        if (orcs.getRel(uPerson.society).state == DipRel.dipState.war)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Smote enemy agent", core.data.influenceGain[ModData.influenceGainAction.ArmyKill]), true);
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
                        core.data.getBattleArmyEnemies(skirmishAtt.battle, uaPerson, out enemies, out enemyComs);
                    }

                    if (skirmishDef != null)
                    {
                        core.data.getBattleArmyEnemies(skirmishDef.battle, uaPerson, out enemies, out enemyComs);
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
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Killed orc agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }

                        List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                        if (orcSocieties?.Count > 0)
                        {
                            foreach (SG_Orc orcs in orcSocieties)
                            {
                                if (orcSociety != null && orcs == orcSociety)
                                {
                                    continue;
                                }

                                if (orcs.getRel(uKiller.society).state == DipRel.dipState.war)
                                {
                                    TryAddInfluenceGain(orcs, new ReasonMsg("Killed enemy agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                                }
                            }
                        }
                    }

                    if (infGainHuman)
                    {
                        if (orcCulture != null)
                        {
                            TryAddInfluenceGain(orcCulture, new ReasonMsg("Killed orc agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }

                        if (orcSociety != null)
                        {
                            TryAddInfluenceGain(orcSociety, new ReasonMsg("Killed orc agent skirmishing a battle", core.data.influenceGain[ModData.influenceGainAction.AgentKill]));
                        }
                    }
                }
            }
            else if (v == "Killed by a volcano")
            {

                if (orcCulture != null)
                {
                    TryAddInfluenceGain(orcCulture, new ReasonMsg("Awakening killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                if (orcSociety != null)
                {
                    TryAddInfluenceGain(orcSociety, new ReasonMsg("Awakening killed orc agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                }

                List<SG_Orc> orcSocieties = core.data.getOrcSocieties(person.map);

                if (orcSocieties?.Count > 0 && uPerson.society != null)
                {
                    foreach (SG_Orc orcs in orcSocieties)
                    {
                        if (orcSociety != null && orcs == orcSociety)
                        {
                            continue;
                        }

                        if (orcs.getRel(uPerson.society).state == DipRel.dipState.war)
                        {
                            TryAddInfluenceGain(orcs, new ReasonMsg("Awakening killed enemy agent", core.data.influenceGain[ModData.influenceGainAction.AgentKill]), true);
                        }
                    }
                }
            }
        }

        public bool checkAlignment(SG_Orc orcSociety, Location loc)
        {
            if (orcSociety == null)
            {
                return false;
            }

            bool result = true;
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
    }
}
