using System.Collections.Generic;

namespace StardewAutoGifter.Blocklist
{
    /// <summary>Serialized shape of the per-save blocklist data.</summary>
    public class BlocklistData
    {
        public HashSet<string> BlockedItemIds { get; set; } = new();
    }
}
