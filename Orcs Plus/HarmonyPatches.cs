using Assets.Code;
using Assets.Code.Modding;
using CommunityLib;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using UnityEngine;

namespace Orcs_Plus
{
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        public static void PatchingInit()
        {
            Patching();
        }

        private static void Patching()
        {
            Harmony.DEBUG = false;
            string harmonyID = "ILikeGoodFood.SOFG.OrcsPlus";
            Harmony harmony = new Harmony(harmonyID);

            if (Harmony.HasAnyPatches(harmonyID))
            {
                return;
            }

            // Patches for Challenges that specialise orc camps
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildFortress), nameof(Ch_Orcs_BuildFortress.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildFortress_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildFortress), nameof(Ch_Orcs_BuildFortress.buildNegativeTags)), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_Gold)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMages), nameof(Ch_Orcs_BuildMages.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMages_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMages), nameof(Ch_Orcs_BuildMages.buildNegativeTags)), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_Gold)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMenagerie), nameof(Ch_Orcs_BuildMenagerie.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMenagerie_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMenagerie), nameof(Ch_Orcs_BuildMenagerie.buildNegativeTags)), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_Gold)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildShipyard), nameof(Ch_Orcs_BuildShipyard.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildShipyard_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildShipyard), nameof(Ch_Orcs_BuildShipyard.buildNegativeTags)), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_Gold)));

            // Patches for Warlord rituals
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_CommandeerShips), nameof(Rt_Orcs_CommandeerShips.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Rt_Orcs_CommandeerShips_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_CommandeerShips), nameof(Rt_Orcs_CommandeerShips.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Rt_Orcs_CommandeerShips_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_ClaimTerritory), nameof(Rt_Orcs_ClaimTerritory.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Rt_Orcs_ClaimTerritory_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_ClaimTerritory), nameof(Rt_Orcs_ClaimTerritory.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Rt_Orcs_ClaimTerritory_validFor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_ClaimTerritory), nameof(Rt_Orcs_ClaimTerritory.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Rt_Orcs_ClaimTerritory_complete_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_RecruitRaiders), nameof(Rt_Orcs_RecruitRaiders.complete), new Type[] { typeof(UA) }), prefix: new HarmonyMethod(patchType, nameof(Rt_Orcs_RecruitRaiders_complete_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orc_ReceiveFunding), nameof(Rt_Orc_ReceiveFunding.buildPositiveTags), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_Orc)));

            // Patches for Challenges in orc camps
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_ChallengeTheHorde), nameof(Ch_Orcs_ChallengeTheHorde.getRestriction),Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_ChallengeTheHorde_getRestriction_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_ChallengeTheHorde), nameof(Ch_Orcs_ChallengeTheHorde.valid), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_ChallengeTheHorde_valid_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_ChallengeTheHorde), nameof(Ch_Orcs_ChallengeTheHorde.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_ChallengeTheHorde_validFor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_ChallengeTheHorde), nameof(Ch_Orcs_ChallengeTheHorde.buildPositiveTags), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_AmbitionOrc)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_DevastateOrcishIndustry), nameof(Ch_Orcs_DevastateOrcishIndustry.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_DevastateOrcishIndustry_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_Expand), nameof(Ch_Orcs_Expand.getName), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_Expand_getName_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_Expand), nameof(Ch_Orcs_Expand.getDesc), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_Expand_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_Expand), nameof(Ch_Orcs_Expand.getRestriction), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_Expand_getRestriction_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_Expand), nameof(Ch_Orcs_Expand.valid), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_Expand_valid_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_Expand), nameof(Ch_Orcs_Expand.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_Orcs_Expand_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_OrcRaiding), nameof(Ch_OrcRaiding.getName), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_OrcRaiding_getName_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_OrcRaiding), nameof(Ch_OrcRaiding.getDesc), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_OrcRaiding_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_OrcRaiding), nameof(Ch_OrcRaiding.valid), Type.EmptyTypes), transpiler: new HarmonyMethod(patchType, nameof(Ch_OrcRaiding_valid_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_OrcRaiding), nameof(Ch_OrcRaiding.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_OrcRaiding_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_OrcRaiding), nameof(Ch_OrcRaiding.buildPositiveTags), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_AmbitionOrc)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_RetreatToTheHills), nameof(Ch_Orcs_RetreatToTheHills.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_RetreatToTheHills_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_RetreatToTheHills), nameof(Ch_Orcs_RetreatToTheHills.valid)), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_RetreatToTheHills_valid_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_RetreatToTheHills), nameof(Ch_Orcs_RetreatToTheHills.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_RetreatToTheHills_validFor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_RetreatToTheHills), nameof(Ch_Orcs_RetreatToTheHills.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_Orcs_RetreatToTheHills_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Subjugate_Orcs), nameof(Ch_Subjugate_Orcs.getDesc)), postfix: new HarmonyMethod(patchType, nameof(Ch_Subjugate_Orcs_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Subjugate_Orcs), nameof(Ch_Subjugate_Orcs.complete), new Type[] { typeof(UA) }));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_OrganiseTheHorde), nameof(Ch_Orcs_OrganiseTheHorde.getDesc), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_OrganiseTheHorde_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_OrganiseTheHorde), nameof(Ch_Orcs_OrganiseTheHorde.valid), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_OrganiseTheHorde_valid_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_OrganiseTheHorde), nameof(Ch_Orcs_OrganiseTheHorde.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_OrganiseTheHorde_validFor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_OpportunisticEncroachment), nameof(Ch_Orcs_OpportunisticEncroachment.getName), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_OpportunisticEncroachment_getName_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_OpportunisticEncroachment), nameof(Ch_Orcs_OpportunisticEncroachment.getDesc), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_OpportunisticEncroachment_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_OpportunisticEncroachment), nameof(Ch_Orcs_OpportunisticEncroachment.valid), Type.EmptyTypes), transpiler: new HarmonyMethod(patchType, nameof(Ch_Orcs_OpportunisticEncroachment_valid_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_OpportunisticEncroachment), nameof(Ch_Orcs_OpportunisticEncroachment.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_Orcs_OpportunisticEncroachment_complete_Transpiler)));

            // Patches for challenges in Pr_OrcPlunder
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_AccessPlunder), nameof(Ch_Orcs_AccessPlunder.getDesc), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_AccessPlunder_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_AccessPlunder), nameof(Ch_Orcs_AccessPlunder.valid), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_AccessPlunder_valid_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_AccessPlunder), nameof(Ch_Orcs_AccessPlunder.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_AccessPlunder_validFor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_AccessPlunder), nameof(Ch_Orcs_AccessPlunder.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_Orcs_AccessPlunder_complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_AccessPlunder), nameof(Ch_Orcs_AccessPlunder.buildNegativeTags), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_Orc)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_StealPlunder), nameof(Ch_Orcs_StealPlunder.getInherentDanger), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_StealPlunder_getInherentDanger_Postfix)));

            // Patches for I_HordeBanner
            harmony.Patch(original: AccessTools.Constructor(typeof(I_HordeBanner), new Type[] { typeof(Map), typeof(SG_Orc), typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(I_HordeBanner_ctor_Postfix)));

            // Patches for challenges in I_HordeBanner
            harmony.Patch(original: AccessTools.Method(typeof(Rti_Orc_AttackHere), nameof(Rti_Orc_AttackHere.buildPositiveTags), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_Orc)));
            harmony.Patch(original: AccessTools.Method(typeof(Rti_Orc_CeaseWar), nameof(Rti_Orc_CeaseWar.buildPositiveTags), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_Orc)));
            harmony.Patch(original: AccessTools.Method(typeof(Rti_Orc_UniteTheHordes), nameof(Rti_Orc_UniteTheHordes.buildPositiveTags), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(postfix_AppendTag_Orc)));
            harmony.Patch(original: AccessTools.Method(typeof(Rti_Orc_UniteTheHordes), nameof(Rti_Orc_UniteTheHordes.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Rti_Orc_UniteTheHordes_complete_Transpiler)));

            // Patch and Branches for getPostiveTags and getNegative Tags
            harmony.Patch(original: AccessTools.Method(typeof(Challenge), nameof(Challenge.getPositiveTags), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Challenge_getPositiveTags_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Challenge), nameof(Challenge.getNegativeTags), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Challenge_getNegativeTags_Postfix)));

            // Patches for UAEN_OrcUpstart
            harmony.Patch(original: AccessTools.Constructor(typeof(UAEN_OrcUpstart), new Type[] { typeof(Location), typeof(SocialGroup), typeof(Person) }), postfix: new HarmonyMethod(patchType, nameof(UAEN_OrcUpstart_ctor_Postfix)));

            // Patches for Unit
            harmony.Patch(original: AccessTools.Method(typeof(Unit), nameof(Unit.hostileTo), new Type[] { typeof(Unit), typeof(bool) }), postfix: new HarmonyMethod(patchType, nameof(Unit_hostileTo_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_OrcUpstart), nameof(UAEN_OrcUpstart.turnTick), new Type[] { typeof(Map) }), postfix: new HarmonyMethod(patchType, nameof(UAEN_OrcsUpstart_turnTick_Postfix)));

            // Patches for UA
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getAttackUtility), new Type[] { typeof(Unit), typeof(List<ReasonMsg>), typeof(bool) }), postfix: new HarmonyMethod(patchType, nameof(UA_getAttackUtility_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getVisibleUnits), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(UA_getVisibleUnits_Postfix)));

            // Patches for SocialGroup
            harmony.Patch(original: AccessTools.Method(typeof(SocialGroup), nameof(SocialGroup.checkIsGone), new Type[] { }), postfix: new HarmonyMethod(patchType, nameof(SocialGroup_checkIsGone_Postfix)));

            // Patches for ManagerMajorThreats
            harmony.Patch(original: AccessTools.Method(typeof(ManagerMajorThreats), nameof(ManagerMajorThreats.turnTick), Type.EmptyTypes), transpiler: new HarmonyMethod(patchType, nameof(ManagerMajorThreats_turnTick_Transpiler)));

            // Patches for SG_Orc
            harmony.Patch(original: AccessTools.Constructor(typeof(SG_Orc), new Type[] { typeof(Map), typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(SG_Orc_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(SG_Orc), nameof(SG_Orc.canSettle), new Type[] { typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(SG_Orc_canSettle_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(SG_Orc), nameof(SG_Orc.getName), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(SG_Orc_getName_Postfix)));

            // Patches for MA_Orc_Expand
            harmony.Patch(original: AccessTools.Method(typeof(MA_Orc_Expand), nameof(MA_Orc_Expand.getUtility), new Type[] { typeof(List<ReasonMsg>) }), postfix: new HarmonyMethod(patchType, nameof(MA_Orcs_Expand_getUtility_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(MA_Orc_Expand), nameof(MA_Orc_Expand.complete), Type.EmptyTypes), transpiler: new HarmonyMethod(patchType, nameof(MA_Orc_Expand_complete_Transpiler)));

            // Patches for Set_OrcCamp
            harmony.Patch(original: AccessTools.Constructor(typeof(Set_OrcCamp), new Type[] { typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(Set_OrcCamp_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Set_OrcCamp), nameof(Set_OrcCamp.turnTick)), postfix: new HarmonyMethod(patchType, nameof(Set_OrcCamp_turnTick_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Set_OrcCamp), nameof(Set_OrcCamp.getMaxDefence)), postfix: new HarmonyMethod(patchType, nameof(Set_OrcCamp_getMaxDefence_Postfix)));

            // Patches for Pr_OrcDefences
            harmony.Patch(original: AccessTools.Method(typeof(Pr_OrcDefences), nameof(Pr_OrcDefences.turnTick), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Pr_OrcDefences_turnTick_Postfix)));

            // Patches for Pr_OrcishIndustry
            harmony.Patch(original: AccessTools.Method(typeof(Pr_OrcishIndustry), nameof(Pr_OrcishIndustry.turnTick), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(Pr_OrcishIndustry_turnTick_Postfix)));

            // Patches for Pr_OrcFunbding
            harmony.Patch(original: AccessTools.Method(typeof(Pr_OrcFunding), nameof(Pr_OrcFunding.turnTick), Type.EmptyTypes), transpiler: new HarmonyMethod(patchType, nameof(Pr_OrcFunding_turnTick_Transpiler)));

            // Patches for UM_OrcArmy
            harmony.Patch(original: AccessTools.Constructor(typeof(UM_OrcArmy), new Type[] { typeof(Location), typeof(SocialGroup), typeof(Set_OrcCamp) }), postfix: new HarmonyMethod(patchType, nameof(UM_OrcArmy_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UM_OrcArmy), nameof(UM_OrcArmy.getName), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(UM_OrcArmy_getName_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UM_OrcArmy), nameof(UM_OrcArmy.turnTickInner), new Type[] { typeof(Map) }), postfix: new HarmonyMethod(patchType, nameof(UM_OrcArmy_turnTickInner_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UM_OrcArmy), nameof(UM_OrcArmy.updateMaxHP), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(UM_OrcArmy_updateMaxHP_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UM_OrcArmy), nameof(UM_OrcArmy.turnTickAI), Type.EmptyTypes), transpiler: new HarmonyMethod(patchType, nameof(UM_OrcArmy_turnTickAI_Transpiler)));

            // Patches for UM_OrcRaiders
            harmony.Patch(original: AccessTools.Constructor(typeof(UM_OrcRaiders), new Type[] { typeof(Location), typeof(SocialGroup) }), postfix: new HarmonyMethod(patchType, nameof(UM_OrcRaiders_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UM_OrcRaiders), nameof(UM_OrcRaiders.assignMaxHP), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(UM_OrcRaiders_assignMaxHP_Postfix)));

            // Pathes for UM_UntamedDead
            harmony.Patch(original: AccessTools.Method(typeof(UM_UntamedDead), nameof(UM_UntamedDead.turnTickInner), new Type[] { typeof(Map) }), postfix: new HarmonyMethod(patchType, nameof(UM_UntamedDead_turnTickInner_Postfix)));

            // Patches getChallengeUtility
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getChallengeUtility), new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) }), postfix: new HarmonyMethod(patchType, nameof(UA_getChallengeUtility_Postfix)));

            // Patches for Ch_DrinkPrimalWaters
            harmony.Patch(original: AccessTools.Method(typeof(Ch_DrinkPrimalWaters), nameof(Ch_DrinkPrimalWaters.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_DrinkPrimalWaters_validFor_Postfix)));

            // Patches for Ch_LearnSecret
            harmony.Patch(original: AccessTools.Method(typeof(Ch_LearnSecret), nameof(Ch_LearnSecret.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_LearnSecret_validFor_Postfix)));

            // Patches for RT_StudyDeath
            harmony.Patch(original: AccessTools.Method(typeof(Rt_StudyDeath), nameof(Rt_StudyDeath.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Rt_StudyDeath_Postfix)));

            // Patches for P_Opha_Crusade
            harmony.Patch(original: AccessTools.Method(typeof(P_Opha_Crusade), nameof(P_Opha_Crusade.getDesc), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(P_Opha_Crusade_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Opha_Crusade), nameof(P_Opha_Crusade.cast), new Type[] { typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(P_Opha_Crusade_cast_Postfix)));

            // Patches for P_Opha_Empower
            harmony.Patch(original: AccessTools.Method(typeof(P_Opha_Empower), nameof(P_Opha_Empower.getDesc), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(P_Opha_Empower_getDesc_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Opha_Empower), nameof(P_Opha_Empower.getRestrictionText), Type.EmptyTypes), postfix: new HarmonyMethod(patchType, nameof(P_Opha_Empower_getRestrictionText_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Opha_Empower), nameof(P_Opha_Empower.validTarget), new Type[] { typeof(Unit) }), postfix: new HarmonyMethod(patchType, nameof(P_Opha_Empower_validTarget_Postfix)));

            // Patches for PC_Card
            harmony.Patch(original: AccessTools.Method(typeof(PC_Card), nameof(PC_Card.cast), new Type[] { typeof(Unit) }), postfix: new HarmonyMethod(patchType, nameof(PC_Card_cast_Postfix)), transpiler: new HarmonyMethod(patchType, nameof(PC_Card_cast_Transpiler)));

            // Patches for T_Et_Sword
            harmony.Patch(original: AccessTools.Method(typeof(T_Et_Sword), nameof(T_Et_Sword.onKill), new Type[] { typeof(Person) }), postfix: new HarmonyMethod(patchType, nameof(T_Et_Sword_onKill_Postfix)));

            // Community Library Patches
            harmony.Patch(original: AccessTools.Method(typeof(CommunityLib.HarmonyPatches), "Rt_Orcs_ClaimTerritory_validFor_TranspilerBody", new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Rt_Orcs_ClaimTerritory_validFor_TranspilerBody_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_RecoverShipwreck), nameof(Ch_RecoverShipwreck.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_RecoverShipwreck_complete_Transpiler)));

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        private static void Ch_Orcs_BuildFortress_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildFortress] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildMages_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildMages] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildMenagerie_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildMenagerie] + " influence with the orc culture by completing this challenge.";
        }

        private static void Ch_Orcs_BuildShipyard_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildShipyard] + " influence with the orc culture by completing this challenge.";
        }

        private static int[] postfix_AppendTag_Gold(int[] result, Challenge __instance)
        {
            //Console.WriteLine("OrcsPlus: Added GOLD to negative tags of " + __instance.getName());
            int[] output = new int[result.Length + 1];
            for (int i = 0; i < result.Length; i++)
            {
                output[i] = result[i];
            }
            output[result.Length] = Tags.GOLD;

            return output;
        }

        private static void Rt_Orcs_CommandeerShips_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.CommandeerShips] + " influence with the orc culture by completing this challenge.";
        }

        private static IEnumerable<CodeInstruction> Rt_Orcs_CommandeerShips_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Rt_Orcs_CommandeerShips_complete_TranspilerBody), new Type[] { typeof(Rt_Orcs_CommandeerShips), typeof(UA), typeof(Set_OrcCamp) });

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Stfld && instructionList[i - 1].opcode == OpCodes.Ldc_I4_5)
                        {
                            targetIndex = 0;

                            yield return instructionList[i];
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldloc_1);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);

                            i++;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed Rt_Orcs_CommandeerShips_complete_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void Rt_Orcs_CommandeerShips_complete_TranspilerBody(Rt_Orcs_CommandeerShips rt, UA u, Set_OrcCamp targetShipyard)
        {
            if (u.isCommandable() && targetShipyard.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(rt.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildShipyard]), true);
            }
        }

        // Challenges in Orc Camps
        private static void Ch_Orcs_ChallengeTheHorde_getRestriction_Postfix(ref string __result)
        {
            __result = "The orc horde cannot have a live orc upstart";
        }

        private static bool Ch_Orcs_ChallengeTheHorde_valid_Postfix(bool _, Ch_Orcs_ChallengeTheHorde __instance)
        {
            SG_Orc orcSociety = __instance.location.soc as SG_Orc;
            if (orcSociety != null)
            {
                if (__instance.map.units.Any(u => u is UAEN_OrcUpstart && u.society == orcSociety))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private static bool Ch_Orcs_ChallengeTheHorde_validFor_Postfix(bool _, UA ua)
        {
            return ua.isCommandable();
        }

        private static int[] postfix_AppendTag_AmbitionOrc(int[] result)
        {
            int[] output = new int[result.Length + 2];

            for (int i = 0; i < result.Length; i++)
            {
                output[i] = result[i];
            }
            output[result.Length] = Tags.ORC;
            output[result.Length + 1] = Tags.AMBITION;

            return output;
        }

        private static void Ch_Orcs_DevastateOrcishIndustry_getDesc_Postfix(ref string __result)
        {
            __result += " If completed by a human agent, they gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.DevastateIndustry] + " influence with the orc culture.";
        }

        private static string Ch_Orcs_Expand_getName_Postfix(string result, Ch_Orcs_Expand __instance)
        {
            bool ambigous = false;
            SG_Orc orcSocity = __instance.location.soc as SG_Orc;

            if (__instance.location.settlement != null && __instance.location.settlement.subs.Count > 0)
            {
                List<Sub_OrcWaystation> waystations = __instance.location.settlement.subs.OfType<Sub_OrcWaystation>().ToList();
                if (waystations.Count > 0)
                {
                    Sub_OrcWaystation waystation = waystations.FirstOrDefault(sub => sub.getChallenges().Contains(__instance));
                    if (waystation != null)
                    {
                        orcSocity = waystation.orcSociety;
                    }

                    if (waystations.Count > 1)
                    {
                        ambigous = true;
                    }
                }

            }

            if (ambigous && orcSocity != null)
            {
                return "Cause " + orcSocity.getName() + " Expansion";
            }

            return result;
        }

        private static string Ch_Orcs_Expand_getDesc_Postfix(string result, Ch_Orcs_Expand __instance)
        {
            if (__instance.location.settlement != null && __instance.location.settlement.subs.Count > 0)
            {
                Sub_OrcWaystation waystation = (Sub_OrcWaystation)__instance.location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(__instance));
                if (waystation != null)
                {
                    result = "Causes this " + waystation.orcSociety.getName() + " waystation to spawn a new already-infiltrated orc camp in a neighbouring usable location. The new camp will have low industry, but increase over time. Moving closer to human socities can cause wars.";
                }
            }

            result += " You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.Expand] + " influence with the orc culture by completing this challenge.";

            return result;
        }

        private static string Ch_Orcs_Expand_getRestriction_Postfix(string __result, Ch_Orcs_Expand __instance)
        {
            return "Requires an infiltrated orc camp, or an orc waystation, with an empty neighbouring location with habilitability > " + ((int)(100.0 * __instance.map.opt_orcHabMult * __instance.map.param.orc_habRequirement)).ToString() + "%";
        }

        private static bool Ch_Orcs_Expand_valid_Postfix(bool result, Ch_Orcs_Expand __instance)
        {
            if (__instance.location.settlement != null)
            {
                Sub_OrcWaystation waystation = (Sub_OrcWaystation)__instance.location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(__instance));
                if (waystation != null)
                {
                    result = __instance.location.getNeighbours().Any(l => waystation.orcSociety.canSettle(l));
                }
            }

            return result;
        }

        private static IEnumerable<CodeInstruction> Ch_Orcs_Expand_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            MethodInfo MI_TranspilerBody_GetOrcs = AccessTools.Method(patchType, nameof(Ch_Orcs_Expand_complete_TranspilerBody_GetOrcs));
            MethodInfo MI_TranspilerBody_RemoveWaystations = AccessTools.Method(patchType, nameof(Ch_Orcs_Expand_complete_TranspilerBody_RemoveWaystations));
            MethodInfo MI_TranspilerBody_InfluenceGain = AccessTools.Method(patchType, nameof(Ch_Orcs_Expand_complete_TranspilerBody_InfluenceGain));

            MethodInfo MI_fallIntoRuin = AccessTools.Method(typeof(Settlement), nameof(Settlement.fallIntoRuin), new Type[] { typeof(string), typeof(object) });

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            Label orcsLabel = ilg.DefineLabel();
            Label nullSettlementLabel = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Stloc_0)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Pop);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_GetOrcs);
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i + 1].opcode == OpCodes.Nop && instructionList[i + 2].opcode == OpCodes.Nop)
                        {
                            targetIndex++;
                            nullSettlementLabel = (Label)instructionList[i].operand;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Ldstr, "Overrun by Orcs");
                            yield return new CodeInstruction(OpCodes.Ldnull);
                            yield return new CodeInstruction(OpCodes.Ldloc, 10);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_fallIntoRuin);
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i].labels.Contains(nullSettlementLabel))
                        {
                            targetIndex = 0;

                            CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_0);
                            code.labels.AddRange(instructionList[i].labels);
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_InfluenceGain);
                            yield return new CodeInstruction(OpCodes.Ldloc_3);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_RemoveWaystations);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed Ch_Orcs_Expand_complete_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static SG_Orc Ch_Orcs_Expand_complete_TranspilerBody_GetOrcs(Ch_Orcs_Expand ch)
        {
            SG_Orc result = ch.location.soc as SG_Orc;

            if (ch.location.settlement != null && ch.location.settlement.subs.Count > 0)
            {
                Sub_OrcWaystation waystation = (Sub_OrcWaystation)ch.location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(ch));
                if (waystation != null)
                {
                    result = waystation.orcSociety;
                }
            }

            return result;
        }

        private static void Ch_Orcs_Expand_complete_TranspilerBody_RemoveWaystations(Location location)
        {
            if (location.settlement != null)
            {
                List<Sub_OrcWaystation> waystations = location.settlement.subs.OfType<Sub_OrcWaystation>().ToList();
                foreach(Sub_OrcWaystation waystation in waystations)
                {
                    location.settlement.subs.Remove(waystation);
                }
            }
        }

        private static void Ch_Orcs_Expand_complete_TranspilerBody_InfluenceGain(Ch_Orcs_Expand ch, UA ua, SG_Orc orcSociety)
        {
            if (orcSociety != null && ua.isCommandable())
            {
                ModCore.Get().TryAddInfluenceGain(orcSociety, new ReasonMsg(ch.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.Expand]), true);
            }
        }

        private static string Ch_OrcRaiding_getName_Postfix(string result, Ch_Orcs_Expand __instance)
        {
            bool ambigous = false;
            SG_Orc orcSocity = __instance.location.soc as SG_Orc;

            if (__instance.location.settlement != null && __instance.location.settlement.subs.Count > 0)
            {
                List<Sub_OrcWaystation> waystations = __instance.location.settlement.subs.OfType<Sub_OrcWaystation>().ToList();
                if (waystations.Count > 0)
                {
                    Sub_OrcWaystation waystation = waystations.FirstOrDefault(sub => sub.getChallenges().Contains(__instance));
                    if (waystation != null)
                    {
                        orcSocity = waystation.orcSociety;
                    }

                    if (waystations.Count > 1)
                    {
                        ambigous = true;
                    }
                }

            }

            if (ambigous && orcSocity != null)
            {
                return orcSocity.getName() + " Raiding";
            }

            return result;
        }

        private static void Ch_OrcRaiding_getDesc_Postfix(Ch_OrcRaiding __instance, ref string __result)
        {
            __result = "Raids the most prosperous neighbouring human settlement, causing <b>devastation</b>, which harms prosperity and food production, and taking " + (int)(100.0 * __instance.map.param.ch_orcRaidingGoldGain) + "% of the settlment's gold reserves. Raiding will not completely destroy a settlement. If the settlement's devastation is too high, the raiding will only return with gold. You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.Raiding] + " influence with the orc culture by completing this challenge.";
        }

        private static IEnumerable<CodeInstruction> Ch_OrcRaiding_valid_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.Ch_OrcRaiding_valid_TranspilerBody), new Type[] { typeof(Ch_OrcRaiding) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("OrcsPlus: Completed complete function replacement transpiler Ch_OrcRaiding_getDesc_Postfix");
        }

        private static bool Ch_OrcRaiding_valid_TranspilerBody(Ch_OrcRaiding ch)
        {
            SG_Orc orcSociety = ch.location.soc as SG_Orc;

            if (ch.location.settlement != null && ch.location.settlement.subs.Count > 0)
            {
                Sub_OrcWaystation waystation = (Sub_OrcWaystation)ch.location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(ch));
                if (waystation != null)
                {
                    orcSociety = waystation.orcSociety;
                }
            }

            foreach (Location neighbour in ch.location.getNeighbours())
            {
                if (neighbour.soc != null && neighbour.settlement is SettlementHuman && ModCore.Get().isHostileAlignment(orcSociety, neighbour))
                {
                    Pr_Devastation devastation = ch.location.properties.OfType<Pr_Devastation>().FirstOrDefault();
                    if (devastation == null || devastation.charge < 150)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static IEnumerable<CodeInstruction> Ch_OrcRaiding_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Ch_OrcRaiding_complete_TranspilerBody), new Type[] { typeof(Ch_OrcRaiding), typeof(UA) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("OrcsPlus: Completed complete function replacement transpiler Ch_OrcRaiding_complete_Transpiler");
        }

        private static void Ch_OrcRaiding_complete_TranspilerBody(Ch_OrcRaiding ch, UA u)
        {
            Map map = u.map;
            Location location = ch.location;

            SG_Orc orcSociety = location.soc as SG_Orc;
            if (location.settlement != null && location.settlement.subs.Count > 0)
            {
                //Console.WriteLine("OrcsPlus: Orc Society is null. Get orc Society from Waystation");
                Sub_OrcWaystation waystation = (Sub_OrcWaystation)location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(ch));
                if (waystation != null)
                {
                    //Console.WriteLine("OrcsPlus: Got orc Society from Waystation");
                    orcSociety = waystation.orcSociety;
                }
            }

            if (orcSociety == null)
            {
                //Console.WriteLine("OrcsPlus: Orc Society is null.");
                return;
            }

            //Console.WriteLine("OrcsPlus: Running Burn The Fields Complete");
            SettlementHuman target = null;
            double prosperity = 0.0;
            double prosperityAlt = 0.0;
            List<SettlementHuman> targets = new List<SettlementHuman>();
            List<SettlementHuman> targetsAlt = new List<SettlementHuman>();

            //Console.WriteLine("OrcsPlus: Finding target location.");
            foreach (Location neighbour in location.getNeighbours())
            {
                if (neighbour.soc != null && neighbour.settlement is SettlementHuman settlementHuman && ModCore.Get().isHostileAlignment(orcSociety, neighbour))
                {
                    //Console.WriteLine("OrcsPlus: Neighbour has social group and settlement.");

                    Pr_Devastation devastation = settlementHuman.location.properties.OfType<Pr_Devastation>().FirstOrDefault();
                    if (devastation == null || devastation.charge < 150)
                    {
                        if (settlementHuman.prosperity >= prosperity)
                        {
                            if (settlementHuman.prosperity > prosperity)
                            {
                                targets.Clear();
                            }

                            targets.Add(settlementHuman);
                            prosperity = settlementHuman.prosperity;
                        }
                    }
                    else
                    {
                        if (settlementHuman.prosperity >= prosperityAlt)
                        {
                            if (settlementHuman.prosperity > prosperityAlt)
                            {
                                targetsAlt.Clear();
                            }

                            targetsAlt.Add(settlementHuman);
                            prosperityAlt = settlementHuman.prosperity;
                        }
                    }
                }
            }

            bool targetIsAlt = false;
            if (targets.Count == 1)
            {
                target = targets[0];
            }
            else if (targets.Count > 1)
            {
                target = targets[Eleven.random.Next(targets.Count)];
            }
            else
            {
                targetIsAlt = true;
                if (targetsAlt.Count == 1)
                {
                    target = targetsAlt[0];
                }
                else if (targetsAlt.Count > 1)
                {
                    target = targetsAlt[Eleven.random.Next(targetsAlt.Count)];
                }
            }

            if (target != null)
            {
                int gold = (int)Math.Ceiling(target.prosperity * 100);

                if (target.ruler != null)
                {
                    gold += (int)Math.Ceiling(target.ruler.gold * map.param.ch_orcRaidingGoldGain);
                    target.ruler.gold -= (int)Math.Floor(target.ruler.gold * map.param.ch_orcRaidingGoldGain);
                }

                u.person.addGold(gold);

                if (!targetIsAlt)
                {
                    Property.addToPropertySingleShot("Orc Raid", Property.standardProperties.DEVASTATION, 100, target.location);

                    ch.msgString = "The orcs devestate the lands around " + target.getName() + ", causing 100% devastation, and taking loot worth " + gold + " gold. The orcs have gained " + map.param.ch_orcishRaidingMenaceGain + " menace as a society as a result of this action.";
                    orcSociety.menace += map.param.ch_orcishRaidingMenaceGain;
                }
                else
                {
                    ch.msgString = "The orcs raided the lands around " + target.getName() + ", causing no devastation, but taking loot worth " + gold + " gold. The orcs have gained " + map.param.ch_orcishRaidingMenaceGain / 2 + " menace as a society as a result of this action.";
                    orcSociety.menace += map.param.ch_orcishRaidingMenaceGain / 2;
                }

                if (u.isCommandable() && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
                {
                    ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(ch.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.Raiding]), true);
                }
            }

            return;
        }

        private static int[] Ch_OrcRaiding_buildPositiveTags_Postfix(int[] result)
        {
            int[] output = new int[result.Length + 1];

            for (int i = 0; i < result.Length; i++)
            {
                output[i] = result[i];
            }
            output[result.Length] = Tags.ORC;

            return output;
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
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Ch_Orcs_RetreatToTheHills_complete_TranspilerBody), new Type[] { typeof(Ch_Orcs_RetreatToTheHills), typeof(UA) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("OrcsPlus: Completed complete function replacement transpiler Ch_Orcs_RetreatToTheHills_complete_Transpiler");
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
                __result += " You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.Subjugate] + " influence with the orc culture by completing this challenge.";
                return;
            }

            __result += " You gain " + (ModCore.Get().data.influenceGain[ModData.influenceGainAction.Subjugate] * 2) + " influence with the orc culture by completing this challenge.";
        }

        public static void Ch_Orcs_AccessPlunder_getDesc_Postfix(Ch_Orcs_AccessPlunder __instance, ref string __result)
        {
            SG_Orc orcSociety = __instance.location.soc as SG_Orc;
            HolyOrder_Orcs orcCulture = __instance.location.soc as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            int maxDraw = 0;
            if (orcCulture != null)
            {
                maxDraw = orcCulture.influenceElder * 2;
            }

            __result = "Opens the trade screen with the orc plunder, allowing you to take or add gold and items from it. When taking gold from the horde, you can only take gold equal to twice the influence you have with that culture (" + maxDraw + ").";
        }

        private static bool Ch_Orcs_AccessPlunder_valid_Postfix(bool result, Ch_Orcs_AccessPlunder __instance)
        {
            return __instance.location.settlement is Set_OrcCamp && __instance.cache.charge > 0.0 && __instance.cache.gold > 0;
        }

        private static bool Ch_Orcs_AccessPlunder_validFor_Postfix(bool result, Ch_Orcs_AccessPlunder __instance, UA ua)
        {
            result = false;

            if (ua.isCommandable() && __instance.location.settlement?.infiltration == 1.0)
            {
                result = true;
            }
            else if (!ua.isCommandable() && ((ua.society is SG_Orc && __instance.location.soc == ua.society) || (ua.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety != null && (orcCulture.orcSociety == __instance.location.soc || orcCulture == __instance.location.soc))))
            {
                result = true;
            }

            return result;
        }

        private static IEnumerable<CodeInstruction> Ch_Orcs_AccessPlunder_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Ch_Orcs_AccessPlunder_complete_TranspilerBody), new Type[] { typeof(Ch_Orcs_AccessPlunder), typeof(UA) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("OrcsPlus: Completed complete function replacemnt transpiler Ch_Orcs_AccessPlunder_complete_Transpiler");
        }

        private static void Ch_Orcs_AccessPlunder_complete_TranspilerBody(Ch_Orcs_AccessPlunder ch, UA ua)
        {
            if (ch.cache.charge == 0.0 || ch.cache.gold == 0)
            {
                ch.map.world.prefabStore.popMsg("This cache was empty for too long and is gone, you must form a new one.", false, false);
                Pr_OrcPlunder plunder = ch.location.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();
                if (plunder != null)
                {
                    ch.location.properties.Remove(plunder);
                }
                return;
            }

            if (ua.isCommandable())
            {
                SG_Orc orcSociety = ua.location.soc as SG_Orc;
                HolyOrder_Orcs orcCulture = ua.location.soc as HolyOrder_Orcs;

                if (orcSociety != null)
                {
                    ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
                }

                if (ch.location.settlement != null && ch.location.settlement.infiltration == 1.0 && orcCulture != null)
                {
                    double initGold = ch.cache.gold;

                    if (!ch.map.automatic)
                    {
                        ch.map.world.prefabStore.popItemTrade(ua.person, new ItemFromOrcPlunder(ch.map, ch.cache, ua.person), "Access Plunder", orcCulture.influenceElder * 2, -1);
                    }
                    else
                    {
                        ua.person.gold += ch.cache.gold;
                        ch.cache.gold = 0;

                        for (int i = 0; i < ch.cache.items.Length; i++)
                        {
                            if (ch.cache.items[i] != null)
                            {
                                ua.person.gainItem(ch.cache.items[i], false);
                                ch.cache.items[i] = null;
                            }
                        }

                        ch.location.properties.Remove(ch.cache);
                    }
                }
            }
            else if ((ua.society is SG_Orc && ch.location.soc == ua.society) || (ua.society is HolyOrder_Orcs orcCulture && orcCulture.orcSociety != null && (orcCulture.orcSociety == ch.location.soc || orcCulture == ch.location.soc)))
            {
                int delta = 0;
                if (ch.cache.gold <= 50)
                {
                    delta = ch.cache.gold;
                }
                else if (ch.cache.gold / 2.0 <= 50.0)
                {
                    delta = 50;
                }
                else
                {
                    delta = ch.cache.gold / 2;
                }

                ua.person.gold += delta;
                ch.cache.gold -= delta;

                bool empty = true;
                foreach (Item item in ch.cache.items)
                {
                    if (item != null)
                    {
                        empty = false;
                        break;
                    }
                }

                if (ch.cache.gold <= 0)
                {
                    ch.cache.gold = 0;

                    if (empty)
                    {
                        ch.location.properties.Remove(ch.cache);
                    }
                }
            }
            else
            {
                ua.person.gold += ch.cache.gold;

                if (ModCore.Get().data.orcSGCultureMap.TryGetValue(ch.location.soc as SG_Orc, out HolyOrder_Orcs orcCulture2) && ua.society != null && !ua.society.isDark())
                {
                    ModCore.Get().TryAddInfluenceGain(orcCulture2, new ReasonMsg("Plunder Looted", -ch.cache.gold / 2));
                }

                ch.cache.gold = 0;

                for (int i = 0; i < ch.cache.items.Length; i++)
                {
                    if (ch.cache.items[i] != null)
                    {
                        ua.person.gainItem(ch.cache.items[i], false);
                        ch.cache.items[i] = null;
                    }
                }

                ch.map.addUnifiedMessage(ua, null, "Cache Looted", ua.getName() + " has looted the orc plunder at " + ua.location.getName(true) + " and taken or destroyed all the items therein", UnifiedMessage.messageType.CACHE_GONE, false);
                ch.location.properties.Remove(ch.cache);
            }
        }

        private static void Ch_Orcs_StealPlunder_getInherentDanger_Postfix(Ch_Orcs_StealPlunder __instance, ref int __result)
        {
            if (__instance.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                switch (orcCulture.tenet_intolerance.status)
                {
                    case 2:
                        __result /= 4;
                        break;
                    case 1:
                        __result /= 2;
                        break;
                    case -1:
                        __result *= 2;
                        break;
                    case -2:
                        __result *= 3;
                        break;
                    default:
                        break;
                }
            }
        }

        // Patches for I_HordeBanner
        private static void I_HordeBanner_ctor_Postfix(I_HordeBanner __instance, Location l)
        {
            __instance.rituals.Add(new Rti_RecruitWarband(l, __instance));
            __instance.rituals.Add(new Rti_RouseHorde(l, __instance));
        }

        // Patches for Challenges in I_HordeBanner
        private static int[] postfix_AppendTag_Orc(int[] __result)
        {
            int[] output = new int[__result.Length + 1];

            for (int i = 0; i < __result.Length; i++)
            {
                output[i] = __result[i];
            }
            output[__result.Length] = Tags.ORC;

            return output;
        }

        private static IEnumerable<CodeInstruction> Rti_Orc_UniteTheHordes_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Rti_Orc_UniteTheHordes_complete_TranspilerBody), new Type[] { typeof(Rti_Orc_UniteTheHordes), typeof(UA) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("OrcsPlus: Completed complete function replacement transpiler Rti_Orc_UniteTheHordes_complete_Transpiler");
        }

        private static int[] Challenge_getPositiveTags_Postfix(int[] __result, Challenge __instance)
        {
            return GetTags.getPositiveTags(__result, __instance);
        }

        private static int[] Challenge_getNegativeTags_Postfix(int[] __result, Challenge __instance)
        {
            return GetTags.getNegativeTags(__result, __instance);
        }

        private static string Ch_Orcs_OrganiseTheHorde_getDesc_Postfix(string result)
        {
            return result + " You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.Raiding] + " influence with the orc culture by completing this challenge.";
        }

        private static bool Ch_Orcs_OrganiseTheHorde_valid_Postfix(bool result, Ch_Orcs_OrganiseTheHorde __instance)
        {
            Pr_OrcishIndustry industry = __instance.location.properties.OfType<Pr_OrcishIndustry>().FirstOrDefault();
            return __instance.location.settlement is Set_OrcCamp && (__instance.location.soc is SG_Orc || __instance.location.soc is HolyOrder_Orcs) && (industry == null || industry.charge < __instance.map.param.ch_orcs_organisethehorde_parameterValue1);
        }

        private static bool Ch_Orcs_OrganiseTheHorde_validFor_Postfix(bool result, Ch_Orcs_OrganiseTheHorde __instance, UA ua)
        {
            if (ua is UAEN_OrcElder elder)
            {
                if (elder.society is HolyOrder_Orcs orcCulture && __instance.location.soc == orcCulture.orcSociety && orcCulture.tenet_industrious.status < 0)
                {
                    return true;
                }
            }
            else if (__instance.location.settlement.infiltration == 1.0)
            {
                return true;
            }

            return false;
        }

        private static string Ch_Orcs_OpportunisticEncroachment_getName_Postfix(string result, Ch_Orcs_Expand __instance)
        {
            bool ambigous = false;
            SG_Orc orcSocity = __instance.location.soc as SG_Orc;

            if (__instance.location.settlement != null && __instance.location.settlement.subs.Count > 0)
            {
                List<Sub_OrcWaystation> waystations = __instance.location.settlement.subs.OfType<Sub_OrcWaystation>().ToList();
                if (waystations.Count > 0)
                {
                    Sub_OrcWaystation waystation = waystations.FirstOrDefault(sub => sub.getChallenges().Contains(__instance));
                    if (waystation != null)
                    {
                        orcSocity = waystation.orcSociety;
                    }

                    if (waystations.Count > 1)
                    {
                        ambigous = true;
                    }
                }

            }

            if (ambigous && orcSocity != null)
            {
                return orcSocity.getName() + " Encroachment";
            }

            return result;
        }

        private static void Ch_Orcs_OpportunisticEncroachment_getDesc_Postfix(Ch_Subjugate_Orcs __instance, ref string __result)
        {
            __result += " You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.Expand] + " influence with the orc culture by completing this challenge.";
        }

        private static IEnumerable<CodeInstruction> Ch_Orcs_OpportunisticEncroachment_valid_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Ch_Orcs_OpportunisticEncroachment_TranspilerBody_GetOrcs), new Type[] { typeof(Ch_Orcs_OpportunisticEncroachment) });

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Stloc_0)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Pop);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed Ch_Orcs_OpportunisticEncroachment_valid_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> Ch_Orcs_OpportunisticEncroachment_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Ch_Orcs_OpportunisticEncroachment_TranspilerBody_GetOrcs), new Type[] { typeof(Ch_Orcs_OpportunisticEncroachment) });

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Stloc_0)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Pop);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed Ch_Orcs_OpportunisticEncroachment_complete_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static SG_Orc Ch_Orcs_OpportunisticEncroachment_TranspilerBody_GetOrcs(Ch_Orcs_OpportunisticEncroachment ch)
        {
            SG_Orc result = ch.location.soc as SG_Orc;

            if (ch.location.settlement != null && ch.location.settlement.subs.Count > 0)
            {
                Sub_OrcWaystation waystation = (Sub_OrcWaystation)ch.location.settlement.subs.FirstOrDefault(sub => sub is Sub_OrcWaystation w && w.getChallenges().Contains(ch));
                if (waystation != null)
                {
                    result = waystation.orcSociety;
                }
            }

            return result;
        }

        private static void Rt_Orcs_ClaimTerritory_getDesc_Postfix(ref string __result)
        {
            __result += " You gain " + ModCore.Get().data.influenceGain[ModData.influenceGainAction.Expand] + " influence with the orc culture by completing this challenge.";
        }

        public static void Rt_Orcs_ClaimTerritory_validFor_Postfix(ref bool __result, UA ua)
        {
            if (!__result)
            {
                if (!ua.location.isOcean && ua.location.hex.getHabilitability() >= ua.location.map.opt_orcHabMult * ua.location.map.param.orc_habRequirement)
                {
                    if (ModCore.Get().data.tryGetModIntegrationData("Escamrak", out ModIntegrationData intDataEscam) && intDataEscam.typeDict.TryGetValue("LivingTerrainSettlement", out Type livingTerrainSettlementType) && intDataEscam.fieldInfoDict.TryGetValue("LivingTerrainSettlement_TypeOfTerrain", out FieldInfo FI_TypeOfTerrain))
                    {
                        if (ua.society is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture.tenet_god is H_Orcs_Fleshweaving fleshweaving && fleshweaving.status < -1)
                        {
                            if (ua.location.settlement.GetType() == livingTerrainSettlementType && (int)FI_TypeOfTerrain.GetValue(ua.location.settlement) == 0)
                            {
                                __result = true;
                            }
                        }
                    }
                }
            }
        }

        public static void Rt_Orcs_ClaimTerritory_complete_Postfix(UA u)
        {
            Set_OrcCamp camp = u.location.settlement as Set_OrcCamp;

            if (camp != null)
            {
                List<Sub_OrcWaystation> waystations = camp.subs.OfType<Sub_OrcWaystation>().ToList();
                foreach (Sub_OrcWaystation waystation in waystations)
                {
                    camp.subs.Remove(waystation);
                }
            }
        }

        public static bool Rt_Orcs_RecruitRaiders_complete_Prefix(UA u)
        {
            SG_Orc orcSociety = u.society as SG_Orc;
            if (orcSociety != null && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_Perfection perfection && perfection.status < -1 && orcCulture.ophanim_PerfectSociety)
            {
                Pr_Ophanim_Perfection perfectionLocal = u.location.properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
                if (perfectionLocal != null && perfectionLocal.charge >= 300.0)
                {
                    UM_PerfectRaiders raiders = new UM_PerfectRaiders(u.location, u.society);
                    raiders.subsumedUnit = u;
                    u.person.unit = raiders;
                    u.isDead = true;
                    GraphicalMap.selectedUnit = raiders;
                    u.map.units.Remove(u);
                    u.location.units.Remove(u);

                    u.map.units.Add(raiders);
                    raiders.location.units.Add(raiders);
                    raiders.person = u.person;
                    raiders.assignMaxHP();
                    raiders.hp = Math.Max(1, raiders.maxHp / 3);
                    u.map.world.ui.checkData();

                    return true;
                }
            }

            return false;
        }

        // Patches for Orc Upstart
        private static void UAEN_OrcUpstart_ctor_Postfix(UAEN_OrcUpstart __instance)
        {
            double roll = Eleven.random.NextDouble();
            if (roll < 0.7)
            {
                __instance.person.stat_command = 2;
            }
            __instance.person.stat_might--;

            __instance.rituals.Add(new Rt_Orcs_Confinement(__instance.location));
            __instance.rituals.Add(new Rt_Orcs_ReclaimHordeBanner(__instance.location));

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
            if (__instance is UM_OrcArmy orcArmy && other is UA target)
            {
                //Console.WriteLine("OrcsPlus: __instance is UM_OrcArmy and other is UA");
                SG_Orc orcSociety = __instance.society as SG_Orc;
                HolyOrder_Orcs orcCulture = __instance.society as HolyOrder_Orcs;

                if (orcSociety != null)
                {
                    ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
                }
                else if (orcCulture != null)
                {
                    orcSociety = orcCulture.orcSociety;
                }

                //Console.WriteLine("OrcsPlus: Got orcSociety");
                if (orcSociety != null && !orcSociety.isGone() && orcCulture != null)
                {
                    //Console.WriteLine("OrcsPlus: Got orcCulture");

                    H_Orcs_Intolerance intolerance = orcCulture.tenet_intolerance;
                    if (intolerance != null)
                    {
                        bool isBloodFeud = target.person.traits.Any(t => t is T_BloodFeud f && f.orcSociety == orcSociety);
                        if (isBloodFeud)
                        {
                            result = true;
                        }
                        else if (target.society != null)
                        {
                            if (target.society != orcSociety && target.society != orcCulture)
                            {
                                if (target.society.getRel(orcSociety).state == DipRel.dipState.war)
                                {
                                    if (target.isCommandable() && intolerance.status < 0)
                                    {
                                        return false;
                                    }

                                    result = true;
                                }
                                else if (target.homeLocation == -1 || (__instance.map.locations[target.homeLocation].soc != orcSociety && orcArmy.map.locations[target.homeLocation].soc != orcCulture))
                                {
                                    if (intolerance.status == 0)
                                    {
                                        result = true;
                                    }
                                    else if (intolerance.status > 0)
                                    {
                                        if (orcSociety.getRel(target.society).state == DipRel.dipState.war || target.society.isDark() || target.isCommandable())
                                        {
                                            result = true;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    else if (intolerance.status < 0)
                                    {
                                        if (!target.isCommandable() && (orcSociety.getRel(target.society).state == DipRel.dipState.war || target.society is SG_Orc || target.society is HolyOrder_Orcs || !target.society.isDark()))
                                        {
                                            result = true;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (target.isCommandable() && intolerance.status < 0)
                            {
                                return false;
                            }
                            else
                            {
                                result = true;
                            }
                        }

                        if (result)
                        {
                            Type dominionBanner = null;
                            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
                            {
                                intDataCCC.typeDict.TryGetValue("Banner", out dominionBanner);
                            }

                            foreach (Item item in target.person.items)
                            {
                                if (item != null)
                                {
                                    if (dominionBanner != null && (item.GetType() == dominionBanner || item.GetType().IsSubclassOf(dominionBanner)))
                                    {
                                        return false;
                                    }
                                    else
                                    {
                                        I_HordeBanner banner = item as I_HordeBanner;
                                        if (banner != null && banner.orcs == orcSociety && (target.society == null || __instance.society.getRel(target.society).state != DipRel.dipState.war) && !isBloodFeud)
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }

                            if (ModCore.Get().checkIsVampire(target))
                            {
                                return false;
                            }

                            if (ModCore.Get().data.tryGetModIntegrationData("LivingWilds", out ModIntegrationData intDataLW) && intDataLW.typeDict.TryGetValue("NatureCritter", out Type natureCritterType))
                            {
                                if (target.GetType().IsSubclassOf(natureCritterType))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            //Console.WriteLine("OrcsPlus: returning result: " + result.ToString());
            return result;
        }

        private static void UAEN_OrcsUpstart_turnTick_Postfix(UAEN_OrcUpstart __instance, Map map)
        {
            if (!map.burnInComplete)
            {
                __instance.person.XP = Math.Max(0, __instance.person.XP - 5);
            }

            SG_Orc orcSociety = __instance.society as SG_Orc;
            if (orcSociety != null && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture))
            {
                if (orcCulture.tenet_god is H_Orcs_Fleshweaving fleshweaving)
                {
                    if (ModCore.Get().data.tryGetModIntegrationData("Escamrak", out ModIntegrationData intDataEscam))
                    {
                        if (intDataEscam.typeDict.TryGetValue("FleshStatBonusTrait", out Type fleshStatBonusType) && intDataEscam.constructorInfoDict.TryGetValue("FleshStatBonusTrait", out ConstructorInfo ci) && intDataEscam.fieldInfoDict.TryGetValue("FleshStatBonusTrait_BonusType", out FieldInfo FI_BonusType))
                        {
                            Trait fleshStatBonusTrait = __instance.person.traits.FirstOrDefault(t => t.GetType() == fleshStatBonusType || t.GetType().IsSubclassOf(fleshStatBonusType));
                            if (fleshweaving.status >= 0 || __instance.isCommandable())
                            {
                                if (fleshStatBonusTrait != null)
                                {
                                    __instance.person.traits.Remove(fleshStatBonusTrait);
                                }
                            }
                            else
                            {
                                if (fleshStatBonusTrait == null)
                                {
                                    fleshStatBonusTrait = (Trait)ci.Invoke(new object[0]);
                                    __instance.person.receiveTrait(fleshStatBonusTrait);
                                    FI_BonusType.SetValue(fleshStatBonusTrait, "Might");
                                }

                                fleshStatBonusTrait.level = -fleshweaving.status;
                            }
                        }
                    }
                }
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

        public static double UAEN_OrcUpstart_getAttackUtility(double utility, UAEN_OrcUpstart ua, Unit other, List<ReasonMsg> reasonMsgs, bool includeDangerousFoe)
        {
            utility = 0.0;

            SG_Orc orcSociety = ua.society as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcSociety != null && !orcSociety.isGone() && orcCulture != null)
            {
                if (other is UA target)
                {
                    H_Orcs_Intolerance intolerance = orcCulture.tenet_intolerance;
                    if (intolerance != null)
                    {
                        bool otherIsTarget = false;

                        T_BloodFeud feud = (T_BloodFeud)target.person.traits.FirstOrDefault(t => t is T_BloodFeud f && f.orcSociety == orcSociety);
                        if (feud != null)
                        {
                            otherIsTarget = true;
                        }
                        else if (target.society != null)
                        {
                            if (target.society != orcSociety && target.society != orcCulture)
                            {
                                if (target.homeLocation == -1 || (ua.map.locations[target.homeLocation].soc != orcSociety && ua.map.locations[target.homeLocation].soc != orcCulture))
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
                            Type dominionBanner = null;
                            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
                            {
                                intDataCCC.typeDict.TryGetValue("Banner", out dominionBanner);
                            }

                            foreach (Item item in target.person.items)
                            {
                                if (item != null)
                                {
                                    if (dominionBanner != null && (item.GetType() == dominionBanner || item.GetType().IsSubclassOf(dominionBanner)))
                                    {
                                        reasonMsgs?.Add(new ReasonMsg("Orcs will not attack banner bearer", -10000.0));
                                        utility -= 10000.0;
                                        return utility;
                                    }
                                    else
                                    {
                                        I_HordeBanner banner = item as I_HordeBanner;
                                        if (banner?.orcs == orcSociety && (target.society == null || ua.society.getRel(target.society).state != DipRel.dipState.war) && (feud == null))
                                        {
                                            reasonMsgs?.Add(new ReasonMsg("Orcs will not attack banner bearer", -10000.0));
                                            utility -= 10000.0;
                                            return utility;
                                        }
                                    }
                                }
                            }

                            if (target.location.soc == orcSociety || (((target.society != null && orcSociety.getRel(target.society).state == DipRel.dipState.war) || feud != null) && (target.location.soc == null || orcSociety.getRel(target.location.soc).state == DipRel.dipState.war)))
                            {
                                double val = 20;
                                reasonMsgs?.Add(new ReasonMsg("Eager for Combat", val));
                                utility += val;

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
                                else if (target.task is Task_GoToPerformChallenge task2 && !(task2.challenge is Ritual) && (task2.challenge.location.soc == orcSociety || task2.challenge.location.soc == orcCulture))
                                {
                                    val = 20;
                                    reasonMsgs?.Add(new ReasonMsg("Agent is travelling to interfere with " + orcSociety.getName() + "'s territory", val));
                                    utility += val;

                                    if (task2.challenge is Ch_Orcs_StealPlunder)
                                    {
                                        val = 20;
                                        reasonMsgs?.Add(new ReasonMsg("Agent is travelling to steal gold from " + orcSociety.getName() + "'s plunder", val));
                                        utility += val;
                                    }
                                }

                                utility += ua.person.getTagUtility(new int[]
                                {
                                    Tags.COMBAT,
                                    Tags.CRUEL,
                                    Tags.DANGER
                                }, new int[0], reasonMsgs);
                                utility += ua.person.getTagUtility(target.getPositiveTags(), target.getNegativeTags(), reasonMsgs);

                                if (orcCulture.tenet_god is H_Orcs_GlorySeeker glory2 && glory2.status < 0)
                                {
                                    val = 10;
                                    reasonMsgs?.Add(new ReasonMsg("Glory Seeker", val));
                                    utility += val;
                                }
                                else if (target.person.hasSoul && orcCulture.tenet_god is H_Orcs_BloodOffering blood && blood.status < 0)
                                {
                                    val = 20;
                                    reasonMsgs?.Add(new ReasonMsg("Blood for the Blood God", val));
                                    utility += val;
                                }

                                if (ModCore.Get().checkIsVampire(target))
                                {
                                    val = -35;
                                    reasonMsgs?.Add(new ReasonMsg("Fear of Vampires", val));
                                    utility += val;
                                }

                                if (ModCore.Get().data.tryGetModIntegrationData("LivingWilds", out ModIntegrationData intDataLW) && intDataLW.typeDict.TryGetValue("NatureCritter", out Type natureCritterType))
                                {
                                    if (target.GetType().IsSubclassOf(natureCritterType))
                                    {
                                        val = -35;
                                        reasonMsgs?.Add(new ReasonMsg("Nature", val));
                                        utility += val;
                                    }
                                }

                                if (includeDangerousFoe)
                                {
                                    val = 0.0;
                                    double dangerUtility = target.getDangerEstimate() - ua.getDangerEstimate();
                                    if (dangerUtility > 0.0)
                                    {
                                        val -= ua.map.param.utility_ua_attackDangerReluctanceOffset;
                                    }
                                    dangerUtility += 2;
                                    if (dangerUtility > 0.0)
                                    {
                                        val += ua.map.param.utility_ua_attackDangerReluctanceMultiplier;
                                    }
                                    reasonMsgs?.Add(new ReasonMsg("Dangerous Foe", val));
                                    utility += val;
                                }

                                if (feud != null)
                                {
                                    val = 50;
                                    reasonMsgs?.Add(new ReasonMsg("Blood Feud", val));
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
                                    utility += holyTenet.addUtilityAttack(ua, target, reasonMsgs);
                                }
                                foreach (ModKernel modKernel in ua.map.mods)
                                {
                                    utility = modKernel.unitAgentAIAttack(ua.map, ua, target, reasonMsgs, utility);
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
                    reasonMsgs?.Add(new ReasonMsg("Target is not Agent", -10000.0));
                    utility -= 10000.0;
                }

            }
            else
            {
                if (orcSociety == null)
                {
                    reasonMsgs?.Add(new ReasonMsg("ERROR: Agent is not of Orc Social Group", -10000.0));
                    utility -= 10000.0;
                }
                else if (orcCulture == null)
                {
                    reasonMsgs?.Add(new ReasonMsg("ERROR: Failed to find orc culture", -10000.0));
                    utility -= 10000.0;
                }
            }

            return utility;
        }

        private static List<Unit> UA_getVisibleUnits_Postfix(List<Unit> units, UA __instance)
        {
            if (__instance is UAEN_OrcUpstart)
            {
                return UAEN_Orc_getVisibleUnits(units, __instance);
            }

            return units;
        }

        private static List<Unit> UAEN_Orc_getVisibleUnits(List<Unit> units, UA ua)
        {
            if (units == null)
            {
                units = new List<Unit>();
            }

            SG_Orc orcSociety = ua.society as SG_Orc;
            HolyOrder_Orcs orcCulture = ua.society as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }
            else if (orcCulture != null)
            {
                orcSociety = orcCulture.orcSociety;
            }


            if (orcSociety != null && !orcSociety.isGone() && orcCulture != null)
            {
                Type dominionBanner = null;
                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
                {
                    intDataCCC.typeDict.TryGetValue("Banner", out dominionBanner);
                }

                foreach (Unit unit in ua.map.units)
                {
                    if (unit == ua)
                    {
                        continue;
                    }

                    if (unit is UA agent)
                    {
                        if (agent.society != null && (agent.society == orcSociety || (orcCulture != null && agent.society == orcCulture)))
                        {
                            units.Add(unit);
                        }
                        else if (agent.person.traits.Any(t => t is T_BloodFeud f && f.orcSociety == orcSociety))
                        {
                            units.Add(unit);
                        }
                        else if (agent.person.items.Any(i => i is I_HordeBanner banner && banner.orcs == orcSociety))
                        {
                            units.Add(unit);
                        }
                        else if (dominionBanner != null && agent.person.items.Any(i => i != null && (i.GetType() == dominionBanner || i.GetType().IsSubclassOf(dominionBanner))))
                        {
                            units.Add(unit);
                        }
                        else if (agent.homeLocation != -1 && (ua.map.locations[agent.homeLocation].soc == orcSociety || (orcCulture != null && ua.map.locations[agent.homeLocation].soc == orcCulture)) && !(agent.task is Task_InHiding))
                        {
                            units.Add(unit);
                        }
                        else if (agent.location.soc != null && (agent.location.soc == orcSociety || (orcCulture != null && agent.location.soc == orcCulture)) && !(agent.task is Task_InHiding))
                        {
                            units.Add(unit);
                        }
                        else if (agent.location.settlement != null && agent.location.settlement.subs.Any(sub => sub is Sub_OrcWaystation way && way.orcSociety == orcSociety))
                        {
                            units.Add(unit);
                        }
                        else if (agent.society != null && (agent.society.getRel(orcSociety).state == DipRel.dipState.war || (orcCulture != null && agent.society.getRel(orcCulture).state == DipRel.dipState.war)) && !(agent.task is Task_InHiding))
                        {
                            units.Add(unit);
                        }
                        else if (agent.task is Task_PerformChallenge performChallenge && performChallenge.challenge.isChannelled() && ua.map.getStepDist(ua.location, agent.location) <= 10)
                        {
                            units.Add(unit);
                        }
                        else if (agent.task is Task_GoToPerformChallenge goPerformChallenge && !(goPerformChallenge.challenge is Ritual) && (goPerformChallenge.challenge.location.soc == orcSociety || goPerformChallenge.challenge.location.soc == orcCulture))
                        {
                            units.Add(unit);
                        }
                        else if (agent.task is Task_AttackUnit attack && attack.target is UA && (attack.target.society == orcSociety || attack.target.society == orcCulture || (orcCulture.tenet_intolerance.status < 0 && attack.target.isCommandable())))
                        {
                            units.Add(unit);
                        }
                    }
                }
            }

            return units;
        }

        private static void Rti_Orc_UniteTheHordes_complete_TranspilerBody(Rti_Orc_UniteTheHordes challenge, UA ua)
        {
            challenge.msgString = ua.getName() + " calls all orcs to join them under their horde's banner, coming together against the common enemies of the orcish clans. ";
            SG_Orc orcSociety = challenge.caster.orcs;
            HolyOrder_Orcs orcCulture = null;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            int locationCount = 0;
            int unitCount = 0;
            int acolyteCount = 0;

            for (int i = 0; i < ua.person.items.Length; i++)
            {
                if (ua.person.items[i] is I_HordeBanner banner && banner.orcs != orcSociety)
                {
                    SG_Orc orcs = banner.orcs;
                    HolyOrder_Orcs culture = null;

                    if (orcs != null)
                    {
                        ModCore.Get().data.orcSGCultureMap.TryGetValue(orcs, out culture);
                    }

                    if (culture != null)
                    {
                        if (culture.seat != null)
                        {
                            if (orcCulture != null)
                            {
                                culture.seat.settlement.subs.Add(new Sub_OrcTemple(culture.seat.settlement, orcCulture));
                            }

                            culture.seat.settlement.subs.Remove(culture.seat);
                            culture.seat = null;
                        }

                        culture.cachedGone = true;

                        if (!orcCulture.ophanim_PerfectSociety && culture.ophanim_PerfectSociety)
                        {
                            orcCulture.ophanim_PerfectSociety = true;
                        }
                    }

                    foreach (Location loc in challenge.map.locations)
                    {
                        if (loc.soc == orcs)
                        {
                            locationCount++;
                            loc.soc = challenge.caster.orcs;
                        }
                    }

                    foreach (Unit unit in challenge.map.units)
                    {
                        if (orcs != null && unit.society == orcs)
                        {
                            unitCount++;
                            unit.society = orcSociety;

                            if (unit is UAEN_OrcUpstart)
                            {
                                foreach (Item item in unit.person.items)
                                {
                                    if (item is I_HordeBanner banner2 && banner2.orcs == orcs)
                                    {
                                        banner2.orcs = orcSociety;
                                    }
                                }
                            }
                        }

                        if (culture != null && unit.society == culture)
                        {
                            if (orcCulture == null)
                            {
                                if (unit.task is Task_PerformChallenge task && task.challenge.claimedBy == unit)
                                {
                                    task.challenge.claimedBy = null;
                                }
                                unit.die(challenge.map, "Slaughtered during the unification of the hordes", null);
                            }
                            else
                            {
                                acolyteCount++;
                                unit.society = orcCulture;
                            }
                        }
                    }

                    banner.orcs = orcSociety;
                }
            }

            orcCulture.orcSociety = orcSociety;
            orcCulture.cachedGone = false;
            orcCulture.updateData();

            challenge.msgString = string.Concat(new string[]
                {
                    challenge.msgString,
                    "\n",
                    locationCount.ToString(),
                    " locations and ",
                    unitCount.ToString(),
                    " units have joined ",
                    orcSociety.getName()
                });

            ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg("Integrated " + locationCount + " settlements", (ModCore.Get().data.influenceGain[ModData.influenceGainAction.Expand] * locationCount)), true);
            ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg("Integrated " + unitCount + " agents", (ModCore.Get().data.influenceGain[ModData.influenceGainAction.AgentKill] * unitCount)), true);

            if (orcCulture != null)
            {
                challenge.msgString = string.Concat(new string[]
                {
                        challenge.msgString,
                        ", and ",
                        acolyteCount.ToString(),
                        " acolytes have joined ",
                        orcCulture.getName()
                });
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg("Integrated " + acolyteCount + " elders", (ModCore.Get().data.influenceGain[ModData.influenceGainAction.AgentKill] * acolyteCount)), true);
            }

            challenge.msgString = string.Concat(new string[]
            {
                    challenge.msgString,
                    "."
            });
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

        private static IEnumerable<CodeInstruction> ManagerMajorThreats_turnTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(ManagerMajorThreats_turnTick_TranspilerBody));

            FieldInfo FI_Map = AccessTools.Field(typeof(ManagerMajorThreats), nameof(ManagerMajorThreats.map));

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldc_I4_2)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Map);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);

                            i++;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed ManagerMajorThreats_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static int ManagerMajorThreats_turnTick_TranspilerBody(Map map)
        {
            int result = ModCore.opt_targetOrcCount;

            if (ModCore.opt_DynamicOrcCount)
            {
                if (map.sizeX * map.sizeY >= 3136)
                {
                    result++;
                }
                else if (map.sizeX * map.sizeY < 1600 )
                {
                    result--;
                }
            }

            if (result < 1)
            {
                result = 1;
            }

            return result;
        }

        private static void SG_Orc_ctor_Postfix(SG_Orc __instance)
        {
            if (!ModCore.Get().data.orcSGCultureMap.ContainsKey(__instance))
            {
                ModCore.Get().data.orcSGCultureMap.Add(__instance, null);
            }

            if (ModCore.Get().data.orcSGCultureMap[__instance] == null)
            {
                ModCore.Get().data.orcSGCultureMap[__instance] = new HolyOrder_Orcs(__instance.map, __instance.map.locations[__instance.capital], __instance);
            }

            if (__instance.map.locations[__instance.capital].settlement is Set_OrcCamp)
            {
                __instance.name = __instance.map.locations[__instance.capital].name + " horde";
            }
        }

        private static void SG_Orc_canSettle_Postfix(ref bool __result, SG_Orc __instance, Location l2)
        {
            if (!__result && l2.settlement != null)
            {
                if (l2.isOcean || l2.hex.getHabilitability() < l2.map.opt_orcHabMult * l2.map.param.orc_habRequirement)
                {
                    return;
                }

                if (ModCore.Get().data.tryGetModIntegrationData("LivingWilds", out ModIntegrationData intDataLW) && intDataLW.typeDict.TryGetValue("WolfRun", out Type wolfRunType))
                {
                    if (l2.settlement.GetType() == wolfRunType)
                    {
                        __result = true;
                        return;
                    }
                }

                if (ModCore.Get().data.tryGetModIntegrationData("Escamrak", out ModIntegrationData intDataEscam) && intDataEscam.typeDict.TryGetValue("LivingTerrainSettlement", out Type livingTerrainSettlementType) && intDataEscam.fieldInfoDict.TryGetValue("LivingTerrainSettlement_TypeOfTerrain", out FieldInfo FI_TypeOfTerrain))
                {
                    if (ModCore.Get().data.orcSGCultureMap.TryGetValue(__instance, out HolyOrder_Orcs orcCulture) && orcCulture.tenet_god is H_Orcs_Fleshweaving fleshweaving && fleshweaving.status < -1)
                    {
                        if (l2.settlement.GetType() == livingTerrainSettlementType && (int)FI_TypeOfTerrain.GetValue(l2.settlement) == 0)
                        {
                            __result = true;
                            return;
                        }
                    }
                }
            }
        }

        private static void SG_Orc_getName_Postfix(ref string __result, SG_Orc __instance)
        {
            if (ModCore.Get().data.orcSGCultureMap.TryGetValue(__instance, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.ophanim_PerfectSociety)
            {
                if (!ModCore.Get().data.perfectHordeNameDict.TryGetValue(__result, out string newResult))
                {
                    int splitPoint = __result.LastIndexOf(" Horde");
                    newResult = __result.Substring(0, splitPoint) + " Perfect Horde";
                    ModCore.Get().data.perfectHordeNameDict.Add(__result, newResult);
                }
                __result = newResult;
                return;
            }
        }

        // Patches for MA_Orcs_Expand
        private static void MA_Orcs_Expand_getUtility_Postfix(MA_Orc_Expand __instance, List<ReasonMsg> reasons, ref double __result)
        {
            if (ModCore.Get().data.orcSGCultureMap.TryGetValue(__instance.soc, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                int status = orcCulture.tenet_expansionism.status;
                if (status != 0)
                {
                    ReasonMsg msg = null;
                    if (reasons != null)
                    {
                        msg = reasons.FirstOrDefault(msg2 => msg2.msg == "Organisational Difficulty");
                    }

                    if (status == 1)
                    {
                        double val = __instance.soc.lastTurnLocs.Count * -5;
                        if (msg != null)
                        {
                            msg.value += val;
                        }
                        __result += val;
                    }
                    else if (status == -1)
                    {
                        double val = __instance.soc.lastTurnLocs.Count * 2.5;
                        if (msg != null)
                        {
                            msg.value += val;
                        }
                        __result += val;
                    }
                    else
                    {
                        double val = __instance.soc.lastTurnLocs.Count * 5;
                        if (msg != null)
                        {
                            reasons.Remove(msg);
                        }
                        __result += val;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> MA_Orc_Expand_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(MA_Orc_Expand_complete_TranspilerBody), new Type[] { typeof(MA_Orc_Expand), typeof(int) });

            FieldInfo FI_ExpandTarget = AccessTools.Field(typeof(SG_Orc), nameof(SG_Orc.expandTarget));
            FieldInfo FI_soc = AccessTools.Field(typeof(MA_Orc_Expand), nameof(MA_Orc_Expand.soc));

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            Label label = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S)
                        {
                            targetIndex++;

                            label = (Label)instructionList[i].operand;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i].labels.Contains(label))
                        {
                            targetIndex = 0;

                            CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_0);
                            code.labels.AddRange(instructionList[i].labels);
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Dup);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_soc);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_ExpandTarget);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);

                            i++;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed MA_Orc_Expand_complete_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void MA_Orc_Expand_complete_TranspilerBody(MA_Orc_Expand ma, int targetIndex)
        {
            if (targetIndex != -1)
            {
                Location location = ma.map.locations[targetIndex];
                if (location.settlement is Set_OrcCamp camp)
                {
                    List<Sub_OrcWaystation> waystations = camp.subs.OfType<Sub_OrcWaystation>().ToList();
                    foreach (Sub_OrcWaystation waystation in waystations)
                    {
                        camp.subs.Remove(waystation);
                    }
                }
            }
        }

        private static void Set_OrcCamp_ctor_Postfix(Set_OrcCamp __instance)
        {
            __instance.customChallenges.Add(new Ch_Orcs_GatherHorde(__instance.location));
            __instance.customChallenges.Add(new Ch_Orcs_FundHorde(__instance.location, __instance));
            __instance.customChallenges.Add(new Ch_Orcs_RaidOutpost(__instance.location, __instance.location.soc as SG_Orc));
            __instance.customChallenges.Add(new Ch_H_Orcs_CleansingFestival(__instance.location));
            __instance.customChallenges.Add(new Ch_H_Orcs_DarkFestival(__instance.location));
            __instance.customChallenges.Add(new Ch_Orcs_FundWaystation(__instance.location));
            __instance.customChallenges.Add(new Ch_Orcs_WarFestival(__instance.location));
            __instance.customChallenges.Add(new Ch_Orcs_BuildTemple(__instance.location));
            __instance.customChallenges.Add(new Ch_Orcs_BloodMoney(__instance.location));
            __instance.customChallenges.Add(new Ch_Orcs_RecruitCorsair(__instance.location, new M_OrcCorsair(__instance.map), 0, null, __instance));

            if (ModCore.Get().data.godTenetTypes.TryGetValue(__instance.map.overmind.god.GetType(), out Type tenetType) && tenetType == typeof(H_Orcs_HarbingersMadness))
            {
                __instance.customChallenges.Add(new Ch_H_Orcs_MadnessFestival(__instance.location));
            }
        }

        private static void Set_OrcCamp_turnTick_Postfix(Set_OrcCamp __instance)
        {
            SG_Orc orcSociety = __instance.location.soc as SG_Orc;

            if (orcSociety != null && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (orcCulture.tenet_shadowWeaving?.status < 0)
                {
                    __instance.shadowPolicy = Settlement.shadowResponse.FULL_FLOW;
                }
                else
                {
                    __instance.shadowPolicy = Settlement.shadowResponse.RECEIVE_ONLY;
                }

                if (orcCulture.tenet_god is H_Orcs_Perfection perfection && perfection.status < 0)
                {
                    Pr_Ophanim_Perfection perfectionLocal = __instance.location.properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
                    if (perfectionLocal == null)
                    {
                        foreach (Location neighbour in __instance.location.getNeighbours())
                        {
                            if (neighbour.settlement is SettlementHuman && neighbour.properties.OfType<Pr_Opha_Faith>().FirstOrDefault()?.charge > 100.0)
                            {
                                perfectionLocal = new Pr_Ophanim_Perfection(__instance.location);
                                __instance.location.properties.Add(perfectionLocal);
                                break;
                            }
                        }
                    }
                }
            }

            if (__instance.specialism == 0)
            {

            }
            else if (__instance.specialism == 1)
            {
                Pr_OrcDefences defences = (Pr_OrcDefences)__instance.location.properties.FirstOrDefault(pr => pr is Pr_OrcDefences);
                if (defences == null)
                {
                    defences = new Pr_OrcDefences(__instance.location);
                    defences.charge = 2.0;
                    __instance.location.properties.Add(defences);
                }
            }
            else if (__instance.specialism == 2)
            {
                Pr_GeomanticLocus locus = (Pr_GeomanticLocus)__instance.location.properties.FirstOrDefault(pr => pr is Pr_GeomanticLocus);
                if (locus != null && locus.charge < 100)
                {
                    locus.influences.Add(new ReasonMsg("Orc Mages", 1.0));
                }
            }
            else if (__instance.specialism == 3 || __instance.specialism == 5)
            {
                if (__instance.army != null && (__instance.army.hp <= 0 || __instance.army.isDead))
                {
                    __instance.army = null;
                }

                if (__instance.army == null)
                {
                    __instance.armyRebuildTimer++;
                    if (__instance.armyRebuildTimer > __instance.map.param.socialGroup_orc_armyRebuildTimer)
                    {
                        if (__instance.specialism == 3)
                        {
                            __instance.army = new UM_OrcBeastArmy(__instance.location, orcSociety, __instance);
                        }
                        else if (__instance.specialism == 5)
                        {
                            __instance.army = new UM_OrcCorsair(__instance.location, orcSociety, __instance);
                        }
                        __instance.army.location.units.Add(__instance.army);
                        __instance.map.units.Add(__instance.army);
                        __instance.armyRebuildTimer = 0;
                    }
                }
            }
        }

        private static double Set_OrcCamp_getMaxDefence_Postfix(double def, Set_OrcCamp __instance)
        {
            Pr_Vinerva_Thorns giftThorns = __instance.location.properties.OfType<Pr_Vinerva_Thorns>().FirstOrDefault();

            if (giftThorns != null)
            {
                def += giftThorns.charge / 2;
            }

            return def;
        }

        private static void Pr_OrcDefences_turnTick_Postfix(Pr_OrcDefences __instance)
        {
            if (__instance.location.settlement is Set_OrcCamp camp)
            {
                double targetDef = 0.0;
                if (camp.specialism == 1)
                {
                    targetDef += 50.0;
                }

                Pr_Ophanim_Perfection perfection = __instance.location.properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
                if (perfection != null)
                {
                    targetDef += Math.Ceiling(perfection.charge / 12);
                }

                ReasonMsg maintenance = __instance.influences.FirstOrDefault(msg => msg.msg == "Lack of maintenance");

                if (__instance.charge == targetDef)
                {
                    __instance.influences.Remove(maintenance);
                }
                if (__instance.charge > targetDef)
                {
                    maintenance.value = Math.Max(-2.0, targetDef - __instance.charge);
                }
                else
                {
                    maintenance.msg = "Building fortifications";
                    maintenance.value = Math.Min(2.0, targetDef - __instance.charge);
                }
            }
        }

        private static void Pr_OrcishIndustry_turnTick_Postfix(Pr_OrcishIndustry __instance)
        {
            if (__instance.location.settlement is Set_OrcCamp camp && __instance.location.soc is SG_Orc orcSociety)
            {
                if (ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null && orcCulture.tenet_god is H_Orcs_MammonClient client && client.status < 0)
                {
                    int income = (int)Math.Round(__instance.charge / 50);

                    if (income > 0)
                    {
                        if (camp.specialism > 0)
                        {
                            income++;
                        }

                        if (__instance.location.properties.Any(p => p is Pr_GeomanticLocus locus && locus.charge >= 25.0))
                        {
                            income++;
                        }

                        foreach (Subsettlement sub in camp.subs)
                        {
                            if (sub is Sub_AncientRuins)
                            {
                                income++;
                                break;
                            }
                        }

                        Pr_OrcPlunder plunder = __instance.location.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();
                        if (plunder == null)
                        {
                            plunder = new Pr_OrcPlunder(__instance.location);
                            plunder.gold = 0;
                            __instance.location.properties.Add(plunder);
                        }
                        plunder.addGold(income);
                    }
                }
            }
        }

        // Patches for Pr_OrcFunbding
        private static IEnumerable<CodeInstruction> Pr_OrcFunding_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Pr_OrcFunding_turnTick_TranspilerBody), new Type[] { typeof(SG_Orc), typeof(int) });

            FieldInfo FI_Fundee = AccessTools.Field(typeof(Pr_OrcFunding), nameof(Pr_OrcFunding.fundees));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Div)
                        {
                            targetIndex++;
                        }
                    }

                    if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Fundee);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 12);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed Pr_OrcFunding_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void Pr_OrcFunding_turnTick_TranspilerBody(SG_Orc orcSociety, int funding)
        {
            ModCore.Get().TryAddInfluenceGain(orcSociety, new ReasonMsg("Recieved funding from the Dark Empire", funding / 4), true);
        }

        // Patches for UM_OrcArmy
        private static void UM_OrcArmy_ctor_Postfix(UM_OrcArmy __instance)
        {
            if (__instance.homeLocation != -1)
            {
                __instance.rituals.Add(new Rt_Orcs_BuildCamp(__instance.map.locations[__instance.homeLocation]));
            }
        }

        private static string UM_OrcArmy_getName_Postfix(string name, UM_OrcArmy __instance)
        {
            if (__instance.parent.specialism == 1)
            {
                return "Orc Horde";
            }

            return name;
        }

        private static void UM_OrcArmy_turnTickInner_Postfix(UM_OrcArmy __instance)
        {
            SG_Orc orcSociety = __instance.society as SG_Orc;

            if (orcSociety != null && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture))
            {
                if (__instance.task == null)
                {
                    List<UA> targetAgents = new List<UA>();
                    if (__instance.parent != null && __instance.parent.location.settlement == __instance.parent && __instance.parent.location.soc == orcSociety && __instance.location == __instance.parent.location)
                    {
                        H_Orcs_Intolerance intolerance = orcCulture.tenet_intolerance;
                        if (intolerance != null)
                        {
                            foreach (Unit unit in __instance.parent.location.units)
                            {
                                if (unit is UA ua && ua.task is Task_PerformChallenge && __instance.hostileTo(unit, true))
                                {
                                    targetAgents.Add(ua);
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
                                            ", as they attempt to perform this challenge.\\n\\nWhile an orc army is at rest, they patrol constantly, attacking any outsiders operating in their home camp, unless they are tolerant towards them, or, in the case of dark agents, including player controlled agents, the army's home camp is >50% shadow. (You CANNOT see this on the map by the red spikes on the army icon when the agent is selected). Note: 100% infiltration will allow your agents to continue to operate normally"
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
                                            ", as they are perform this challenge in an orc camp that is not tolerant of them. They have taken 1HP adamge while escaping, and will continue to do so until they leave the location. \\n\\nWhile an orc army is at rest, they patrol constantly, attacking any outsiders operating in their home camp, unless they are tolerant towards them, or, in the case of dark agents, including player controlled agents, the army's home camp is >50% shadow. (You CANNOT see this on the map by the red spikes on the army icon when the agent is selected). Note: 100% infiltration will allow your agents to continue to operate normally"
                                            }), UnifiedMessage.messageType.ARMY_BLOCKS, false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (__instance.task is Task_RazeOutpost)
                {
                    Pr_HumanOutpost outpost = __instance.location.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
                    if (outpost != null && orcSociety != null && orcSociety.capital != -1 && __instance.map.locations[orcSociety.capital].soc == orcSociety && __instance.map.locations[orcSociety.capital].settlement is Set_OrcCamp camp)
                    {
                        Pr_OrcPlunder plunder = camp.location.properties.OfType<Pr_OrcPlunder>().FirstOrDefault();
                        if (plunder == null)
                        {
                            plunder = new Pr_OrcPlunder(__instance.map.locations[orcSociety.capital]);
                            __instance.map.locations[orcSociety.capital].properties.Add(plunder);
                        }

                        int gold = (int)Math.Floor((50.0 / outpost.charge) * outpost.funding);
                        outpost.funding -= gold;
                        plunder.gold += gold;
                        __instance.map.addMessage(__instance.getName() + " plunders " + gold + " gold", 0.2, true, __instance.location.hex);
                        if (__instance.map.burnInComplete && gold >= 10.0)
                        {
                            __instance.map.addUnifiedMessage(__instance, __instance.location, "Plunder", string.Concat(new string[]
                            {
                        __instance.getName(),
                        " plunders ",
                        gold.ToString(),
                        " gold from ",
                        __instance.location.getName(true),
                        " while razing the outpost, which will be gathered in the orc main fortress's plunder, which your agents can access."
                            }), UnifiedMessage.messageType.ORC_PLUNDER, true);
                        }
                        __instance.map.hintSystem.popHint(HintSystem.hintType.ORC_PLUNDER);
                    }
                }

                if (orcCulture.tenet_god is H_Orcs_Fleshweaving fleshweaving && fleshweaving.status < 0)
                {
                    if (__instance.hp < __instance.maxHp)
                    {
                        __instance.hp++;
                    }
                }
            }
        }

        private static void UM_OrcArmy_updateMaxHP_Postfix(UM_OrcArmy __instance)
        {
            if (__instance.homeLocation != -1 && __instance.map.locations[__instance.homeLocation].settlement is Set_OrcCamp && __instance.map.locations[__instance.homeLocation].soc == __instance.society)
            {
                Pr_Ophanim_Perfection perfection = __instance.map.locations[__instance.homeLocation].properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
                if (perfection != null)
                {
                    __instance.maxHp = (int)Math.Ceiling(__instance.maxHp * (1 + (perfection.charge / 1200)));
                }
            }
        }

        private static IEnumerable<CodeInstruction> UM_OrcArmy_turnTickAI_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(UM_Orc_army_turnTickAI_TranspilerBody), new Type[] { typeof(UM_OrcArmy) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("OrcsPlus: Completed complete function replacement transpiler UM_OrcArmy_turnTickAI_Transpiler");
        }

        private static void UM_Orc_army_turnTickAI_TranspilerBody(UM_OrcArmy um)
        {
            SG_Orc orcSociety = um.society as SG_Orc;
            HolyOrder_Orcs orcCulture = null;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (um.hp < um.maxHp * 0.3)
            {
                if (um.location.index == um.homeLocation)
                {
                    um.task = new Task_Recruit();
                }
                else
                {
                    um.task = new Task_GoToLocation(um.map.locations[um.homeLocation]);
                }
                return;
            }

            if (um.location.soc != null && um.location.soc != um.society && um.society.getRel(um.location.soc).state == DipRel.dipState.war && um.location.settlement != null && !(um.location.settlement is Set_CityRuins) && !(um.location.settlement is Set_TombOfGods))
            {
                um.task = new Task_RazeLocation();
                return;
            }

            Pr_HumanOutpost outpost = um.location.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
            if (outpost != null && outpost.parent != null && um.society.getRel(outpost.parent).state == DipRel.dipState.war)
            {
                um.task = new Task_RazeOutpost();
                return;
            }

            int steps = -1;
            List<UM> targets = new List<UM>();
            UM target = null;

            foreach (Unit unit in um.location.map.units)
            {
                // Will not attack non militray units or units of this society.
                if (unit is UM um2 && um2.society != um.society)
                {
                    int dist = um.map.getStepDist(um.location, um2.location);

                    if (ModCore.Get().isAttackingSociety(um, um2))
                    {
                        if (steps == -1 || dist <= steps)
                        {
                            if (dist < steps)
                            {
                                targets.Clear();
                            }

                            targets.Add(um2);
                            steps = dist;
                        }
                        continue;
                    }

                    // Will not attack refugees that are further than 1 step distance. Also will not attack refugees that are en-route to a Cordyceps hive if Cordyceps' god tenet is Elder aligned.
                    if (!(um2 is UM_Refugees) || (dist <= 1))
                    {
                        // Will attack military units in own societ's territory or at a distance up to 4 step distance away.
                        if (um2.location.soc == um.society || dist <= 4)
                        {
                            // Will only attack military units in unit's own society if they are above recruitment threshold.
                            if (um2.location.soc != um2.society || um2.hp >= um2.maxHp / 3)
                            {
                                DipRel rel = null;
                                if (um2.society != null)
                                {
                                    rel = um.society.getRel(um2.society);
                                }

                                // Will attack military units that they are at war with, or that they are hostile with and are within this army's territory.
                                if (um2.society == null
                                    || rel.state == DipRel.dipState.war
                                    || (rel.state == DipRel.dipState.hostile && unit.location.soc == um.society && ModCore.Get().isHostileAlignment(um.society as SG_Orc, um2.society)))
                                {
                                    // Ignore military units that are already in combat with armies that the orcs are also at war with.
                                    if (!um.fightingMutualEnemy(um2) && ModCore.Get().isHostileAlignment(um, um2))
                                    {
                                        if (steps == -1 || dist <= steps)
                                        {
                                            if (dist < steps)
                                            {
                                                targets.Clear();
                                            }

                                            targets.Add(um2);
                                            steps = dist;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            if (targets.Count > 0)
            {
                target = targets[0];

                if (targets.Count > 1)
                {
                    target = targets[Eleven.random.Next(targets.Count)];
                }
            }

            if (target != null)
            {
                um.task = new Task_AttackArmy(target, um);
                return;
            }

            if (orcCulture?.tenet_expansionism?.status < -1 && !um.location.units.Any(u => u is UM_OrcArmy army && army.society == um.society && army.task is Task_PerformChallenge tChallenge && tChallenge.challenge is Rt_Orcs_BuildCamp))
            {
                Rt_Orcs_BuildCamp cBuildCamp = (Rt_Orcs_BuildCamp)um.rituals.FirstOrDefault(rt => rt is Rt_Orcs_BuildCamp);
                if (cBuildCamp != null && cBuildCamp.validFor(um))
                {
                    um.task = new Task_PerformChallenge(cBuildCamp);
                    return;
                }
            }

            if (um.society.isAtWar())
            {
                steps = -1;
                List<Location> targetLocations = new List<Location>();
                Location targetLocation = null;

                if (orcCulture != null && orcCulture.tenet_god is H_Orcs_InsectileSymbiosis symbiosis && symbiosis.status < 0)
                {
                    if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.typeDict.TryGetValue("God", out Type cordycepsType))
                    {
                        if (um.map.overmind.god.GetType() == cordycepsType || um.map.overmind.god.GetType().IsSubclassOf(cordycepsType))
                        {
                            FieldInfo FI_VespidicAttack = AccessTools.Field(cordycepsType, "God_Insect.vespidicSwarmTarget");
                            if (FI_VespidicAttack != null)
                            {
                                Location vespidicTarget = (Location)FI_VespidicAttack.GetValue(um.map.overmind.god);
                                if (vespidicTarget != null && vespidicTarget != um.location && um.map.getStepDist(um.location, vespidicTarget) < 3)
                                {
                                    if (vespidicTarget.soc != null && vespidicTarget.soc != orcSociety && vespidicTarget.soc != orcCulture && orcSociety.getRel(vespidicTarget.soc).state == DipRel.dipState.war)
                                    {
                                        if (vespidicTarget.settlement != null && !(vespidicTarget.settlement is Set_TombOfGods) && !(vespidicTarget.settlement is Set_CityRuins))
                                        {
                                            targetLocation = vespidicTarget;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (targetLocation == null)
                {
                    foreach (Location loc in um.map.locations)
                    {
                        if (loc.soc == null)
                        {
                            Pr_HumanOutpost targetOutpost = loc.properties.OfType<Pr_HumanOutpost>().FirstOrDefault();
                            if (targetOutpost != null && loc.settlement == null && targetOutpost.parent != null && targetOutpost.parent != um.society && um.society.getRel(targetOutpost.parent).state == DipRel.dipState.war)
                            {
                                int dist = um.map.getStepDist(um.location, loc);
                                if (steps == -1 || dist <= steps)
                                {
                                    if (dist < steps)
                                    {
                                        targetLocations.Clear();
                                    }

                                    targetLocations.Add(loc);
                                    steps = dist;
                                }
                            }
                        }
                        else if (loc.soc != um.society && um.society.getRel(loc.soc).state == DipRel.dipState.war)
                        {
                            if (loc.settlement != null && !(loc.settlement is Set_TombOfGods) && !(loc.settlement is Set_CityRuins))
                            {
                                int dist = um.map.getStepDist(um.location, loc);
                                if (steps == -1 || dist <= steps)
                                {
                                    if (dist < steps)
                                    {
                                        targetLocations.Clear();
                                    }

                                    targetLocations.Add(loc);
                                    steps = dist;
                                }
                            }
                        }
                    }

                    if (targetLocations.Count == 1)
                    {
                        targetLocation = targetLocations[0];
                    }
                    else if (targetLocations.Count > 1)
                    {
                        targetLocation = targetLocations[Eleven.random.Next(targetLocations.Count)];
                    }
                }

                if (targetLocation != null)
                {
                    um.task = new Task_GoToLocation(targetLocation);
                    return;
                }
            }

            if (um.location.index == um.homeLocation)
            {
                if (um.hp < um.maxHp)
                {
                    um.task = new Task_Recruit();
                    return;
                }
            }
            else
            {
                um.task = new Task_GoToLocation(um.map.locations[um.homeLocation]);
                return;
            }
        }

        private static void UM_OrcRaiders_ctor_Postfix(UM_OrcRaiders __instance)
        {
            __instance.rituals.Add(new Rt_Orcs_BuildCamp(__instance.location));
        }

        private static void UM_OrcRaiders_assignMaxHP_Postfix(UM_OrcRaiders __instance)
        {
            if (__instance.homeLocation != -1 && __instance.map.locations[__instance.homeLocation].settlement is Set_OrcCamp && __instance.map.locations[__instance.homeLocation].soc == __instance.society)
            {
                Pr_Ophanim_Perfection perfection = __instance.map.locations[__instance.homeLocation].properties.OfType<Pr_Ophanim_Perfection>().FirstOrDefault();
                if (perfection != null)
                {
                    __instance.maxHp = (int)Math.Ceiling(__instance.maxHp * (1 + (perfection.charge / 1200)));
                }
            }
        }

        private static void UM_UntamedDead_turnTickInner_Postfix(UM_UntamedDead __instance)
        {
            if (__instance.master is UAEN_OrcShaman shaman && !shaman.isCommandable())
            {
                __instance.master = null;
            }
        }

        private static double UA_getChallengeUtility_Postfix(double utility, UA __instance, Challenge c, List<ReasonMsg> reasons)
        {
            bool willIntercept = false;

            if (c.location.settlement is Set_OrcCamp camp && camp.army != null && camp.army.location == camp.location && camp.location.soc is SG_Orc orcSociety)
            {
                if (orcSociety != null && ModCore.Get().data.orcSGCultureMap.ContainsKey(orcSociety))
                {
                    HolyOrder_Orcs orcCulture = ModCore.Get().data.orcSGCultureMap[orcSociety];

                    if (orcCulture == null || __instance.society == orcSociety || __instance.society == orcCulture)
                    {
                        return utility;
                    }

                    if (__instance.homeLocation != -1 && (__instance.map.locations[__instance.homeLocation].soc == orcSociety || __instance.map.locations[__instance.homeLocation].soc == orcCulture))
                    {
                        return utility;
                    }

                    H_Orcs_Intolerance tolerance = orcCulture.tenet_intolerance;
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

        private static bool Ch_DrinkPrimalWaters_validFor_Postfix(bool result, Ch_DrinkPrimalWaters __instance, UA ua)
        {
            if (!ua.isCommandable() && ua.society is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                if (orcCulture.tenet_alignment.status < 0 && __instance.font.control >= __instance.map.param.ch_drinkprimalwaters_parameterValue2)
                {
                    return true;
                }
                else if (orcCulture.tenet_alignment.status > 0 && __instance.font.control <= __instance.map.param.ch_drinkprimalwaters_parameterValue3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (!ua.isCommandable() && ua.society is HolyOrder_Orcs orcCulture2)
            {
                if (orcCulture2.tenet_alignment.status < 0 && __instance.font.control >= __instance.map.param.ch_drinkprimalwaters_parameterValue2)
                {
                    return true;
                }
                else if (orcCulture2.tenet_alignment.status > 0 && __instance.font.control <= __instance.map.param.ch_drinkprimalwaters_parameterValue3)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return result;
        }

        private static bool Ch_LearnSecret_validFor_Postfix(bool result, Ch_LearnSecret __instance, UA ua)
        {
            if (ua is UAEN_OrcShaman && __instance.secret.library == null)
            {
                return true;
            }

            return result;
        }

        // Patches for Rt_StudyDeath

        private static bool Rt_StudyDeath_Postfix(bool __result, Rt_StudyDeath __instance, UA ua)
        {
            if (ua is UAEN_OrcShaman)
            {
                T_ArcaneKnowledge t_ArcaneKnowledge = ua.person.traits.OfType<T_ArcaneKnowledge>().FirstOrDefault();

                if (t_ArcaneKnowledge == null || t_ArcaneKnowledge.level == 0)
                {
                    return false;
                }

                int level = 0;
                T_MasteryDeath t_MasteryDeath = ua.person.traits.OfType<T_MasteryDeath>().FirstOrDefault();

                if (t_MasteryDeath == null)
                {
                    return false;
                }
                else
                {
                    level = t_MasteryDeath.level;
                }

                int req = __instance.getReq(level);
                return t_ArcaneKnowledge.level >= req;
            }

            return __result;
        }

        // Patches for P_Opha_Crusade
        private static string P_Opha_Crusade_getDesc_Postfix(string desc)
        {
            return "Causes all societies you have turned into Ophanim's Theocracies, and all perfected orc tribes, to begin a war against the target society if they are not already at war.";
        }

        private static void P_Opha_Crusade_cast_Postfix(P_Opha_Crusade __instance, Location location)
        {
            if (location.soc != null)
            {
                foreach (SocialGroup sg in __instance.map.socialGroups)
                {
                    if (sg is HolyOrder_Orcs orcCulture && orcCulture.ophanim_PerfectSociety && location.soc != orcCulture && location.soc != orcCulture.orcSociety)
                    {
                        if (orcCulture.orcSociety.getRel(location.soc).state != DipRel.dipState.war)
                        {
                            War war = orcCulture.map.declareWar(orcCulture.orcSociety, location.soc, true, null);
                            war.attackerObjective = War.warType.INVASION;
                        }
                        else
                        {
                            __instance.map.world.prefabStore.popMsg(orcCulture.orcSociety.getName() + " is already at war with " + location.soc.getName() + ", so will not start a new war until this one is completed", false, false);
                        }
                    }
                }
            }
        }

        // Patches for P_Opha_Empower
        private static string P_Opha_Empower_getDesc_Postfix(string desc)
        {
            return "Heals a human army from a city which Ophanim's Faith has perfected, or a perfect horde. Heals 50% of their missing HP.";
        }

        private static string P_Opha_Empower_getRestrictionText_Postfix(string desc)
        {
            desc = desc.TrimEnd('.');

            return desc + ", or a perfect horde.";
        }

        private static bool P_Opha_Empower_validTarget_Postfix(bool result, Unit unit)
        {
            if (unit is UM_PerfectHorde)
            {
                result = true;
            }

            return result;
        }

        // Patches for PC_Card
        private static IEnumerable<CodeInstruction> PC_Card_cast_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(PC_Card_cast_TranspilerBody), new Type[] { typeof(bool), typeof(Unit) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i + 1].opcode == OpCodes.Nop && instructionList[i + 2].opcode == OpCodes.Ldarg_0)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Stloc_S, 6);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed PC_Card_cast_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        // Deals with result AFTER it has been negated.
        private static bool PC_Card_cast_TranspilerBody(bool result, Unit u)
        {
            if (result)
            {
                SG_Orc orcSociety = u.society as SG_Orc;
                HolyOrder_Orcs orcCulture = u.society as HolyOrder_Orcs;

                if (orcSociety != null)
                {
                    ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
                }

                if (orcCulture != null && orcCulture.tenet_god is H_Orcs_Lucky lucky)
                {
                    if (lucky.status == -1 && Eleven.random.NextDouble() < 0.5)
                    {
                        return false;
                    }
                    else if (lucky.status == -2 && Eleven.random.NextDouble() < 0.75)
                    {
                        return false;
                    }
                }
            }

            return result;
        }

        private static void PC_Card_cast_Postfix(PC_Card __instance, Unit u)
        {
            bool beneficial = false;

            if (__instance is PC_T1_Coin || __instance is PC_T1_Hammer || __instance is PC_T3_Blindfold || __instance is PC_T1_Move)
            {
                beneficial = true;
            }

            if (beneficial)
            {
                SG_Orc orcSociety = u.society as SG_Orc;
                HolyOrder_Orcs orcCulture = u.society as HolyOrder_Orcs;

                if (orcSociety != null)
                {
                    ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
                }

                if (orcCulture != null)
                {
                    ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg("Benefited from Death's Games", ModCore.Get().data.influenceGain[ModData.influenceGainAction.RecieveGift]), true);
                }
            }
        }

        // Patches for T_Et_Sword
        public static void T_Et_Sword_onKill_Postfix(T_Et_Sword __instance, Person victim)
        {
            SocialGroup sg = __instance.assignedTo.getLocation().soc;

            if (sg == null)
            {
                return;
            }

            SG_Orc orcSociety = sg as SG_Orc;
            HolyOrder_Orcs orcCulture = sg as HolyOrder_Orcs;

            if (orcSociety != null)
            {
                ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out orcCulture);
            }

            if (orcCulture != null && orcCulture.camps.Count > 1)
            {
                __instance.assignedTo.unit.addMenace(20.0);
                orcCulture.triggerCivilWar();
            }

        }

        // Community Library Patches
        private static IEnumerable<CodeInstruction> Rt_Orcs_ClaimTerritory_validFor_TranspilerBody_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Rt_Orcs_ClaimTerritory_validFor_TranspilerBody_TranspilerBody), new Type[] { typeof(bool), typeof(UA) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldftn)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Stloc_S, 12);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 12);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed Rt_Orcs_ClaimTerritory_validFor_TranspilerBody_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool Rt_Orcs_ClaimTerritory_validFor_TranspilerBody_TranspilerBody(bool result, UA ua)
        {
            if (ua.location.getNeighbours().Any(n => n.settlement != null && n.settlement.subs.Any(sub => sub is Sub_OrcWaystation way && way.orcSociety == ua.society)))
            {
                result = true;
            }

            return result;
        }

        private static IEnumerable<CodeInstruction> Ch_RecoverShipwreck_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Ch_RecoverShipwreck_complete_TranspilerBody), new Type[] { typeof(Ch_RecoverShipwreck), typeof(UA), typeof(Set_OrcCamp) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i-1].opcode == OpCodes.Nop && instructionList[i+1].opcode == OpCodes.Ldfld)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("OrcsPlus: Completed Ch_RecoverShipwreck_complete_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("OrcsPlus: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void Ch_RecoverShipwreck_complete_TranspilerBody(Ch_RecoverShipwreck challenge, UA ua, Set_OrcCamp emptyShipyard)
        {
            if (ua.isCommandable() && emptyShipyard.location.soc is SG_Orc orcSociety && ModCore.Get().data.orcSGCultureMap.TryGetValue(orcSociety, out HolyOrder_Orcs orcCulture) && orcCulture != null)
            {
                ModCore.Get().TryAddInfluenceGain(orcCulture, new ReasonMsg(challenge.getName(), ModCore.Get().data.influenceGain[ModData.influenceGainAction.BuildShipyard]), true);
            }
        }
    }
}