﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FeatureDefinitionFeatureSet;

namespace SolastaUnfinishedBusiness.Models;

internal static class MulticlassWildshapeContext
{
    private const string TagWildShape = "99WildShape";
    private const string NaturalAcTitle = "Tooltip/&MonsterNaturalArmorTitle";
    private const string TagMonsterBase = "<Base>";
    private const string TagNaturalAc = "<NaturalArmor>";

    private static readonly List<string> AllowedAttributes = new()
    {
        AttributeDefinitions.RageDamage,
        AttributeDefinitions.RagePoints,
        AttributeDefinitions.KiPoints,
        AttributeDefinitions.SorceryPoints,
        AttributeDefinitions.BardicInspirationDie,
        AttributeDefinitions.BardicInspirationNumber
    };

    private static readonly List<string> MentalAttributes = new()
    {
        AttributeDefinitions.Intelligence, AttributeDefinitions.Wisdom, AttributeDefinitions.Charisma
    };

    internal static void FinalizeMonster(RulesetCharacterMonster monster, bool keepMentalAbilityScores)
    {
        UpdateAttributeModifiers(monster, keepMentalAbilityScores);
        FixShapeShiftedAc(monster);
    }

    private static void UpdateAttributeModifiers(RulesetCharacterMonster monster, bool keepMentalAbilityScores)
    {
        if (monster.originalFormCharacter is not RulesetCharacterHero hero)
        {
            return;
        }

        // copy modifiers from original hero
        hero.EnumerateFeaturesToBrowse<FeatureDefinitionAttributeModifier>(monster.FeaturesToBrowse);

        foreach (var feature in monster.FeaturesToBrowse)
        {
            if (feature is not FeatureDefinitionAttributeModifier mod
                || !AllowedAttributes.Contains(mod.ModifiedAttribute)
                || !monster.TryGetAttribute(mod.ModifiedAttribute, out _))
            {
                continue;
            }

            mod.ApplyModifiers(monster.Attributes, TagWildShape);
        }

        // TA copies only base values of hero, let's copy current value to not tackle modifiers
        if (keepMentalAbilityScores)
        {
            foreach (var attribute in MentalAttributes)
            {
                var heroAttr = hero.GetAttribute(attribute);
                var monsterAttr = monster.GetAttribute(attribute);
                monsterAttr.BaseValue = heroAttr.BaseValue;
                //copy all race/class/subclass modifiers
                monsterAttr.ActiveModifiers.AddRange(heroAttr.ActiveModifiers
                    .Where(x => x.Tags.Any(t => t.Contains(AttributeDefinitions.TagRace)
                                                || t.Contains(AttributeDefinitions.TagClass)
                                                || t.Contains(AttributeDefinitions.TagSubclass))));
            }
        }

        // sync various spent pools
        monster.usedKiPoints = hero.usedKiPoints;
        monster.usedRagePoints = hero.usedRagePoints;
        monster.usedSorceryPoints = hero.usedSorceryPoints;
        monster.usedBardicInspiration = hero.usedBardicInspiration;
    }

    private static void FixShapeShiftedAc(RulesetCharacterMonster monster)
    {
        if (monster.originalFormCharacter is not RulesetCharacterHero)
        {
            return;
        }

        //for some reason TA didn't set armor properly and many game checks consider these forms as armored (1.4.24)
        //set armor to 'Natural' as intended
        monster.MonsterDefinition.armor = EquipmentDefinitions.EmptyMonsterArmor;

        var ac = monster.GetAttribute(AttributeDefinitions.ArmorClass);

        //Vanilla game (as of 1.4.24) sets this to monster's AC from definition
        //this breaks many AC stacking rules
        //set base AC to 0, so we can properly apply modifiers to it
        ac.BaseValue = 0;

        //basic AC - sets AC to 10
        var mod = RulesetAttributeModifier.BuildAttributeModifier(
            FeatureDefinitionAttributeModifier.AttributeModifierOperation.Set,
            10, TagMonsterBase
        );

        ac.AddModifier(mod);

        //natural armor of the monster
        mod = RulesetAttributeModifier.BuildAttributeModifier(
            FeatureDefinitionAttributeModifier.AttributeModifierOperation.ForceIfBetter,
            monster.MonsterDefinition.ArmorClass, TagNaturalAc
        );

        ac.AddModifier(mod);

        //DEX bonus to AC
        mod = RulesetAttributeModifier.BuildAttributeModifier(
            FeatureDefinitionAttributeModifier.AttributeModifierOperation.AddAbilityScoreBonus,
            0,
            AttributeDefinitions.TagAbilityScore,
            AttributeDefinitions.Dexterity
        );

        ac.AddModifier(mod);
    }

    internal static void RefreshWildShapeAcFeatures(RulesetCharacterMonster monster, RulesetAttribute ac)
    {
        if (monster.OriginalFormCharacter is not RulesetCharacterHero hero)
        {
            return;
        }

        var ruleset = ServiceRepository.GetService<IRulesetImplementationService>();

        ac.RemoveModifiersByTags(TagWildShape);

        monster.FeaturesToBrowse.Clear();

        foreach (var pair in hero.ActiveFeatures)
        {
            //process only class and subclass features, feats are processed on base method
            if (!pair.Key.Contains(AttributeDefinitions.TagClass)
                && !pair.Key.Contains(AttributeDefinitions.TagSubclass))
            {
                continue;
            }

            EnumerateFeaturesToBrowseHierarchicaly<FeatureDefinition>(pair.Value, monster.FeaturesToBrowse,
                RuleDefinitions.FeatureSourceType.MonsterFeature, null, hero);
        }

        monster.RefreshArmorClassInFeatures(ruleset, ac, monster.FeaturesToBrowse, TagWildShape,
            RuleDefinitions.FeatureSourceType.CharacterFeature, string.Empty);
    }

    internal static void UpdateWildShapeAcTrends(List<RulesetAttributeModifier> modifiers,
        RulesetCharacterMonster monster, RulesetAttribute ac)
    {
        //Add trends for built-in AC mods (base ac, natural armor, dex bonus)
        foreach (var mod in modifiers)
        {
            if (mod.Tags.Contains(TagMonsterBase))
            {
                ac.ValueTrends.Add(new RuleDefinitions.TrendInfo((int)mod.value,
                    RuleDefinitions.FeatureSourceType.Base, string.Empty, monster, mod) { additive = false });
            }
            else if (mod.Tags.Contains(TagNaturalAc))
            {
                ac.ValueTrends.Add(new RuleDefinitions.TrendInfo((int)mod.value,
                    RuleDefinitions.FeatureSourceType.ExplicitFeature, NaturalAcTitle, monster, mod)
                {
                    additive = false
                });
            }
            else if (mod.operation == FeatureDefinitionAttributeModifier.AttributeModifierOperation.AddAbilityScoreBonus
                     && mod.SourceAbility == AttributeDefinitions.Dexterity)
            {
                ac.ValueTrends.Add(new RuleDefinitions.TrendInfo(Mathf.RoundToInt(mod.Value),
                    RuleDefinitions.FeatureSourceType.AbilityScore, mod.SourceAbility, monster,
                    mod) { additive = true });
            }
        }
    }
}
