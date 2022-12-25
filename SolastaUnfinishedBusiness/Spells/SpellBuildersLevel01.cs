﻿using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Properties;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Spells;

internal static partial class SpellBuilders
{
    #region LEVEL 01

    internal static SpellDefinition BuildChromaticOrb()
    {
        const string NAME = "ChromaticOrb";

        var sprite = Sprites.GetSprite(NAME, Resources.ChromaticOrb, 128);
        var subSpells = new SpellDefinition[6];
        var particleTypes = new[] { AcidSplash, ConeOfCold, FireBolt, LightningBolt, PoisonSpray, Thunderwave };
        var damageTypes = new[]
        {
            DamageTypeAcid, DamageTypeCold, DamageTypeFire, DamageTypeLightning, DamageTypePoison, DamageTypeThunder
        };

        for (var i = 0; i < subSpells.Length; i++)
        {
            var damageType = damageTypes[i];
            var particleType = particleTypes[i];
            var title = Gui.Localize($"Tooltip/&Tag{damageType}Title");
            var spell = SpellDefinitionBuilder
                .Create(NAME + damageType)
                .SetGuiPresentation(
                    title,
                    Gui.Format("Spell/&SubSpellChromaticOrbDescription", title),
                    sprite)
                .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
                .SetSpellLevel(1)
                .SetMaterialComponent(MaterialComponentType.Specific)
                .SetSpecificMaterialComponent(TagsDefinitions.ItemTagDiamond, 50, false)
                .SetVocalSpellSameType(VocalSpellSemeType.Attack)
                .SetCastingTime(ActivationTime.Action)
                .SetEffectDescription(EffectDescriptionBuilder
                    .Create()
                    .SetTargetFiltering(TargetFilteringMethod.CharacterOnly)
                    .SetTargetingData(Side.Enemy, RangeType.RangeHit, 12, TargetType.Individuals)
                    .SetDurationData(DurationType.Instantaneous)
                    .SetEffectForms(EffectFormBuilder.Create()
                        .SetDamageForm(damageType, 3, DieType.D8)
                        .Build())
                    .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, 1,
                        additionalDicePerIncrement: 1)
                    .SetParticleEffectParameters(particleType)
                    .SetSpeed(SpeedType.CellsPerSeconds, 8.5f)
                    .SetupImpactOffsets(offsetImpactTimePerTarget: 0.1f)
                    .Build())
                .SetSubSpells()
                .AddToDB();

            subSpells[i] = spell;
        }

        return SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, sprite)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(1)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetSpecificMaterialComponent(TagsDefinitions.ItemTagDiamond, 50, false)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetCastingTime(ActivationTime.Action)
            .SetSubSpells(subSpells)
            .AddToDB();
    }

    internal static SpellDefinition BuildMule()
    {
        const string NAME = "Mule";

        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetTargetingData(Side.Ally, RangeType.Touch, 1, TargetType.IndividualsUnique)
            .SetDurationData(DurationType.Hour, 8)
            .SetParticleEffectParameters(ExpeditiousRetreat)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetConditionForm(
                        ConditionDefinitionBuilder
                            .Create($"Condition{NAME}")
                            .SetGuiPresentation(Category.Condition, Longstrider)
                            .SetFeatures(
                                FeatureDefinitionMovementAffinityBuilder
                                    .Create($"MovementAffinity{NAME}")
                                    .SetGuiPresentationNoContent(true)
                                    .SetImmunities(true, true)
                                    .AddToDB(),
                                FeatureDefinitionEquipmentAffinityBuilder
                                    .Create($"EquipmentAffinity{NAME}")
                                    .SetGuiPresentationNoContent(true)
                                    .SetAdditionalCarryingCapacity(20)
                                    .AddToDB())
                            .AddToDB(),
                        ConditionForm.ConditionOperation.Add,
                        false,
                        false,
                        ConditionJump.AdditionalCondition)
                    .Build())
            .Build();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Longstrider)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolTransmutation)
            .SetSpellLevel(1)
            .SetCastingTime(ActivationTime.Action)
            .SetConcentrationAction(ActionDefinitions.ActionParameter.None)
            .SetEffectDescription(effectDescription)
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildRadiantMotes()
    {
        const string NAME = "RadiantMotes";

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.SpellRadiantMotes, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(1)
            .SetMaterialComponent(MaterialComponentType.None)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetCastingTime(ActivationTime.Action)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetFiltering(TargetFilteringMethod.AllCharacterAndGadgets)
                .SetTargetingData(Side.Enemy, RangeType.RangeHit, 12, TargetType.Individuals, 4)
                .SetDurationData(DurationType.Minute, 1)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetDamageForm(DamageTypeRadiant, 1, DieType.D4)
                    .Build())
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, 1, 1)
                .SetParticleEffectParameters(Sparkle)
                .SetSpeed(SpeedType.CellsPerSeconds, 20)
                .SetupImpactOffsets(offsetImpactTimePerTarget: 0.1f)
                .Build())
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildSearingSmite()
    {
        const string NAME = "SearingSmite";

        var sprite = Sprites.GetSprite(NAME, Resources.SearingSmite, 128);

        var additionalDamageSearingSmite = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{NAME}")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag(NAME)
            .SetDamageDice(DieType.D6, 1)
            .SetAdditionalDamageType(AdditionalDamageType.Specific)
            .SetAdvancement(AdditionalDamageAdvancement.SlotLevel, 1)
            .SetSpecificDamageType(DamageTypeFire)
            .SetConditionOperations(
                new ConditionOperationDescription
                {
                    hasSavingThrow = true,
                    canSaveToCancel = true,
                    saveAffinity = EffectSavingThrowType.Negates,
                    saveOccurence = TurnOccurenceType.StartOfTurn,
                    conditionDefinition = ConditionOnFire1D4,
                    operation = ConditionOperationDescription.ConditionOperation.Add
                })
            .AddToDB();

        var conditionBrandingSmite = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation(NAME, Category.Spell, ConditionBrandingSmite)
            .SetFeatures(additionalDamageSearingSmite)
            .SetSpecialInterruptions(ConditionInterruption.AttacksAndDamages)
            .AddToDB();

        var spell = SpellDefinitionBuilder
            .Create(BrandingSmite, NAME)
            .SetGuiPresentation(Category.Spell, sprite)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(1)
            .SetCastingTime(ActivationTime.BonusAction)
            .SetVerboseComponent(true)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Minute, 1)
                .SetEffectForms(EffectFormBuilder
                    .Create()
                    .SetConditionForm(conditionBrandingSmite, ConditionForm.ConditionOperation.Add)
                    .Build())
                .Build())
            .AddToDB();

        return spell;
    }

    #endregion
}
