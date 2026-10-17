using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec scenarios (HW6 review 01b, reviews/01-spec-drift.md):
    /// specs/daily-plan-review - "Plan computed for the next day when the menu is opened",
    /// "Edited plan carried into the next day", "Next day's plan computed automatically at the end
    /// of the day"; specs/proximity-delivery - "Plan delivered only on its target day".
    /// </summary>
    public class PlanTimingTests
    {
        // "Plan targets the day after it was computed": opened on day 10 -> target day 11.
        [Fact]
        public void PlanComputedOnDay10_TargetsDay11()
        {
            Assert.Equal(11, PlanTiming.NextTargetDay(today: 10));
        }

        // "Plan computed today is not delivered today".
        [Fact]
        public void PlanForDay11_IsNotForToday_OnDay10()
        {
            Assert.False(PlanTiming.IsPlanForToday(planTargetDay: 11, today: 10));
        }

        // "Plan delivered on its target day" / "Plan becomes today's plan on its target day".
        [Fact]
        public void PlanForDay11_IsForToday_OnDay11()
        {
            Assert.True(PlanTiming.IsPlanForToday(planTargetDay: 11, today: 11));
        }

        // "Yesterday's plan is not delivered again".
        [Fact]
        public void PlanForDay11_IsNotForToday_OnDay12()
        {
            Assert.False(PlanTiming.IsPlanForToday(planTargetDay: 11, today: 12));
        }

        // "Plan for another day is not used": no stored plan -> that day has no plan.
        [Fact]
        public void NoPlan_IsNotForToday()
        {
            Assert.False(PlanTiming.IsPlanForToday(planTargetDay: null, today: 11));
        }

        // "Menu not opened during the day".
        [Fact]
        public void DayEnding_WithNoNextPlan_ComputesOne()
        {
            Assert.True(PlanTiming.ShouldComputeAtDayEnding(nextPlanTargetDay: null, nextPlanIsActive: false, today: 10));
        }

        // "Plan already made during the day".
        [Fact]
        public void DayEnding_WithActivePlanForTomorrow_KeepsIt()
        {
            Assert.False(PlanTiming.ShouldComputeAtDayEnding(nextPlanTargetDay: 11, nextPlanIsActive: true, today: 10));
        }

        // "Only an inactive plan exists".
        [Fact]
        public void DayEnding_WithOnlyInactivePlanForTomorrow_ComputesAgain()
        {
            Assert.True(PlanTiming.ShouldComputeAtDayEnding(nextPlanTargetDay: 11, nextPlanIsActive: false, today: 10));
        }

        // "Reopening the menu keeps toggled-off rows": the toggled-off NPC stays off, every other row stays on.
        [Fact]
        public void Recompute_KeepsToggledOffRow_AndLeavesOtherRowsOn()
        {
            var previous = new DailyPlan { TargetDay = 11 };
            previous.Gifts.Add(new PlannedGift("Haley", "(O)88") { Excluded = true });
            previous.Gifts.Add(new PlannedGift("Abigail", "(O)66"));

            var recomputed = new DailyPlan { TargetDay = 11 };
            recomputed.Gifts.Add(new PlannedGift("Haley", "(O)88"));
            recomputed.Gifts.Add(new PlannedGift("Abigail", "(O)66"));

            PlanTiming.CarryExclusions(previous, recomputed);

            Assert.True(recomputed.FindFor("Haley")!.Excluded);
            Assert.False(recomputed.FindFor("Abigail")!.Excluded);
        }

        // "Toggled-off NPC without a planned item after recomputing": nothing stays toggled off.
        [Fact]
        public void Recompute_ToggledOffNpcWithoutPlannedItem_IsNotToggledOff()
        {
            var previous = new DailyPlan { TargetDay = 11 };
            previous.Gifts.Add(new PlannedGift("Haley", "(O)88") { Excluded = true });

            var recomputed = new DailyPlan { TargetDay = 11 };
            recomputed.Gifts.Add(new PlannedGift("Haley", GiftPlanStatus.NoEligibleItem));

            PlanTiming.CarryExclusions(previous, recomputed);

            Assert.False(recomputed.FindFor("Haley")!.Excluded);
        }

        // "Plan survives saving and loading": same target day and rows, including the toggled-off one.
        [Fact]
        public void StoredPlan_RoundTrip_KeepsTargetDayRowsAndToggledOffRows()
        {
            var plan = new DailyPlan { TargetDay = 11, IsActive = true };
            plan.Gifts.Add(new PlannedGift("Haley", "(O)88") { Excluded = true });
            plan.Gifts.Add(new PlannedGift("Abigail", GiftPlanStatus.WeeklyCapReached));

            DailyPlan loaded = StoredPlanData.FromPlan(plan).ToPlan();

            Assert.Equal(11, loaded.TargetDay);
            Assert.True(loaded.IsActive);
            Assert.Equal(2, loaded.Gifts.Count);
            Assert.Equal("(O)88", loaded.FindFor("Haley")!.QualifiedItemId);
            Assert.True(loaded.FindFor("Haley")!.Excluded);
            Assert.Equal(GiftPlanStatus.WeeklyCapReached, loaded.FindFor("Abigail")!.Status);
        }

        // "Each day starts with no NPC marked as already delivered or already notified".
        [Fact]
        public void StoredPlan_RoundTrip_StartsWithNothingDelivered()
        {
            var plan = new DailyPlan { TargetDay = 11 };
            plan.Gifts.Add(new PlannedGift("Haley", "(O)88") { Resolved = true });

            DailyPlan loaded = StoredPlanData.FromPlan(plan).ToPlan();

            Assert.False(loaded.FindFor("Haley")!.Resolved);
        }
    }
}
