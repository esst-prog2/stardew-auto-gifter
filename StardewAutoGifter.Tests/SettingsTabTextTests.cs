using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected value taken from the spec (HW6 review 04g, reviews/04-copies-and-names.md):
    /// specs/daily-plan-review - "Anchor-chest-only setting in the mod menu", scenario "Settings tab
    /// names where the liked-items fallback is set".
    /// </summary>
    public class SettingsTabTextTests
    {
        [Fact]
        public void LikedItemsNote_HasTheSpecifiedText()
        {
            Assert.Equal("Whether villagers get a liked item when no loved one is available is set in config.json (IncludeLikedItems), not in this menu.", SettingsTabText.LikedItemsNote);
        }
    }
}
