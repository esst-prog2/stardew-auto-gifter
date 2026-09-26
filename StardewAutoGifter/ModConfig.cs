using StardewModdingAPI;

namespace StardewAutoGifter
{
    /// <summary>Player-editable settings, generated as config.json by SMAPI on first launch.</summary>
    public class ModConfig
    {
        /// <summary>Opens the mod's menu (daily plan preview, blocklist editor, warnings panel). See specs/daily-plan-review.</summary>
        public SButton MenuKey { get; set; } = SButton.F8;

        /// <summary>While a chest's inventory is open, marks/unmarks it as the mod's on/off anchor. See specs/gift-selection.</summary>
        public SButton ChestMarkKey { get; set; } = SButton.G;

        /// <summary>When true, selection falls back to an NPC's liked items if no eligible loved item exists. See specs/gift-selection.</summary>
        public bool IncludeLikedItems { get; set; } = true;
    }
}
