using System.Collections.Generic;
using StardewModdingAPI;

namespace StardewAutoGifter.Npcs
{
    /// <summary>
    /// Persists which villagers the mod should consider gifting, chosen from the mod menu's NPCs
    /// tab (through <see cref="SaveDataStore{T}"/>). See
    /// specs/daily-plan-review - "Configured NPCs selected from the mod menu".
    /// </summary>
    public class ConfiguredNpcStore
    {
        private readonly SaveDataStore<ConfiguredNpcData> _store;
        private ConfiguredNpcData _data = new();

        public ConfiguredNpcStore(IModHelper helper)
        {
            _store = new SaveDataStore<ConfiguredNpcData>(helper, "configured-npcs");
        }

        public IReadOnlyCollection<string> SelectedNames => _data.SelectedNpcNames;

        public void Load()
        {
            _data = _store.Read() ?? new ConfiguredNpcData();
        }

        private void Save() => _store.Write(_data);

        public bool Contains(string npcName) => _data.SelectedNpcNames.Contains(npcName);

        public void Toggle(string npcName)
        {
            if (!_data.SelectedNpcNames.Remove(npcName))
                _data.SelectedNpcNames.Add(npcName);

            Save();
        }
    }
}
