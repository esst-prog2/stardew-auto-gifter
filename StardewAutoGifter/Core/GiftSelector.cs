using System.Collections.Generic;

namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Pure selection logic, decoupled from the game types so it can be unit tested without a
    /// running game. Implements specs/gift-selection - "Tiered, quantity-based item selection",
    /// "No selection when nothing is eligible", "Minimum one unit reserved per item", and the
    /// blocklist rules.
    /// </summary>
    public static class GiftSelector
    {
        /// <summary>
        /// An item needs at least this many units in the farm-wide pool to be selectable, so that
        /// giving one away always leaves at least one behind. See specs/gift-selection - "Minimum
        /// one unit reserved per item".
        /// </summary>
        private const int MinimumUnitsToBeEligible = 2;

        /// <summary>
        /// Picks the best item to gift an NPC, or null if none is eligible.
        /// </summary>
        /// <param name="npcName">The NPC being considered.</param>
        /// <param name="farmWidePool">Qualified item id -> total quantity summed across all farm chests.</param>
        /// <param name="blocklist">Qualified item ids the player never wants given away.</param>
        /// <param name="includeLikedItems">Whether to fall back to "liked" items when no loved item is eligible.</param>
        /// <param name="tasteProvider">Resolves an NPC's taste for a given item.</param>
        public static string? SelectItem(
            string npcName,
            IReadOnlyDictionary<string, int> farmWidePool,
            ISet<string> blocklist,
            bool includeLikedItems,
            IGiftTasteProvider tasteProvider)
        {
            string? loved = BestByTaste(npcName, farmWidePool, blocklist, tasteProvider, GiftTaste.Love);
            if (loved != null)
                return loved;

            return includeLikedItems
                ? BestByTaste(npcName, farmWidePool, blocklist, tasteProvider, GiftTaste.Like)
                : null;
        }

        private static string? BestByTaste(
            string npcName,
            IReadOnlyDictionary<string, int> farmWidePool,
            ISet<string> blocklist,
            IGiftTasteProvider tasteProvider,
            GiftTaste taste)
        {
            string? best = null;
            int bestQuantity = 0;

            foreach (KeyValuePair<string, int> entry in farmWidePool)
            {
                string itemId = entry.Key;
                int quantity = entry.Value;

                if (quantity < MinimumUnitsToBeEligible || blocklist.Contains(itemId))
                    continue;

                if (tasteProvider.GetTaste(npcName, itemId) != taste)
                    continue;

                if (quantity > bestQuantity)
                {
                    best = itemId;
                    bestQuantity = quantity;
                }
            }

            return best;
        }
    }
}
