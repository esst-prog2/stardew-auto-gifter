using StardewModdingAPI;

namespace StardewAutoGifter.Settings
{
    /// <summary>
    /// Persists menu-editable mod settings per save (through <see cref="SaveDataStore{T}"/>).
    /// See specs/daily-plan-review - "Anchor-chest-only setting in the mod menu".
    /// </summary>
    public class ModSettingsStore
    {
        private readonly SaveDataStore<ModSettingsData> _store;
        private ModSettingsData _data = new();

        public ModSettingsStore(IModHelper helper)
        {
            _store = new SaveDataStore<ModSettingsData>(helper, "settings");
        }

        public bool AnchorChestOnly => _data.AnchorChestOnly;

        public void Load()
        {
            _data = _store.Read() ?? new ModSettingsData();
        }

        public void ToggleAnchorChestOnly()
        {
            _data.AnchorChestOnly = !_data.AnchorChestOnly;
            _store.Write(_data);
        }
    }
}
