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

#if false
// Monster/&OwlFamiliarDescription=Owl Familiar.
// Monster/&OwlFamiliarTitle=Owl
// Spell/&FindFamiliarDescription=You gain the service of a familiar.
// Spell/&FindFamiliarTitle=Find Familiar
    internal static SpellDefinition BuildFindFamiliar()
    {
        var owlFamiliar = MonsterDefinitionBuilder
            .Create(Eagle_Matriarch, "OwlFamiliar")
            .SetGuiPresentation("OwlFamiliar", Category.Monster, Eagle_Matriarch)
            .SetFeatures(
                FeatureDefinitionSenses.SenseNormalVision,
                FeatureDefinitionSenses.SenseDarkvision24,
                FeatureDefinitionMoveModes.MoveModeMove2,
                FeatureDefinitionMoveModes.MoveModeFly12,
                FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityKeenSight,
                FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityKeenHearing,
                FeatureDefinitionCombatAffinitys.CombatAffinityFlyby,
                FeatureDefinitionMovementAffinitys.MovementAffinityNoClimb,
                FeatureDefinitionMovementAffinitys.MovementAffinityNoSpecialMoves,
                FeatureDefinitionConditionAffinitys.ConditionAffinityProneImmunity,
                CharacterContext.FeatureDefinitionPowerHelpAction)
            .ClearAttackIterations()
            .SetSkillScores(
                (DatabaseHelper.SkillDefinitions.Perception.Name, 3),
                (DatabaseHelper.SkillDefinitions.Stealth.Name, 3))
            .SetArmorClass(11)
            .SetAbilityScores(3, 13, 8, 2, 12, 7)
            .SetHitDiceNumber(1)
            .SetHitDiceType(DieType.D4)
            .SetHitPointsBonus(-1)
            .SetStandardHitPoints(1)
            .SetSizeDefinition(CharacterSizeDefinitions.Tiny)
            .SetAlignment("Neutral")
            .SetCharacterFamily(CharacterFamilyDefinitions.Fey)
            .SetChallengeRating(0)
            .SetDroppedLootDefinition(null)
            .SetDefaultBattleDecisionPackage(DecisionPackageDefinitions.DefaultSupportCasterWithBackupAttacksDecisions)
            .SetFullyControlledWhenAllied(true)
            .SetDefaultFaction(FactionDefinitions.Party)
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.None)
            .AddToDB();

        var spell = SpellDefinitionBuilder.Create(Fireball, "FindFamiliar")
            .SetGuiPresentation(Category.Spell, AnimalFriendship)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolConjuration)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetVocalSpellSameType(VocalSpellSemeType.Buff)
            .SetSpellLevel(1)
            .SetUniqueInstance()
            .SetCastingTime(ActivationTime.Minute10)
            .SetRitualCasting(ActivationTime.Minute10)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create(ConjureAnimalsOneBeast.EffectDescription)
                .SetTargetingData(Side.Ally, RangeType.Distance, 2, TargetType.Position)
                .SetDurationData(DurationType.Permanent)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetSummonCreatureForm(1, owlFamiliar.name)
                        .CreatedByCharacter()
                        .Build())
                .Build())
            .AddToDB();

        GlobalUniqueEffects.AddToGroup(GlobalUniqueEffects.Group.Familiar, spell);

        return spell;
    }
#endif

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
                            .Create("ConditionMule")
                            .SetGuiPresentation(Category.Condition, Longstrider)
                            .SetFeatures(
                                FeatureDefinitionMovementAffinityBuilder
                                    .Create("MovementAffinityConditionMule")
                                    .SetGuiPresentationNoContent(true)
                                    .SetImmunities(true, true)
                                    .AddToDB(),
                                FeatureDefinitionEquipmentAffinityBuilder
                                    .Create("EquipmentAffinityConditionMule")
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
            .SetMaterialComponent(MaterialComponentType.Mundane)
            .SetEffectDescription(effectDescription)
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildRadiantMotes()
    {
        const string NAME = "RadiantMotes";

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell,
                Sprites.GetSprite("SpellRadiantMotes", Resources.SpellRadiantMotes, 128))
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
                    .SetDamageForm(dieType: DieType.D4, diceNumber: 1, damageType: DamageTypeRadiant)
                    .Build())
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, 1, 1)
                .SetParticleEffectParameters(Sparkle)
                .SetSpeed(SpeedType.CellsPerSeconds, 20)
                .SetupImpactOffsets(offsetImpactTimePerTarget: 0.1f)
                .Build())
            .AddToDB();

        return spell;
    }

    #endregion
}
