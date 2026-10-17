namespace StardewAutoGifter.Core
{
    /// <summary>What a delivery attempt does, decided before anything is removed from a chest.</summary>
    public enum DeliveryStep
    {
        /// <summary>The gift limit (daily or weekly) does not allow a gift today.</summary>
        SkipGiftLimit,

        /// <summary>The planned item cannot be created as a giftable object; nothing is removed.</summary>
        SkipNotGiftable,

        /// <summary>Fewer than <see cref="StockRule.MinimumUnitsForGift"/> units remain on the farm.</summary>
        SkipLowStock,

        /// <summary>Remove one unit and give it.</summary>
        Deliver,
    }

    /// <summary>
    /// The pure order of delivery checks: gift limit, then whether the item can be handed over,
    /// then stock. See specs/proximity-delivery - "Delivery never removes an item it cannot hand
    /// over" (HW6 review 05b).
    /// </summary>
    public static class DeliveryRule
    {
        public static DeliveryStep Decide(bool withinGiftLimit, bool itemCanBeHandedOver, int unitsOnFarm)
        {
            if (!withinGiftLimit)
                return DeliveryStep.SkipGiftLimit;
            if (!itemCanBeHandedOver)
                return DeliveryStep.SkipNotGiftable;
            if (unitsOnFarm < StockRule.MinimumUnitsForGift)
                return DeliveryStep.SkipLowStock;
            return DeliveryStep.Deliver;
        }

        /// <summary>Whether the step settles the planned gift for the day. A gift that could not be handed over stays pending, so a later attempt can still deliver it.</summary>
        public static bool ResolvesGift(DeliveryStep step) => step != DeliveryStep.SkipNotGiftable;
    }
}
