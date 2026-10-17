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
        /// Picks the best item to gift an NPC, or null if none is eligible.
        /// </summary>
        /// <param name="npcName">The NPC being considered.</param>
        /// <param name="pool">
        /// Qualified item id -> total quantity in the pool being considered. Planning calls this
        /// twice: first with the anchor chest's own contents, then - only if that finds nothing and
        /// the "anchor chest only" setting is off - with every farm chest summed (specs/gift-selection
        /// - "Anchor chest prioritized before the wider farm pool").
        /// </param>
        /// <param name="blocklist">Qualified item ids the player never wants given away.</param>
        /// <param name="includeLikedItems">Whether to fall back to "liked" items when no loved item is eligible.</param>
        /// <param name="tasteProvider">Resolves an NPC's taste for a given item.</param>
        public static string? SelectItem(
            string npcName,
            IReadOnlyDictionary<string, int> pool,
            ISet<string> blocklist,
            bool includeLikedItems,
            IGiftTasteProvider tasteProvider)
        {
            string? loved = BestByTaste(npcName, pool, blocklist, tasteProvider, GiftTaste.Love);
            if (loved != null)
                return loved;

            return includeLikedItems
                ? BestByTaste(npcName, pool, blocklist, tasteProvider, GiftTaste.Like)
                : null;
        }

        private static string? BestByTaste(
            string npcName,
            IReadOnlyDictionary<string, int> pool,
            ISet<string> blocklist,
            IGiftTasteProvider tasteProvider,
            GiftTaste taste)
        {
            string? best = null;
            int bestQuantity = 0;

            foreach (KeyValuePair<string, int> entry in pool)
            {
                string itemId = entry.Key;
                int quantity = entry.Value;

                if (quantity < StockRule.MinimumUnitsForGift || blocklist.Contains(itemId))
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
