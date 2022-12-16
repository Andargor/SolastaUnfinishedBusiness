﻿using System.Collections;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomInterfaces;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class RoguishOpportunist : AbstractSubclass
{
    internal RoguishOpportunist()
    {
        // Grant advantage when attack enemies whose initiative is lower than your
        // or when perform an attack of opportunity.
        var onComputeAttackModifierOpportunistQuickStrike = FeatureDefinitionBuilder
            .Create("OnComputeAttackModifierOpportunistQuickStrike")
            .SetGuiPresentation(Category.Feature)
            .SetCustomSubFeatures(new OnComputeAttackModifierOpportunistQuickStrike())
            .AddToDB();

        // Enemies struck by your sneak attack suffered from one of the following condition (Baned, Blinded, Bleed, Stunned)
        // if they fail a CON save against the DC of 8 + your DEX mod + your prof.
        var powerOpportunistDebilitatingStrike = FeatureDefinitionPowerBuilder
            .Create("PowerOpportunistDebilitatingStrike")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.OnSneakAttackHitAuto)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 1)
                    .SetTargetingData(Side.Enemy, RangeType.MeleeHit, 1, TargetType.Individuals)
                    .SetSavingThrowData(
                        false,
                        AttributeDefinitions.Constitution,
                        true,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency,
                        AttributeDefinitions.Dexterity,
                        20)
                    .SetEffectForms(EffectFormBuilder
                        .Create()
                        .SetConditionForm(
                            ConditionDefinitionBuilder
                                .Create(ConditionDummy, "ConditionOpportunistDebilitated")
                                .SetOrUpdateGuiPresentation(Category.Condition)
                                .AddToDB(),
                            ConditionForm.ConditionOperation.AddRandom,
                            false,
                            false,
                            ConditionBaned,
                            ConditionBleeding,
                            ConditionDefinitions.ConditionBlinded,
                            ConditionDefinitions.ConditionStunned)
                        .Build())
                    .Build())
            .AddToDB();

        var followupStrikeOnTargetFailedSavingThrow = FeatureDefinitionBuilder
            .Create("FeatureRoguishOpportunistSeizeTheChance")
            .SetGuiPresentation(Category.Feature)
            .SetCustomSubFeatures(new FollowUpStrikeWhenFoesFailedSavingThrow())
            .AddToDB();

        Subclass = CharacterSubclassDefinitionBuilder
            .Create("RoguishOpportunist")
            .SetGuiPresentation(Category.Subclass, MartialCommander)
            .AddFeaturesAtLevel(3,
                onComputeAttackModifierOpportunistQuickStrike)
            .AddFeaturesAtLevel(9,
                powerOpportunistDebilitatingStrike)
            .AddFeaturesAtLevel(13, followupStrikeOnTargetFailedSavingThrow)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceRogueRoguishArchetypes;

    private sealed class OnComputeAttackModifierOpportunistQuickStrike : IOnComputeAttackModifier
    {
        public void ComputeAttackModifier(
            RulesetCharacter myself,
            RulesetCharacter defender,
            RulesetAttackMode attackMode,
            ref ActionModifier attackModifier)
        {
            if (attackMode == null || defender == null)
            {
                return;
            }

            var hero = GameLocationCharacter.GetFromActor(myself);
            var target = GameLocationCharacter.GetFromActor(defender);

            // refresh sneak attack
            if (attackMode.AttackTags.Contains("RefreshSneakAttack"))
            {
                hero.UsedSpecialFeatures.Remove(
                    FeatureDefinitionAdditionalDamages.AdditionalDamageRogueSneakAttack.Name);
            }

            // grant advantage if attacker is performing an opportunity attack or has higher initiative.
            if (hero.LastInitiative <= target.LastInitiative &&
                attackMode.actionType != ActionDefinitions.ActionType.Reaction)
            {
                return;
            }

            attackModifier.attackAdvantageTrends.Add(new TrendInfo(1,
                FeatureSourceType.CharacterFeature, "Feature/&OnComputeAttackModifierOpportunistQuickStrikeTitle",
                null));
        }
    }

    private sealed class FollowUpStrikeWhenFoesFailedSavingThrow : IOnDefenderFailedSavingThrow
    {
        public IEnumerator OnDefenderFailedSavingThrow(GameLocationBattleManager __instance, CharacterAction action,
            GameLocationCharacter me, GameLocationCharacter target, ActionModifier saveModifier, bool hasHitVisual,
            bool hasBorrowedLuck)
        {
            if (target.Side == me.Side || me == target ||
                me.GetActionTypeStatus(ActionDefinitions.ActionType.Reaction) !=
                ActionDefinitions.ActionStatus.Available)
            {
                yield break;
            }

            var attackMode = me.FindActionAttackMode(ActionDefinitions.Id.AttackMain);
            attackMode.AttackTags.Add("RefreshSneakAttack");

            var attackParam = new BattleDefinitions.AttackEvaluationParams();
            if (attackMode.Ranged)
            {
                attackParam.FillForPhysicalRangeAttack(me, me.LocationPosition, attackMode,
                    target, target.LocationPosition, new ActionModifier());
            }
            else
            {
                attackParam.FillForPhysicalReachAttack(me, me.LocationPosition, attackMode,
                    target, target.LocationPosition, new ActionModifier());
            }

            if (!__instance.CanAttack(attackParam))
            {
                yield break;
            }

            var actionService = ServiceRepository.GetService<IGameLocationActionService>();
            var count = actionService.PendingReactionRequestGroups.Count;

            var reactionParams = new CharacterActionParams(me,
                ActionDefinitions.Id.AttackOpportunity,
                me.RulesetCharacter.AttackModes[0], target, new ActionModifier());
            actionService.ReactForOpportunityAttack(reactionParams);

            yield return __instance.WaitForReactions(me, actionService, count);
        }
    }
}
