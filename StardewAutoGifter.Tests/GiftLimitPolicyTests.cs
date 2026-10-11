using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values sourced from the Stardew Valley Wiki's Friendship page ("one gift per day,
    /// up to two gifts per week"), not from running our own program. See
    /// hw5/expected-value-source.md and PLANNING_LOG.md (hw5 entries, 2026-10-11).
    /// </summary>
    public class GiftLimitPolicyTests
    {
        [Fact]
        public void CannotGiveGift_WhenDailyLimitAlreadyUsed()
        {
            // GiftsToday = 1, GiftsThisWeek = 0 - well under the weekly cap, so this isolates the
            // daily check specifically (a failure here cannot be explained by the weekly rule).
            bool canGive = GiftLimitPolicy.CanGiveGift(giftsToday: 1, giftsThisWeek: 0);

            Assert.False(canGive);
        }

        [Fact]
        public void CannotGiveGift_WhenWeeklyLimitAlreadyUsed()
        {
            bool canGive = GiftLimitPolicy.CanGiveGift(giftsToday: 0, giftsThisWeek: 2);

            Assert.False(canGive);
        }

        [Fact]
        public void CanGiveGift_WhenNeitherLimitReached()
        {
            bool canGive = GiftLimitPolicy.CanGiveGift(giftsToday: 0, giftsThisWeek: 1);

            Assert.True(canGive);
        }

        // Planning-time expected values (hw5/expected-value-source.md, "Planning-time expected
        // values"): the plan is made in the evening for the next day, which is a new day.

        [Fact]
        public void CanPlanForNextDay_WhenGiftedTodayButWeeklyLimitNotReached()
        {
            bool canPlan = GiftLimitPolicy.CanPlanGiftForNextDay(giftsToday: 1, giftsThisWeek: 1);

            Assert.True(canPlan);
        }

        [Fact]
        public void CannotPlanForNextDay_WhenWeeklyLimitAlreadyUsed()
        {
            bool canPlan = GiftLimitPolicy.CanPlanGiftForNextDay(giftsToday: 0, giftsThisWeek: 2);

            Assert.False(canPlan);
        }
    }
}
