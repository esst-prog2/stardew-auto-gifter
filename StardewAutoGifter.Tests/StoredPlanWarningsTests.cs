using System.Collections.Generic;
using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec scenarios (HW6 review 05e, reviews/05-swallowed-errors.md):
    /// specs/daily-plan-review - "Edited plan carried into the next day", scenarios "Damaged stored
    /// plan row" and "Undamaged stored plan".
    /// </summary>
    public class StoredPlanWarningsTests
    {
        // "Damaged stored plan row": loaded as no eligible item, one warning naming Haley.
        [Fact]
        public void DamagedRow_LoadsAsNoEligibleItem_WithOneWarningNamingTheNpc()
        {
            var data = new StoredPlanData { TargetDay = 11, IsActive = true };
            data.Gifts.Add(new StoredGiftData { NpcName = "Haley", Status = GiftPlanStatus.Planned, QualifiedItemId = null });
            var warnings = new List<string>();

            DailyPlan plan = data.ToPlan(warnings);

            Assert.Equal(GiftPlanStatus.NoEligibleItem, plan.FindFor("Haley")!.Status);
            Assert.Single(warnings);
            Assert.Contains("Haley", warnings[0]);
        }

        // "Undamaged stored plan": no warning.
        [Fact]
        public void CompleteRows_LoadWithoutWarnings()
        {
            var data = new StoredPlanData { TargetDay = 11, IsActive = true };
            data.Gifts.Add(new StoredGiftData { NpcName = "Haley", Status = GiftPlanStatus.Planned, QualifiedItemId = "(O)88" });
            data.Gifts.Add(new StoredGiftData { NpcName = "Abigail", Status = GiftPlanStatus.WeeklyCapReached });
            var warnings = new List<string>();

            data.ToPlan(warnings);

            Assert.Empty(warnings);
        }
    }
}
