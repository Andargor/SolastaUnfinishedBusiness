﻿using System;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.CustomDefinitions;

namespace SolastaUnfinishedBusiness.Builders.Features;

[UsedImplicitly]
internal class FeatureDefinitionReduceDamageBuilder
    : DefinitionBuilder<FeatureDefinitionReduceDamage, FeatureDefinitionReduceDamageBuilder>
{
    [NotNull]
    internal FeatureDefinitionReduceDamageBuilder SetAlwaysActiveReducedDamage(
        ReducedDamageHandler reducedDamage,
        params string[] damageTypes)
    {
        Definition.DamageTypes.SetRange(damageTypes);
        Definition.TriggerCondition = RuleDefinitions.AdditionalDamageTriggerCondition.AlwaysActive;
        Definition.ReducedDamage = reducedDamage;
        return this;
    }

    [NotNull]
    internal FeatureDefinitionReduceDamageBuilder SetConsumeSpellSlotsReducedDamage(
        CharacterClassDefinition spellCastingClass,
        ReducedDamageHandler reducedDamage,
        params string[] damageTypes)
    {
        Definition.SpellCastingClass = spellCastingClass;
        Definition.TriggerCondition = RuleDefinitions.AdditionalDamageTriggerCondition.SpendSpellSlot;
        Definition.ReducedDamage = reducedDamage;
        Definition.DamageTypes.SetRange(damageTypes);
        return this;
    }

    #region Constructors

    protected FeatureDefinitionReduceDamageBuilder(string name, Guid namespaceGuid) : base(name,
        namespaceGuid)
    {
    }

    protected FeatureDefinitionReduceDamageBuilder(
        FeatureDefinitionReduceDamage original, string name,
        Guid namespaceGuid) : base(original, name, namespaceGuid)
    {
    }

    #endregion
}
