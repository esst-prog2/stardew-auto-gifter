using StardewModdingAPI;

namespace StardewAutoGifter.Settings
{
    /// <summary>
    /// Persists menu-editable mod settings per save (mirrors <see cref="StardewAutoGifter.Blocklist.BlocklistStore"/>).
    /// See specs/daily-plan-review - "Anchor-chest-only setting in the mod menu".
    /// </summary>
    public class ModSettingsStore
    {
        private const string SaveDataKey = "settings";

        private readonly IModHelper _helper;
        private ModSettingsData _data = new();

        public ModSettingsStore(IModHelper helper)
        {
            _helper = helper;
        }

        public bool AnchorChestOnly => _data.AnchorChestOnly;

        public void Load()
        {
            _data = _helper.Data.ReadSaveData<ModSettingsData>(SaveDataKey) ?? new ModSettingsData();
        }

        public void ToggleAnchorChestOnly()
        {
            _data.AnchorChestOnly = !_data.AnchorChestOnly;
            _helper.Data.WriteSaveData(SaveDataKey, _data);
        }
    }
}
