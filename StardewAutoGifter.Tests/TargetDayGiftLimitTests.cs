using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from reviews/03-week-rollover-source.md (Stardew Valley Wiki,
    /// Friendship: "the week begins on Sunday" and "one gift per day, up to two gifts per week"),
    /// recorded in commit cb8d09e before these tests were written. Days are SMAPI's day count:
    /// day 2 = Tuesday, day 3 = Wednesday, day 6 = Saturday, day 7 = Sunday.
    /// </summary>
    public class TargetDayGiftLimitTests
    {
        // Saturday evening (day 6) planning for Sunday (day 7), GiftsThisWeek = 2 -> can plan.
        [Fact]
        public void SaturdayEveningForSunday_WeeklyLimitUsed_CanPlan()
        {
            Assert.True(TargetDayGiftLimit.CanGiveGiftOn(targetDay: 7, today: 6, giftsToday: 0, giftsThisWeek: 2));
        }

        // Weekday evening (day 2) planning for the next day in the same week (day 3), GiftsThisWeek = 2 -> cannot plan.
        [Fact]
        public void WeekdayEveningForNextDay_WeeklyLimitUsed_CannotPlan()
        {
            Assert.False(TargetDayGiftLimit.CanGiveGiftOn(targetDay: 3, today: 2, giftsToday: 0, giftsThisWeek: 2));
        }

        // Today (day 3 -> day 3), GiftsToday = 1 -> no gift can be given.
        [Fact]
        public void Today_DailyLimitUsed_CannotGive()
        {
            Assert.False(TargetDayGiftLimit.CanGiveGiftOn(targetDay: 3, today: 3, giftsToday: 1, giftsThisWeek: 0));
        }
    }
}
