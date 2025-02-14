using Assets.Code;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Orcs_Plus
{
    public static class HarmonyPatches_Conditional
    {
        private static readonly Type patchType = typeof(HarmonyPatches_Conditional);

        private static Harmony harmony = null;

        public static void PatchingInit()
        {
            string harmonyID = "ILikeGoodFood.SOFG.OrcsPlus_Conditional";
            harmony = new Harmony(harmonyID);

            Harmony.DEBUG = false;

            if (Harmony.HasAnyPatches(harmonyID))
            {
                harmony.UnpatchAll(harmonyID);
            }

            Patching_CovensCursesCurios();
            Patching_Cordyceps();
            Patching_Escamrak();
        }

        private static void Patching_CovensCursesCurios()
        {
            ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC);
            ModCore.Get().data.tryGetModIntegrationData("CovensCursesCuriosRecast", out ModIntegrationData intDataCCCR);

            if (intDataCCC != null || intDataCCCR != null)
            {
                harmony.Patch(original: AccessTools.Method(typeof(Ch_Subjugate_Orcs), nameof(Ch_Subjugate_Orcs.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Subjugate_Orcs_validFor_Postfix)));

                if (intDataCCC != null && intDataCCC.typeDict.TryGetValue("Banner", out Type T_I_BarbDominion))
                {
                    harmony.Patch(original: AccessTools.Constructor(T_I_BarbDominion, new Type[] { typeof(Map) }), postfix: new HarmonyMethod(patchType, nameof(I_BarbDominion_ctor_Postfix)));
                }

                if (intDataCCCR != null && intDataCCCR.typeDict.TryGetValue("Banner", out Type T_I_BarbDominion2))
                {
                    harmony.Patch(original: AccessTools.Constructor(T_I_BarbDominion2, new Type[] { typeof(Map) }), postfix: new HarmonyMethod(patchType, nameof(I_BarbDominion_ctor_Postfix)));
                }
            }

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        private static void I_BarbDominion_ctor_Postfix(Item __instance)
        {
            __instance.challenges.Add(new Rti_RecruitWarband(__instance.map.locations[0], __instance));
            __instance.challenges.Add(new Rti_RouseHorde(__instance.map.locations[0], __instance));
        }

        private static bool Ch_Subjugate_Orcs_validFor_Postfix(bool result, UA ua)
        {
            if (!result)
            {
                List<Type> dominionBannerTypes = new List<Type>();
                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.typeDict.TryGetValue("Banner", out Type dominionBanner))
                {
                    dominionBannerTypes.Add(dominionBanner);
                }
                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCCR) && intDataCCCR.typeDict.TryGetValue("Banner", out Type dominionBanner2))
                {
                    dominionBannerTypes.Add(dominionBanner2);
                }

                foreach (Item item in ua.person.items)
                {
                    if (item != null)
                    {
                        if (dominionBannerTypes.Any(t => item.GetType() == t || item.GetType().IsSubclassOf(t)))
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private static void Patching_Cordyceps()
        {
            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.typeDict.TryGetValue("Kernel", out Type T_Kernel_Cordyceps))
            {
                harmony.Patch(original: AccessTools.Method(T_Kernel_Cordyceps, "onTurnEnd", new Type[] { typeof(Map) }), transpiler: new HarmonyMethod(patchType, nameof(Cordyceps_ModCore_onTurnEnd_Transpiler)));
            }

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        private static IEnumerable<CodeInstruction> Cordyceps_ModCore_onTurnEnd_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Cordyceps_ModCore_onTurnEnd_TranspilerBody), new Type[] { typeof(HolyOrder) });

            Label skip = ilg.DefineLabel();

            int targetIndex = 0;

            for (int i = instructionList.Count - 1; i >= 0; i--)
            {
                if (instructionList[i].opcode == OpCodes.Brfalse_S)
                {
                    targetIndex = i;
                    break;
                }
            }

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (i == targetIndex)
                {
                    skip = (Label)instructionList[i].operand;

                    yield return new CodeInstruction(OpCodes.Brfalse_S, skip);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 26);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);

                    targetIndex = 0;
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed Cordyceps_ModCore_onTurnEnd_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool Cordyceps_ModCore_onTurnEnd_TranspilerBody(HolyOrder order)
        {
            if (order is HolyOrder_Orcs)
            {
                return false;
            }

            return true;
        }

        private static void Patching_Escamrak()
        {
            if (ModCore.Get().data.tryGetModIntegrationData("Escamrak", out ModIntegrationData intDataEscam) && intDataEscam.typeDict.TryGetValue("LivingTerrain", out Type T_LivingTerrain) && intDataEscam.methodInfoDict.TryGetValue("LivingTerrain_TurnTick", out MethodInfo MI_LivingTerrain_turnTick))
            {
                harmony.Patch(original: MI_LivingTerrain_turnTick, transpiler: new HarmonyMethod(patchType, nameof(Escamrak_Pr_EscamLandCorruption_turnTick_Transpiler)));
            }

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        private static IEnumerable<CodeInstruction> Escamrak_Pr_EscamLandCorruption_turnTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Escamrak_Pr_EscamLandCorruption_turnTick_TranspilerBody), new Type[] { typeof(Property) });

            Label skipLabel = ilg.DefineLabel();

            int targetIndex = 1;
            for(int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex < 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Isinst && instructionList[i-1].opcode == OpCodes.Ldfld && instructionList[i+1].opcode == OpCodes.Ldnull)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S)
                        {
                            targetIndex++;

                            skipLabel = (Label)instructionList[i].operand;
                        }
                    }
                    else if (targetIndex == 5)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Nop);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, skipLabel);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed Escamrak_Pr_EscamLandCorruption_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool Escamrak_Pr_EscamLandCorruption_turnTick_TranspilerBody(Property pr)
        {
            if (pr.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture))
            {
                if (orcCulture.tenet_god is H_Orcs_Fleshweaving && orcCulture.tenet_god.status < -1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
