using System.Collections.Generic;

namespace StardewAutoGifter.Npcs
{
    /// <summary>Serialized shape of the per-save configured-NPC selection.</summary>
    public class ConfiguredNpcData
    {
        public HashSet<string> SelectedNpcNames { get; set; } = new();
    }
}
