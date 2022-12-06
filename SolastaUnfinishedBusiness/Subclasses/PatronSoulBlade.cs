﻿using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Subclasses.CommonBuilders;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class PatronSoulBlade : AbstractSubclass
{
    internal PatronSoulBlade()
    {
        var spellListSoulBlade = SpellListDefinitionBuilder
            .Create(SpellListDefinitions.SpellListWizard, "SpellListSoulBlade")
            .SetGuiPresentationNoContent(true)
            .ClearSpells()
            .SetSpellsAtLevel(1, Shield, FalseLife)
            .SetSpellsAtLevel(2, Blur, BrandingSmite)
            .SetSpellsAtLevel(3, Haste, Slow)
            .SetSpellsAtLevel(4, PhantasmalKiller, BlackTentacles)
            .SetSpellsAtLevel(5, ConeOfCold, MindTwist)
            .FinalizeSpells(true, 9)
            .AddToDB();

        var magicAffinitySoulBladeExpandedSpells = FeatureDefinitionMagicAffinityBuilder
            .Create("MagicAffinitySoulBladeExpandedSpells")
            .SetOrUpdateGuiPresentation("MagicAffinityPatronExpandedSpells", Category.Feature)
            .SetExtendedSpellList(spellListSoulBlade)
            .AddToDB();

        var powerSoulBladeEmpowerWeapon = FeatureDefinitionPowerBuilder
            .Create("PowerSoulBladeEmpowerWeapon")
            .SetGuiPresentation(Category.Feature, PowerOathOfDevotionSacredWeapon)
            .SetUniqueInstance()
            .SetCustomSubFeatures(SkipEffectRemovalOnLocationChange.Always)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetDurationData(DurationType.Permanent)
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Item,
                    //TODO: with new Inventor code we can make it RAW: implement target limiter for the weapon to work on 1-hand or pact weapon
                    itemSelectionType: ActionDefinitions.ItemSelectionType.Weapon)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetItemPropertyForm(
                        ItemPropertyUsage.Unlimited,
                        1, new FeatureUnlockByLevel(
                            FeatureDefinitionAttackModifierBuilder
                                .Create("AttackModifierSoulBladeEmpowerWeapon")
                                .SetGuiPresentation(Category.Feature, PowerOathOfDevotionSacredWeapon)
                                .SetMagicalWeapon()
                                .SetAbilityScoreReplacement(AbilityScoreReplacement.SpellcastingAbility)
                                .AddToDB(),
                            0))
                    .Build())
                .Build())
            .SetBonusToAttack(true, true, AttributeDefinitions.Charisma)
            .AddToDB();

        var powerSoulBladeSummonPactWeapon = FeatureDefinitionPowerBuilder
            .Create(PowerTraditionShockArcanistArcaneFury, "PowerSoulBladeSummonPactWeapon")
            .SetOrUpdateGuiPresentation(Category.Feature, SpiritualWeapon)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(SpiritualWeapon.EffectDescription)
                    .Build())
            .AddToDB();

        powerSoulBladeSummonPactWeapon.EffectDescription.savingThrowDifficultyAbility = AttributeDefinitions.Charisma;

        var powerSoulBladeSoulShield = FeatureDefinitionPowerBuilder
            .Create("PowerSoulBladeSoulShield")
            .SetGuiPresentation(Category.Feature, PowerFighterSecondWind)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(PowerFighterSecondWind.EffectDescription)
                    .SetDurationData(DurationType.UntilLongRest)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetTempHpForm(-1, DieType.D1, 1)
                            .SetBonusMode(AddBonusMode.AbilityBonus)
                            .SetLevelAdvancement(EffectForm.LevelApplianceType.AddBonus, LevelSourceType.ClassLevel)
                            .Build())
                    .Build())
            .AddToDB();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("PatronSoulBlade")
            .SetGuiPresentation(Category.Subclass, CharacterSubclassDefinitions.OathOfTheMotherland)
            .AddFeaturesAtLevel(1,
                FeatureSetCasterFightingProficiency,
                magicAffinitySoulBladeExpandedSpells,
                powerSoulBladeEmpowerWeapon)
            .AddFeaturesAtLevel(6,
                powerSoulBladeSummonPactWeapon)
            .AddFeaturesAtLevel(10,
                powerSoulBladeSoulShield)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceWarlockOtherworldlyPatrons;
}
