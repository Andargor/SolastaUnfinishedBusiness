﻿using System.Collections.Generic;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFightingStyleChoices;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.FightingStyles;

internal sealed class Torchbearer : AbstractFightingStyle
{
    private static readonly FeatureDefinitionPower PowerFightingStyleTorchbearer = FeatureDefinitionPowerBuilder
        .Create("PowerFightingStyleTorchbearer")
        .SetGuiPresentation(Category.Feature, PowerDragonbornBreathWeaponGold)
        .SetUsesFixed(ActivationTime.BonusAction)
        .SetEffectDescription(EffectDescriptionBuilder
            .Create(SpellDefinitions.Fireball.EffectDescription)
            .SetCanBePlacedOnCharacter(false)
            .SetDurationData(DurationType.Round, 3)
            .SetSpeed(SpeedType.Instant, 11f)
            .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetConditionForm(
                        ConditionDefinitions.ConditionOnFire1D4,
                        ConditionForm.ConditionOperation.Add)
                    .HasSavingThrow(EffectSavingThrowType.Negates, TurnOccurenceType.StartOfTurn)
                    .CreatedByCharacter()
                    .Build())
            .SetSavingThrowData(
                false,
                AttributeDefinitions.Dexterity,
                false,
                EffectDifficultyClassComputation.FixedValue,
                AttributeDefinitions.Dexterity,
                8)
            .Build())
        .SetShowCasting(false)
        .SetCustomSubFeatures(new ValidatorsPowerUse(ValidatorsCharacter.OffHandHasLightSource))
        .AddToDB();

    internal override FightingStyleDefinition FightingStyle { get; } = FightingStyleBuilder
        .Create("Torchbearer")
        .SetGuiPresentation(Category.FightingStyle, CharacterSubclassDefinitions.DomainElementalFire)
        .SetFeatures(
            FeatureDefinitionBuilder
                .Create("AddExtraAttackTorchbearer")
                .SetGuiPresentationNoContent(true)
                .SetCustomSubFeatures(new AddBonusTorchAttack(PowerFightingStyleTorchbearer))
                .AddToDB())
        .AddToDB();

    internal override List<FeatureDefinitionFightingStyleChoice> FightingStyleChoice => new()
    {
        FightingStyleChampionAdditional, FightingStyleFighter, FightingStyleRanger
    };
}
