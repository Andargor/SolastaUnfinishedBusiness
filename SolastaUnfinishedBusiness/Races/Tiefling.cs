﻿using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using TA;
using static FeatureDefinitionAttributeModifier;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterRaceDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionMoveModes;
using static SolastaUnfinishedBusiness.Races.DarkelfSubraceBuilder;

namespace SolastaUnfinishedBusiness.Races;

internal static class TieflingRaceBuilder
{
    internal static CharacterRaceDefinition RaceTiefling { get; } = BuildTiefling();

    [NotNull]
    private static CharacterRaceDefinition BuildTiefling()
    {
        #region subraces

        var tieflingSpriteReference = Sprites.GetSprite("Tiefling", Resources.HalfDarkelf, 1024, 512);

        var attributeModifierTieflingIntelligenceAbilityScoreIncrease = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierTieflingIntelligenceAbilityScoreIncrease")
            .SetGuiPresentation(Category.Feature)
            .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.Intelligence, 1)
            .AddToDB();

        var castSpellTieflingAsmodeus = FeatureDefinitionCastSpellBuilder
            .Create("CastSpellTieflingAsmodeus")
            .SetGuiPresentation(Category.Feature)
            .SetSpellCastingOrigin(FeatureDefinitionCastSpell.CastingOrigin.Race)
            .SetSpellCastingAbility(AttributeDefinitions.Charisma)
            .SetSpellKnowledge(SpellKnowledge.FixedList)
            .SetSpellReadyness(SpellReadyness.AllKnown)
            .SetSlotsRecharge(RechargeRate.LongRest)
            .SetSlotsPerLevel(SharedSpellsContext.RaceCastingSlots)
            .SetKnownCantrips(1, 1, FeatureDefinitionCastSpellBuilder.CasterProgression.Flat)
            .SetSpellList(SpellListDefinitionBuilder
                .Create("SpellListTieflingAsmodeus")
                .SetGuiPresentationNoContent(true)
                .ClearSpells()
                .SetSpellsAtLevel(0, SpellDefinitions.DancingLights)
                .SetSpellsAtLevel(1, SpellDefinitions.HellishRebuke)
                .SetSpellsAtLevel(2, SpellDefinitions.Darkness)
                .FinalizeSpells(true, -1)
                .AddToDB())
            .AddToDB();

        var raceTieflingAsmodeus = CharacterRaceDefinitionBuilder
            .Create(SubraceDarkelf, "RaceTieflingAsmodeus")
            .SetGuiPresentation(Category.Race, tieflingSpriteReference)
            .SetFeaturesAtLevel(1,
                attributeModifierTieflingIntelligenceAbilityScoreIncrease,
                castSpellTieflingAsmodeus)
            .AddToDB();

        var attributeModifierTieflingDexterityAbilityScoreIncrease = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierTieflingDexterityAbilityScoreIncrease")
            .SetGuiPresentation(Category.Feature)
            .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.Dexterity, 1)
            .AddToDB();

        var castSpellTieflingMephistopheles = FeatureDefinitionCastSpellBuilder
            .Create("CastSpellTieflingMephistopheles")
            .SetGuiPresentation(Category.Feature)
            .SetSpellCastingOrigin(FeatureDefinitionCastSpell.CastingOrigin.Race)
            .SetSpellCastingAbility(AttributeDefinitions.Charisma)
            .SetSpellKnowledge(SpellKnowledge.FixedList)
            .SetSpellReadyness(SpellReadyness.AllKnown)
            .SetSlotsRecharge(RechargeRate.LongRest)
            .SetSlotsPerLevel(SharedSpellsContext.RaceCastingSlots)
            .SetKnownCantrips(1, 1, FeatureDefinitionCastSpellBuilder.CasterProgression.Flat)
            .SetSpellList(SpellListDefinitionBuilder
                .Create("SpellListTieflingMephistopheles")
                .SetGuiPresentationNoContent(true)
                .ClearSpells()
                .SetSpellsAtLevel(0, SpellDefinitions.FireBolt)
                .SetSpellsAtLevel(1, SpellDefinitions.BurningHands)
                .SetSpellsAtLevel(2, SpellDefinitions.FlameBlade)
                .FinalizeSpells(true, -1)
                .AddToDB())
            .AddToDB();

        var raceTieflingMephistopheles = CharacterRaceDefinitionBuilder
            .Create(ElfHigh, "RaceTieflingMephistopheles")
            .SetGuiPresentation(Category.Race, tieflingSpriteReference)
            .SetFeaturesAtLevel(1,
                attributeModifierTieflingDexterityAbilityScoreIncrease,
                castSpellTieflingMephistopheles)
            .AddToDB();

        var attributeModifierTieflingStrengthAbilityScoreIncrease = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierTieflingStrengthAbilityScoreIncrease")
            .SetGuiPresentation(Category.Feature)
            .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.Strength, 1)
            .AddToDB();

        var castSpellTieflingZariel = FeatureDefinitionCastSpellBuilder
            .Create("CastSpellTieflingZariel")
            .SetGuiPresentation(Category.Feature)
            .SetSpellCastingOrigin(FeatureDefinitionCastSpell.CastingOrigin.Race)
            .SetSpellCastingAbility(AttributeDefinitions.Charisma)
            .SetSpellKnowledge(SpellKnowledge.FixedList)
            .SetSpellReadyness(SpellReadyness.AllKnown)
            .SetSlotsRecharge(RechargeRate.LongRest)
            .SetSlotsPerLevel(SharedSpellsContext.RaceCastingSlots)
            .SetKnownCantrips(1, 1, FeatureDefinitionCastSpellBuilder.CasterProgression.Flat)
            .SetSpellList(SpellListDefinitionBuilder
                .Create("SpellListTieflingZariel")
                .SetGuiPresentationNoContent(true)
                .ClearSpells()
                .SetSpellsAtLevel(0, SpellsContext.SunlightBlade)
                .SetSpellsAtLevel(1, SpellDefinitions.FaerieFire)
                .SetSpellsAtLevel(2, SpellDefinitions.BrandingSmite)
                .FinalizeSpells(true, -1)
                .AddToDB())
            .AddToDB();

        var raceTieflingZariel = CharacterRaceDefinitionBuilder
            .Create(ElfSylvan, "RaceTieflingZariel")
            .SetGuiPresentation(Category.Race, tieflingSpriteReference)
            .SetFeaturesAtLevel(1,
                attributeModifierTieflingStrengthAbilityScoreIncrease,
                castSpellTieflingZariel)
            .AddToDB();

        #endregion

        #region Main Race

        var attributeModifierTieflingCharismaAbilityScoreIncrease = FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierTieflingCharismaAbilityScoreIncrease")
            .SetGuiPresentation(Category.Feature)
            .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.Charisma, 2)
            .AddToDB();

        var damageAffinityTieflingHellishResistance = FeatureDefinitionDamageAffinityBuilder
            .Create("DamageAffinityTieflingHellishResistance")
            .SetGuiPresentation(Category.Feature)
            .SetDamageAffinityType(DamageAffinityType.Resistance)
            .SetDamageType(DamageTypeFire)
            .AddToDB();

        var languageInfernal = LanguageDefinitionBuilder
            .Create("LanguageInfernal")
            .SetGuiPresentation(Category.Language)
            .AddToDB();

        var proficiencyTieflingLanguages = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencyTieflingLanguages")
            .SetGuiPresentation(Category.Feature)
            .SetProficiencies(ProficiencyType.Language, "Language_Common", languageInfernal.Name)
            .AddToDB();

        var tieflingRacePresentation = Elf.RacePresentation.DeepCopy();
        var newMorphotypeCategories = new List<MorphotypeElementDefinition.ElementCategory>(
            tieflingRacePresentation.availableMorphotypeCategories)
        {
            MorphotypeElementDefinition.ElementCategory.Horns
        };

        tieflingRacePresentation.availableMorphotypeCategories = newMorphotypeCategories.ToArray();
        tieflingRacePresentation.femaleNameOptions = new List<string>();
        tieflingRacePresentation.maleNameOptions = new List<string>();
        tieflingRacePresentation.surNameOptions = new List<string>();
        tieflingRacePresentation.maleHornsOptions = Dragonborn.RacePresentation.maleHornsOptions;
        tieflingRacePresentation.femaleHornsOptions = Dragonborn.RacePresentation.femaleHornsOptions;

        #endregion

        var raceTiefling = CharacterRaceDefinitionBuilder
            .Create(ElfHigh, "RaceTiefling")
            .SetGuiPresentation(Category.Race, tieflingSpriteReference)
            .SetRacePresentation(tieflingRacePresentation)
            .SetSizeDefinition(CharacterSizeDefinitions.Medium)
            .SetMinimalAge(20)
            .SetMaximalAge(120)
            .SetFeaturesAtLevel(1,
                MoveModeMove6,
                FeatureDefinitionSenses.SenseSuperiorDarkvision,
                proficiencyTieflingLanguages,
                attributeModifierTieflingCharismaAbilityScoreIncrease,
                damageAffinityTieflingHellishResistance)
            .AddToDB();

        raceTiefling.subRaces.SetRange(raceTieflingAsmodeus, raceTieflingMephistopheles, raceTieflingZariel);

        return raceTiefling;
    }
}

