using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Objects;

namespace StardewAutoGifter.GiftSelection
{
    /// <summary>
    /// Builds and mutates the combined, quantity-summed pool of items across a set of farm chests.
    /// See specs/gift-selection - "Farm-wide item pool" and "Tiered, quantity-based item selection".
    /// </summary>
    public static class FarmItemPool
    {
        /// <summary>Sums each item's quantity across all given chests, keyed by qualified item id.</summary>
        public static Dictionary<string, int> BuildPool(IEnumerable<Chest> chests)
        {
            var pool = new Dictionary<string, int>();
            foreach (Chest chest in chests)
            {
                foreach (Item? item in chest.Items)
                {
                    if (item == null)
                        continue;

                    pool.TryGetValue(item.QualifiedItemId, out int existing);
                    pool[item.QualifiedItemId] = existing + item.Stack;
                }
            }
            return pool;
        }

        /// <summary>An item needs at least this many units present before one can be removed, so at least one is always left behind.</summary>
        private const int MinimumUnitsBeforeRemoval = 2;

        /// <summary>
        /// Removes one unit of the given item from whichever chest holds it first, but only if doing
        /// so leaves at least one unit behind. Returns false if no chest currently holds any of it
        /// (specs/proximity-delivery - "Delivery skipped if the planned item is no longer available")
        /// or if only one unit remains in total (specs/proximity-delivery - "Delivery never drops an
        /// item's farm-wide total below one").
        /// </summary>
        public static bool TryRemoveOneUnit(IEnumerable<Chest> chests, string qualifiedItemId)
        {
            List<Chest> chestList = chests as List<Chest> ?? chests.ToList();

            if (GetTotalQuantity(chestList, qualifiedItemId) < MinimumUnitsBeforeRemoval)
                return false;

            foreach (Chest chest in chestList)
            {
                IList<Item?> items = chest.Items;
                for (int i = 0; i < items.Count; i++)
                {
                    Item? item = items[i];
                    if (item == null || item.QualifiedItemId != qualifiedItemId)
                        continue;

                    if (item.Stack <= 1)
                        items[i] = null;
                    else
                        item.Stack--;

                    chest.clearNulls();
                    return true;
                }
            }
            return false;
        }

        /// <summary>Total quantity of the given item currently available across all given chests.</summary>
        public static int GetTotalQuantity(IEnumerable<Chest> chests, string qualifiedItemId)
        {
            int total = 0;
            foreach (Chest chest in chests)
            {
                foreach (Item? item in chest.Items)
                {
                    if (item != null && item.QualifiedItemId == qualifiedItemId)
                        total += item.Stack;
                }
            }
            return total;
        }
    }
}
