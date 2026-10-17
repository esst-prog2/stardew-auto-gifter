namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Texts on the Settings tab that the spec fixes word for word. See specs/daily-plan-review -
    /// "Anchor-chest-only setting in the mod menu".
    /// </summary>
    public static class SettingsTabText
    {
        /// <summary>Tells the player that the liked-items fallback is not a menu setting but lives in config.json (HW6 review 04g, D5).</summary>
        public const string LikedItemsNote = "Whether villagers get a liked item when no loved one is available is set in config.json (IncludeLikedItems), not in this menu.";
    }
}
