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
        /// weekly cap (<see cref="WeeklyFriendshipGiftCap"/>). Delivery (for today) and planning (for
        /// the target day) both reach it through <see cref="TargetDayGiftLimit.CanGiveGiftOn"/>, which
        /// first adjusts the counts for the day asked about. See hw5/expected-value-source.md.
        /// </summary>
        public static bool CanGiveGift(int giftsToday, int giftsThisWeek)
            => giftsToday < 1 && giftsThisWeek < WeeklyFriendshipGiftCap;

        /// <summary>
        /// Kept only for the HW5 tests (GiftLimitPolicyTests); nothing in the mod calls it any more.
        /// Planning uses <see cref="TargetDayGiftLimit.CanGiveGiftOn"/> instead (HW6 review 03c),
        /// which also handles the Sunday week rollover. Do not use it in new code.
        /// </summary>
        public static bool CanPlanGiftForNextDay(int giftsToday, int giftsThisWeek)
            => giftsThisWeek < WeeklyFriendshipGiftCap;
    }
}
