﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

public static class CharacterActionCastSpellPatcher
{
    [HarmonyPatch(typeof(CharacterActionCastSpell), "ApplyMagicEffect")]
    [HarmonyPatch(
        new[]
        {
            typeof(GameLocationCharacter), typeof(ActionModifier), typeof(int), typeof(int),
            typeof(RuleDefinitions.RollOutcome), typeof(bool), typeof(RuleDefinitions.RollOutcome), typeof(int),
            typeof(int), typeof(bool)
        },
        new[]
        {
            ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Out
        })]
    public static class ApplyMagicEffect_Patch
    {
        public static bool Prefix(CharacterActionCastSpell __instance,
            GameLocationCharacter target,
            ActionModifier actionModifier,
            int targetIndex,
            int targetCount,
            RuleDefinitions.RollOutcome outcome,
            bool rolledSaveThrow,
            RuleDefinitions.RollOutcome saveOutcome,
            int saveOutcomeDelta,
            ref int damageReceived,
            ref bool damageAbsorbedByTemporaryHitPoints
        )
        {
            //PATCH: re-implements base method to allow `ICustomSpellEffectLevel` to provide customized spell effect level

            var activeSpell = __instance.ActiveSpell;
            var effectLevelProvider = activeSpell.SpellDefinition.GetFirstSubFeatureOfType<ICustomSpellEffectLevel>();

            if (effectLevelProvider == null)
            {
                return true;
            }

            var actingCharacter = __instance.ActingCharacter;
            var effectLevel = effectLevelProvider.GetEffectLevel(actingCharacter.RulesetActor);

            //Re-implementing CharacterActionMagicEffect.ApplyForms
            var formsParams = new RulesetImplementationDefinitions.ApplyFormsParams();

            formsParams.FillSourceAndTarget(actingCharacter.RulesetCharacter, target.RulesetActor);
            formsParams.FillFromActiveEffect(activeSpell);
            formsParams.FillSpecialParameters(
                rolledSaveThrow,
                __instance.AddDice,
                __instance.AddHP,
                __instance.AddTempHP,
                effectLevel,
                actionModifier,
                saveOutcome,
                saveOutcomeDelta,
                outcome == RuleDefinitions.RollOutcome.CriticalSuccess,
                targetIndex,
                targetCount,
                __instance.TargetItem
            );
            formsParams.effectSourceType = RuleDefinitions.EffectSourceType.Spell;
            formsParams.targetSubstitute = __instance.ActionParams.TargetSubstitute;

            var spellEffectDescription = activeSpell.EffectDescription;
            var rangeType = spellEffectDescription.RangeType;

            if (rangeType is RuleDefinitions.RangeType.MeleeHit or RuleDefinitions.RangeType.RangeHit)
            {
                formsParams.attackOutcome = outcome;
            }

            var actualEffectForms = __instance.actualEffectForms;

            damageReceived = ServiceRepository.GetService<IRulesetImplementationService>()
                .ApplyEffectForms(actualEffectForms[targetIndex], formsParams, __instance.effectiveDamageTypes,
                    out damageAbsorbedByTemporaryHitPoints, effectApplication: spellEffectDescription.EffectApplication,
                    filters: spellEffectDescription.EffectFormFilters);

            return false;
        }
    }

    [HarmonyPatch(typeof(CharacterActionCastSpell), "GetAdvancementData")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class GetAdvancementData_Patch
    {
        [NotNull]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            //PATCH: enforces cantrips to be cast at character level (MULTICLASS)
            //replaces repertoire's SpellCastingLevel with character level for cantrips
            var spellCastingLevelMethod = typeof(RulesetSpellRepertoire).GetMethod("get_SpellCastingLevel");
            var spellCastingLevel =
                new Func<RulesetSpellRepertoire, CharacterActionCastSpell, int>(MulticlassContext.SpellCastingLevel)
                    .Method;

            return instructions.ReplaceCalls(spellCastingLevelMethod,
                "CharacterActionCastSpell.GetAdvancementData",
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, spellCastingLevel));
        }
    }

    [HarmonyPatch(typeof(CharacterActionCastSpell), "StartConcentrationAsNeeded")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class StartConcentrationAsNeeded_Patch
    {
        public static bool Prefix(CharacterActionCastSpell __instance)
        {
            //PATCH: BestowCurseNoConcentrationRequiredForSlotLevel5OrAbove
            if (!Main.Settings.BestowCurseNoConcentrationRequiredForSlotLevel5OrAbove)
            {
                return true;
            }

            // Per SRD Bestow Curse does not need concentration when cast with slot level 5 or above
            // If the active spell is a sub-spell of Bestow Curse and the slot level is >= 5 don't run StartConcentrationAsNeeded
            return
                !__instance.ActiveSpell.SpellDefinition.IsSubSpellOf(DatabaseHelper.SpellDefinitions.BestowCurse)
                || __instance.ActiveSpell.SlotLevel < 5;
        }
    }
}
