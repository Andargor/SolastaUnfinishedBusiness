﻿using System.Collections;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;

//This should have default namespace so that it can be properly created by `CharacterActionPatcher`
// ReSharper disable once CheckNamespace
[UsedImplicitly]
#pragma warning disable CA1050
public class CharacterActionAudaciousWhirlToggle : CharacterAction
#pragma warning restore CA1050
{
    private const ActionDefinitions.Id Action = (ActionDefinitions.Id)ExtraActionId.AudaciousWhirlToggle;

    public CharacterActionAudaciousWhirlToggle(CharacterActionParams actionParams) : base(actionParams)
    {
    }

    public override IEnumerator ExecuteImpl()
    {
        var rulesetCharacter = ActingCharacter.RulesetCharacter;

        if (rulesetCharacter.IsToggleEnabled(Action))
        {
            rulesetCharacter.DisableToggle(Action);
        }
        else
        {
            rulesetCharacter.EnableToggle(Action);
            rulesetCharacter.DisableToggle((ActionDefinitions.Id)ExtraActionId.MasterfulWhirlToggle);
        }

        yield return null;
    }
}
