using System.Collections.Generic;
using WDE.Common.DBC;
using WDE.Common.DBC.Structs;
using WDE.Common.Tasks;
using WDE.Common.Utils;
using WDE.DbcStore.Providers;
using WDE.DbcStore.Spells.Cataclysm;
using WDE.DbcStore.Structs;
using WDE.Module.Attributes;

namespace WDE.DbcStore.Loaders;

[AutoRegister]
internal class MopDbcLoader : BaseDbcLoader
{
    public MopDbcLoader(IDbcSettingsProvider dbcSettingsProvider, 
        IDatabaseClientFileOpener opener,
        DBCD.DBCD dbcd) : base(dbcSettingsProvider, opener, dbcd)
    {
    }

    public override DBCVersions Version => DBCVersions.MOP_18414;

    public override int StepsCount => 49;
    
    protected override void LoadDbcCore(DbcData data, ITaskProgress progress)
    {
        var fileData = new Dictionary<long, string>();
        Load("Achievement_Criteria.dbc", 0, 10, data.AchievementCriteriaStore);
        Load("FileData.dbc", 0, 1, fileData);
        Load("AreaTrigger.dbc", row => data.AreaTriggerStore.Add(row.GetInt(0), $"Area trigger"));
        Load("BattlemasterList.dbc", 0, 19, data.BattlegroundStore);
        Load("SkillLine.dbc", 0, 2, data.SkillStore);
        Load("Faction.dbc", row =>
        {
            var faction = new Faction(row.GetUShort(0), row.GetString(23));
            data.Factions.Add(faction);
            data.FactionStore[faction.FactionId] = faction.Name;
        });
        Load("FactionTemplate.dbc", row =>
        {
            var template = new FactionTemplate()
            {
                TemplateId = row.GetUInt(0),
                Faction = row.GetUShort(1),
                Flags = row.GetUShort(2),
                FactionGroup = (FactionGroupMask)row.GetUShort(3),
                FriendGroup = (FactionGroupMask)row.GetUShort(4),
                EnemyGroup = (FactionGroupMask)row.GetUShort(5)
            };
            data.FactionTemplates.Add(template);
            data.FactionTemplateStore[row.GetUInt(0)] = row.GetUInt(1);
        });
        Load("CurrencyTypes.dbc", 0, 2, data.CurrencyTypeStore);
        Load("CurrencyTypes.dbc", data.CurrencyTypes, row => 
            new CurrencyType()
            {
                Id = row.GetUInt(0),
                CategoryId = (byte)row.GetUShort(1),
                Name = row.GetString(2),
                InventoryIconPath = row.GetString(3),
                SpellWeight = row.GetUInt(5),
                SpellCategory = (byte)row.GetUShort(6),
                MaxQuantity = row.GetUInt(7),
                MaxEarnablePerWeek = row.GetUInt(8),
                Flags = (CurrencyTypesFlags)row.GetUInt(9),
                Quality = (byte)row.GetUShort(10),
                Description = row.GetString(11)
            });
        Load("Spell.dbc", 0, 1, data.SpellStore);
        Load("Movie.dbc", row => data.MovieStore.Add(row.GetInt(0), fileData.GetValueOrDefault(row.GetInt(3)) ?? "Unknown movie"));
        Load("Map.dbc", row =>
        {
            var map = new MapEntry()
            {
                Id = row.GetUInt(0),
                Name = row.GetString(5),
                Directory = row.GetString(1),
                Type = (InstanceType)row.GetUInt(2),
            };
            data.Maps.Add(map);
        });
        Load("Achievement.dbc", 0, 4, data.AchievementStore);
        Load("AreaTable.dbc", row =>
        {
            var entry = new AreaEntry()
            {
                Id = row.GetUInt(0),
                MapId = row.GetUInt(1),
                ParentAreaId = row.GetUInt(2),
                Flags1 = row.GetUInt(4),
                Flags2 = row.GetUInt(5),
                Name = row.GetString(13)
            };
            data.Areas.Add(entry);
        });
        FillMapAreas(data);
        Load("ChrClasses.dbc", 0, 3, data.ClassStore);
        Load("ChrRaces.dbc", 0, 14, data.RaceStore);
        Load("Difficulty.dbc", 0, 11, data.DifficultyStore);
        Load("Emotes.dbc", row =>
        {
            var proc = row.GetUInt(4);
            if (proc == 0)
                data.EmoteOneShotStore.Add(row.GetUInt(0), row.GetString(1));
            else if (proc == 2)
                data.EmoteStateStore.Add(row.GetUInt(0), row.GetString(1));
            data.EmoteStore.Add(row.GetUInt(0), row.GetString(1));
        });
        Load("EmotesText.dbc", 0, 1, data.TextEmoteStore);
        Load("Item-sparse.db2", 0, 100, data.ItemStore);
        Load("Phase.dbc", 0, 1, data.PhaseStore);
        Load("SoundEntries.dbc", 0, 2, data.SoundStore);
        Load("SpellFocusObject.dbc", 0, 1, data.SpellFocusObjectStore);
        Load("QuestInfo.dbc", 0, 1, data.QuestInfoStore);
        Load("CharTitles.dbc", 0, 2, data.CharTitleStore);
        Load("CreatureModelData.dbc", 0, 2, data.CreatureModelDataStore);
        Load("CreatureDisplayInfo.dbc", 0, 1, data.CreatureDisplayInfoStore);
        Load("GameObjectDisplayInfo.dbc", 0, 1, data.GameObjectDisplayInfoStore);
        Load("Languages.dbc", 0, 1, data.LanguageStore);
        Load("QuestSort.dbc", 0, 1, data.QuestSortStore);
        Load("ItemExtendedCost.dbc", row => data.ExtendedCostStore.Add(row.GetInt(0), GenerateCostDescription(row.GetInt(1), row.GetInt(2), row.GetInt(4))));
        Load("TaxiNodes.dbc", 0, 5, data.TaxiNodeStore);
        Load("TaxiPath.dbc",  row => data.TaxiPathsStore.Add(row.GetUInt(0), (row.GetInt(1), row.GetInt(2))));
        Load("SpellItemEnchantment.dbc", 0, 11, data.SpellItemEnchantmentStore);
        Load("AreaGroup.dbc",  row => data.AreaGroupStore.Add(row.GetUInt(0), BuildAreaGroupName(data, row, 1, 6)));
        Load("ItemDisplayInfo.dbc", row =>
        {
            data.ItemDisplayInfos.Add(new ItemDisplayInfoEntry()
            {
                Id = row.GetUInt(0),
                InventoryIconPath = row.GetString(5)
            });
        });
        Load("MailTemplate.dbc", row =>
        {
            var subject = row.GetString(1);
            var body = row.GetString(2);
            var name = string.IsNullOrEmpty(subject) ? body.TrimToLength(50) : subject;
            data.MailTemplateStore.Add(row.GetUInt(0), name.Replace("\n", ""));
        });
        Load("LFGDungeons.dbc", 0, 1, data.LFGDungeonStore);
        Load("ItemSet.dbc", 0, 1, data.ItemSetStore);
        Load("DungeonEncounter.dbc", 0, 5, data.DungeonEncounterStore);
        Load("HolidayNames.dbc", 0, 1, data.HolidayNamesStore);
        Load("Holidays.dbc", row =>
        {
            var id = row.GetUInt(0);
            var nameId = row.GetUInt(49);
            if (data.HolidayNamesStore.TryGetValue(nameId, out var name))
                data.HolidaysStore[id] = name;
            else
                data.HolidaysStore[id] = "Holiday " + id;
        });
        Load("WorldSafeLocs.dbc", 0, 6, data.WorldSafeLocsStore);
        Load("Item.dbc", row =>
        {
            data.Items.Add(new DbcItemEntry()
            {
                Id = row.GetUInt(0),
                DisplayInfoId = row.GetUInt(5)
            });
        });
        Load("LockType.dbc", 0, 1, data.LockTypeStore);
        Load("Vignette.dbc", 0, 1, data.VignetteStore);
        LoadAndRegister(data,"SpellCastTimes.dbc", "SpellCastTimeParameter", 0, row => GetCastTimeDescription(row.GetInt(1), row.GetInt(2), row.GetInt(3)));
        LoadAndRegister(data,"SpellDuration.dbc", "SpellDurationParameter", 0, row => GetDurationTimeDescription(row.GetInt(1), row.GetInt(2), row.GetInt(3)));
        LoadAndRegister(data, "SpellRange.dbc", "SpellRangeParameter", 0, row => GetRangeDescription(row.GetFloat(1), row.GetFloat(3), row.GetString(6), row.GetFloat(2), row.GetFloat(4)));
        LoadAndRegister(data,"SpellRadius.dbc", "SpellRadiusParameter", 0, row => GetRadiusDescription(row.GetFloat(1), row.GetFloat(2), row.GetFloat(4)));
    }
}