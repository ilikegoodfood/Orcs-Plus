using Assets.Code;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orcs_Plus
{
    public static class HarmonyPatches_Conditional
    {
        private static readonly Type patchType = typeof(HarmonyPatches_Conditional);

        private static Type T_I_BarbDominion = null;

        public static void PatchingInit()
        {
            if (ModCore.core.data.tryGetModAssembly("CovensCursesCurios", out Assembly asmCCC) && asmCCC != null)
            {
                T_I_BarbDominion = asmCCC.GetType("CovenExpansion.I_BarbDominion", false);
                if (T_I_BarbDominion != null)
                {
                    Patching_CovensCursesCurios();
                }
            }
        }

        private static void Patching_CovensCursesCurios()
        {
            Harmony harmony = new Harmony("ILikeGoodFood.SOFG.OrcsPlus");

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
    }
}
