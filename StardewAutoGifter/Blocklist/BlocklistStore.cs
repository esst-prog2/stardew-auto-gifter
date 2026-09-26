using StardewModdingAPI;

namespace StardewAutoGifter.Blocklist
{
    /// <summary>
    /// Persists the blocklist per save via SMAPI's save-data helper. See specs/daily-plan-review -
    /// "Toggling blocklist membership via checkbox".
    /// </summary>
    public class BlocklistStore
    {
        private const string SaveDataKey = "blocklist";

        private readonly IModHelper _helper;
        private BlocklistData _data = new();

        public BlocklistStore(IModHelper helper)
        {
            _helper = helper;
        }

        public System.Collections.Generic.IReadOnlyCollection<string> BlockedItemIds => _data.BlockedItemIds;

        public void Load()
        {
            _data = _helper.Data.ReadSaveData<BlocklistData>(SaveDataKey) ?? new BlocklistData();
        }

        private void Save() => _helper.Data.WriteSaveData(SaveDataKey, _data);

        public bool Contains(string qualifiedItemId) => _data.BlockedItemIds.Contains(qualifiedItemId);

        public void Toggle(string qualifiedItemId)
        {
            if (!_data.BlockedItemIds.Remove(qualifiedItemId))
                _data.BlockedItemIds.Add(qualifiedItemId);

            Save();
        }
    }
}
