﻿using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Subclasses;
using UnityEngine.AddressableAssets;

namespace SolastaUnfinishedBusiness.CustomUI;

public class ReactionResourceArcaneShot : ICustomReactionResource
{
    private ReactionResourceArcaneShot()
    {
    }

    public static ICustomReactionResource Instance { get; } = new ReactionResourceArcaneShot();
    public AssetReferenceSprite Icon => DatabaseHelper.ActionDefinitions.DisengageMain.GuiPresentation.SpriteReference;

    public string GetUses(RulesetCharacter character)
    {
        var usablePower = UsablePowersProvider.Get(MartialArcaneArcher.PowerArcaneShot, character);

        return character.GetRemainingUsesOfPower(usablePower).ToString();
    }
}
