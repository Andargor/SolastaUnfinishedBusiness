﻿using System;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.Extensions;

namespace SolastaUnfinishedBusiness.Builders.Features;

[UsedImplicitly]
internal class FeatureDefinitionMovementAffinityBuilder
    : DefinitionBuilder<FeatureDefinitionMovementAffinity, FeatureDefinitionMovementAffinityBuilder>
{
    internal FeatureDefinitionMovementAffinityBuilder SetAdditiveModifierAdvancement(
        RuleDefinitions.MovementAffinityAdvancement value)
    {
        Definition.additiveModifierAdvancement = value;
        return this;
    }

    internal FeatureDefinitionMovementAffinityBuilder SetBaseSpeedAdditiveModifier(int value)
    {
        Definition.baseSpeedAdditiveModifier = value;
        return this;
    }

    internal FeatureDefinitionMovementAffinityBuilder SetSituationalContext(ExtraSituationalContext situationalContext)
    {
        Definition.situationalContext = (RuleDefinitions.SituationalContext)situationalContext;
        return this;
    }

    internal FeatureDefinitionMovementAffinityBuilder SetBaseSpeedMultiplicativeModifier(float value)
    {
        Definition.baseSpeedMultiplicativeModifier = value;
        return this;
    }

    internal FeatureDefinitionMovementAffinityBuilder SetImmunities(
        bool encumbranceImmunity = false,
        bool heavyArmorImmunity = false,
        bool difficultTerrainImmunity = false)
    {
        Definition.encumbranceImmunity = encumbranceImmunity;
        Definition.heavyArmorImmunity = heavyArmorImmunity;
        Definition.immuneDifficultTerrain = difficultTerrainImmunity;
        return this;
    }

    #region Constructors

    protected FeatureDefinitionMovementAffinityBuilder(string name, Guid namespaceGuid) : base(name, namespaceGuid)
    {
    }

    protected FeatureDefinitionMovementAffinityBuilder(FeatureDefinitionMovementAffinity original, string name,
        Guid namespaceGuid) : base(original, name, namespaceGuid)
    {
    }

    #endregion
}
