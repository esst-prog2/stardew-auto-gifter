using System.Collections.Generic;
using StardewModdingAPI;
using StardewAutoGifter.Core;

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

        /// <summary>Loads the saved data; a list stored as null is reset to empty with a Warn line (HW6 review 08c).</summary>
        public void Load(IMonitor monitor)
        {
            _data = _store.Read() ?? new ConfiguredNpcData();

            var warnings = new System.Collections.Generic.List<string>();
            _data.SelectedNpcNames = SaveDataRepair.SetOrEmpty(_data.SelectedNpcNames, "SelectedNpcNames", warnings);
            foreach (string warning in warnings)
                monitor.Log(warning, LogLevel.Warn);
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
