namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Pure rules and texts for telling the player why the mod is inactive. See
    /// specs/daily-plan-review - "Reason shown for configured NPCs with no gift" and "Warnings and
    /// info panel".
    /// </summary>
    public static class MarkingConflict
    {
        /// <summary>The active warning shown once when a marking conflict starts.</summary>
        public const string HudMessage = "Auto-Gifter paused: more than one chest is marked. Unmark all but one with G.";

        /// <summary>Show the HUD message only at the moment a conflict starts, never again while it lasts.</summary>
        public static bool ShouldNotify(bool wasConflict, bool isConflict) => !wasConflict && isConflict;

        /// <summary>The Plan tab's message for an inactive mod, naming the fix for the cause; null while active (exactly one marked chest).</summary>
        public static string? InactivePlanMessage(int markedChestCount)
        {
            if (AnchorRule.IsActive(markedChestCount))
                return null;

            return markedChestCount == 0
                ? "Mod inactive: open a chest on your farm and press G to mark it."
                : "Mod paused: more than one chest is marked. Open each extra chest and press G to unmark it until only one remains.";
        }
    }
}
