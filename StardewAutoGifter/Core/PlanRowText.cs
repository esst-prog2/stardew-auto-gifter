namespace StardewAutoGifter.Core
{
    /// <summary>
    /// The Plan tab's reason line for a configured NPC with no planned gift. See
    /// specs/daily-plan-review - "Reason shown for configured NPCs with no gift".
    /// </summary>
    public static class PlanRowText
    {
        public static string ReasonLine(GiftPlanStatus status, string npcName) => status switch
        {
            GiftPlanStatus.NoEligibleItem => $"{npcName}: nothing to give (no loved/liked item available)",
            GiftPlanStatus.WeeklyCapReached => $"{npcName}: already reached this week's gift limit",
            GiftPlanStatus.NpcNotFound => $"{npcName}: not found in the game",
            _ => npcName,
        };
    }
}
