using Assets.Code;
using Assets.Code.Modding;
using HarmonyLib;
using LivingWilds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Orcs_Plus
{
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        public static ModCore mod;

        private static bool patched = false;

        public static void PatchingInit(ModCore core)
        {
            if (patched)
            {
                return;
            }
            else
            {
                patched = true;
            }

            mod = core;

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
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_BuildFortress), name: nameof(Ch_Orcs_BuildFortress.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildFortress_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_BuildFortress), name: nameof(Ch_Orcs_BuildFortress.complete), parameters: new Type[] { typeof(UA) } ), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildFortress_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_BuildMages), name: nameof(Ch_Orcs_BuildMages.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMages_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_BuildMages), name: nameof(Ch_Orcs_BuildMages.complete), parameters: new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMages_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_BuildMenagerie), name: nameof(Ch_Orcs_BuildMenagerie.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMenagerie_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_BuildMenagerie), name: nameof(Ch_Orcs_BuildMenagerie.complete), parameters: new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMenagerie_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_BuildShipyard), name: nameof(Ch_Orcs_BuildShipyard.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildShipyard_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_BuildShipyard), name: nameof(Ch_Orcs_BuildShipyard.complete), parameters: new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildShipyard_complete_Postfix)));

            // Patches for Warlord rituals
            harmony.Patch(original: AccessTools.Method(type: typeof(Rt_Orcs_CommandeerShips), name: nameof(Rt_Orcs_CommandeerShips.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Rt_Orcs_CommandeerShips_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Rt_Orcs_CommandeerShips), name: nameof(Rt_Orcs_CommandeerShips.complete), parameters: new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Rt_Orcs_CommandeerShips_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Rt_Orcs_ClaimTerritory), name: nameof(Rt_Orcs_ClaimTerritory.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Rt_Orcs_ClaimTerritory_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Rt_Orcs_ClaimTerritory), name: nameof(Rt_Orcs_ClaimTerritory.complete), parameters: new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Rt_Orcs_ClaimTerritory_complete_Postfix)));

            // Patches for Challenges in orc camps
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_DevastateOrcishIndustry), name: nameof(Ch_Orcs_DevastateOrcishIndustry.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_DevastateOrcishIndustry_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_DevastateOrcishIndustry), name: nameof(Ch_Orcs_DevastateOrcishIndustry.complete), parameters: new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_DevastateOrcishIndustry_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_Expand), name: nameof(Ch_Orcs_Expand.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_Expand_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_Expand), name: nameof(Ch_Orcs_Expand.complete), parameters: new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_Expand_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_OrcRaiding), name: nameof(Ch_OrcRaiding.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_OrcRaiding_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_OrcRaiding), name: nameof(Ch_OrcRaiding.valid)), transpiler: new HarmonyMethod(patchType, nameof(Ch_OrcRaiding_valid_Transpiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_OrcRaiding), name: nameof(Ch_OrcRaiding.complete), parameters: new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_OrcRaiding_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_RetreatToTheHills), name: nameof(Ch_Orcs_RetreatToTheHills.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_RetreatToTheHills_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Orcs_RetreatToTheHills), name: nameof(Ch_Orcs_RetreatToTheHills.complete), parameters: new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_Orcs_RetreatToTheHills_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Subjugate_Orcs), name: nameof(Ch_Subjugate_Orcs.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Subjugate_Orcs_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Ch_Subjugate_Orcs), name: nameof(Ch_Subjugate_Orcs.complete), parameters: new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Subjugate_Orcs_complete_Postfix)));

            // Patches for UAEN_OrcUpstart
            harmony.Patch(original: AccessTools.Constructor(typeof(UAEN_OrcUpstart), new Type[] { typeof(Location), typeof(SocialGroup), typeof(Person) }), postfix: new HarmonyMethod(patchType, nameof(UAEN_OrcUpstart_ctor_Postfix)));

            // Patches for UA
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getAttackUtility)), postfix: new HarmonyMethod(patchType, nameof(UA_getAttackUtility_Postfix)));

            // Patches for I_HordeBanner
            harmony.Patch(original: AccessTools.Constructor(typeof(I_HordeBanner), new Type[] { typeof(Map), typeof(SG_Orc), typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(I_HordeBanner_ctor_Postfix)));

            // Patches for Set_OrcCamp
            harmony.Patch(original: AccessTools.Constructor(typeof(Set_OrcCamp), new Type[] { typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(Set_OrcCamp_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Set_OrcCamp), nameof(Set_OrcCamp.turnTick)), postfix: new HarmonyMethod(patchType, nameof(Set_OrcCamp_turnTick_Postfix)));

            // Patches for Task_RazeLocation
            harmony.Patch(original: AccessTools.Method(typeof(Task_RazeLocation), nameof(Task_RazeLocation.turnTick)), transpiler: new HarmonyMethod(patchType, nameof(Task_RazeLocation_turnTick_Transpiler)));

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        private static void Ch_Orcs_BuildFortress_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.data.influenceGain[ModData.influenceGainAction.BuildFortress] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildFortress_complete_Postfix(Ch_Orcs_BuildFortress __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_OrcsPlus_Orcs orcCulture = null;

            if (orcs != null && ModCore.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        private static void Ch_Orcs_BuildMages_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.data.influenceGain[ModData.influenceGainAction.BuildMages] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildMages_complete_Postfix(Ch_Orcs_BuildMages __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_OrcsPlus_Orcs orcCulture = null;

            if (orcs != null && ModCore.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        private static void Ch_Orcs_BuildMenagerie_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.data.influenceGain[ModData.influenceGainAction.BuildMenagerie] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildMenagerie_complete_Postfix(Ch_Orcs_BuildMenagerie __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_OrcsPlus_Orcs orcCulture = null;

            if (orcs != null && ModCore.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.BuildMenagerie]), true);
            }
        }

        private static void Ch_Orcs_BuildShipyard_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.data.influenceGain[ModData.influenceGainAction.BuildShipyard] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildShipyard_complete_Postfix(Ch_Orcs_BuildShipyard __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_OrcsPlus_Orcs orcCulture = null;

            if (orcs != null && ModCore.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        private static void Rt_Orcs_CommandeerShips_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.data.influenceGain[ModData.influenceGainAction.CommandeerShips] + " influence with the orc culture by completing this challenge.";
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
            HolyOrder_OrcsPlus_Orcs orcCulture = null;

            if (orcSociety != null && ModCore.data.orcSGCultureMap.ContainsKey(orcSociety) && ModCore.data.orcSGCultureMap[orcSociety] != null)
            {
                orcCulture = ModCore.data.orcSGCultureMap[orcSociety];
            }

            if (orcCulture != null)
            {
                mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(rt.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.BuildFortress]), true);
            }
        }

        private static void Ch_Orcs_DevastateOrcishIndustry_getDesc_Postfix(ref string __result)
        {
            __result += " If completed by a human agent, they gain " + ModCore.data.influenceGain[ModData.influenceGainAction.DevastateIndustry] + " influence with the orc culture.";
        }

        private static void Ch_Orcs_DevastateOrcishIndustry_complete_Postfix(Ch_Orcs_DevastateOrcishIndustry __instance, UA u)
        {
            if (!u.isCommandable() && !u.society.isDark())
            {
                HolyOrder_OrcsPlus_Orcs orcCulture = null;

                if (__instance.orcs != null && ModCore.data.orcSGCultureMap.ContainsKey(__instance.orcs) && ModCore.data.orcSGCultureMap[__instance.orcs] != null)
                {
                    orcCulture = ModCore.data.orcSGCultureMap[__instance.orcs];
                }

                if (orcCulture != null)
                {
                    mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.DevastateIndustry]));
                }
            }
        }

        private static void Ch_Orcs_Expand_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.data.influenceGain[ModData.influenceGainAction.Expand] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_Expand_complete_Postfix(Ch_Orcs_Expand __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_OrcsPlus_Orcs orcCulture = null;

            if (orcs != null && ModCore.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.Expand]), true);
            }
        }

        private static void Ch_OrcRaiding_getDesc_Postfix(Ch_OrcRaiding __instance, ref string __result)
        {
            __result = "Raids the most prosperous neighbouring human settlement, causing <b>devastation</b>, which harms prosperity and food production, and taking " + (int)(100.0 * __instance.map.param.ch_orcRaidingGoldGain) + "% of the settlment's gold reserves. Raiding will not completely destroy a settlement. If the settlement's devastation is too high, the raiding will only return with gold. You gain " + ModCore.data.influenceGain[ModData.influenceGainAction.Raiding] + " influence with the orc culture by completing this challenge.";
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
                HolyOrder_OrcsPlus_Orcs orcCulture = null;
                if (u.isCommandable() && ModCore.data.orcSGCultureMap.ContainsKey(orcSociety) && ModCore.data.orcSGCultureMap[orcSociety] != null)
                {
                    orcCulture = ModCore.data.orcSGCultureMap[orcSociety];
                }

                if (orcCulture != null)
                {
                    mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(ch.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.Raiding]), true);
                }
            }

            return;
        }

        private static void Ch_Orcs_RetreatToTheHills_getDesc_Postfix(ref string __result)
        {
            __result = "Causes half of all orc industry in this orc camp, and neighbouring orc camps, to be turned into defensive positions. Good at quickly reducing threat to avoid war, or surviving one against a powerful foe. Reduces societal menace by " + ModCore.data.menaceGain[ModData.menaceGainAction.Retreat] + ".";
        }

        private static IEnumerable<CodeInstruction> Ch_Orcs_RetreatToTheHills_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.Ch_Orcs_RetreatToTheHills_complete_TranspilerBody), new Type[] { typeof(Ch_Orcs_RetreatToTheHills) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static void Ch_Orcs_RetreatToTheHills_complete_TranspilerBody(Ch_Orcs_RetreatToTheHills ch)
        {
            SG_Orc orcSociety = ch.location.soc as SG_Orc;
            List<UM_OrcArmy> armies = null;

            if (orcSociety == null || !(ch.location.settlement is Set_OrcCamp) || !ch.location.settlement.isInfiltrated)
            {
                return;
            }

            List<Location> targetLocations = new List<Location> { ch.location };
            targetLocations.AddRange(ch.location.getNeighbours());

            foreach (Location targetLocation in targetLocations)
            {
                Set_OrcCamp targetCamp = targetLocation.settlement as Set_OrcCamp;
                if (targetCamp != null && targetLocation.soc == orcSociety)
                {
                    Pr_OrcishIndustry industry = null;
                    Pr_OrcDefences defences = null;
                    foreach (Property property in targetLocation.properties)
                    {
                        if (property is Pr_OrcishIndustry)
                        {
                            industry = property as Pr_OrcishIndustry;
                        }

                        if (property is Pr_OrcDefences)
                        {
                            defences = property as Pr_OrcDefences;
                        }

                        if (industry != null && defences != null)
                        {
                            break;
                        }
                    }

                    if (defences == null)
                    {
                        defences = new Pr_OrcDefences(targetLocation);
                        targetLocation.properties.Add(defences);
                    }

                    if (industry == null)
                    {
                        industry = new Pr_OrcishIndustry(targetLocation);
                        industry.charge = 30.0;
                        targetLocation.properties.Add(industry);
                    }

                    industry.charge /= 2.0;
                    defences.charge += industry.charge * ModCore.data.orcDefenceFactor;
                    targetCamp.defences += industry.charge;
                }
            }

            foreach (Unit unit in ch.map.units)
            {
                UM_OrcArmy army = unit as UM_OrcArmy;
                if (army != null && army.society == orcSociety)
                {
                    armies.Add(army);
                }
            }

            if (armies != null)
            {
                foreach (UM_OrcArmy army in armies)
                {
                    army.updateMaxHP();
                }
            }

            orcSociety.menace += ModCore.data.menaceGain[ModData.menaceGainAction.Retreat];
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
                __result += " You gain " + ModCore.data.influenceGain[ModData.influenceGainAction.Subjugate] + " influence with the orc culture by completing this challenge.";
                return;
            }

            __result += " You gain " + (ModCore.data.influenceGain[ModData.influenceGainAction.Subjugate] * 2) + " influence with the orc culture by completing this challenge.";
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

            HolyOrder_OrcsPlus_Orcs orcCulture = null;
            if (orcs != null && ModCore.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                if (orcCamp.specialism == 0)
                {
                    mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.Subjugate]), true);
                    return;
                }

                mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.Subjugate] * 2), true);
            }
        }

        private static void Rt_Orcs_ClaimTerritory_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.data.influenceGain[ModData.influenceGainAction.Expand] + " influence with the orc culture by completing this challenge.";
        }

        private static void Rt_Orcs_ClaimTerritory_complete_Postfix(Ch_Orcs_BuildFortress __instance, UA u)
        {
            if (u == null || !u.isCommandable())
            {
                return;
            }

            SG_Orc orcs = u.location.soc as SG_Orc;
            HolyOrder_OrcsPlus_Orcs orcCulture = null;

            if (orcs != null && ModCore.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.data.orcSGCultureMap[orcs] != null)
            {
                orcCulture = ModCore.data.orcSGCultureMap[orcs];
            }

            if (orcCulture != null)
            {
                mod.TryAddInfluenceGain(orcCulture, new ReasonMsg(__instance.getName(), ModCore.data.influenceGain[ModData.influenceGainAction.Expand]), true);
            }
        }

        private static void UAEN_OrcUpstart_ctor_Postfix(UAEN_OrcUpstart __instance)
        {
            if (__instance.getStatCommand() < 3)
            {
                __instance.person.stat_command = 3;
            }

            I_HordeBanner banner = __instance.person.items.OfType<I_HordeBanner>().FirstOrDefault();
            if (banner != null)
            {
                Rti_OrcsPlus_RecruitWarband recruit = banner.rituals.OfType<Rti_OrcsPlus_RecruitWarband>().FirstOrDefault();
                recruit?.complete(__instance);
            }
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
            UA target = other as UA;
            SG_Orc orcs = upstart.society as SG_Orc;
            if (target != null && orcs != null && target.society != orcs)
            {
                utility = 0.0;
                HolyOrder_OrcsPlus_Orcs orcCulture = null;
                if (ModCore.data.orcSGCultureMap.ContainsKey(orcs) && ModCore.data.orcSGCultureMap[orcs] != null)
                {
                    orcCulture = ModCore.data.orcSGCultureMap[orcs];
                }
                if (orcCulture != null && target.society != orcCulture)
                {
                    H_OrcsPlus_Intolerance intolerance = orcCulture.tenets.OfType<H_OrcsPlus_Intolerance>().FirstOrDefault();
                    if (intolerance != null)
                    {
                        bool otherIsTarget = false;
                        if (target.society != null)
                        {
                            if (target.society != orcs && target.society != orcCulture)
                            {
                                if (upstart.map.locations[target.homeLocation].soc != orcs || upstart.map.locations[target.homeLocation].soc != orcCulture)
                                {
                                    if (intolerance.status == 0)
                                    {
                                        otherIsTarget = true;
                                    }
                                    else if (intolerance.status > 0)
                                    {
                                        if (upstart.society.getRel(target.society).state == DipRel.dipState.war || target.society.isDark() || target.isCommandable())
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
                                        if (upstart.society.getRel(target.society).state == DipRel.dipState.war || target.society is SG_Orc || target.society is HolyOrder_OrcsPlus_Orcs || !target.society.isDark() || !target.isCommandable())
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
                                I_HordeBanner banner = item as I_HordeBanner;
                                if (banner?.orcs == orcs && !(target.society != null && upstart.society.getRel(target.society).state == DipRel.dipState.war))
                                {
                                    reasonMsgs?.Add(new ReasonMsg("Orcs will not attack banner bearer", -10000.0));
                                    utility -= 10000.0;
                                    return utility;
                                }
                            }

                            if (target.location.soc == orcs || (target.society != null && orcs.getRel(target.society).state == DipRel.dipState.war && (target.location.soc == null || orcs.getRel(target.location.soc).state == DipRel.dipState.war)))
                            {
                                double val;
                                if (target.location.soc != orcs)
                                {
                                    val = -25;
                                    reasonMsgs?.Add(new ReasonMsg("Target is outside of " + orcs.getName() + "'s territory", val));
                                    utility += val;
                                }
                                else if (target.task is Task_PerformChallenge)
                                {
                                    val = 20;
                                    reasonMsgs?.Add(new ReasonMsg("Agent is interfereing with " + orcs.getName() + "'s territory", val));
                                    utility += val;
                                }

                                utility += upstart.person.getTagUtility(new int[]
                                {
                                Tags.COMBAT,
                                Tags.CRUEL,
                                Tags.DANGER
                                }, new int[0], reasonMsgs);
                                utility += upstart.person.getTagUtility(target.getNegativeTags(), target.getPositiveTags(), reasonMsgs);

                                val = 20;
                                reasonMsgs?.Add(new ReasonMsg("Eager for Combat", val));
                                utility += val;

                                if (target is UAEN_Vampire)
                                {
                                    val = -30;
                                    reasonMsgs?.Add(new ReasonMsg("Fear of Vampires", val));
                                    utility += val;
                                }

                                if (ModCore.modLivingWilds)
                                {
                                    Assembly asm = Assembly.Load("LivingWilds");
                                    if (asm != null)
                                    {
                                        Type t = asm.GetType("LivingWilds.UAEN_Nature_Critter");
                                        if (t != null && target.GetType().IsSubclassOf(t))
                                        {
                                            val = -30;
                                            reasonMsgs?.Add(new ReasonMsg("Nature", val));
                                            utility += val;
                                        }
                                    }
                                }

                                val = upstart.map.getStepDist(target.location, upstart.location);
                                reasonMsgs?.Add(new ReasonMsg("Distance", -val));
                                utility -= val;

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

                                if (target.society != null && orcs.getRel(target.society).state == DipRel.dipState.war)
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
                                if (target.society != null && orcs.getRel(target.society).state == DipRel.dipState.war)
                                {
                                    reasonMsgs?.Add(new ReasonMsg("Tresspassing in territory of " + target.location.soc.getName(), -10000.0));
                                    utility -= 10000.0;
                                }
                                else
                                {
                                    reasonMsgs?.Add(new ReasonMsg("Target is outside of " + orcs.getName() + "'s territory", -10000.0));
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
                if (target == null)
                {
                    reasonMsgs?.Add(new ReasonMsg("Target is not Agent", -10000.0));
                    utility -= 10000.0;
                }
                else if (orcs == null)
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

        private static void I_HordeBanner_ctor_Postfix(I_HordeBanner __instance, Location l)
        {
            __instance.rituals.Add(new Rti_OrcsPlus_RecruitWarband(l, __instance));
        }

        private static void Set_OrcCamp_ctor_Postfix(Set_OrcCamp __instance)
        {
            __instance.customChallenges.Add(new Ch_OrcsPlus_GatherHorde(__instance.location));
            __instance.customChallenges.Add(new Ch_OrcsPlus_FundHorde(__instance.location, __instance));
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
                        mod.TryAddInfluenceGain(orcs, new ReasonMsg("Razing Orc Camp", ModCore.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                    }
                    else if (!u.society.isDark())
                    {
                        mod.TryAddInfluenceGain(orcs, new ReasonMsg("Razing Orc Camp", ModCore.data.influenceGain[ModData.influenceGainAction.RazingLocation]));
                    }
                }
            }

            if (set != null && u.isCommandable())
            {
                List<SG_Orc> orcSocieties = ModCore.data.getOrcSocieties(u.map);

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
                            mod.TryAddInfluenceGain(orcs, new ReasonMsg("Razing Emeny Settlement", ModCore.data.influenceGain[ModData.influenceGainAction.RazingLocation]), true);
                        }
                    }
                }
            }
        }
    }
}