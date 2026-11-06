namespace StardewAutoGifter.Core
{
    /// <summary>
    /// The pure order of checks a configured villager goes through before an item is selected:
    /// found in the game, then met by the player, then the gift limit for the plan's target day.
    /// See specs/gift-selection - "Villagers not met yet are not gifted" (HW7a).
    /// </summary>
    public static class PlanPrecheck
    {
        /// <summary>The status that stops selection for this villager, or null to go on and select an item.</summary>
        public static GiftPlanStatus? StatusBeforeSelection(bool npcFound, bool hasMet, bool withinGiftLimit)
        {
            if (!npcFound)
                return GiftPlanStatus.NpcNotFound;
            if (!hasMet)
                return GiftPlanStatus.NotMetYet;
            if (!withinGiftLimit)
                return GiftPlanStatus.WeeklyCapReached;
            return null;
        }
    }
}
