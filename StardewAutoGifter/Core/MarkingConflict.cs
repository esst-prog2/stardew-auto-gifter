namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Pure rules and texts for telling the player why the mod is inactive. Key names are passed in
    /// from the configuration, never written into the text. See
    /// specs/daily-plan-review - "Reason shown for configured NPCs with no gift" and "Warnings and
    /// info panel".
    /// </summary>
    public static class MarkingConflict
    {
        /// <summary>The active warning shown once when a marking conflict starts, naming the configured chest-marking key.</summary>
        public static string HudMessage(string chestMarkKey)
            => $"Auto-Gifter paused: more than one chest is marked. Unmark all but one with {chestMarkKey}.";

        /// <summary>Show the HUD message only at the moment a conflict starts, never again while it lasts.</summary>
        public static bool ShouldNotify(bool wasConflict, bool isConflict) => !wasConflict && isConflict;

        /// <summary>The Plan tab's message for an inactive mod, naming the fix for the cause and the configured chest-marking key; null while active (exactly one marked chest).</summary>
        public static string? InactivePlanMessage(int markedChestCount, string chestMarkKey)
        {
            if (AnchorRule.IsActive(markedChestCount))
                return null;

            return markedChestCount == 0
                ? $"Mod inactive: open a chest on your farm and press {chestMarkKey} to mark it."
                : $"Mod paused: more than one chest is marked. Open each extra chest and press {chestMarkKey} to unmark it until only one remains.";
        }
    }
}
