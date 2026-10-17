namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Pure gift-limit decision logic, decoupled from the game so it can be unit tested without a
    /// running game. See PLANNING_LOG.md (hw5) and hw5/expected-value-source.md for the
    /// wiki-sourced expected values this is built against.
    /// </summary>
    public static class GiftLimitPolicy
    {
        /// <summary>The vanilla cap on friendship-point-counting gifts per NPC per week.</summary>
        public const int WeeklyFriendshipGiftCap = 2;

        /// <summary>
        /// Whether a gift can be given to an NPC right now, given their gift counts. Enforces both
        /// the daily limit (one gift per day: <paramref name="giftsToday"/> must be 0) and the
        /// weekly cap (<see cref="WeeklyFriendshipGiftCap"/>). Used by the delivery path only;
        /// planning uses <see cref="CanPlanGiftForNextDay"/>, which ignores the daily limit because
        /// the plan is for the next day. See hw5/expected-value-source.md.
        /// </summary>
        public static bool CanGiveGift(int giftsToday, int giftsThisWeek)
            => giftsToday < 1 && giftsThisWeek < WeeklyFriendshipGiftCap;

        /// <summary>
        /// Whether a villager can be put in the plan for the next day, given their gift counts as
        /// read at evening planning time. Used by the planning path only; delivery uses
        /// <see cref="CanGiveGift"/>.
        /// </summary>
        public static bool CanPlanGiftForNextDay(int giftsToday, int giftsThisWeek)
            => giftsThisWeek < WeeklyFriendshipGiftCap;
    }
}
