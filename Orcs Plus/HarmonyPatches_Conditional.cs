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

        private static Type T_I_BarbDominion = null;

        private static Type T_Kernel_Cordyceps = null;

        private static Harmony harmony = null;

        public static void PatchingInit()
        {
            string harmonyID = "ILikeGoodFood.SOFG.OrcsPlus_Conditional";
            harmony = new Harmony(harmonyID);

            if (Harmony.HasAnyPatches(harmonyID))
            {
                harmony.UnpatchAll(harmonyID);
            }

            if (ModCore.core.data.tryGetModAssembly("CovensCursesCurios", out ModData.ModIntegrationData intDataCCC) && intDataCCC.assembly != null && intDataCCC.typeDict.TryGetValue("Banner", out T_I_BarbDominion) && T_I_BarbDominion != null)
            {
                Patching_CovensCursesCurios();
            }

            if (ModCore.core.data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null && intDataCord.typeDict.TryGetValue("Kernel", out T_Kernel_Cordyceps) && T_Kernel_Cordyceps != null)
            {
                Patching_Cordyceps();
            }
        }

        private static void Patching_CovensCursesCurios()
        {
            harmony.Patch(original: AccessTools.Constructor(T_I_BarbDominion, new Type[] { typeof(Map) }), postfix: new HarmonyMethod(patchType, nameof(I_BarbDominion_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Subjugate_Orcs), nameof(Ch_Subjugate_Orcs.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Subjugate_Orcs_validFor_Postfix)));

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
                foreach (Item item in ua.person.items)
                {
                    if (item != null && item.GetType() == T_I_BarbDominion)
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private static void Patching_Cordyceps()
        {
            harmony.Patch(original: AccessTools.Method(T_Kernel_Cordyceps, "onTurnEnd", new Type[] { typeof(Map) }), transpiler: new HarmonyMethod(patchType, nameof(Cordyceps_ModCore_onTurnEnd_Transpiler)));

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
        }

        private static bool Cordyceps_ModCore_onTurnEnd_TranspilerBody(HolyOrder order)
        {
            if (order is HolyOrder_Orcs)
            {
                return false;
            }

            return false;
        }
    }
}
