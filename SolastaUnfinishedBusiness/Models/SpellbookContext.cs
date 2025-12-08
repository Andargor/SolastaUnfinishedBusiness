using System;
using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;

namespace SolastaUnfinishedBusiness.Models;
internal static class SpellbookContext
{
    private static Dictionary<string, SpellDefinition> wizardSubspellParent = new Dictionary<string, SpellDefinition>();
    private static Dictionary<string, List<SpellDefinition>> monsterSpellCache = new Dictionary<string, List<SpellDefinition>>();

    private static ItemDefinition _spellbookDefinition = DatabaseHelper.GetDefinition<ItemDefinition>("Spellbook");

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

    internal static List<SpellDefinition> GetWizardSpells(MonsterDefinition monsterDef)
    {
        var result = new List<SpellDefinition>();

        if (monsterDef == null || monsterDef.Name == null) return result;
        if (monsterSpellCache.ContainsKey(monsterDef.Name)) return monsterSpellCache[monsterDef.Name];

        if (monsterDef.features != null)
        {
            foreach (var feature in monsterDef.Features)
            {
                if (feature is FeatureDefinitionCastSpell featureCastSpell)
                {
                    Main.Info($"* {monsterDef.Name} CastSpell {featureCastSpell.Name} ability {featureCastSpell.SpellcastingAbility} castlevel {featureCastSpell.SpellCastingLevel} maxlevel {featureCastSpell.SpellListDefinition?.MaxSpellLevel}");

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
                                    Main.Info($"> {monsterDef.Name} LVL {level} Spell {s.Name} WIZARD {SpellListDefinitions.SpellListWizard.ContainsSpell(s)}");
                                    if (!result.Contains(s) && SpellListDefinitions.SpellListWizard.ContainsSpell(s)) result.Add(s);
                                }

                            }
                        }

                        foreach (var s in featureCastSpell.SpellListDefinition.SourceSpellLists)
                        {
                            Main.Info($"! {monsterDef.Name} SourceSpellLists {s.Name}");
                        }
                    }
                }
            }
        }

        foreach (var spell in result)
        {
            Main.Info($"++ {monsterDef.Name} GetWizardSpells {spell.Name}");
        }

        monsterSpellCache[monsterDef.Name] = result;

        return result;
    }

    internal static RulesetItemSpellbook MakeBlankSpellbook()
    {
        IRulesetItemFactoryService service = ServiceRepository.GetService<IRulesetItemFactoryService>();
        RulesetItem item = service.CreateStandardItem(_spellbookDefinition);
        if (item is RulesetItemSpellbook result) return result;

        return null;
    }

    internal static void AddSpellbookToDroppedLoot(RulesetCharacterMonster monster)
    {
        if (monster == null || monster.Name == null || monster.MonsterDefinition == null || monster.droppedItems == null) return;

        Main.Info($"AddSpellbookToDroppedLoot {monster.Name}");

        var spellList = GetWizardSpells(monster.MonsterDefinition);
        if (spellList.Count > 0)
        {
            foreach (var droppedItem in monster.droppedItems)
            {
                if (droppedItem is RulesetItemSpellbook droppedSpellbook)
                {
                    if (droppedSpellbook.ScribedSpells.Count > 0)
                    {
                        Main.Info($"AddSpellbookToDroppedLoot {monster.Name} SKIPPED");
                        return;
                    }
                }
            }

            var spellbook = MakeBlankSpellbook();
            if (spellbook != null)
            {
                spellbook.ScribedSpells = spellList;
                spellbook.OwnerName = monster.Name;

                monster.droppedItems.Add(spellbook);
                Main.Info($"AddSpellbookToDroppedLoot {monster.Name} ADDED");
            }
        }
    }
}
