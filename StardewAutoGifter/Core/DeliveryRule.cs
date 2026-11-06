namespace StardewAutoGifter.Core
{
    /// <summary>What a delivery attempt does, decided before anything is removed from a chest.</summary>
    public enum DeliveryStep
    {
        /// <summary>The player has not met the villager yet; nothing is removed, and the gift stays pending (HW7a).</summary>
        SkipNotMet,

        /// <summary>The gift limit (daily or weekly) does not allow a gift today.</summary>
        SkipGiftLimit,

        /// <summary>The planned item cannot be created as a giftable object; nothing is removed.</summary>
        SkipNotGiftable,

        /// <summary>Fewer than <see cref="StockRule.MinimumUnitsForGift"/> units remain on the farm.</summary>
        SkipLowStock,

        /// <summary>Give the gift, then remove one unit.</summary>
        Deliver,

        /// <summary>Set by the caller when the game threw while accepting the gift; nothing was removed, and the gift is resolved for the day (HW7a).</summary>
        SkipGameRejected,
    }

    /// <summary>
    /// The pure order of delivery checks: met by the player, then the gift limit, then whether the
    /// item can be handed over, then stock. See specs/proximity-delivery - "Delivery never removes
    /// an item it cannot hand over" (HW6 review 05b, HW7a).
    /// </summary>
    public static class DeliveryRule
    {
        public static DeliveryStep Decide(bool withinGiftLimit, bool itemCanBeHandedOver, int unitsOnFarm, bool hasMet = true)
        {
            if (!hasMet)
                return DeliveryStep.SkipNotMet;
            if (!withinGiftLimit)
                return DeliveryStep.SkipGiftLimit;
            if (!itemCanBeHandedOver)
                return DeliveryStep.SkipNotGiftable;
            if (unitsOnFarm < StockRule.MinimumUnitsForGift)
                return DeliveryStep.SkipLowStock;
            return DeliveryStep.Deliver;
        }

        /// <summary>Whether the step settles the planned gift for the day. A gift that could not be handed over, or whose villager the player has not met yet, stays pending, so a later attempt can still deliver it.</summary>
        public static bool ResolvesGift(DeliveryStep step) => step != DeliveryStep.SkipNotGiftable && step != DeliveryStep.SkipNotMet;
    }
}
