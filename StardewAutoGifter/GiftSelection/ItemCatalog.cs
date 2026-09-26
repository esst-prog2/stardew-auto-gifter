using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace StardewAutoGifter.GiftSelection
{
    /// <summary>
    /// Builds the full list of giftable item types for the blocklist editor. See
    /// specs/daily-plan-review - "Blocklist editor lists the full item catalog".
    /// </summary>
    public static class ItemCatalog
    {
        public static List<Item> BuildFullCatalog()
        {
            var items = new List<Item>();

            foreach (string unqualifiedId in Game1.objectData.Keys)
            {
                Item? item = ItemRegistry.Create("(O)" + unqualifiedId, allowNull: true);
                if (item != null)
                    items.Add(item);
            }

            return items.OrderBy(item => item.DisplayName).ToList();
        }
    }
}
