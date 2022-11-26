﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomDefinitions;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Races;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFeatureSets;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPointPools;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterRaceDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MorphotypeElementDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Models;

internal static class CharacterContext
{
    internal const int MinInitialFeats = 0;
    internal const int MaxInitialFeats = 2; // don't increase this value to avoid issue reports on crazy scenarios

    internal const int GameMaxAttribute = 15;
    internal const int GameBuyPoints = 27;

    internal const int ModMaxAttribute = 17;
    internal const int ModBuyPoints = 35;

    private static int PreviousTotalFeatsGrantedFirstLevel { get; set; } = -1;
    private static bool PreviousAlternateHuman { get; set; }
    internal static FeatureDefinitionPower FeatureDefinitionPowerHelpAction { get; set; }

    internal static void Load()
    {
        // +1 here as need to count the Alternate Human Feat
        // + 20 here to allow it to be set directly on XML file
        for (var i = 2; i <= MaxInitialFeats + 1 + 20; i++)
        {
            var s = i.ToString();

            _ = FeatureDefinitionPointPoolBuilder
                .Create($"PointPool{i}BonusFeats")
                .SetGuiPresentation(
                    Gui.Format("Feature/&PointPoolSelectBonusFeatsTitle", s),
                    Gui.Format("Feature/&PointPoolSelectBonusFeatsDescription", s))
                .SetPool(HeroDefinitions.PointsPoolType.Feat, i)
                .AddToDB();
        }

        LoadFighterArmamentAdroitness();
        LoadHelpPower();
        LoadEpicArray();
        LoadAdditionalNames();
        LoadVisuals();
    }

    internal static void LateLoad()
    {
        SwitchHelpPower();
        FlexibleBackgroundsContext.SwitchFlexibleBackgrounds();
        FlexibleRacesContext.SwitchFlexibleRaces();
        SwitchFirstLevelTotalFeats(); // alternate human here as well
        SwitchAsiAndFeat();
        SwitchEvenLevelFeats();
        SwitchFighterArmamentAdroitness();
    }

    private static void LoadHelpPower()
    {
        var effectDescription = EffectDescriptionBuilder
            .Create(TrueStrike.EffectDescription)
            .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
            .SetDurationData(DurationType.Round, 1)
            .Build();

        effectDescription.EffectForms[0].ConditionForm.ConditionDefinition = CustomConditionsContext.Distracted;
        effectDescription.EffectForms[0].ConditionForm.conditionDefinitionName =
            CustomConditionsContext.Distracted.Name;

        FeatureDefinitionPowerHelpAction = FeatureDefinitionPowerBuilder
            .Create("PowerHelp")
            .SetGuiPresentation(Category.Feature, Aid)
            .SetUsesFixed(ActivationTime.Action)
            .SetEffectDescription(effectDescription)
            .SetUniqueInstance()
            .AddToDB();
    }

    private static void LoadAdditionalNames()
    {
        if (!Main.Settings.OfferAdditionalLoreFriendlyNames)
        {
            return;
        }

        var payload = Resources.Names;
        var lines = new List<string>(payload.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

        foreach (var line in lines)
        {
            var columns = line.Split(new[] { '\t' }, 3);

            if (columns.Length != 3)
            {
                Main.Error($"additional names cannot load: {line}.");

                continue;
            }

            var raceName = columns[0];
            var gender = columns[1];
            var name = columns[2];

            if (DatabaseRepository.GetDatabase<CharacterRaceDefinition>().TryGetElement(raceName, out var race))
            {
                var racePresentation = race.RacePresentation;

                switch (gender)
                {
                    case "Male":
                        racePresentation.MaleNameOptions.Add(name);
                        break;

                    case "Female":
                        racePresentation.FemaleNameOptions.Add(name);
                        break;

                    case "Sur":
                        racePresentation.SurNameOptions.Add(name);
                        break;
                }
            }
            else
            {
                Main.Error($"additional names cannot load: {line}.");
            }
        }
    }

    private static void LoadVisuals()
    {
        var dbMorphotypeElementDefinition = DatabaseRepository.GetDatabase<MorphotypeElementDefinition>();

        if (Main.Settings.UnlockSkinColors)
        {
            foreach (var morphotype in dbMorphotypeElementDefinition.Where(
                         x => x.Category == MorphotypeElementDefinition.ElementCategory.Skin &&
                              x != FaceAndSkin_12 &&
                              x != FaceAndSkin_13 &&
                              x != FaceAndSkin_14 &&
                              x != FaceAndSkin_15 &&
                              x != FaceAndSkin_16 &&
                              x != FaceAndSkin_17 &&
                              x != FaceAndSkin_18))
            {
                morphotype.playerSelectable = true;
                morphotype.originAllowed = EyeColor_001.OriginAllowed;
                if (morphotype.Name.Contains("Dragonborn"))
                {
                    morphotype.GuiPresentation.sortOrder = morphotype.GuiPresentation.SortOrder + 54;
                }
            }
        }

        if (Main.Settings.UnlockGlowingColorsForAllMarksAndTattoos)
        {
            foreach (var morphotype in dbMorphotypeElementDefinition.Where(
                         x => x.Category == MorphotypeElementDefinition.ElementCategory.BodyDecorationColor &&
                              x.SubclassFilterMask == GraphicsDefinitions.MorphotypeSubclassFilterTag
                                  .SorcererManaPainter))
            {
                morphotype.subClassFilterMask = GraphicsDefinitions.MorphotypeSubclassFilterTag.All;
            }
        }

        var brightEyes = new List<MorphotypeElementDefinition>();

        Morphotypes.CreateBrightEyes(brightEyes);

        if (!Main.Settings.AddNewBrightEyeColors)
        {
            foreach (var morphotype in brightEyes)
            {
                morphotype.playerSelectable = false;
            }
        }

        var glowingEyes = new List<MorphotypeElementDefinition>();

        Morphotypes.CreateGlowingEyes(glowingEyes);

        if (!Main.Settings.UnlockGlowingEyeColors)
        {
            foreach (var morphotype in glowingEyes)
            {
                morphotype.playerSelectable = false;
            }
        }

        if (Main.Settings.UnlockEyeStyles)
        {
            foreach (var morphotype in dbMorphotypeElementDefinition.Where(x =>
                         x.Category == MorphotypeElementDefinition.ElementCategory.Eye))
            {
                morphotype.subClassFilterMask = GraphicsDefinitions.MorphotypeSubclassFilterTag.All;
                morphotype.originAllowed = EyeColor_001.OriginAllowed;
            }

            var races = DatabaseRepository.GetDatabase<CharacterRaceDefinition>();

            foreach (var race in races)
            {
                var presentation = race.racePresentation;

                if (presentation.IsAvailable(MorphotypeElementDefinition.ElementCategory.Eye))
                {
                    continue;
                }

                var all = new List<MorphotypeElementDefinition.ElementCategory>(
                    presentation.availableMorphotypeCategories) { MorphotypeElementDefinition.ElementCategory.Eye };

                presentation.availableMorphotypeCategories = all.ToArray();
            }
        }

        if (Main.Settings.UnlockAllNpcFaces)
        {
            HalfElf.RacePresentation.FemaleFaceShapeOptions.Add("FaceShape_NPC_Princess");
            HalfElf.RacePresentation.MaleFaceShapeOptions.Add("FaceShape_HalfElf_NPC_Bartender");
            Human.RacePresentation.MaleFaceShapeOptions.Add("FaceShape_NPC_TavernGuy");
            Human.RacePresentation.MaleFaceShapeOptions.Add("FaceShape_NPC_TomWorker");

            foreach (var morphotype in dbMorphotypeElementDefinition.Where(x =>
                         x.Category == MorphotypeElementDefinition.ElementCategory.FaceShape &&
                         x != FaceShape_NPC_Aksha))
            {
                morphotype.playerSelectable = true;
            }
        }

        if (Main.Settings.AllowBeardlessDwarves)
        {
            Dwarf.RacePresentation.needBeard = false;
            DwarfHill.RacePresentation.needBeard = false;
            DwarfSnow.RacePresentation.needBeard = false;
            Dwarf.RacePresentation.MaleBeardShapeOptions.Add(BeardShape_None.Name);
        }

        if (Main.Settings.UnlockMarkAndTattoosForAllCharacters)
        {
            foreach (var morphotype in dbMorphotypeElementDefinition.Where(x =>
                         x.Category == MorphotypeElementDefinition.ElementCategory.BodyDecoration))
            {
                morphotype.subClassFilterMask = GraphicsDefinitions.MorphotypeSubclassFilterTag.All;
            }
        }

        if (!Main.Settings.AllowUnmarkedSorcerers)
        {
            return;
        }

        SorcerousDraconicBloodline.morphotypeSubclassFilterTag = GraphicsDefinitions.MorphotypeSubclassFilterTag
            .Default;
        SorcerousManaPainter.morphotypeSubclassFilterTag = GraphicsDefinitions.MorphotypeSubclassFilterTag
            .Default;
        SorcerousChildRift.morphotypeSubclassFilterTag = GraphicsDefinitions.MorphotypeSubclassFilterTag
            .Default;
        SorcerousHauntedSoul.morphotypeSubclassFilterTag = GraphicsDefinitions.MorphotypeSubclassFilterTag
            .Default;
    }

    private static void LoadEpicArray()
    {
        AttributeDefinitions.PredeterminedRollScores = Main.Settings.EnableEpicPointsAndArray
            ? new[] { 17, 15, 13, 12, 10, 8 }
            : new[] { 15, 14, 13, 12, 10, 8 };
    }

#if false
    private static void LoadVision()
    {
        if (Main.Settings.DisableSenseDarkVisionFromAllRaces)
        {
            foreach (var featureUnlocks in DatabaseRepository.GetDatabase<CharacterRaceDefinition>()
                         .Select(crd => crd.FeatureUnlocks))
            {
                featureUnlocks.RemoveAll(x => x.FeatureDefinition.name == "SenseDarkvision");
                // Half-orcs have a different darkvision.
                featureUnlocks.RemoveAll(x => x.FeatureDefinition.name == "SenseDarkvision12");
            }
        }

        if (Main.Settings.DisableSenseSuperiorDarkVisionFromAllRaces)
        {
            foreach (var characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>())
            {
                characterRaceDefinition.FeatureUnlocks.RemoveAll(x =>
                    x.FeatureDefinition.name == "SenseSuperiorDarkvision");
            }
        }
    }
#endif

    internal static void SwitchHelpPower()
    {
        var dbCharacterRaceDefinition = DatabaseRepository.GetDatabase<CharacterRaceDefinition>();
        var subRaces = dbCharacterRaceDefinition
            .SelectMany(x => x.SubRaces);
        var races = dbCharacterRaceDefinition
            .Where(x => !subRaces.Contains(x));

        if (Main.Settings.AddHelpActionToAllRaces)
        {
            foreach (var characterRaceDefinition in races
                         .Where(a => !a.FeatureUnlocks.Exists(x =>
                             x.Level == 1 && x.FeatureDefinition == FeatureDefinitionPowerHelpAction)))
            {
                characterRaceDefinition.FeatureUnlocks.Add(
                    new FeatureUnlockByLevel(FeatureDefinitionPowerHelpAction, 1));
            }
        }
        else
        {
            foreach (var characterRaceDefinition in races
                         .Where(a => a.FeatureUnlocks.Exists(x =>
                             x.Level == 1 && x.FeatureDefinition == FeatureDefinitionPowerHelpAction)))
            {
                characterRaceDefinition.FeatureUnlocks.RemoveAll(x =>
                    x.Level == 1 && x.FeatureDefinition == FeatureDefinitionPowerHelpAction);
            }
        }
    }

    internal static void SwitchAsiAndFeat()
    {
        FeatureSetAbilityScoreChoice.mode = Main.Settings.EnablesAsiAndFeat
            ? FeatureDefinitionFeatureSet.FeatureSetMode.Union
            : FeatureDefinitionFeatureSet.FeatureSetMode.Exclusion;
    }

    internal static void SwitchEvenLevelFeats()
    {
        var levels = new[] { 2, 6, 10, 14 };
        var dbCharacterClassDefinition = DatabaseRepository.GetDatabase<CharacterClassDefinition>();
        var dbFeatureDefinitionPointPool = DatabaseRepository.GetDatabase<FeatureDefinitionPointPool>();
        var pointPool2BonusFeats = dbFeatureDefinitionPointPool.GetElement("PointPool2BonusFeats");

        foreach (var characterClassDefinition in dbCharacterClassDefinition)
        {
            foreach (var level in levels)
            {
                var featureUnlockPointPool1 = new FeatureUnlockByLevel(PointPoolBonusFeat, level);
                var featureUnlockPointPool2 = new FeatureUnlockByLevel(pointPool2BonusFeats, level);

                if (Main.Settings.EnableFeatsAtEvenLevels)
                {
                    if (characterClassDefinition.FeatureUnlocks.Contains(featureUnlockPointPool1))
                    {
                        characterClassDefinition.FeatureUnlocks.Remove(featureUnlockPointPool1);
                        characterClassDefinition.FeatureUnlocks.Add(featureUnlockPointPool2);
                    }
                    else
                    {
                        characterClassDefinition.FeatureUnlocks.Add(featureUnlockPointPool1);
                    }
                }
                else
                {
                    if (characterClassDefinition.FeatureUnlocks.Contains(featureUnlockPointPool2))
                    {
                        characterClassDefinition.FeatureUnlocks.Add(featureUnlockPointPool1);
                    }
                    else if (characterClassDefinition.FeatureUnlocks.Contains(featureUnlockPointPool1))
                    {
                        characterClassDefinition.FeatureUnlocks.Remove(featureUnlockPointPool1);
                    }
                }
            }

            if (Main.Settings.EnableSortingFutureFeatures)
            {
                characterClassDefinition.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
            }
        }
    }

    internal static void SwitchFirstLevelTotalFeats()
    {
        if (PreviousTotalFeatsGrantedFirstLevel > -1)
        {
            UnloadRacesLevel1Feats(PreviousTotalFeatsGrantedFirstLevel, PreviousAlternateHuman);
        }

        PreviousTotalFeatsGrantedFirstLevel = Main.Settings.TotalFeatsGrantedFirstLevel;
        PreviousAlternateHuman = Main.Settings.EnableAlternateHuman;
        LoadRacesLevel1Feats(Main.Settings.TotalFeatsGrantedFirstLevel, Main.Settings.EnableAlternateHuman);
    }

    private static void BuildFeatureUnlocks(
        int initialFeats,
        bool alternateHuman,
        [CanBeNull] out FeatureUnlockByLevel featureUnlockByLevelNonHuman,
        [CanBeNull] out FeatureUnlockByLevel featureUnlockByLevelHuman)
    {
        string name;

        featureUnlockByLevelNonHuman = null;
        featureUnlockByLevelHuman = null;

        switch (initialFeats)
        {
            case 0:
            {
                if (alternateHuman)
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(PointPoolBonusFeat, 1);
                }

                break;
            }
            case 1:
            {
                featureUnlockByLevelNonHuman = new FeatureUnlockByLevel(PointPoolBonusFeat, 1);

                name = "PointPool2BonusFeats";
                if (alternateHuman && TryGetDefinition<FeatureDefinitionPointPool>(name, out var pointPool2BonusFeats))
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(pointPool2BonusFeats, 1);
                }

                break;
            }
            case > 1:
            {
                name = $"PointPool{initialFeats}BonusFeats";
                if (TryGetDefinition<FeatureDefinitionPointPool>(name, out var featureDefinitionPointPool))
                {
                    featureUnlockByLevelNonHuman = new FeatureUnlockByLevel(featureDefinitionPointPool, 1);
                }

                name = $"PointPool{initialFeats + 1}BonusFeats";
                if (alternateHuman && TryGetDefinition<FeatureDefinitionPointPool>(name, out var pointPoolXBonusFeats))
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(pointPoolXBonusFeats, 1);
                }

                break;
            }
        }
    }

    private static void LoadRacesLevel1Feats(int initialFeats, bool alternateHuman)
    {
        var human = Human;

        BuildFeatureUnlocks(initialFeats, alternateHuman, out var featureUnlockByLevelNonHuman,
            out var featureUnlockByLevelHuman);

        foreach (var characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>())
        {
            if (IsSubRace(characterRaceDefinition))
            {
                continue;
            }

            if (alternateHuman && characterRaceDefinition == human)
            {
                if (featureUnlockByLevelHuman != null)
                {
                    human.FeatureUnlocks.Add(featureUnlockByLevelHuman);
                }

                var pointPoolAbilityScoreImprovement =
                    new FeatureUnlockByLevel(PointPoolAbilityScoreImprovement, 1);
                human.FeatureUnlocks.Add(pointPoolAbilityScoreImprovement);

                var pointPoolHumanSkillPool = new FeatureUnlockByLevel(PointPoolHumanSkillPool, 1);
                human.FeatureUnlocks.Add(pointPoolHumanSkillPool);

                Remove(human,
                    FeatureDefinitionAttributeModifiers
                        .AttributeModifierHumanAbilityScoreIncrease);
            }
            else
            {
                if (featureUnlockByLevelNonHuman != null)
                {
                    characterRaceDefinition.FeatureUnlocks.Add(featureUnlockByLevelNonHuman);
                }
            }
        }
    }

    private static void UnloadRacesLevel1Feats(int initialFeats, bool alternateHuman)
    {
        var human = Human;

        BuildFeatureUnlocks(initialFeats, alternateHuman,
            out var featureUnlockByLevelNonHuman,
            out var featureUnlockByLevelHuman);

        foreach (var characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>())
        {
            if (IsSubRace(characterRaceDefinition))
            {
                continue;
            }

            if (alternateHuman && characterRaceDefinition == human)
            {
                if (featureUnlockByLevelHuman != null)
                {
                    Remove(human, featureUnlockByLevelHuman);
                }

                Remove(human, PointPoolAbilityScoreImprovement);
                Remove(human, PointPoolHumanSkillPool);

                var humanAttributeIncrease = new FeatureUnlockByLevel(
                    FeatureDefinitionAttributeModifiers.AttributeModifierHumanAbilityScoreIncrease, 1);

                human.FeatureUnlocks.Add(humanAttributeIncrease);
            }
            else
            {
                if (featureUnlockByLevelNonHuman != null)
                {
                    Remove(characterRaceDefinition, featureUnlockByLevelNonHuman);
                }
            }
        }
    }

    private static void Remove(
        [NotNull] CharacterRaceDefinition characterRaceDefinition,
        BaseDefinition toRemove)
    {
        var ndx = -1;

        for (var i = 0; i < characterRaceDefinition.FeatureUnlocks.Count; i++)
        {
            if (characterRaceDefinition.FeatureUnlocks[i].Level == 1 &&
                characterRaceDefinition.FeatureUnlocks[i].FeatureDefinition == toRemove)
            {
                ndx = i;
            }
        }

        if (ndx >= 0)
        {
            characterRaceDefinition.FeatureUnlocks.RemoveAt(ndx);
        }
    }

    private static void Remove(
        [NotNull] CharacterRaceDefinition characterRaceDefinition,
        [NotNull] FeatureUnlockByLevel featureUnlockByLevel)
    {
        Remove(characterRaceDefinition, featureUnlockByLevel.FeatureDefinition);
    }

    private static bool IsSubRace(CharacterRaceDefinition raceDefinition)
    {
        return DatabaseRepository.GetDatabase<CharacterRaceDefinition>()
            .Any(crd => crd.SubRaces.Contains(raceDefinition));
    }

    private sealed class ModifyAttackModeForWeaponFighterArmamentAdroitness : IModifyAttackModeForWeapon
    {
        private const string SourceName =
            "Feature/&ModifyAttackModeForWeaponFighterArmamentAdroitnessTitle";

        private readonly WeaponTypeDefinition _weaponTypeDefinition;

        public ModifyAttackModeForWeaponFighterArmamentAdroitness(WeaponTypeDefinition weaponTypeDefinition)
        {
            _weaponTypeDefinition = weaponTypeDefinition;
        }

        public void ModifyAttackMode(RulesetCharacter character, [CanBeNull] RulesetAttackMode attackMode)
        {
            var damage = attackMode?.EffectDescription?.FindFirstDamageForm();

            if (damage == null)
            {
                return;
            }

            if (attackMode.sourceDefinition is not ItemDefinition { IsWeapon: true } sourceDefinition ||
                sourceDefinition.WeaponDescription.WeaponTypeDefinition != _weaponTypeDefinition)
            {
                return;
            }

            attackMode.ToHitBonus += 1;
            attackMode.ToHitBonusTrends.Add(new TrendInfo(1, FeatureSourceType.CharacterFeature, SourceName, null));

            damage.BonusDamage += 1;
            damage.DamageBonusTrends.Add(new TrendInfo(1, FeatureSourceType.CharacterFeature, SourceName, null));
        }
    }

    private static FeatureDefinitionCustomInvocationPool InvocationPoolFighterArmamentAdroitness { get; set; }

    private static void LoadFighterArmamentAdroitness()
    {
        InvocationPoolFighterArmamentAdroitness = CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolFighterArmamentAdroitness")
            .SetGuiPresentation(Category.Feature)
            .Setup(InvocationPoolTypeCustom.Pools.ArmamentAdroitness)
            .AddToDB();

        var db = DatabaseRepository.GetDatabase<WeaponTypeDefinition>()
            .Where(x => x != WeaponTypeDefinitions.UnarmedStrikeType &&
                        x != CustomWeaponsContext.ThunderGauntletType &&
                        x != CustomWeaponsContext.LightningLauncherType);

        foreach (var weaponTypeDefinition in db)
        {
            var modifyAttackModeForWeaponFighterArmamentAdroitness = FeatureDefinitionBuilder
                .Create($"ModifyAttackModeForWeaponFighterArmamentAdroitness{weaponTypeDefinition.name}")
                .SetGuiPresentation("ModifyAttackModeForWeaponFighterArmamentAdroitness", Category.Feature)
                .SetCustomSubFeatures(new ModifyAttackModeForWeaponFighterArmamentAdroitness(weaponTypeDefinition))
                .AddToDB();

            CustomInvocationDefinitionBuilder
                .Create($"CustomInvocationArmamentAdroitness{weaponTypeDefinition.name}")
                .SetGuiPresentation(
                    weaponTypeDefinition.GuiPresentation.Title,
                    modifyAttackModeForWeaponFighterArmamentAdroitness.GuiPresentation.Description,
                    CustomWeaponsContext.GetStandardWeaponOfType(weaponTypeDefinition.Name))
                .SetPoolType(InvocationPoolTypeCustom.Pools.ArmamentAdroitness)
                .SetGrantedFeature(modifyAttackModeForWeaponFighterArmamentAdroitness)
                .SetCustomSubFeatures(Hidden.Marker)
                .AddToDB();
        }
    }

    internal static void SwitchFighterArmamentAdroitness()
    {
        var levels = new[] { 2, 6, 10, 14 };

        if (Main.Settings.EnableFighterArmamentAdroitness)
        {
            foreach (var level in levels)
            {
                CharacterClassDefinitions.Fighter.FeatureUnlocks.TryAdd(
                    new FeatureUnlockByLevel(InvocationPoolFighterArmamentAdroitness, level));
            }
        }
        else
        {
            foreach (var level in levels)
            {
                CharacterClassDefinitions.Fighter.FeatureUnlocks
                    .RemoveAll(x => x.level == level &&
                                    x.FeatureDefinition == InvocationPoolFighterArmamentAdroitness);
            }
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            CharacterClassDefinitions.Fighter.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }
}
