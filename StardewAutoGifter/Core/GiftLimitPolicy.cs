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
        /// Whether a gift can currently be given to an NPC, given their gift counts.
        /// NOTE: as of this commit, <paramref name="giftsToday"/> is accepted but intentionally not
        /// used - this mirrors the mod's current (incomplete) behavior, which only enforces the
        /// weekly cap and does not yet check the daily one. See hw5/expected-value-source.md and
        /// PLANNING_LOG.md for the daily-limit gap this deliberately does not yet close.
        /// </summary>
        public static bool CanGiveGift(int giftsToday, int giftsThisWeek)
            => giftsToday < 1 && giftsThisWeek < WeeklyFriendshipGiftCap;
    }
}
