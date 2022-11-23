﻿using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.Models;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Subclasses;

internal sealed class WayOfTheSilhouette : AbstractSubclass
{
    internal WayOfTheSilhouette()
    {
        var powerWayOfSilhouetteDarkness = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfSilhouetteDarkness")
            .SetGuiPresentation(SpellDefinitions.Darkness.GuiPresentation)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.KiPoints, 2)
            .SetEffectDescription(SpellDefinitions.Darkness.EffectDescription)
            .AddToDB();

        var powerWayOfSilhouetteDarkvision = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfSilhouetteDarkvision")
            .SetGuiPresentation(SpellDefinitions.Darkvision.GuiPresentation)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.KiPoints, 2)
            .SetEffectDescription(SpellDefinitions.Darkvision.EffectDescription)
            .AddToDB();

        var powerWayOfSilhouettePassWithoutTrace = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfSilhouettePassWithoutTrace")
            .SetGuiPresentation(SpellDefinitions.PassWithoutTrace.GuiPresentation)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.KiPoints, 2)
            .SetEffectDescription(SpellDefinitions.PassWithoutTrace.EffectDescription)
            .AddToDB();

        var powerWayOfSilhouetteSilence = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfSilhouetteSilence")
            .SetGuiPresentation(SpellDefinitions.Silence.GuiPresentation)
            .SetUsesFixed(ActivationTime.Action, RechargeRate.KiPoints, 2)
            .SetEffectDescription(SpellDefinitions.Silence.EffectDescription)
            .AddToDB();

        var featureSetWayOfSilhouetteSilhouetteArts = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetWayOfSilhouetteSilhouetteArts")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                powerWayOfSilhouetteDarkness,
                powerWayOfSilhouetteDarkvision,
                powerWayOfSilhouettePassWithoutTrace,
                powerWayOfSilhouetteSilence)
            .AddToDB();

        var powerWayOfSilhouetteSilhouetteStep = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfSilhouetteSilhouetteStep")
            .SetGuiPresentation(Category.Feature, SpellDefinitions.MistyStep)
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(SpellDefinitions.MistyStep.EffectDescription)
            .SetShowCasting(true)
            .AddToDB();

        var lightAffinityWayOfSilhouetteCloakOfSilhouettesWeak = FeatureDefinitionLightAffinityBuilder
            .Create("LightAffinityWayOfSilhouetteCloakOfSilhouettesWeak")
            .SetGuiPresentation(Category.Feature)
            .AddLightingEffectAndCondition(new FeatureDefinitionLightAffinity.LightingEffectAndCondition
            {
                lightingState = LocationDefinitions.LightingState.Unlit,
                condition = CustomConditionsContext.InvisibilityEveryRound
            })
            .AddToDB();

        var lightAffinityWayOfSilhouetteStrong = FeatureDefinitionLightAffinityBuilder
            .Create("LightAffinityWayOfSilhouetteCloakOfSilhouettesStrong")
            .SetGuiPresentation(Category.Feature)
            .AddLightingEffectAndCondition(new FeatureDefinitionLightAffinity.LightingEffectAndCondition
            {
                lightingState = LocationDefinitions.LightingState.Dim,
                condition = CustomConditionsContext.InvisibilityEveryRound
            })
            .AddLightingEffectAndCondition(new FeatureDefinitionLightAffinity.LightingEffectAndCondition
            {
                lightingState = LocationDefinitions.LightingState.Darkness,
                condition = CustomConditionsContext.InvisibilityEveryRound
            })
            .AddToDB();

        var powerWayOfSilhouetteImprovedSilhouetteStep = FeatureDefinitionPowerBuilder
            .Create("PowerWayOfSilhouetteImprovedSilhouetteStep")
            .SetGuiPresentation(Category.Feature, DimensionDoor)
            .SetOverriddenPower(powerWayOfSilhouetteSilhouetteStep)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .SetEffectDescription(DimensionDoor.EffectDescription)
            .SetUniqueInstance()
            .AddToDB();
        
        Subclass = CharacterSubclassDefinitionBuilder
            .Create("WayOfSilhouette")
            .SetOrUpdateGuiPresentation(Category.Subclass, CharacterSubclassDefinitions.RoguishShadowCaster)
            .AddFeaturesAtLevel(3,
                featureSetWayOfSilhouetteSilhouetteArts,
                lightAffinityWayOfSilhouetteCloakOfSilhouettesWeak,
                FeatureDefinitionCastSpells.CastSpellTraditionLight)
            .AddFeaturesAtLevel(6,
                lightAffinityWayOfSilhouetteStrong,
                powerWayOfSilhouetteSilhouetteStep)
            .AddFeaturesAtLevel(11,
                powerWayOfSilhouetteImprovedSilhouetteStep)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceMonkMonasticTraditions;
}
