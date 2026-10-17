using System.Collections.Generic;
using System.Linq;
using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec scenarios (HW6 review 08c, reviews/08-quiet-defaults.md):
    /// specs/daily-plan-review - "Edited plan carried into the next day" (scenarios "Stored plan
    /// without a valid target day", "Stored row without a villager name", "Stored plan without a
    /// row list") and "Damaged saved lists reset with a warning".
    /// </summary>
    public class SaveDataRepairTests
    {
        // "Stored plan without a valid target day": one warning about the target day.
        [Fact]
        public void TargetDayZero_GivesOneTargetDayWarning()
        {
            var data = new StoredPlanData { TargetDay = 0, IsActive = true };
            var warnings = new List<string>();

            data.ToPlan(warnings);

            Assert.Single(warnings);
            Assert.Contains("target day", warnings[0]);
        }

        // "Stored row without a villager name": the nameless row is skipped, Haley's row stays, one warning.
        [Fact]
        public void NamelessRow_IsSkipped_WithOneWarning()
        {
            var data = new StoredPlanData { TargetDay = 11, IsActive = true };
            data.Gifts.Add(new StoredGiftData { NpcName = "", Status = GiftPlanStatus.Planned, QualifiedItemId = "(O)88" });
            data.Gifts.Add(new StoredGiftData { NpcName = "Haley", Status = GiftPlanStatus.Planned, QualifiedItemId = "(O)88" });
            var warnings = new List<string>();

            DailyPlan plan = data.ToPlan(warnings);

            Assert.Equal(new[] { "Haley" }, plan.Gifts.Select(g => g.NpcName));
            Assert.Single(warnings);
        }

        // "Stored plan without a row list": no rows, one warning.
        [Fact]
        public void NullRowList_LoadsNoRows_WithOneWarning()
        {
            var data = new StoredPlanData { TargetDay = 11, IsActive = true, Gifts = null! };
            var warnings = new List<string>();

            DailyPlan plan = data.ToPlan(warnings);

            Assert.Empty(plan.Gifts);
            Assert.Single(warnings);
        }

        // "Saved blocklist list is null": loaded empty, one warning naming BlockedItemIds.
        [Fact]
        public void NullSavedList_IsResetToEmpty_WithWarningNamingTheField()
        {
            var warnings = new List<string>();

            HashSet<string> set = SaveDataRepair.SetOrEmpty(null, "BlockedItemIds", warnings);

            Assert.Empty(set);
            Assert.Single(warnings);
            Assert.Contains("BlockedItemIds", warnings[0]);
        }

        // "Saved list is present": loaded as it is, no warning.
        [Fact]
        public void PresentSavedList_IsKept_WithoutWarning()
        {
            var saved = new HashSet<string> { "(O)74" };
            var warnings = new List<string>();

            HashSet<string> set = SaveDataRepair.SetOrEmpty(saved, "BlockedItemIds", warnings);

            Assert.Same(saved, set);
            Assert.Empty(warnings);
        }
    }
}
