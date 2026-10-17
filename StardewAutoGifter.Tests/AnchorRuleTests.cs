using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec (HW6 review 01c, reviews/01-spec-drift.md):
    /// specs/gift-selection - "Marked chest acts as the mod's on/off anchor" (active only while
    /// exactly one chest is marked) and "Ambiguous marking treated as unresolved";
    /// specs/proximity-delivery - "Delivery paused while the mod is inactive".
    /// </summary>
    public class AnchorRuleTests
    {
        // "No chest currently marked" / "Anchor chest unmarked after the plan was made".
        [Fact]
        public void NoMarkedChest_ModIsInactive()
        {
            Assert.False(AnchorRule.IsActive(markedChestCount: 0));
        }

        // "Mod active once a farm chest is marked" / "Marking restored later the same day".
        [Fact]
        public void ExactlyOneMarkedChest_ModIsActive()
        {
            Assert.True(AnchorRule.IsActive(markedChestCount: 1));
        }

        // "Two farm chests carry the sentinel color" / "Two chests marked at delivery time".
        [Fact]
        public void TwoMarkedChests_ModIsInactive()
        {
            Assert.False(AnchorRule.IsActive(markedChestCount: 2));
        }
    }
}
