using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec scenarios (HW6 review 05b, reviews/05-swallowed-errors.md):
    /// specs/proximity-delivery - "Delivery never removes an item it cannot hand over".
    /// </summary>
    public class DeliveryRuleTests
    {
        // "Planned item cannot be created as a gift": limit allows, 10 units, not giftable.
        [Fact]
        public void ItemCannotBeHandedOver_SkipsWithoutRemoving()
        {
            Assert.Equal(DeliveryStep.SkipNotGiftable, DeliveryRule.Decide(withinGiftLimit: true, itemCanBeHandedOver: false, unitsOnFarm: 10));
        }

        // "Planned item cannot be created as a gift": the planned gift stays pending.
        [Fact]
        public void ItemCannotBeHandedOver_LeavesGiftPending()
        {
            Assert.False(DeliveryRule.ResolvesGift(DeliveryStep.SkipNotGiftable));
        }

        // "Gift limit reached": skipped for the limit, resolved for the day.
        [Fact]
        public void GiftLimitReached_SkipsAndResolves()
        {
            Assert.Equal(DeliveryStep.SkipGiftLimit, DeliveryRule.Decide(withinGiftLimit: false, itemCanBeHandedOver: true, unitsOnFarm: 10));
            Assert.True(DeliveryRule.ResolvesGift(DeliveryStep.SkipGiftLimit));
        }

        // "Not enough stock": 1 unit left, skipped for low stock, resolved for the day.
        [Fact]
        public void OnlyOneUnitLeft_SkipsAndResolves()
        {
            Assert.Equal(DeliveryStep.SkipLowStock, DeliveryRule.Decide(withinGiftLimit: true, itemCanBeHandedOver: true, unitsOnFarm: 1));
            Assert.True(DeliveryRule.ResolvesGift(DeliveryStep.SkipLowStock));
        }

        // "Everything allows the hand-off": 5 units, one is removed and given.
        [Fact]
        public void EverythingAllows_Delivers()
        {
            Assert.Equal(DeliveryStep.Deliver, DeliveryRule.Decide(withinGiftLimit: true, itemCanBeHandedOver: true, unitsOnFarm: 5));
        }
    }
}
