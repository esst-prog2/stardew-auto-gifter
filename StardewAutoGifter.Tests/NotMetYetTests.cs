using System.Linq;
using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec scenarios (HW7a, reviews/10-in-game-results.md, D2):
    /// specs/gift-selection - "Villagers not met yet are not gifted"; specs/daily-plan-review -
    /// "Reason shown for configured NPCs with no gift"; specs/proximity-delivery - "Delivery never
    /// removes an item it cannot hand over" and "In-person notice when nothing is available".
    /// </summary>
    public class NotMetYetTests
    {
        // "Configured villager not met yet".
        [Fact]
        public void Planning_VillagerNotMetYet_GetsNotMetYet()
        {
            Assert.Equal(GiftPlanStatus.NotMetYet, PlanPrecheck.StatusBeforeSelection(npcFound: true, hasMet: false, withinGiftLimit: true));
        }

        // "Villager not found in the game": not found wins, whether or not they could have been met.
        [Fact]
        public void Planning_VillagerNotFound_GetsNpcNotFound()
        {
            Assert.Equal(GiftPlanStatus.NpcNotFound, PlanPrecheck.StatusBeforeSelection(npcFound: false, hasMet: false, withinGiftLimit: true));
        }

        // "Villager met, gift limit reached".
        [Fact]
        public void Planning_MetButLimitReached_GetsWeeklyCapReached()
        {
            Assert.Equal(GiftPlanStatus.WeeklyCapReached, PlanPrecheck.StatusBeforeSelection(npcFound: true, hasMet: true, withinGiftLimit: false));
        }

        // "Villager met, gift limit not reached": go on to select an item (no status yet).
        [Fact]
        public void Planning_MetAndWithinLimit_GoesOnToSelection()
        {
            Assert.Null(PlanPrecheck.StatusBeforeSelection(npcFound: true, hasMet: true, withinGiftLimit: true));
        }

        // daily-plan-review "Configured villager not met yet": the exact row text.
        [Fact]
        public void PlanRow_NotMetYet_SaysTalkToThemFirst()
        {
            Assert.Equal("Caroline: not met yet - talk to them first", PlanRowText.ReasonLine(GiftPlanStatus.NotMetYet, "Caroline"));
        }

        // proximity-delivery "Villager not met yet at delivery time": skipped, gift stays pending.
        [Fact]
        public void Delivery_VillagerNotMetYet_SkipsAndStaysPending()
        {
            DeliveryStep step = DeliveryRule.Decide(withinGiftLimit: true, itemCanBeHandedOver: true, unitsOnFarm: 10, hasMet: false);

            Assert.Equal(DeliveryStep.SkipNotMet, step);
            Assert.False(DeliveryRule.ResolvesGift(step));
        }

        // proximity-delivery "The game rejects the gift": resolved for the day.
        [Fact]
        public void Delivery_GameRejectedGift_IsResolvedForTheDay()
        {
            Assert.True(DeliveryRule.ResolvesGift(DeliveryStep.SkipGameRejected));
        }

        // proximity-delivery "In-person notice …": no notice for a villager not met yet.
        [Fact]
        public void Notice_NotForVillagerNotMetYet()
        {
            var plan = new DailyPlan { TargetDay = 11 };
            plan.Gifts.Add(new PlannedGift("Caroline", GiftPlanStatus.NotMetYet));
            plan.Gifts.Add(new PlannedGift("Abigail", GiftPlanStatus.NoEligibleItem));

            Assert.Equal(new[] { "Abigail" }, plan.PendingNoGiftNotifications().Select(g => g.NpcName));
        }
    }
}
