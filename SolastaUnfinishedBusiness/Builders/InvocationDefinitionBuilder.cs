﻿using System;
using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.Builders;

[UsedImplicitly]
internal class InvocationDefinitionBuilder :
    InvocationDefinitionBuilder<InvocationDefinition, InvocationDefinitionBuilder>
{
    #region Constructors

    internal InvocationDefinitionBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    internal InvocationDefinitionBuilder(InvocationDefinition original, string name, Guid namespaceGuid) : base(
        original, name, namespaceGuid)
    {
    }

    #endregion
}

internal class InvocationDefinitionBuilder<TDefinition, TBuilder> : DefinitionBuilder<TDefinition, TBuilder>
    where TDefinition : InvocationDefinition
    where TBuilder : InvocationDefinitionBuilder<TDefinition, TBuilder>
{
    internal TBuilder SetRequirements(int level = 1,
        SpellDefinition spell = null,
        FeatureDefinition pact = null)
    {
        Definition.requiredLevel = level;
        Definition.requiredKnownSpell = spell;
        Definition.requiredPact = pact;
        return (TBuilder)this;
    }

    internal TBuilder SetGrantedFeature(FeatureDefinition feature, bool longRestRecharge = false)
    {
        Definition.grantedFeature = feature;
        Definition.longRestRecharge = longRestRecharge;
        return (TBuilder)this;
    }

    internal TBuilder SetGrantedSpell(SpellDefinition spell,
        bool consumesSpellSlot = false,
        bool longRestRecharge = false,
        bool overrideMaterialComponent = true)
    {
        Definition.grantedSpell = spell;
        Definition.consumesSpellSlot = consumesSpellSlot;
        Definition.longRestRecharge = longRestRecharge;
        Definition.overrideMaterialComponent = overrideMaterialComponent;
        return (TBuilder)this;
    }

    #region Constructors

    internal InvocationDefinitionBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    internal InvocationDefinitionBuilder(TDefinition original, string name, Guid namespaceGuid) : base(original, name,
        namespaceGuid)
    {
    }

    #endregion
}
