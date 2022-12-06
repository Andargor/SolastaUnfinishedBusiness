﻿using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static RuleDefinitions.RollContext;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionActionAffinitys;

namespace SolastaUnfinishedBusiness.Feats;

internal static class MeleeCombatFeats
{
    private static readonly FeatureDefinitionDieRollModifier DieRollModifierFeatPiercerNonMagic =
        FeatureDefinitionDieRollModifierBuilder
            .Create("DieRollModifierFeatPiercerNonMagic")
            .SetGuiPresentationNoContent(true)
            .SetModifiers(AttackDamageValueRoll, 1, 1, 1, "Feat/&FeatPiercerReroll")
            .AddToDB();

    private static readonly FeatureDefinition FeatureFeatPiercer = FeatureDefinitionBuilder
        .Create("FeatureFeatPiercer")
        .SetGuiPresentationNoContent(true)
        .SetCustomSubFeatures(new IsCriticalAndSpecificDamageType(FeatureDefinitionAdditionalDamageBuilder
                .Create("AdditionalDamageFeatPiercer")
                .SetGuiPresentationNoContent(true)
                .SetNotificationTag("Piercer")
                .SetDamageValueDetermination(AdditionalDamageValueDetermination.SameAsBaseWeaponDie)
                .AddToDB(),
            DamageTypePiercing))
        .AddToDB();

    internal static FeatDefinition FeatGroupPiercer { get; private set; }

    internal static void CreateFeats([NotNull] List<FeatDefinition> feats)
    {
        var featPowerAttack = BuildPowerAttack();
        var featRecklessAttack = BuildRecklessAttack();
        var featSavageAttack = BuildSavageAttack();
        var featPiercerDex = BuildPiercerDex();
        var featPiercerStr = BuildPiercerStr();

        feats.AddRange(featPowerAttack, featRecklessAttack, featSavageAttack, featPiercerDex, featPiercerStr);

        FeatGroupPiercer = GroupFeats.MakeGroup("FeatGroupPiercer", "Piercer",
            featPiercerDex,
            featPiercerStr);

        GroupFeats.MakeGroup("FeatGroupMeleeCombat", null,
            FeatDefinitions.CloakAndDagger,
            FeatDefinitions.DauntingPush,
            FeatDefinitions.DistractingGambit,
            FeatDefinitions.TripAttack,
            featPowerAttack,
            featRecklessAttack,
            featSavageAttack,
            FeatGroupPiercer);
    }

    private static FeatDefinition BuildPowerAttack()
    {
        var concentrationProvider = new StopPowerConcentrationProvider("PowerAttack",
            "Tooltip/&PowerAttackConcentration",
            Sprites.GetSprite("PowerAttackConcentrationIcon", Resources.PowerAttackConcentrationIcon, 64, 64));

        var conditionPowerAttackTrigger = ConditionDefinitionBuilder
            .Create("ConditionPowerAttackTrigger")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(FeatureDefinitionBuilder
                .Create("TriggerFeaturePowerAttack")
                .SetGuiPresentationNoContent(true)
                .SetCustomSubFeatures(concentrationProvider)
                .AddToDB())
            .AddToDB();

        var conditionPowerAttack = ConditionDefinitionBuilder
            .Create("ConditionPowerAttack")
            .SetGuiPresentation("FeatPowerAttack", Category.Feat, ConditionDefinitions.ConditionHeraldOfBattle)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(
                FeatureDefinitionBuilder
                    .Create("ModifyAttackModeForWeaponFeatPowerAttack")
                    .SetGuiPresentation("FeatPowerAttack", Category.Feat)
                    .SetCustomSubFeatures(new ModifyPowerAttackPower())
                    .AddToDB())
            .AddToDB();

        var powerAttack = FeatureDefinitionPowerBuilder
            .Create("PowerAttack")
            .SetGuiPresentation("FeatPowerAttack", Category.Feat,
                Sprites.GetSprite("PowerAttackIcon", Resources.PowerAttackIcon, 128, 64))
            .SetUsesFixed(ActivationTime.NoCost)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Permanent)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionPowerAttackTrigger, ConditionForm.ConditionOperation.Add)
                        .Build(),
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionPowerAttack, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .AddToDB();

        Global.PowersThatIgnoreInterruptions.Add(powerAttack);

        var powerTurnOffPowerAttack = FeatureDefinitionPowerBuilder
            .Create("PowerTurnOffPowerAttack")
            .SetGuiPresentationNoContent(true)
            .SetUsesFixed(ActivationTime.NoCost)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Round, 1)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionPowerAttackTrigger, ConditionForm.ConditionOperation.Remove)
                        .Build(),
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionPowerAttack, ConditionForm.ConditionOperation.Remove)
                        .Build())
                .Build())
            .AddToDB();

        Global.PowersThatIgnoreInterruptions.Add(powerTurnOffPowerAttack);
        concentrationProvider.StopPower = powerTurnOffPowerAttack;

        return FeatDefinitionBuilder
            .Create("FeatPowerAttack")
            .SetGuiPresentation(Category.Feat,
                Gui.Format("Feat/&FeatPowerAttackDescription", Main.Settings.DeadEyeAndPowerAttackBaseValue.ToString()))
            .SetFeatures(
                powerAttack,
                powerTurnOffPowerAttack
            )
            .AddToDB();
    }

    private static FeatDefinition BuildRecklessAttack()
    {
        return FeatDefinitionWithPrerequisitesBuilder
            .Create("FeatRecklessAttack")
            .SetGuiPresentation("RecklessAttack", Category.Action)
            .SetFeatures(ActionAffinityBarbarianRecklessAttack)
            .SetValidators(ValidatorsFeat.ValidateNotClass(CharacterClassDefinitions.Barbarian))
            .AddToDB();
    }

    private static FeatDefinition BuildSavageAttack()
    {
        return FeatDefinitionBuilder
            .Create("FeatSavageAttack")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionDieRollModifierBuilder
                    .Create("DieRollModifierFeatSavageAttackNonMagic")
                    .SetGuiPresentationNoContent(true)
                    .SetModifiers(AttackDamageValueRoll, 1, 1, 1, "Feat/&FeatSavageAttackReroll")
                    .AddToDB(),
                FeatureDefinitionDieRollModifierBuilder
                    .Create("DieRollModifierFeatSavageAttackMagic")
                    .SetGuiPresentationNoContent(true)
                    .SetModifiers(MagicDamageValueRoll, 1, 1, 1, "Feat/&FeatSavageAttackReroll")
                    .AddToDB())
            .AddToDB();
    }

    private static FeatDefinition BuildPiercerDex()
    {
        return FeatDefinitionBuilder
            .Create("FeatPiercerDex")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionAttributeModifiers.AttributeModifierCreed_Of_Misaye,
                DieRollModifierFeatPiercerNonMagic,
                FeatureFeatPiercer)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Dexterity, 13)
            .AddToDB();
    }

    private static FeatDefinition BuildPiercerStr()
    {
        return FeatDefinitionBuilder
            .Create("FeatPiercerStr")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionAttributeModifiers.AttributeModifierCreed_Of_Einar,
                DieRollModifierFeatPiercerNonMagic,
                FeatureFeatPiercer)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Strength, 13)
            .AddToDB();
    }

    //
    // HELPERS
    //

    private sealed class ModifyPowerAttackPower : IModifyAttackModeForWeapon
    {
        public void ModifyAttackMode(RulesetCharacter character, RulesetAttackMode attackMode)
        {
            if (attackMode == null ||
                !(ValidatorsWeapon.IsMelee(attackMode) || ValidatorsWeapon.IsUnarmedWeapon(attackMode)))
            {
                return;
            }

            SrdAndHouseRulesContext.ModifyAttackModeAndDamage(character, "Feat/&FeatPowerAttackTitle", attackMode);
        }
    }

    private sealed class IsCriticalAndSpecificDamageType : CustomAdditionalDamage
    {
        private readonly string _damageType;

        public IsCriticalAndSpecificDamageType(IAdditionalDamageProvider provider, string damageType) : base(provider)
        {
            _damageType = damageType;
        }

        internal override bool IsValid(
            GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            RulesetEffect rulesetEffect,
            bool criticalHit,
            bool firstTarget,
            out CharacterActionParams reactionParams)
        {
            reactionParams = null;

            var damage = attackMode?.EffectDescription?.FindFirstDamageForm();

            return criticalHit && damage != null && damage.DamageType == _damageType;
        }
    }
}
