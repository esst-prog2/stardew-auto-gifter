using System.Collections.Generic;
using StardewModdingAPI;

namespace StardewAutoGifter.Npcs
{
    /// <summary>
    /// Persists which villagers the mod should consider gifting, chosen from the mod menu's NPCs
    /// tab (mirrors <see cref="StardewAutoGifter.Blocklist.BlocklistStore"/>). See
    /// specs/daily-plan-review - "Configured NPCs selected from the mod menu".
    /// </summary>
    public class ConfiguredNpcStore
    {
        private const string SaveDataKey = "configured-npcs";

        private readonly IModHelper _helper;
        private ConfiguredNpcData _data = new();

        public ConfiguredNpcStore(IModHelper helper)
        {
            _helper = helper;
        }

        public IReadOnlyCollection<string> SelectedNames => _data.SelectedNpcNames;

        public void Load()
        {
            _data = _helper.Data.ReadSaveData<ConfiguredNpcData>(SaveDataKey) ?? new ConfiguredNpcData();
        }

        private void Save() => _helper.Data.WriteSaveData(SaveDataKey, _data);

        public bool Contains(string npcName) => _data.SelectedNpcNames.Contains(npcName);

        public void Toggle(string npcName)
        {
            if (!_data.SelectedNpcNames.Remove(npcName))
                _data.SelectedNpcNames.Add(npcName);

            Save();
        }
    }
}
