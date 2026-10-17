using System.Linq;
using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec (HW6 review 02d, reviews/02-unchosen-names.md):
    /// specs/gift-selection - "Starter chest and welcome message on first farm exit".
    /// </summary>
    public class WelcomeTextTests
    {
        // "Welcome message names the default keys".
        [Fact]
        public void DefaultKeys_PagesNameGAndF8()
        {
            var pages = WelcomeText.Pages(menuKey: "F8", chestMarkKey: "G");

            Assert.Contains("Open the chest and press G to mark it as your gift chest - it will be renamed and sparkle occasionally so you can spot it.", pages);
            Assert.Contains("Press F8 anytime to open the mod menu: choose which villagers to gift, preview and edit tomorrow's plan, manage your blocklist, and adjust settings.", pages);
        }

        // "Welcome message names remapped keys": "press K", "Press F9", and no page names G or F8.
        [Fact]
        public void RemappedKeys_PagesNameKAndF9_AndNeverGOrF8()
        {
            var pages = WelcomeText.Pages(menuKey: "F9", chestMarkKey: "K");

            Assert.Contains(pages, page => page.Contains("press K"));
            Assert.Contains(pages, page => page.StartsWith("Press F9"));
            Assert.DoesNotContain(pages, page => page.Contains("press G") || page.Contains("F8"));
        }
    }
}
