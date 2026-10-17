using StardewModdingAPI;

namespace StardewAutoGifter
{
    /// <summary>
    /// Reads and writes one piece of the mod's per-save data under a fixed key, through SMAPI's
    /// save-data helper (written to disk when the game saves). Every store in the mod uses this
    /// instead of its own copy of the same calls (HW6 review 04f). The keys are fixed: changing one
    /// would lose existing saves' data (PLANNING_LOG.md, HW6 review 02a, C8).
    /// </summary>
    public sealed class SaveDataStore<T> where T : class
    {
        private readonly IModHelper _helper;
        private readonly string _key;

        public SaveDataStore(IModHelper helper, string key)
        {
            _helper = helper;
            _key = key;
        }

        /// <summary>The stored value for the current save, or null if none was stored yet.</summary>
        public T? Read() => _helper.Data.ReadSaveData<T>(_key);

        /// <summary>Stores the value for the current save; null removes it.</summary>
        public void Write(T? data) => _helper.Data.WriteSaveData(_key, data);
    }
}
