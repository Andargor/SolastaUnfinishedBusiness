﻿using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.Extensions;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

internal delegate bool IsCharacterValidHandler(RulesetCharacter character);

internal static class ValidatorsCharacter
{
    internal static readonly IsCharacterValidHandler HasAttacked = character => character.ExecutedAttacks > 0;

    internal static readonly IsCharacterValidHandler NoArmor = character => !character.IsWearingArmor();

    // internal static readonly CharacterValidator MediumArmor = character => character.IsWearingMediumArmor();

    internal static readonly IsCharacterValidHandler NoShield = character => !character.IsWearingShield();

    internal static readonly IsCharacterValidHandler HasShield = character => character.IsWearingShield();

    // internal static readonly CharacterValidator EmptyOffhand = character => character.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeOffHand].EquipedItem == null;

    internal static readonly IsCharacterValidHandler HasPolearm = character =>
    {
        var slotsByName = character.CharacterInventory.InventorySlotsByName;

        return ValidatorsWeapon.IsPolearm(slotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem)
               || ValidatorsWeapon.IsPolearm(slotsByName[EquipmentDefinitions.SlotTypeOffHand].EquipedItem);
    };

#if false
    internal static readonly IsCharacterValidHandler HasLightRangeWeapon = character =>
    {
        var slotsByName = character.CharacterInventory.InventorySlotsByName;
        var equipedItem = slotsByName[EquipmentDefinitions.SlotTypeMainHand];

        if (equipedItem == null)
        {
            return false;
        }

        var equipedItemDescription = equipedItem.EquipedItem?.ItemDefinition;

        if (equipedItemDescription == null)
        {
            return false;
        }

        return equipedItemDescription.WeaponDescription.WeaponTypeDefinition ==
               DatabaseHelper.WeaponTypeDefinitions.ShortbowType
               || equipedItemDescription.WeaponDescription.WeaponTypeDefinition ==
               DatabaseHelper.WeaponTypeDefinitions.LightCrossbowType
               || equipedItemDescription.WeaponDescription.WeaponTypeDefinition ==
               CustomWeaponsContext.HandXbowWeaponType;
    };
#endif

    internal static readonly IsCharacterValidHandler HasTwoHandedRangeWeapon = character =>
    {
        var slotsByName = character.CharacterInventory.InventorySlotsByName;
        var equipedItem = slotsByName[EquipmentDefinitions.SlotTypeMainHand];

        if (equipedItem == null)
        {
            return false;
        }

        var equipedItemDescription = equipedItem.EquipedItem?.ItemDefinition;

        if (equipedItemDescription == null)
        {
            return false;
        }

        return equipedItemDescription.WeaponDescription.WeaponTypeDefinition ==
               DatabaseHelper.WeaponTypeDefinitions.LongbowType
               || equipedItemDescription.WeaponDescription.WeaponTypeDefinition ==
               DatabaseHelper.WeaponTypeDefinitions.ShortbowType
               || equipedItemDescription.WeaponDescription.WeaponTypeDefinition ==
               DatabaseHelper.WeaponTypeDefinitions.LightCrossbowType
               || equipedItemDescription.WeaponDescription.WeaponTypeDefinition ==
               DatabaseHelper.WeaponTypeDefinitions.HeavyCrossbowType;
    };

#if false
    internal static readonly IsCharacterValidHandler MainHandIsFinesseWeapon = character =>
        ValidatorsWeapon.HasAnyWeaponTag(character.GetItemInSlot(EquipmentDefinitions.SlotTypeMainHand),
            TagsDefinitions.WeaponTagFinesse);
#endif

    internal static readonly IsCharacterValidHandler MainHandIsVersatileWeapon = character =>
        ValidatorsWeapon.HasAnyWeaponTag(character.GetItemInSlot(EquipmentDefinitions.SlotTypeMainHand),
            TagsDefinitions.WeaponTagVersatile);

    internal static readonly IsCharacterValidHandler MainHandIsMeleeWeapon = character =>
        ValidatorsWeapon.IsMelee(character.GetItemInSlot(EquipmentDefinitions.SlotTypeMainHand));

#if false
    internal static readonly CharacterValidator FullyUnarmed = character =>
    {
        var slotsByName = character.CharacterInventory.InventorySlotsByName;
        return WeaponValidators.IsUnarmedWeapon(slotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem)
               && WeaponValidators.IsUnarmedWeapon(slotsByName[EquipmentDefinitions.SlotTypeOffHand].EquipedItem);
    };

    internal static readonly CharacterValidator UsedAllMainAttacks = character =>
        character.ExecutedAttacks >= character.GetAttribute(AttributeDefinitions.AttacksNumber).CurrentValue;

    internal static readonly CharacterValidator InBattle = _ =>
        ServiceRepository.GetService<IGameLocationBattleService>().IsBattleInProgress;
    
    
    [NotNull]
    internal static IsCharacterValidHandler HasAnyOfConditions(params string[] conditions)
    {
        return character => conditions.Any(character.HasConditionOfType);
    }

    [NotNull]
    internal static CharacterValidator HasBeenGrantedFeature(FeatureDefinition feature)
    {
        return character =>
        {
            Main.Log($"Checking for {feature.Name}", true);
            return character is RulesetCharacterHero hero &&
                   hero.activeFeatures.Any(item => item.Value.Contains(feature));
        };
    }
#endif

    internal static readonly IsCharacterValidHandler HasUnarmedHand = character =>
    {
        var slotsByName = character.CharacterInventory.InventorySlotsByName;
        var main = slotsByName[EquipmentDefinitions.SlotTypeMainHand].EquipedItem;
        var off = slotsByName[EquipmentDefinitions.SlotTypeOffHand].EquipedItem;

        return ValidatorsWeapon.IsUnarmedWeapon(main)
               || (!ValidatorsWeapon.IsTwoHanded(main) && ValidatorsWeapon.IsUnarmedWeapon(off));
    };

    internal static readonly IsCharacterValidHandler HasFreeHand = character => character.HasFreeHandSlot();

    internal static readonly IsCharacterValidHandler LightArmor = character =>
    {
        var equipedItem = character.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeTorso]
            .EquipedItem;

        if (equipedItem == null || !equipedItem.ItemDefinition.IsArmor)
        {
            return false;
        }

        var armorDescription = equipedItem.ItemDefinition.ArmorDescription;
        var element = DatabaseRepository.GetDatabase<ArmorTypeDefinition>().GetElement(armorDescription.ArmorType);

        return DatabaseRepository.GetDatabase<ArmorCategoryDefinition>().GetElement(element.ArmorCategory)
            .IsPhysicalArmor && element.ArmorCategory == EquipmentDefinitions.LightArmorCategory;
    };

    internal static readonly IsCharacterValidHandler OffHandHasLightSource = character =>
    {
        switch (character)
        {
            case null:
                return false;
            case RulesetCharacterHero hero:
            {
                var offItem = hero.CharacterInventory.InventorySlotsByName[EquipmentDefinitions.SlotTypeOffHand]
                    .EquipedItem;

                return offItem != null && offItem.ItemDefinition != null && offItem.ItemDefinition.IsLightSourceItem;
            }
            default:
                return false;
        }
    };

    // Does character has free offhand in TA's terms as used in RefreshAttackModes for bonus unarmed attack for Monk?
    // defined as having offhand empty or being not a weapon
    internal static bool IsFreeOffhandForUnarmedTa(RulesetCharacter character)
    {
        if (character is not RulesetCharacterHero hero)
        {
            return true;
        }

        var offHand = hero.CharacterInventory
            .InventorySlotsByType[EquipmentDefinitions.SlotTypeOffHand][0]
            .EquipedItem;

        return offHand == null || !offHand.ItemDefinition.IsWeapon;
    }

    internal static bool IsFreeOffhand(RulesetCharacter character)
    {
        if (character is not RulesetCharacterHero hero)
        {
            return true;
        }

        return hero.CharacterInventory
            .InventorySlotsByType[EquipmentDefinitions.SlotTypeOffHand][0]
            .EquipedItem == null;
    }

    [NotNull]
    internal static IsCharacterValidHandler HasAnyOfConditions(params ConditionDefinition[] conditions)
    {
        return character => conditions.Any(c => character.HasConditionOfType(c.Name));
    }
    
    [NotNull]
    internal static IsCharacterValidHandler HasNoCondition(params string[] conditions)
    {
        return character => !conditions.Any(c => character.HasConditionOfType(c));
    }

    internal static bool HasConditionWithSubFeatureOfType<T>(this RulesetCharacter character) where T : class
    {
        return character.conditionsByCategory
            .Any(keyValuePair => keyValuePair.Value
                .Any(rulesetCondition => rulesetCondition.ConditionDefinition.HasSubFeatureOfType<T>()));
    }

#if false
    internal static IsCharacterValidHandler HasAnyFeature(params FeatureDefinition[] features)
    {
        return character => character.HasAnyFeature(features);
    }
#endif
}
