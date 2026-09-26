namespace StardewAutoGifter.Planning
{
    /// <summary>
    /// Why a configured NPC does or doesn't have a planned gift. See specs/daily-plan-review -
    /// "Reason shown for configured NPCs with no gift".
    /// </summary>
    public enum GiftPlanStatus
    {
        /// <summary>An eligible item was selected; delivery is pending unless toggled off.</summary>
        Planned,

        /// <summary>No eligible loved/liked item was available in the farm-wide pool.</summary>
        NoEligibleItem,

        /// <summary>The NPC already reached the weekly friendship-point-counting gift limit.</summary>
        WeeklyCapReached,
    }
}
