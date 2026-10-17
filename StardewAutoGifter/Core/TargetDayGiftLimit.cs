namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Judges the gift limit for a given target day instead of for today (HW6 review 03, R1). It
    /// only adjusts today's counts to what they will be on the target day, then asks the unchanged
    /// <see cref="GiftLimitPolicy.CanGiveGift"/>. Days are SMAPI's day count (day 1 = spring 1 of
    /// year 1, a Monday); weeks begin on Sunday. See reviews/03-week-rollover-source.md and
    /// specs/gift-selection - "Gift-limit state respected during selection".
    /// </summary>
    public static class TargetDayGiftLimit
    {
        /// <summary>Whether a gift could be given to an NPC on <paramref name="targetDay"/>, given their gift counts as read on <paramref name="today"/>.</summary>
        public static bool CanGiveGiftOn(int targetDay, int today, int giftsToday, int giftsThisWeek)
        {
            int dailyCount = targetDay == today ? giftsToday : 0;
            int weeklyCount = StartsNewWeek(targetDay, today) ? 0 : giftsThisWeek;
            return GiftLimitPolicy.CanGiveGift(dailyCount, weeklyCount);
        }

        /// <summary>
        /// Week number with weeks beginning on Sunday: day 7 (spring 7) is the first Sunday, so
        /// <c>day / 7</c> is 0 for days 1-6 and 1 for days 7-13. A year is 112 days (a multiple of
        /// 7), so this also holds across years.
        /// </summary>
        public static int WeekOf(int day) => day / 7;

        /// <summary>True if the target day falls in a later week than today.</summary>
        public static bool StartsNewWeek(int targetDay, int today) => WeekOf(targetDay) != WeekOf(today);
    }
}
