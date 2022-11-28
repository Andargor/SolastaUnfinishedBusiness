﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.Extensions;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

public static class RulesetSpellRepertoirePatcher
{
    private static bool FormatTitle(RulesetSpellRepertoire __instance, ref string __result)
    {
        if (__instance.SpellCastingClass != null
            || __instance.SpellCastingSubclass != null
            || __instance.SpellCastingRace != null)
        {
            return true;
        }

        __result = __instance.SpellCastingFeature.FormatTitle();

        return false;
    }

    //PATCH: handles all different scenarios of spell slots consumption (casts, smites, point buys)
    [HarmonyPatch(typeof(RulesetSpellRepertoire), "SpendSpellSlot")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class SpendSpellSlot_Patch
    {
        public static bool Prefix(RulesetSpellRepertoire __instance, int slotLevel)
        {
            if (__instance.SpellCastingFeature.SpellCastingOrigin is FeatureDefinitionCastSpell.CastingOrigin.Race
                or FeatureDefinitionCastSpell.CastingOrigin.Monster)
            {
                return true;
            }

            if (slotLevel == 0)
            {
                return true;
            }

            var heroWithSpellRepertoire = __instance.GetCasterHero();

            if (heroWithSpellRepertoire == null)
            {
                return true;
            }

            if (!SharedSpellsContext.IsMulticaster(heroWithSpellRepertoire))
            {
                return true;
            }

            var warlockSpellRepertoire = SharedSpellsContext.GetWarlockSpellRepertoire(heroWithSpellRepertoire);

            // handles MC non-Warlock
            if (warlockSpellRepertoire == null)
            {
                foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires
                             .Where(x => x.SpellCastingFeature.SpellCastingOrigin !=
                                         FeatureDefinitionCastSpell.CastingOrigin.Race))
                {
                    var usedSpellsSlots = spellRepertoire.usedSpellsSlots;

                    usedSpellsSlots.TryAdd(slotLevel, 0);
                    usedSpellsSlots[slotLevel]++;
                    spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
                }
            }

            // handles MC Warlock
            else
            {
                SpendMulticasterWarlockSlots(__instance, heroWithSpellRepertoire, slotLevel);
            }

            return false;
        }

        private static void SpendWarlockSlots(RulesetSpellRepertoire rulesetSpellRepertoire,
            RulesetCharacterHero heroWithSpellRepertoire)
        {
            var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(heroWithSpellRepertoire);
            var usedSpellsSlots =
                rulesetSpellRepertoire.usedSpellsSlots;

            for (var i = SharedSpellsContext.PactMagicSlotsTab; i <= warlockSpellLevel; i++)
            {
                // don't mess with cantrips
                if (i == 0)
                {
                    continue;
                }

                usedSpellsSlots.TryAdd(i, 0);
                usedSpellsSlots[i]++;
            }

            rulesetSpellRepertoire.RepertoireRefreshed?.Invoke(rulesetSpellRepertoire);
        }

        private static void SpendMulticasterWarlockSlots(
            RulesetSpellRepertoire __instance,
            RulesetCharacterHero heroWithSpellRepertoire,
            int slotLevel)
        {
            var pactMaxSlots = SharedSpellsContext.GetWarlockMaxSlots(heroWithSpellRepertoire);
            var pactUsedSlots = SharedSpellsContext.GetWarlockUsedSlots(heroWithSpellRepertoire);
            var isShiftPressed = Global.CurrentAction is (CharacterActionCastSpell or CharacterActionSpendSpellSlot)
                                 && Global.CurrentAction.actionParams.BoolParameter5;
            var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(heroWithSpellRepertoire);
            var canConsumePactSlot = pactMaxSlots - pactUsedSlots > 0 && slotLevel <= warlockSpellLevel;

            // uses short rest slots across all non race repertoires
            if (canConsumePactSlot &&
                ((__instance.SpellCastingClass == DatabaseHelper.CharacterClassDefinitions.Warlock && !isShiftPressed)
                 || (__instance.SpellCastingClass != DatabaseHelper.CharacterClassDefinitions.Warlock &&
                     isShiftPressed)))
            {
                foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires
                             .Where(x => x.SpellCastingFeature.SpellCastingOrigin !=
                                         FeatureDefinitionCastSpell.CastingOrigin.Race))
                {
                    SpendWarlockSlots(spellRepertoire, heroWithSpellRepertoire);
                }
            }

            // otherwise uses long rest slots across all non race repertoires
            else
            {
                foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires
                             .Where(x => x.SpellCastingFeature.SpellCastingOrigin !=
                                         FeatureDefinitionCastSpell.CastingOrigin.Race))
                {
                    var usedSpellsSlots = spellRepertoire.usedSpellsSlots;

                    usedSpellsSlots.TryAdd(slotLevel, 0);
                    usedSpellsSlots[slotLevel]++;
                    spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
                }
            }
        }
    }

    //PATCH: handles all different scenarios to determine max spell level
    [HarmonyPatch(typeof(RulesetSpellRepertoire), "MaxSpellLevelOfSpellCastingLevel", MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class MaxSpellLevelOfSpellCastingLevel_Getter_Patch
    {
        public static void Postfix(RulesetSpellRepertoire __instance, ref int __result)
        {
            if (__instance.SpellCastingFeature == null)
            {
                return;
            }

            if (__instance.SpellCastingFeature.SpellCastingOrigin is FeatureDefinitionCastSpell.CastingOrigin.Race
                or FeatureDefinitionCastSpell.CastingOrigin.Monster)
            {
                return;
            }

            if (SharedSpellsContext.UseMaxSpellLevelOfSpellCastingLevelDefaultBehavior)
            {
                return;
            }

            var heroWithSpellRepertoire = __instance.GetCasterHero();

            if (heroWithSpellRepertoire == null || !SharedSpellsContext.IsMulticaster(heroWithSpellRepertoire))
            {
                return;
            }

            var sharedSpellLevel = SharedSpellsContext.GetSharedSpellLevel(heroWithSpellRepertoire);
            var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(heroWithSpellRepertoire);

            __result = Math.Max(sharedSpellLevel, warlockSpellLevel);
        }
    }

    //PATCH: handles Arcane Recovery granted spells on short rests
    [HarmonyPatch(typeof(RulesetSpellRepertoire), "RecoverMissingSlots")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class RecoverMissingSlots_Patch
    {
        public static bool Prefix(RulesetSpellRepertoire __instance, Dictionary<int, int> recoveredSlots)
        {
            if (__instance.SpellCastingFeature.SpellCastingOrigin is FeatureDefinitionCastSpell.CastingOrigin.Race
                or FeatureDefinitionCastSpell.CastingOrigin.Monster)
            {
                return true;
            }

            var heroWithSpellRepertoire = __instance.GetCasterHero();

            if (heroWithSpellRepertoire == null)
            {
                return true;
            }

            if (!SharedSpellsContext.IsMulticaster(heroWithSpellRepertoire))
            {
                return true;
            }

            foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires)
            {
                var usedSpellsSlots = spellRepertoire.usedSpellsSlots;

                foreach (var recoveredSlot in recoveredSlots)
                {
                    var key = recoveredSlot.Key;

                    if (usedSpellsSlots.TryGetValue(key, out var used) && used > 0)
                    {
                        usedSpellsSlots[key] = Mathf.Max(0, used - recoveredSlot.Value);
                    }
                }

                spellRepertoire.RepertoireRefreshed?.Invoke(__instance);
            }

            return false;
        }
    }

    //PATCH: only offers upcast Warlock pact at their correct slot level
    [HarmonyPatch(typeof(RulesetSpellRepertoire), "CanUpcastSpell")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class CanUpcastSpell_Patch
    {
        public static void Postfix(
            RulesetSpellRepertoire __instance,
            List<int> availableSlotLevels)
        {
            if (__instance.SpellCastingFeature.SpellCastingOrigin is FeatureDefinitionCastSpell.CastingOrigin.Race
                or FeatureDefinitionCastSpell.CastingOrigin.Monster)
            {
                return;
            }

            if (__instance.SpellCastingClass == DatabaseHelper.CharacterClassDefinitions.Warlock)
            {
                return;
            }

            var heroWithSpellRepertoire = __instance.GetCasterHero();

            if (heroWithSpellRepertoire == null || !SharedSpellsContext.IsMulticaster(heroWithSpellRepertoire))
            {
                return;
            }

            var sharedSpellLevel = SharedSpellsContext.GetSharedSpellLevel(heroWithSpellRepertoire);
            var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(heroWithSpellRepertoire);

            for (var i = sharedSpellLevel + 1; i < warlockSpellLevel; i++)
            {
                availableSlotLevels.Remove(i);
            }
        }
    }

    [HarmonyPatch(typeof(RulesetSpellRepertoire), "FormatHeader")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class FormatHeader_Patch
    {
        public static bool Prefix(RulesetSpellRepertoire __instance, ref string __result)
        {
            //PATCH: prevent null pointer crashes if all origin sources are null
            return FormatTitle(__instance, ref __result);
        }
    }

    [HarmonyPatch(typeof(RulesetSpellRepertoire), "FormatShortHeader")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    public static class FormatShortHeader_Patch
    {
        public static bool Prefix(RulesetSpellRepertoire __instance, ref string __result)
        {
            //PATCH: prevent null pointer crashes if all origin sources are null
            return FormatTitle(__instance, ref __result);
        }
    }
}
