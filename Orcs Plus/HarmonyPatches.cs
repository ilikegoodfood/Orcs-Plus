using Assets.Code;
using Assets.Code.Modding;
using CommunityLib;
using DuloGames.UI;
using HarmonyLib;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static UnityEngine.GraphicsBuffer;

namespace Orcs_Plus
{
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        private static bool patched = false;

        public static void PatchingInit()
        {
            if (patched)
            {
                return;
            }
            else
            {
                patched = true;
            }

            Patching();
        }

        private static void Patching()
        {
            Harmony.DEBUG = false;
            Harmony harmony = new Harmony("ILikeGoodFood.SOFG.OrcsPlus");

            if (Harmony.HasAnyPatches(harmony.Id))
            {
                return;
            }

            // Patches for Challenges that specialise orc camps
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildFortress), nameof(Ch_Orcs_BuildFortress.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildFortress_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildFortress), nameof(Ch_Orcs_BuildFortress.complete), new Type[] { typeof(UA) } ), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildFortress_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMages), nameof(Ch_Orcs_BuildMages.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMages_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMages), nameof(Ch_Orcs_BuildMages.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMages_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMenagerie), nameof(Ch_Orcs_BuildMenagerie.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMenagerie_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMenagerie), nameof(Ch_Orcs_BuildMenagerie.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMenagerie_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildShipyard), nameof(Ch_Orcs_BuildShipyard.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildShipyard_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildShipyard), nameof(Ch_Orcs_BuildShipyard.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildShipyard_complete_Postfix)));

            // Patches for Warlord rituals
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_CommandeerShips), nameof(Rt_Orcs_CommandeerShips.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Rt_Orcs_CommandeerShips_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_CommandeerShips), nameof(Rt_Orcs_CommandeerShips.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Rt_Orcs_CommandeerShips_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_ClaimTerritory), nameof(Rt_Orcs_ClaimTerritory.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Rt_Orcs_ClaimTerritory_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_ClaimTerritory), nameof(Rt_Orcs_ClaimTerritory.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Rt_Orcs_ClaimTerritory_complete_Postfix)));

            // Patches for Challenges in orc camps
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_DevastateOrcishIndustry), nameof(Ch_Orcs_DevastateOrcishIndustry.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_DevastateOrcishIndustry_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_DevastateOrcishIndustry), nameof(Ch_Orcs_DevastateOrcishIndustry.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_DevastateOrcishIndustry_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_Expand), nameof(Ch_Orcs_Expand.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_Expand_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_Expand), nameof(Ch_Orcs_Expand.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_Expand_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_OrcRaiding), nameof(Ch_OrcRaiding.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_OrcRaiding_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_OrcRaiding), nameof(Ch_OrcRaiding.valid)), transpiler: new HarmonyMethod(patchType, nameof(Ch_OrcRaiding_valid_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_OrcRaiding), nameof(Ch_OrcRaiding.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_OrcRaiding_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_RetreatToTheHills), nameof(Ch_Orcs_RetreatToTheHills.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_RetreatToTheHills_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_RetreatToTheHills), nameof(Ch_Orcs_RetreatToTheHills.valid)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_RetreatToTheHills_valid_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_RetreatToTheHills), nameof(Ch_Orcs_RetreatToTheHills.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_RetreatToTheHills_validFor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_RetreatToTheHills), nameof(Ch_Orcs_RetreatToTheHills.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_Orcs_RetreatToTheHills_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Rest_InOrcCamp), nameof(Ch_Rest_InOrcCamp.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Rest_InOrcCamp_validFor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Subjugate_Orcs), nameof(Ch_Subjugate_Orcs.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Subjugate_Orcs_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Subjugate_Orcs), nameof(Ch_Subjugate_Orcs.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Subjugate_Orcs_complete_Postfix)));

            // Patches for UAEN_OrcUpstart
            harmony.Patch(original: AccessTools.Constructor(typeof(UAEN_OrcUpstart), new Type[] { typeof(Location), typeof(SocialGroup), typeof(Person) }), postfix: new HarmonyMethod(patchType, nameof(UAEN_OrcUpstart_ctor_Postfix)));

            // Patches for Unit
            harmony.Patch(original: AccessTools.Method(typeof(Unit), nameof(Unit.hostileTo), new Type[] { typeof(Unit), typeof(bool) }), postfix: new HarmonyMethod(patchType, nameof(Unit_hostileTo_Postfix)));

            // Patches for UA
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getAttackUtility)), postfix: new HarmonyMethod(patchType, nameof(UA_getAttackUtility_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getVisibleUnits)), postfix: new HarmonyMethod(patchType, nameof(UA_getVisibleUnits_Postfix)));

            // Patches for I_HordeBanner
            harmony.Patch(original: AccessTools.Constructor(typeof(I_HordeBanner), new Type[] { typeof(Map), typeof(SG_Orc), typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(I_HordeBanner_ctor_Postfix)));

            // Patches for SocialGroup
            harmony.Patch(original: AccessTools.Method(typeof(SocialGroup), nameof(SocialGroup.checkIsGone), new Type[] { }), postfix: new HarmonyMethod(patchType, nameof(SocialGroup_checkIsGone_Postfix)));

            // Patches for SG_Orc
            harmony.Patch(original: AccessTools.Constructor(typeof(SG_Orc), new Type[] { typeof(Map), typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(SG_Orc_ctor_Postfox)));

            // Patches for Set_OrcCamp
            harmony.Patch(original: AccessTools.Constructor(typeof(Set_OrcCamp), new Type[] { typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(Set_OrcCamp_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Set_OrcCamp), nameof(Set_OrcCamp.turnTick)), postfix: new HarmonyMethod(patchType, nameof(Set_OrcCamp_turnTick_Postfix)));

            // Patches for Task_RazeLocation
            harmony.Patch(original: AccessTools.Method(typeof(Task_RazeLocation), nameof(Task_RazeLocation.turnTick)), transpiler: new HarmonyMethod(patchType, nameof(Task_RazeLocation_turnTick_Transpiler)));

            // Patches for UM_OrcArmy
            harmony.Patch(original: AccessTools.Method(typeof(UM_OrcArmy), nameof(UM_OrcArmy.turnTickInner), new Type[] { typeof(Map) }), postfix: new HarmonyMethod(patchType, nameof(UM_OrcArmy_turnTickInner_Postfix)));

            // Patches for Ch_Orcs_StealPlunder
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getChallengeUtility), new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) }), postfix: new HarmonyMethod(patchType, nameof(UA_getChallengeUtility_Postfix)));

            // COMMUNITY LIBRARY PATCHES
            harmony.Patch(original: AccessTools.Method(typeof(AIChallenge), nameof(AIChallenge.checkChallengeUtility), new Type[] { typeof(AgentAI.ChallengeData), typeof(UA), typeof(List<ReasonMsg>)}), postfix: new HarmonyMethod(patchType, nameof(AIChallenge_checkChallengeUtility_Postfix)));

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        private static void Ch_Orcs_BuildFortress_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildFortress] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildFortress_complete_Postfix(Ch_Orcs_BuildFortress __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcs != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.core.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.core.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        private static void Ch_Orcs_BuildMages_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildMages] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildMages_complete_Postfix(Ch_Orcs_BuildMages __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcs != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.core.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.core.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        private static void Ch_Orcs_BuildMenagerie_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildMenagerie] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildMenagerie_complete_Postfix(Ch_Orcs_BuildMenagerie __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcs != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.core.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.core.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildMenagerie]), true);
            }
        }

        private static void Ch_Orcs_BuildShipyard_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildShipyard] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildShipyard_complete_Postfix(Ch_Orcs_BuildShipyard __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcs != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.core.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.core.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        private static void Rt_Orcs_CommandeerShips_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.CommandeerShips] + " influence with the orc culture by completing this challenge.";
        }

        private static IEnumerable<CodeInstruction> Rt_Orcs_CommandeerShips_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.Rt_Orcs_CommandeerShips_complete_TranspilerBody), new Type[] { typeof(Rt_Orcs_CommandeerShips), typeof(UA), typeof(Set_OrcCamp) });

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            bool flag = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                yield return instructionList[i];

                if (!flag && instructionList[i].opcode == OpCodes.Stfld && instructionList[i-1].opcode == OpCodes.Ldc_I4_5)
                {
                    flag = true;

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                }
            }
        }

        private static void Rt_Orcs_CommandeerShips_complete_TranspilerBody(Rt_Orcs_CommandeerShips rt, UA u, Set_OrcCamp targetShipyard)
        {
            if (u == null || targetShipyard == null)
            {
                return;
            }

            SG_Orc orcSociety = targetShipyard.location?.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcSociety != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcSociety) && ModCore.core.data.orcSGCultureMap[orcSociety] != null)
            {
                orcCulture = ModCore.core.data.orcSGCultureMap[orcSociety];
            }

            if (orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(rt.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        private static void Ch_Orcs_DevastateOrcishIndustry_getDesc_Postfix(ref string __result)
        {
            __result += " If completed by a human agent, they gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.DevastateIndustry] + " influence with the orc culture.";
        }

        private static void Ch_Orcs_DevastateOrcishIndustry_complete_Postfix(Ch_Orcs_DevastateOrcishIndustry __instance, UA u)
        {
            if (!u.isCommandable() && !u.society.isDark())
            {
                HolyOrder_Orcs orcCulture = null;

                if (__instance.orcs != null && ModCore.core.data.orcSGCultureMap.ContainsKey(__instance.orcs) && ModCore.core.data.orcSGCultureMap[__instance.orcs] != null)
                {
                    orcCulture = ModCore.core.data.orcSGCultureMap[__instance.orcs];
                }

                if (orcCulture != null)
                {
                    ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.DevastateIndustry]));
                }
            }
        }

        private static void Ch_Orcs_Expand_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.Expand] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_Expand_complete_Postfix(Ch_Orcs_Expand __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcs != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.core.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.core.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Expand]), true);
            }
        }

        private static void Ch_OrcRaiding_getDesc_Postfix(Ch_OrcRaiding __instance, ref string __result)
        {
            __result = "Raids the most prosperous neighbouring human settlement, causing <b>devastation</b>, which harms prosperity and food production, and taking " + (int)(100.0 * __instance.map.param.ch_orcRaidingGoldGain) + "% of the settlment's gold reserves. Raiding will not completely destroy a settlement. If the settlement's devastation is too high, the raiding will only return with gold. You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.Raiding] + " influence with the orc culture by completing this challenge.";
        }

        private static IEnumerable<CodeInstruction> Ch_OrcRaiding_valid_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.Ch_OrcRaiding_valid_TranspilerBody), new Type[] { typeof(Ch_OrcRaiding) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static bool Ch_OrcRaiding_valid_TranspilerBody(Ch_OrcRaiding ch)
        {
            foreach (Location neighbour in ch.location.getNeighbours())
            {
                if (neighbour.soc is Society)
                {
                    SettlementHuman settlementHuman = neighbour.settlement as SettlementHuman;
                    if (settlementHuman != null)
                    {
                        Pr_Devastation devastation = settlementHuman.location.properties.OfType<Pr_Devastation>().FirstOrDefault();
                        if (devastation == null || devastation.charge < 150 || settlementHuman.ruler?.getGold() > 5)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static IEnumerable<CodeInstruction> Ch_OrcRaiding_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.Ch_OrcRaiding_complete_TranspilerBody), new Type[] { typeof(Ch_OrcRaiding), typeof(UA) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static void Ch_OrcRaiding_complete_TranspilerBody(Ch_OrcRaiding ch, UA u)
        {
            Map map = ch.map;
            Location location = ch.location;
            Settlement sub = ch.sub;

            SG_Orc orcSociety = sub.location.soc as SG_Orc;
            if (orcSociety == null)
            {
                return;
            }

            //Console.WriteLine("OrcsPlus: Running Burn The Fields Complete");
            List<Location> locationsDevastate = new List<Location>();
            List<Location> locationsSteal = new List<Location>();
            Location targetLocation;
            double prosperity = 0.0;
            double gold = 0.0;
            SettlementHuman settlementHuman;
            Pr_Devastation devastation = null;

            //Console.WriteLine("OrcsPlus: Finding target location.");
            foreach (Location neighbour in location.getNeighbours())
            {
                if (neighbour.soc != null && neighbour.soc is Society && neighbour.settlement != null)
                {
                    //Console.WriteLine("OrcsPlus: Neighbour has social group and settlement.");
                    settlementHuman = neighbour.settlement as SettlementHuman;
                    if (settlementHuman != null)
                    {
                        if (settlementHuman.ruler?.getGold() > gold)
                        {
                            gold = settlementHuman.ruler.getGold();
                            locationsSteal.Clear();
                            locationsSteal.Add(neighbour);
                        }
                        else if (settlementHuman.ruler?.getGold() == gold)
                        {
                            locationsSteal.Add(neighbour);
                        }

                        //Console.WriteLine("OrcsPlus: Settlement is human settlement.");
                        devastation = settlementHuman.location.properties.OfType<Pr_Devastation>().FirstOrDefault();
                        if (devastation == null || devastation.charge < 150)
                        {
                            //Console.WriteLine("OrcsPlus: Devastation is less thna 150.");
                            if (settlementHuman.prosperity > prosperity)
                            {
                                //Console.WriteLine("OrcsPlus: Neighbour is most prosperous so far.");
                                prosperity = settlementHuman.prosperity;
                                locationsDevastate.Clear();
                                locationsDevastate.Add(neighbour);
                            }
                            else if (settlementHuman.prosperity == prosperity)
                            {
                                //Console.WriteLine("OrcsPlus: Neighbour's prosperity is equal to most prosperous neighbour so far.");
                                locationsDevastate.Add(neighbour);
                            }
                        }
                    }
                }
            }

            if (locationsDevastate == null || locationsDevastate.Count == 0)
            {
                //Console.WriteLine("OrcsPlus: No devestate location found. Steal instead");
                if (locationsSteal == null || locationsSteal.Count == 0)
                {
                    //Console.WriteLine("OrcsPlus: No steal location found.");
                    return;
                }
                else if (locationsSteal.Count == 1)
                {
                    targetLocation = locationsSteal[0];
                }
                else
                {
                    targetLocation = locationsSteal[Eleven.random.Next(locationsSteal.Count)];
                }
            }
            else if (locationsDevastate.Count == 1)
            {
                targetLocation = locationsDevastate[0];
            }
            else
            {
                targetLocation = locationsDevastate[Eleven.random.Next(locationsDevastate.Count)];
            }
            //Console.WriteLine("OrcsPlus: Target location found.");

            //Console.WriteLine("OrcsPlus: Applying effect to Location " + targetLocation.getName() + ".");
            settlementHuman = targetLocation.settlement as SettlementHuman;

            if (settlementHuman == null)
            {
                return;
            }

            devastation = settlementHuman.location.properties.OfType<Pr_Devastation>().FirstOrDefault();
            if (devastation == null || devastation.charge < 150)
            {
                Property.addToPropertySingleShot("Orc Raid", Property.standardProperties.DEVASTATION, 100, targetLocation);

                if (settlementHuman?.ruler != null && gold > 5)
                {
                    ch.msgString = "The orcs devestate the lands around " + targetLocation.getName() + ", causing 100% devastation, and taking loot worth " + (int)(gold * map.param.ch_orcRaidingGoldGain) + " gold. The orcs have gained " + map.param.ch_orcishRaidingMenaceGain + " menace as a society as a result of this action.";
                    settlementHuman.ruler.addGold(-(int)(gold * map.param.ch_orcRaidingGoldGain));
                    u.person.addGold((int)(gold * map.param.ch_orcRaidingGoldGain));
                }
                else
                {
                    ch.msgString = "The orcs devestate the lands around " + targetLocation.getName() + ", causing 100% devastation, but there was nothing of value to loot. The orcs have gained " + map.param.ch_orcishRaidingMenaceGain + " menace as a society as a result of this action.";
                }
            }
            else
            {
                if (settlementHuman?.ruler != null && gold > 5)
                {
                    ch.msgString = "The orcs raided the lands around " + targetLocation.getName() + ", causing no devastation, and taking loot worth " + (int)(gold * map.param.ch_orcRaidingGoldGain) + " gold. The orcs have gained " + map.param.ch_orcishRaidingMenaceGain + " menace as a society as a result of this action.";
                    settlementHuman.ruler.addGold(-(int)(gold * map.param.ch_orcRaidingGoldGain));
                    u.person.addGold((int)(gold * map.param.ch_orcRaidingGoldGain));
                }
                else
                {
                    ch.msgString = "All nearby settlements are too badly devastated, and too poor, to be worth raiding. The orcs did not conduct a raid, but their activity on the boarders of the nearby human settlements still caused significant alarm. The orcs have gained " + map.param.ch_orcishRaidingMenaceGain + " menace as a society as a result of this action.";
                }
            }

            if (orcSociety != null)
            {
                orcSociety.menace += map.param.ch_orcishRaidingMenaceGain;
                HolyOrder_Orcs orcCulture = null;
                if (u.isCommandable() && ModCore.core.data.orcSGCultureMap.ContainsKey(orcSociety) && ModCore.core.data.orcSGCultureMap[orcSociety] != null)
                {
                    orcCulture = ModCore.core.data.orcSGCultureMap[orcSociety];
                }

                if (orcCulture != null)
                {
                    ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(ch.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Raiding]), true);
                }
            }

            return;
        }

        private static void Ch_Orcs_RetreatToTheHills_getDesc_Postfix(ref string __result)
        {
            __result = "Causes half of all orc industry in this orc camp, and neighbouring orc camps, to be turned into defensive positions. Good at quickly reducing threat to avoid war, or surviving one against a powerful foe.";
        }

        private static bool Ch_Orcs_RetreatToTheHills_valid_Postfix(bool result, Ch_Orcs_RetreatToTheHills __instance)
        {
            return __instance.location.settlement is Set_OrcCamp;
        }

        private static bool Ch_Orcs_RetreatToTheHills_validFor_Postfix(bool result, Ch_Orcs_RetreatToTheHills __instance, UA ua)
        {
            if (ua.isCommandable() && __instance.location.settlement?.infiltration == 1.0)
            {
                return true;
            }
            else if (!ua.isCommandable() && ua.society != null && (ua.society == __instance.location.soc || (ua.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety == __instance.location.soc)))
            {
                return true;
            }

            return false;
        }

        private static IEnumerable<CodeInstruction> Ch_Orcs_RetreatToTheHills_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.Ch_Orcs_RetreatToTheHills_complete_TranspilerBody), new Type[] { typeof(Ch_Orcs_RetreatToTheHills), typeof(UA) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static void Ch_Orcs_RetreatToTheHills_complete_TranspilerBody(Ch_Orcs_RetreatToTheHills ch, UA ua)
        {
            SG_Orc orcSociety = ch.location.soc as SG_Orc;
            List<UM_OrcArmy> armies = new List<UM_OrcArmy>();

            if (orcSociety == null || !(ch.location.settlement is Set_OrcCamp))
            {
                return;
            }

            List<Location> targetLocations = new List<Location> { ch.location };
            foreach (Location location in ch.location.getNeighbours())
            {
                if (location.settlement is Set_OrcCamp camp && location.soc == orcSociety)
                {
                    if (!ua.isCommandable() || camp.infiltration == 1.0)
                    {
                        targetLocations.Add(location);
                    }
                }
            }

            foreach (Location location in targetLocations)
            {
                Set_OrcCamp camp = location.settlement as Set_OrcCamp;
                Pr_OrcishIndustry industry = location.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();
                Pr_OrcDefences defences = location.properties.OfType<Pr_OrcDefences>().FirstOrDefault();

                if (defences == null)
                {
                    defences = new Pr_OrcDefences(location);
                    location.properties.Add(defences);
                }

                if (industry == null)
                {
                    industry = new Pr_OrcishIndustry(location);
                    industry.charge = 0.0;
                    location.properties.Add(industry);
                }

                industry.charge /= 2.0;
                defences.charge += industry.charge;
                camp.defences += industry.charge / 2;
            }

            foreach (Unit unit in ch.map.units)
            {
                UM_OrcArmy army = unit as UM_OrcArmy;
                if (army != null && army.society == orcSociety)
                {
                    armies.Add(army);
                }
            }

            if (armies.Count > 0)
            {
                foreach (UM_OrcArmy army in armies)
                {
                    army.updateMaxHP();
                }
            }

            //orcSociety.menace += ModCore.core.data.menaceGain[ModData.menaceGainAction.Retreat];
        }

        private static bool Ch_Rest_InOrcCamp_validFor_Postfix(bool result, UA ua)
        {
            if (ua is UAA_OrcElder)
            {
                return true;
            }

            return result;
        }

        private static void Ch_Subjugate_Orcs_getDesc_Postfix(Ch_Subjugate_Orcs __instance, ref string __result)
        {
            Set_OrcCamp orcCamp = __instance.sub as Set_OrcCamp;

            if (orcCamp == null)
            {
                return;
            }

            if (orcCamp.specialism == 0)
            {
                __result += " You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.Subjugate] + " influence with the orc culture by completing this challenge.";
                return;
            }

            __result += " You gain " + (ModCore.core.data.influenceGain[ModData.influenceGainAction.Subjugate] * 2) + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Subjugate_Orcs_complete_Postfix(Ch_Subjugate_Orcs __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            Set_OrcCamp orcCamp = __instance.location.settlement as Set_OrcCamp;

            if (orcCamp == null)
            {
                return;
            }

            HolyOrder_Orcs orcCulture = null;
            if (orcs != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.core.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.core.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                if (orcCamp.specialism == 0)
                {
                    ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Subjugate]), true);
                    return;
                }

                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Subjugate] * 2), true);
            }
        }

        private static void Rt_Orcs_ClaimTerritory_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.core.data.influenceGain[ModData.influenceGainAction.Expand] + " influence with the orc culture by completing this challenge.";
        }

        private static void Rt_Orcs_ClaimTerritory_complete_Postfix(Ch_Orcs_BuildFortress __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcs != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.core.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.core.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                ModCore.core.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.core.data.influenceGain[ModData.influenceGainAction.Expand]), true);
            }
        }

        private static void UAEN_OrcUpstart_ctor_Postfix(UAEN_OrcUpstart __instance)
        {
            if (__instance.getStatCommand() < 3)
            {
                __instance.person.stat_command = 3;
            }

            __instance.rituals.Add(new Rt_Orcs_Confinement(__instance.location));

            I_HordeBanner banner = __instance.person.items.OfType<I_HordeBanner>().FirstOrDefault();
            if (banner != null)
            {
                Rti_RecruitWarband recruit = banner.rituals.OfType<Rti_RecruitWarband>().FirstOrDefault();
                recruit?.complete(__instance);
            }
        }

        private static bool Unit_hostileTo_Postfix(bool result, Unit __instance, Unit other, bool allowRecursion)
        {
            //Console.WriteLine("OrcsPlus: Running Unit_hostileTo_Postfix");
            if (__instance is UM_OrcArmy orcArmy && other is UA)
            {
                //Console.WriteLine("OrcsPlus: __instance is UM_OrcArmy and other is UA");
                SG_Orc orcSociety = __instance.society as SG_Orc;
                //Console.WriteLine("OrcsPlus: Got orcSociety");
                if (orcSociety != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcSociety) && ModCore.core.data.orcSGCultureMap[orcSociety] != null)
                {
                    //Console.WriteLine("OrcsPlus: __instance.society is SG_Orc and orcCulture does not equal null");
                    HolyOrder_Orcs orcCulture = ModCore.core.data.orcSGCultureMap[orcSociety];
                    //Console.WriteLine("OrcsPlus: Got orcCulture");

                    if (other.society == orcSociety || other.society == orcCulture)
                    {
                        //Console.WriteLine("OrcsPlus: other.society is orcSociety or orcCulture");
                        return false;
                    }

                    if (other.homeLocation != -1 && (other.map.locations[other.homeLocation].soc == orcSociety || other.map.locations[other.homeLocation].soc == orcCulture))
                    {
                        //Console.WriteLine("OrcsPlus: other.homeLocation is index of location that is of orcSociety or orcCulture");
                        return result;
                    }

                    foreach (Item item in other.person.items)
                    {
                        if (item != null)
                        {
                            I_HordeBanner banner = item as I_HordeBanner;
                            if (ModCore.core.data.modCovensCursesCurios && ModCore.core.data.asmCovensCursesCurtios != null)
                            {
                                Type t = ModCore.core.data.asmCovensCursesCurtios.GetType("CovenExpansion.I_BarbDominion");
                                if (t != null && item.GetType() == t)
                                {
                                    return false;
                                }
                            }

                            if (banner?.orcs == orcSociety && !(other.society != null && __instance.society.getRel(other.society).state == DipRel.dipState.war))
                            {
                                return false;
                            }
                        }
                    }

                    if (ModCore.core.data.modLivingWilds && ModCore.core.data.asmLivingWilds != null)
                    {
                        //Console.WriteLine("OrcsPlus: LivingWilds assembly loaded");
                        Type t = ModCore.core.data.asmLivingWilds.GetType("LivingWilds.UAEN_Nature_Critter");
                        if (t != null && other.GetType().IsSubclassOf(t))
                        {
                            //Console.WriteLine("OrcsPlus: other.GetType() is typeof(UAEN_Nature_Critter)");
                            return result;
                        }
                    }

                    H_Intolerance tolerance = orcCulture.tenets.OfType<H_Intolerance>().FirstOrDefault();
                    if (tolerance != null)
                    {
                        //Console.WriteLine("OrcsPlus: tolerance is not null");
                        if (other.society is SG_Orc || other.society is HolyOrder_Orcs)
                        {
                            //Console.WriteLine("OrcsPlus: other.society is SG_Orc or HolyOrder_Orcs");
                            result = true;
                        }
                        else if (tolerance.status < 0)
                        {
                            //Console.WriteLine("OrcsPlus: tolerance.status is less than 0");
                            if ((other.society == null || !other.society.isDark()) && !other.isCommandable())
                            {
                                result = true;
                            }
                        }
                        else if (tolerance.status > 0)
                        {
                            //Console.WriteLine("OrcsPlus: tolerance.status is greater than 0");
                            if (other.isCommandable() && orcArmy.parent.shadow < 0.5 && orcArmy.parent.infiltration < 1.0)
                            {
                                result = true;
                            }
                            else if (other.society == null)
                            {
                                result = true;
                            }
                            else if (orcArmy.parent.shadow < 0.5 && other.society.isDark())
                            {
                                result = true;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("OrcsPlus: tolerance.status is 0");
                            if ((other.society == null || !other.society.isDark()) && !other.isCommandable())
                            {
                                result = true;
                            }
                            else if (other.isCommandable() && orcArmy.parent.shadow < 0.5 && orcArmy.parent.infiltration < 1.0)
                            {
                                result = true;
                            }
                            else if (orcArmy.parent.shadow < 0.5 && other.society.isDark())
                            {
                                result = true;
                            }
                        }
                    }
                }
            }

            //Console.WriteLine("OrcsPlus: returning result: " + result.ToString());
            return result;
        }

        private static double UA_getAttackUtility_Postfix(double utility, UA __instance, Unit other, List<ReasonMsg> reasons, bool includeDangerousFoe)
        {
            if (__instance is UAEN_OrcUpstart)
            {
                return UAEN_OrcUpstart_getAttackUtility(utility, __instance as UAEN_OrcUpstart, other, reasons, includeDangerousFoe);
            }

            return utility;
        }

        public static double UAEN_OrcUpstart_getAttackUtility(double utility, UAEN_OrcUpstart upstart, Unit other, List<ReasonMsg> reasonMsgs, bool includeDangerousFoe)
        {
            utility = 0.0;
            if (other is UA target && upstart.society is SG_Orc orcSociety && !orcSociety.checkIsGone() && target.society != orcSociety)
            {
                if (ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && target.society != orcCulture)
                {
                    H_Intolerance intolerance = orcCulture.tenets.OfType<H_Intolerance>().FirstOrDefault();
                    if (intolerance != null)
                    {
                        bool otherIsTarget = false;
                        if (target.society != null)
                        {
                            if (target.society != orcSociety && target.society != orcCulture)
                            {
                                if (target.homeLocation == -1 || upstart.map.locations[target.homeLocation].soc != orcSociety || upstart.map.locations[target.homeLocation].soc != orcCulture)
                                {
                                    if (intolerance.status == 0)
                                    {
                                        otherIsTarget = true;
                                    }
                                    else if (intolerance.status > 0)
                                    {
                                        if (orcSociety.getRel(target.society).state == DipRel.dipState.war || target.society.isDark() || target.isCommandable())
                                        {
                                            otherIsTarget = true;
                                        }
                                        else
                                        {
                                            reasonMsgs?.Add(new ReasonMsg("Orcs of a human-tolerant culture will not attack agents of good societies", -10000.0));
                                            utility -= 10000.0;
                                        }
                                    }
                                    else if (intolerance.status < 0)
                                    {
                                        if (!target.isCommandable() && (orcSociety.getRel(target.society).state == DipRel.dipState.war || target.society is SG_Orc || target.society is HolyOrder_Orcs || !target.society.isDark()))
                                        {
                                            otherIsTarget = true;
                                        }
                                        else
                                        {
                                            reasonMsgs?.Add(new ReasonMsg("Orcs of an elder-tolerant culture will not attack the elder's agents or societies", -10000.0));
                                            utility -= 10000.0;
                                        }
                                    }
                                    else
                                    {
                                        reasonMsgs?.Add(new ReasonMsg("ERROR: Invalid Intolerance Tenet Status", -10000.0));
                                        utility -= 10000.0;
                                    }
                                }
                                else
                                {
                                    reasonMsgs?.Add(new ReasonMsg("Orcs will only attack outsiders", -10000.0));
                                    utility -= 10000.0;
                                }
                            }
                            else
                            {
                                reasonMsgs?.Add(new ReasonMsg("Target is of own culture", -10000.0));
                                utility -= 10000.0;
                            }
                        }
                        else
                        {
                            if (!(target.isCommandable() && intolerance.status < 0))
                            {
                                otherIsTarget = true;
                            }
                            else
                            {
                                reasonMsgs?.Add(new ReasonMsg("Orcs of an elder-tolerant culture will not attack the elder's agents", -10000.0));
                                utility -= 10000.0;
                            }
                        }

                        if (otherIsTarget)
                        {
                            foreach (Item item in target.person.items)
                            {
                                if (item != null)
                                {
                                    if (ModCore.core.data.modCovensCursesCurios && ModCore.core.data.asmCovensCursesCurtios != null)
                                    {
                                        Type t = ModCore.core.data.asmCovensCursesCurtios.GetType("CovenExpansion.I_BarbDominion");
                                        if (t != null && item.GetType() == t)
                                        {
                                            reasonMsgs?.Add(new ReasonMsg("Orcs will not attack banner bearer", -10000.0));
                                            utility -= 10000.0;
                                            return utility;
                                        }
                                    }

                                    I_HordeBanner banner = item as I_HordeBanner;
                                    if (banner?.orcs == orcSociety && (target.society == null || upstart.society.getRel(target.society).state != DipRel.dipState.war))
                                    {
                                        reasonMsgs?.Add(new ReasonMsg("Orcs will not attack banner bearer", -10000.0));
                                        utility -= 10000.0;
                                        return utility;
                                    }
                                }
                            }

                            if (target.location.soc == orcSociety || (target.society != null && orcSociety.getRel(target.society).state == DipRel.dipState.war && (target.location.soc == null || orcSociety.getRel(target.location.soc).state == DipRel.dipState.war)))
                            {
                                double val;
                                if (target.location.soc != orcSociety)
                                {
                                    val = -25;
                                    reasonMsgs?.Add(new ReasonMsg("Target is outside of " + orcSociety.getName() + "'s territory", val));
                                    utility += val;
                                }
                                else if (target.task is Task_PerformChallenge task)
                                {
                                    val = 20;
                                    reasonMsgs?.Add(new ReasonMsg("Agent is interfereing with " + orcSociety.getName() + "'s territory", val));
                                    utility += val;

                                    if (task.challenge is Ch_Orcs_StealPlunder)
                                    {
                                        val = 20;
                                        reasonMsgs?.Add(new ReasonMsg("Agent stealing gold from " + orcSociety.getName() + "'s plunder", val));
                                        utility += val;
                                    }
                                }

                                utility += upstart.person.getTagUtility(new int[]
                                {
                                    Tags.COMBAT,
                                    Tags.CRUEL,
                                    Tags.DANGER
                                }, new int[0], reasonMsgs);
                                utility += upstart.person.getTagUtility(target.getPositiveTags(), target.getNegativeTags(), reasonMsgs);

                                val = 20;
                                reasonMsgs?.Add(new ReasonMsg("Eager for Combat", val));
                                utility += val;

                                if (target is UAEN_Vampire)
                                {
                                    val = -30;
                                    reasonMsgs?.Add(new ReasonMsg("Fear of Vampires", val));
                                    utility += val;
                                }

                                if (ModCore.core.data.modLivingWilds && ModCore.core.data.asmLivingWilds != null)
                                {
                                    Type t = ModCore.core.data.asmLivingWilds.GetType("LivingWilds.UAEN_Nature_Critter", false);
                                    if (t != null && target.GetType().IsSubclassOf(t))
                                    {
                                        val = -30;
                                        reasonMsgs?.Add(new ReasonMsg("Nature", val));
                                        utility += val;
                                    }
                                }

                                val = upstart.map.getStepDist(target.location, upstart.location);
                                reasonMsgs?.Add(new ReasonMsg("Distance", -val * 10));
                                utility -= val * 10;

                                if (includeDangerousFoe)
                                {
                                    val = 0.0;
                                    double dangerUtility = target.getDangerEstimate() - upstart.getDangerEstimate();
                                    if (dangerUtility > 0.0)
                                    {
                                        val -= upstart.map.param.utility_ua_attackDangerReluctanceOffset;
                                    }
                                    dangerUtility += 2;
                                    if (dangerUtility > 0.0)
                                    {
                                        val += upstart.map.param.utility_ua_attackDangerReluctanceMultiplier;
                                    }
                                    reasonMsgs?.Add(new ReasonMsg("Dangerous Foe", val));
                                    utility += val;
                                }

                                if (target.society != null && orcSociety.getRel(target.society).state == DipRel.dipState.war)
                                {
                                    val = 50.0;
                                    reasonMsgs?.Add(new ReasonMsg("At War", val));
                                    utility += val;
                                }

                                foreach (HolyTenet holyTenet in orcCulture.tenets)
                                {
                                    utility += holyTenet.addUtilityAttack(upstart, target, reasonMsgs);
                                }
                                foreach (ModKernel modKernel in upstart.map.mods)
                                {
                                    utility = modKernel.unitAgentAIAttack(upstart.map, upstart, target, reasonMsgs, utility);
                                }
                            }
                            else
                            {
                                if (target.society != null && orcSociety.getRel(target.society).state == DipRel.dipState.war)
                                {
                                    reasonMsgs?.Add(new ReasonMsg("Tresspassing in territory of " + target.location.soc.getName(), -10000.0));
                                    utility -= 10000.0;
                                }
                                else
                                {
                                    reasonMsgs?.Add(new ReasonMsg("Target is outside of " + orcSociety.getName() + "'s territory", -10000.0));
                                    utility -= 10000.0;
                                }
                            }
                        }
                        else
                        {
                            reasonMsgs?.Add(new ReasonMsg("Invalid Target", -10000.0));
                            utility -= 10000.0;
                        }
                    }
                    else
                    {
                        reasonMsgs?.Add(new ReasonMsg("ERROR: Failed to find Intolerence Tenet", -10000.0));
                        utility -= 10000.0;
                    }
                }
                else
                {
                    if (orcCulture == null)
                    {
                        reasonMsgs?.Add(new ReasonMsg("ERROR: Failed to find Orc Culture", -10000.0));
                        utility -= 10000.0;
                    }
                    else
                    {
                        reasonMsgs?.Add(new ReasonMsg("Target is of own Orc Culture (Holy Order)", -10000.0));
                        utility -= 10000.0;
                    }
                }
            }
            else
            {
                if (!(other is UA))
                {
                    reasonMsgs?.Add(new ReasonMsg("Target is not Agent", -10000.0));
                    utility -= 10000.0;
                }
                else if (!(upstart.society is SG_Orc))
                {
                    reasonMsgs?.Add(new ReasonMsg("Orc Upstart is not of Orc Social Group", -10000.0));
                    utility -= 10000.0;
                }
                else
                {
                    reasonMsgs?.Add(new ReasonMsg("Target is of own culture", -10000.0));
                    utility -= 10000.0;
                }
            }

            return utility;
        }

        private static List<Unit> UA_getVisibleUnits_Postfix(List<Unit> units, UA __instance)
        {
            if (__instance is UAEN_OrcUpstart)
            {
                return UAEN_OrcUpstart_getvisibleUnits(units, __instance as UAEN_OrcUpstart);
            }

            return units;
        }

        private static List<Unit> UAEN_OrcUpstart_getvisibleUnits(List<Unit> units, UAEN_OrcUpstart upstart)
        {
            if (units == null)
            {
                units = new List<Unit>();
            }

            if (upstart.society is SG_Orc orcSociety && !orcSociety.checkIsGone())
            {
                ModCore.core.data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture);

                foreach (Unit unit in upstart.map.units)
                {
                    Type ccc_Banner = null;
                    if (unit == upstart)
                    {
                        continue;
                    }

                    if (ModCore.core.data.modCovensCursesCurios)
                    {
                        ccc_Banner = ModCore.core.data.asmCovensCursesCurtios.GetType("I_BarbDominion");
                    }

                    if (unit is UA agent)
                    {
                        if (agent.society != null && (agent.society == orcSociety || agent.society == orcCulture))
                        {
                            units.Add(unit);
                        }
                        else if (agent.person.items.OfType<I_HordeBanner>().FirstOrDefault()?.orcs == orcSociety)
                        {
                            units.Add(unit);
                        }
                        else if (ModCore.core.data.modCovensCursesCurios && ccc_Banner != null)
                        {
                            foreach (Item item in agent.person.items)
                            {
                                if (item.GetType() == ccc_Banner)
                                {
                                    units.Add(unit);
                                    break;
                                }
                            }
                        }
                        else if (agent.homeLocation != -1 && (upstart.map.locations[agent.homeLocation].soc == orcSociety || (orcCulture != null && upstart.map.locations[agent.homeLocation].soc == orcCulture)) && !(agent.task is Task_InHiding))
                        {
                            units.Add(unit);
                        }
                        else if (agent.location.soc != null && (agent.location.soc == orcSociety || agent.location.soc == orcCulture) && !(agent.task is Task_InHiding))
                        {
                            units.Add(unit);
                        }
                        else if (agent.society != null && (agent.society.getRel(orcSociety).state == DipRel.dipState.war ||agent.society.getRel(orcCulture).state == DipRel.dipState.war) && !(agent.task is Task_InHiding))
                        {
                            units.Add(unit);
                        }
                        else if (agent.task is Task_PerformChallenge performChallenge && performChallenge.challenge.isChannelled() && upstart.map.getStepDist(upstart.location, agent.location) <= 10)
                        {
                            units.Add(unit);
                        }
                    }
                }
            }

            return units;
        }

        private static void I_HordeBanner_ctor_Postfix(I_HordeBanner __instance, Location l)
        {
            __instance.rituals.Add(new Rti_RecruitWarband(l, __instance));
        }

        private static bool SocialGroup_checkIsGone_Postfix(bool result, SocialGroup __instance)
        {
            if (__instance is SG_Orc orcSociety && orcSociety != null)
            {
                bool isGone = true;
                List<Location> locations = new List<Location>();
                foreach (Location location in orcSociety.map.locations)
                {
                    if (location.soc == orcSociety)
                    {
                        locations.Add(location);

                        if (location.settlement is Set_OrcCamp)
                        {
                            isGone = false;
                        }
                    }
                }

                if (isGone)
                {
                    orcSociety.cachedGone = true;
                    foreach (Location location in locations)
                    {
                        location.soc = null;
                    }
                }

                return isGone;
            }

            return result;
        }

        private static void SG_Orc_ctor_Postfox(SG_Orc __instance)
        {
            if (!ModCore.core.data.orcSGCultureMap.ContainsKey(__instance))
            {
                ModCore.core.data.orcSGCultureMap.Add(__instance, null);
            }

            if (ModCore.core.data.orcSGCultureMap[__instance] == null)
            {
                ModCore.core.data.orcSGCultureMap[__instance] = new HolyOrder_Orcs(__instance.map, __instance.map.locations[__instance.capital], __instance);
            }
        }


        private static void Set_OrcCamp_ctor_Postfix(Set_OrcCamp __instance)
        {
            __instance.customChallenges.Add(new Ch_Orcs_GatherHorde(__instance.location));
            __instance.customChallenges.Add(new Ch_Orcs_FundHorde(__instance.location, __instance));
            __instance.customChallenges.Add(new Ch_Orcs_RaidOutpost(__instance.location, __instance));
        }

        private static void Set_OrcCamp_turnTick_Postfix(Set_OrcCamp __instance)
        {
            ReasonMsg fortressDefenceMsg = new ReasonMsg("Fortifications (max 50%)", 4.0);

            if (__instance.specialism == 1)
            {

                if (__instance.location.settlement != __instance)
                {
                    return;
                }

                Pr_OrcDefences defences = __instance.location.properties.OfType<Pr_OrcDefences>().FirstOrDefault();
                if (defences == null)
                {
                    defences = new Pr_OrcDefences(__instance.location);
                    __instance.location.properties.Add(defences);
                }

                if (defences.charge <= 50.0)
                {
                    fortressDefenceMsg.value = Math.Min(4, 2 + defences.charge - 50);
                    defences.influences.Add(fortressDefenceMsg);
                }
            }
        }

        private static IEnumerable<CodeInstruction> Task_RazeLocation_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(typeof(HarmonyPatches), nameof(Task_RazeLocation_turnTick_TranspilerBody));

            bool found = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!found)
                {
                    if (i > 0 && instructionList[i].opcode == OpCodes.Ldloc_0 && instructionList[i-1].opcode == OpCodes.Stfld && instructionList[i-2].opcode == OpCodes.Sub)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        found = true;
                    }
                }

                yield return instructionList[i];
            }
        }

        private static void Task_RazeLocation_turnTick_TranspilerBody(UM u)
        {
            Settlement set = u.location?.settlement;
            SG_Orc orcs = u.location.soc as SG_Orc;

            if (set is Set_OrcCamp)
            {
                Pr_Death death = u.location.properties.OfType<Pr_Death>().FirstOrDefault();
                if (death == null)
                {
                    death = new Pr_Death(u.location);
                    death.charge = 0;
                    u.location.properties.Add(death);
                }

                Property.addToProperty("Militray Action", Property.standardProperties.DEATH, 2.0, set.location);

                if (orcs != null)
                {
                    if (u.isCommandable())
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razing Orc Camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                    }
                    else if (!u.society.isDark())
                    {
                        ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razing Orc Camp", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                    }
                }
            }

            if (set != null && u.isCommandable())
            {
                List<SG_Orc> orcSocieties = ModCore.core.data.getOrcSocieties(u.map);

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
                            ModCore.core.TryAddInfluenceGain(orcs, new ReasonMsg("Razing Emeny Settlement", ModCore.core.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                        }
                    }
                }
            }
        }

        private static void UM_OrcArmy_turnTickInner_Postfix(UM_OrcArmy __instance)
        {
            if (__instance.task == null)
            {
                List<UA> targetAgents = new List<UA>();
                if (__instance.society != null && __instance.parent != null && __instance.parent.location.settlement == __instance.parent && __instance.parent.location.soc == __instance.society && __instance.location == __instance.parent.location && __instance.task == null)
                {
                    if (ModCore.core.data.orcSGCultureMap.TryGetValue(__instance.society as SG_Orc, out HolyOrder_Orcs orcCulture))
                    {
                        H_Intolerance tolerance = orcCulture.tenets.OfType<H_Intolerance>().FirstOrDefault();
                        if (tolerance != null)
                        {
                            foreach (Unit unit in __instance.parent.location.units)
                            {
                                if (unit.society == __instance.society || unit.society == orcCulture)
                                {
                                    continue;
                                }

                                if (unit.homeLocation != -1 && (__instance.map.locations[unit.homeLocation].soc == __instance.society || __instance.map.locations[unit.homeLocation].soc == orcCulture))
                                {
                                    continue;
                                }

                                if (ModCore.core.data.modLivingWilds && ModCore.core.data.asmLivingWilds != null)
                                {
                                    Type t = ModCore.core.data.asmLivingWilds.GetType("LivingWilds.UAEN_Nature_Critter", false);
                                    if (t != null && unit.GetType().IsSubclassOf(t))
                                    {
                                        continue;
                                    }
                                }

                                if (unit is UA agent && agent.task is Task_PerformChallenge task)
                                {

                                    foreach (Item item in agent.person.items)
                                    {
                                        if (item != null)
                                        {
                                            I_HordeBanner banner = item as I_HordeBanner;
                                            if (ModCore.core.data.modCovensCursesCurios && ModCore.core.data.asmCovensCursesCurtios != null)
                                            {
                                                Type t = ModCore.core.data.asmCovensCursesCurtios.GetType("CovenExpansion.I_BarbDominion", false);
                                                if (t != null && item.GetType() == t)
                                                {
                                                    continue;
                                                }
                                            }

                                            if (banner?.orcs == __instance.society && !(agent.society != null && __instance.society.getRel(agent.society).state == DipRel.dipState.war))
                                            {
                                                continue;
                                            }
                                        }
                                    }

                                    if (agent.society is SG_Orc || agent.society is HolyOrder_Orcs)
                                    {
                                        targetAgents.Add(agent);
                                    }
                                    else if (task.challenge is Ch_Orcs_StealPlunder)
                                    {
                                        targetAgents.Add(agent);
                                    }
                                    else if (tolerance.status < 0)
                                    {
                                        if ((agent.society == null || !agent.society.isDark()) && !agent.isCommandable())
                                        {
                                            targetAgents.Add(agent);
                                        }
                                    }
                                    else if (tolerance.status > 0)
                                    {
                                        if (agent.isCommandable() && __instance.parent.shadow < 0.5 && __instance.parent.infiltration < 1.0)
                                        {
                                            targetAgents.Add(agent);
                                        }
                                        else if (agent.society == null)
                                        {
                                            targetAgents.Add(agent);
                                        }
                                        else if (__instance.parent.shadow < 0.5 && agent.society.isDark())
                                        {
                                            targetAgents.Add(agent);
                                        }
                                    }
                                    else
                                    {
                                        if ((agent.society == null || !agent.society.isDark()) && !agent.isCommandable())
                                        {
                                            targetAgents.Add(agent);
                                        }
                                        else if (agent.isCommandable() && __instance.parent.shadow < 0.5 && __instance.parent.infiltration < 1.0)
                                        {
                                            targetAgents.Add(agent);
                                        }
                                        else if (__instance.parent.shadow < 0.5 && agent.society.isDark())
                                        {
                                            targetAgents.Add(agent);
                                        }
                                    }
                                }
                            }

                            if (targetAgents.Count > 0)
                            {
                                foreach (UA agent in targetAgents)
                                {
                                    agent.hp--;
                                    if (agent.hp <= 0)
                                    {
                                        if (agent.isCommandable() || agent.person.isWatched())
                                        {
                                            __instance.map.addUnifiedMessage(__instance, agent, "Army Kills Agent", string.Concat(new string[]
                                            {
                                                __instance.getName(),
                                                " has inflicted 1 HP damage and thus killed ",
                                                agent.getName(),
                                                ", as they attempt to perform this challenge.\n\nWhile an orc army is at rest, they patrol constantly, attacking any outsiders operating in their home camp, unless they are tolerant towards them, or, in the case of dark agents, including player controlled agents, the army's home camp is >50% shadow. (You CANNOT see this on the map by the red spikes on the army icon when the agent is selected). Note: 100% infiltration will allow your agents to continue to operate normally"
                                            }), UnifiedMessage.messageType.ARMY_BLOCKS, false);
                                        }
                                        agent.die(__instance.map, "Killed by " + __instance.getName(), __instance.person);
                                    }
                                    else
                                    {
                                        if (agent.isCommandable() || agent.person.isWatched())
                                        {
                                            __instance.map.addUnifiedMessage(__instance, agent, "Army Intercepts", string.Concat(new string[]
                                            {
                                                __instance.getName(),
                                                " has blocked ",
                                                agent.getName(),
                                                ", as they are perform this challenge in an orc camp that is not tolerant of them. They have taken 1HP adamge while escaping, and will continue to do so until they leave the location.\n\nWhile an orc army is at rest, they patrol constantly, attacking any outsiders operating in their home camp, unless they are tolerant towards them, or, in the case of dark agents, including player controlled agents, the army's home camp is >50% shadow. (You CANNOT see this on the map by the red spikes on the army icon when the agent is selected). Note: 100% infiltration will allow your agents to continue to operate normally"
                                            }), UnifiedMessage.messageType.ARMY_BLOCKS, false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static double UA_getChallengeUtility_Postfix(double utility, UA __instance, Challenge c, List<ReasonMsg> reasons)
        {
            bool willIntercept = false;

            if (c.location.settlement is Set_OrcCamp camp && camp.army != null && camp.army.location == camp.location && camp.location.soc is SG_Orc orcSociety)
            {
                if (orcSociety != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcSociety))
                {
                    HolyOrder_Orcs orcCulture = ModCore.core.data.orcSGCultureMap[orcSociety];

                    if (orcCulture == null || __instance.society == orcSociety || __instance.society == orcCulture)
                    {
                        return utility;
                    }

                    if ( __instance.homeLocation != -1 && (__instance.map.locations[__instance.homeLocation].soc == orcSociety || __instance.map.locations[__instance.homeLocation].soc == orcCulture))
                    {
                        return utility;
                    }

                    H_Intolerance tolerance = orcCulture.tenets.OfType<H_Intolerance>().FirstOrDefault();
                    if (tolerance != null)
                    {
                        if (__instance.society is SG_Orc || __instance.society is HolyOrder_Orcs)
                        {
                            willIntercept = true;
                        }
                        else if (tolerance.status < 0)
                        {
                            if ((__instance.society == null || !__instance.society.isDark()) && !__instance.isCommandable())
                            {
                                willIntercept = true;
                            }
                        }
                        else if (tolerance.status > 0)
                        {
                            if (__instance.isCommandable() && camp.shadow < 0.5 && camp.infiltration < 1.0)
                            {
                                willIntercept = true;
                            }
                            else if (__instance.society == null)
                            {
                                willIntercept = true;
                            }
                            else if (camp.shadow < 0.5 && __instance.society.isDark())
                            {
                                willIntercept = true;
                            }
                        }
                        else
                        {
                            if ((__instance.society == null || !__instance.society.isDark()) && !__instance.isCommandable())
                            {
                                willIntercept = true;
                            }
                            else if (__instance.isCommandable() && camp.shadow < 0.5 && camp.infiltration < 1.0)
                            {
                                willIntercept = true;
                            }
                            else if (camp.shadow < 0.5 && __instance.society.isDark())
                            {
                                willIntercept = true;
                            }
                        }
                    }
                }

                if (willIntercept)
                {
                    double val = -125;
                    reasons?.Add(new ReasonMsg("Army Blocking me", val));
                    utility += val;
                }
            }

            return utility;
        }

        private static double AIChallenge_checkChallengeUtility_Postfix(double utility, AgentAI.ChallengeData challengeData, UA ua, List<ReasonMsg> reasonMsgs)
        {
            bool willIntercept = false;

            if (challengeData.location.settlement is Set_OrcCamp camp && camp.army != null && camp.army.location == camp.location && camp.location.soc is SG_Orc orcSociety)
            {
                if (orcSociety != null && ModCore.core.data.orcSGCultureMap.ContainsKey(orcSociety))
                {
                    HolyOrder_Orcs orcCulture = ModCore.core.data.orcSGCultureMap[orcSociety];

                    if (orcCulture == null || ua.society == orcSociety || ua.society == orcCulture)
                    {
                        return utility;
                    }

                    if (ua.homeLocation != -1 && (ua.map.locations[ua.homeLocation].soc == orcSociety || ua.map.locations[ua.homeLocation].soc == orcCulture))
                    {
                        return utility;
                    }

                    H_Intolerance tolerance = orcCulture.tenets.OfType<H_Intolerance>().FirstOrDefault();
                    if (tolerance != null)
                    {
                        if (ua.society is SG_Orc || ua.society is HolyOrder_Orcs)
                        {
                            willIntercept = true;
                        }
                        else if (tolerance.status < 0)
                        {
                            if ((ua.society == null || !ua.society.isDark()) && !ua.isCommandable())
                            {
                                willIntercept = true;
                            }
                        }
                        else if (tolerance.status > 0)
                        {
                            if (ua.isCommandable() && camp.shadow < 0.5 && camp.infiltration < 1.0)
                            {
                                willIntercept = true;
                            }
                            else if (ua.society == null)
                            {
                                willIntercept = true;
                            }
                            else if (camp.shadow < 0.5 && ua.society.isDark())
                            {
                                willIntercept = true;
                            }
                        }
                        else
                        {
                            if ((ua.society == null || !ua.society.isDark()) && !ua.isCommandable())
                            {
                                willIntercept = true;
                            }
                            else if (ua.isCommandable() && camp.shadow < 0.5 && camp.infiltration < 1.0)
                            {
                                willIntercept = true;
                            }
                            else if (camp.shadow < 0.5 && ua.society.isDark())
                            {
                                willIntercept = true;
                            }
                        }
                    }
                }

                if (willIntercept)
                {
                    double val = -125;
                    reasonMsgs?.Add(new ReasonMsg("Army Blocking me", val));
                    utility += val;
                }
            }

            return utility;
        }
    }
}