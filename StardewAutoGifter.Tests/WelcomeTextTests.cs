using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec (HW6 reviews 02d and 03e, reviews/02-unchosen-names.md,
    /// reviews/03-patched-where-it-broke.md): specs/gift-selection - "Starter chest and welcome
    /// message on first farm exit", scenarios "Welcome pages with the default settings", "Welcome
    /// pages when "anchor chest only" is already on", and the two key-name scenarios.
    /// </summary>
    public class WelcomeTextTests
    {
        // "Welcome pages with the default settings": one test per page, F8 / G / anchor chest only off.
        [Theory]
        [InlineData(0, "Welcome to Auto-Gifter!")]
        [InlineData(1, "A chest has been added to your inventory. Place it anywhere on your farm (or inside the barn, coop, shed, or greenhouse) - not in the farmhouse or cellar.")]
        [InlineData(2, "Open the chest and press G to mark it as your gift chest - it will be renamed and sparkle occasionally so you can spot it.")]
        [InlineData(3, "That chest is checked FIRST for gifts. If it doesn't have what's needed, the mod falls back to every other chest on your farm.")]
        [InlineData(4, "You can turn that fallback off in the mod menu's Settings tab, to only ever use the marked chest.")]
        [InlineData(5, "Press F8 anytime to open the mod menu: choose which villagers to gift, preview and edit tomorrow's plan, manage your blocklist, and adjust settings.")]
        [InlineData(6, "IMPORTANT: automatic gifting always starts the NEXT day, never the same day.")]
        [InlineData(7, "Selecting a villager or editing the plan today has no effect on today - check the \"Tomorrow's Plan\" label in the Plan tab to see what's actually queued up.")]
        public void DefaultSettings_PageMatchesSpec(int pageIndex, string expected)
        {
            var pages = WelcomeText.Pages(menuKey: "F8", chestMarkKey: "G", anchorChestOnly: false);

            Assert.Equal(8, pages.Count);
            Assert.Equal(expected, pages[pageIndex]);
        }

        // "Welcome pages when "anchor chest only" is already on": page 4 (index 3) changes.
        [Fact]
        public void AnchorChestOnlyOn_Page4SaysOnlySource()
        {
            var pages = WelcomeText.Pages(menuKey: "F8", chestMarkKey: "G", anchorChestOnly: true);

            Assert.Equal("That chest is your ONLY source of gifts: the fallback to every other chest on your farm is turned off.", pages[3]);
        }

        // "Welcome pages when "anchor chest only" is already on": page 5 (index 4) changes, the rest stay.
        [Fact]
        public void AnchorChestOnlyOn_Page5SaysHowToTurnFallbackBackOn_OtherPagesUnchanged()
        {
            var on = WelcomeText.Pages(menuKey: "F8", chestMarkKey: "G", anchorChestOnly: true);
            var off = WelcomeText.Pages(menuKey: "F8", chestMarkKey: "G", anchorChestOnly: false);

            Assert.Equal("You can turn that fallback back on in the mod menu's Settings tab, to also use every other chest on your farm.", on[4]);
            foreach (int i in new[] { 0, 1, 2, 5, 6, 7 })
                Assert.Equal(off[i], on[i]);
        }

        // "Welcome message names the default keys".
        [Fact]
        public void DefaultKeys_PagesNameGAndF8()
        {
            var pages = WelcomeText.Pages(menuKey: "F8", chestMarkKey: "G", anchorChestOnly: false);

            Assert.Contains("Open the chest and press G to mark it as your gift chest - it will be renamed and sparkle occasionally so you can spot it.", pages);
            Assert.Contains("Press F8 anytime to open the mod menu: choose which villagers to gift, preview and edit tomorrow's plan, manage your blocklist, and adjust settings.", pages);
        }

        // "Welcome message names remapped keys": "press K", "Press F9", and no page names G or F8.
        [Fact]
        public void RemappedKeys_PagesNameKAndF9_AndNeverGOrF8()
        {
            var pages = WelcomeText.Pages(menuKey: "F9", chestMarkKey: "K", anchorChestOnly: false);

            Assert.Contains(pages, page => page.Contains("press K"));
            Assert.Contains(pages, page => page.StartsWith("Press F9"));
            Assert.DoesNotContain(pages, page => page.Contains("press G") || page.Contains("F8"));
        }
    }
}
