using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Patches;
using SolastaUnfinishedBusiness.Subclasses;
using TinyJson;
using UnityEngine;
using UniverseLib.UI.Widgets.ScrollView;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static UnityEngine.UI.Image;
using Resources = SolastaUnfinishedBusiness.Properties.Resources;

namespace SolastaUnfinishedBusiness.Models;
internal static class SpellbookContext
{
    private static bool _initialized;
    internal readonly static string LOOTED_SPELLBOOK_PREFIX = "LootedSpellbook_";
    private static Dictionary<string, bool> lootPackDefinitionsProcessed = new Dictionary<string, bool>();
    private static Dictionary<string, MonsterDefinition> monsterProcessed = new Dictionary<string, MonsterDefinition>();
    private static Dictionary<string, SpellDefinition> wizardSubspellParent = new Dictionary<string, SpellDefinition>();
    private static Dictionary<string, List<SpellDefinition>> monsterSpells = new Dictionary<string, List<SpellDefinition>>();

    private static ItemDefinition _spellbookDefinition = DatabaseHelper.GetDefinition<ItemDefinition>("Spellbook");

    internal static void Load()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        AddSpellbookToAllMonsters();
    }

    internal static void AddSpellbookToAllMonsters()
    {
        Main.Info("AddSpellbookToAllMonsters");
        foreach (var monster in DatabaseRepository.GetDatabase<MonsterDefinition>())
        {
            if (monster != null) AddSpellbookToMonster(monster);
        }
    }

    private static void initializeWizardSubspellParents()
    {
        if (wizardSubspellParent.Count == 0)
        {
            foreach (var duplet in SpellListDefinitions.SpellListWizard.SpellsByLevel)
            {
                var spells = duplet.spells;
                foreach (var spell in spells)
                {
                    if (!wizardSubspellParent.ContainsKey(spell.Name))
                    {
                        wizardSubspellParent[spell.Name] = spell;
                        foreach (var subspell in spell.SubspellsList)
                        {
                            wizardSubspellParent[subspell.Name] = spell;
                        }
                    }
                }
            }
        }
    }

    internal static SpellDefinition GetSpellRoot(SpellDefinition spell)
    {
        initializeWizardSubspellParents();

        if (wizardSubspellParent.ContainsKey(spell.Name)) return wizardSubspellParent[spell.Name];
        else return spell;
    }

    internal static List<SpellDefinition> GetWizardSpells(MonsterDefinition monster)
    {
        var result = new List<SpellDefinition>();

        if (monster.features != null)
        {
            foreach (var feature in monster.Features)
            {
                if (feature is FeatureDefinitionCastSpell featureCastSpell)
                {
                    Main.Info($"* {monster.Name} CastSpell {featureCastSpell.Name} ability {featureCastSpell.SpellcastingAbility} castlevel {featureCastSpell.SpellCastingLevel} maxlevel {featureCastSpell.SpellListDefinition?.MaxSpellLevel}");

                    if (featureCastSpell.SpellListDefinition != null)
                    {
                        foreach (var spellDuplet in featureCastSpell.SpellListDefinition.SpellsByLevel)
                        {
                            var level = spellDuplet.Level;
                            foreach (var spell in spellDuplet.Spells)
                            {
                                if (level > 0)
                                {
                                    var s = GetSpellRoot(spell);
                                    Main.Info($"> {monster.Name} LVL {level} Spell {s.Name} WIZARD {SpellListDefinitions.SpellListWizard.ContainsSpell(s)}");
                                    if (!result.Contains(s) && SpellListDefinitions.SpellListWizard.ContainsSpell(s)) result.Add(s);
                                }

                            }
                        }

                        foreach (var s in featureCastSpell.SpellListDefinition.SourceSpellLists)
                        {
                            Main.Info($"! {monster.Name} SourceSpellLists {s.Name}");
                        }
                    }
                }
            }
        }

        foreach (var spell in result)
        {
            Main.Info($"++ {monster.Name} GetWizardSpells {spell.Name}");
        }

        return result;
    }

    internal static void AddSpellbookToMonster(MonsterDefinition monster)
    {
        if (!monster) return;

        if (monsterProcessed.ContainsKey(monster.name)) return;
        monsterProcessed.Add(monster.name, monster);

        Main.Info($"SpellbookContext MONSTER {monster.Name} family {monster.CharacterFamily} lootdef {monster.DroppedLootDefinition?.Name}");
        var wizardSpellList = GetWizardSpells(monster);
        if (wizardSpellList.Count == 0) return;

        monsterSpells[monster.Name] = wizardSpellList;

        if (monster.DroppedLootDefinition == null)
        {
            Main.Info($"SpellbookContext MONSTER {monster.Name} NEW");
            var spellbookLootPack = LootPackDefinitionBuilder
                .Create($"CE_SpellbookLoot_{monster.Name}")
                .SetGuiPresentationNoContent()
                .AddExplicitItem(ItemDefinitions._1D6_Gold_Coins)
                .AddToDB();

            monster.DroppedLootDefinition = spellbookLootPack;
        }

        if (!lootPackDefinitionsProcessed.ContainsKey(monster.DroppedLootDefinition.Name))
        {
            lootPackDefinitionsProcessed.Add(monster.DroppedLootDefinition.Name, true);

            foreach (var spell in wizardSpellList)
            {
                Main.Info($"!! AddSpellbookToMonster spell {monster.name} {spell.Name}");
            }

            
            var spellbookDef = ItemDefinitionBuilder
                .Create(_spellbookDefinition, $"{LOOTED_SPELLBOOK_PREFIX}{monster.Name}")
                .SetGuiPresentation(_spellbookDefinition.GuiPresentation)
                .MakeMagical()
                .HideFromDungeonEditor()
                .AddToDB();
            

            //var spellbookDef = _spellbookDefinition;
            //spellbookDef.ItemTags.Add($"{LOOTED_SPELLBOOK_PREFIX}{monster.Name}");

            
            var guiWrapperService = ServiceRepository.GetService<IGuiWrapperService>();
            guiWrapperService.AddUserItem(spellbookDef);
            

            monster.DroppedLootDefinition.ItemOccurencesList.Add(
                new ItemOccurence
                {
                    itemMode = ItemOccurence.SelectionMode.Explicit,
                    itemDefinition = spellbookDef,
                    diceNumber = 1,
                    diceType = RuleDefinitions.DieType.D1,
                    additiveModifier = 0
                }
             );
        }
    }

    private static string GetMonsterIDFromSpellbook(RulesetItemSpellbook spellbook)
    {
        var result = "";

        Main.Info($"GetMonsterNameFromSpellbook");
        /*
        foreach(var tag in spellbook.ItemDefinition.ItemTags)
        {
            Main.Info($"> GetMonsterNameFromSpellbook tag {tag}");
            if (tag.Contains(LOOTED_SPELLBOOK_PREFIX))
            {
                result = tag.Replace(LOOTED_SPELLBOOK_PREFIX, "");
                break;
            }
        }
        */

        if (spellbook.ItemDefinition.Name.Contains(LOOTED_SPELLBOOK_PREFIX)) result = spellbook.ItemDefinition.Name.ReplaceFirst(LOOTED_SPELLBOOK_PREFIX, "");


        return result;
    }

    internal static void LootGroundScribeSpellbooks(CharacterActionLootGround action)
    {
        var groundItems = new Dictionary<RulesetItem, TA.int3>();

        var itemService = ServiceRepository.GetService<IGameLocationItemService>();
        var gameLocationCharacter = action.ActingCharacter;

        if (itemService != null && gameLocationCharacter != null) itemService.EnumerateGroundItemsAroundCharacter(action.ActingCharacter, 10, groundItems);

        foreach (var entry in groundItems)
        {
            var item = entry.Key;

            Main.Info($"LootGroundScribeSpellbooks {item.Name} owner {item.OwnerName}");

            if (item is RulesetItemSpellbook spellbook)
            {
                if (spellbook.ScribedSpells == null || spellbook.ScribedSpells.Count == 0)
                {
                    //var monsterName = spellbook.ItemDefinition.Name.ReplaceFirst("Spellbook_", "");
                    var monsterName = GetMonsterIDFromSpellbook(spellbook);
                    if (monsterName != null && monsterName != "" && monsterProcessed.ContainsKey(monsterName))
                    {
                        var monster = monsterProcessed[monsterName];
                        spellbook.ItemDefinition = _spellbookDefinition;
                        spellbook.ScribedSpells = GenerateSpellList(monster);
                        spellbook.OwnerName = monsterName.Replace("_", " ");
                    }
                }
            }
        }
    }

    internal static List<SpellDefinition> GenerateSpellList(MonsterDefinition monster)
    {
        var result = new List<SpellDefinition>();

        if (monsterSpells.ContainsKey(monster.Name)) result = monsterSpells[monster.Name];
        else result.Add(SpellDefinitions.MagicMissile);

        foreach (var spell in result)
        {
            Main.Info($"++ {monster.Name} GenerateSpellList {spell.Name}");
        }

        return result;
    }

    internal static List<SpellDefinition> GenerateSpellList(int maxSpellLevel = 1)
    {
        var result = new List<SpellDefinition>();

        result.Add(SpellDefinitions.CharmPerson);

        return result;
    }
}
