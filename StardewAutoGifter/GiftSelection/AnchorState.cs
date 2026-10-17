using StardewValley.Objects;
using StardewAutoGifter.Core;

namespace StardewAutoGifter.GiftSelection
{
    /// <summary>
    /// A snapshot of the anchor-chest marking, taken with a single scan of the farm's chests (HW6
    /// review 04c). Every caller asks this instead of scanning on its own. See specs/gift-selection -
    /// "Marked chest acts as the mod's on/off anchor" and "Ambiguous marking treated as unresolved".
    /// </summary>
    public sealed class AnchorState
    {
        /// <summary>How many farm chests carry the sentinel color right now.</summary>
        public int MarkedChestCount { get; }

        /// <summary>The single valid anchor chest, or null when none or more than one chest is marked.</summary>
        public Chest? Anchor { get; }

        /// <summary>Whether the mod is active: exactly one chest is marked (<see cref="AnchorRule.IsActive"/>).</summary>
        public bool IsActive => AnchorRule.IsActive(MarkedChestCount);

        /// <summary>Whether more than one chest is marked (a marking conflict).</summary>
        public bool IsConflict => MarkedChestCount > 1;

        public AnchorState(int markedChestCount, Chest? anchor)
        {
            MarkedChestCount = markedChestCount;
            Anchor = anchor;
        }
    }
}
