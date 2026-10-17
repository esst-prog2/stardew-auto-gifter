namespace StardewAutoGifter.Core
{
    /// <summary>
    /// The shared "always leave at least one behind" rule, used both when selecting an item and
    /// right before removing one at delivery time. See specs/gift-selection - "Minimum one unit
    /// reserved per item" and specs/proximity-delivery - "Delivery never drops an item's farm-wide
    /// total below one".
    /// </summary>
    public static class StockRule
    {
        /// <summary>An item needs at least this many units across the farm's chests before one can be selected or given away.</summary>
        public const int MinimumUnitsForGift = 2;
    }
}
