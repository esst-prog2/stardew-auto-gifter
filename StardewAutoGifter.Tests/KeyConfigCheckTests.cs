using System.Collections.Generic;
using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values (including the exact warning texts) taken from the spec (HW6 review 08b,
    /// reviews/08-quiet-defaults.md): specs/daily-plan-review - "Key settings checked when the mod
    /// starts". Keys are passed as numbers, as SMAPI's SButton values are; this fake key table uses
    /// SButton's numbers for None (0), G (71), K (75) and F8 (119).
    /// </summary>
    public class KeyConfigCheckTests
    {
        private const int None = 0, G = 71, K = 75, F8 = 119;

        private static readonly Dictionary<int, string> KeyNames = new() { [None] = "None", [G] = "G", [K] = "K", [F8] = "F8" };

        private static KeyCheckResult Check(int menuKey, int chestMarkKey)
            => KeyConfigCheck.Validate(menuKey, chestMarkKey, KeyNames.ContainsKey, None, defaultMenuKey: F8, defaultChestMarkKey: G, key => KeyNames[key]);

        // "Menu key is not a real key".
        [Fact]
        public void MenuKeyNotARealKey_UsesDefaultF8_AndWarns()
        {
            KeyCheckResult result = Check(menuKey: 9999, chestMarkKey: G);

            Assert.Equal(F8, result.MenuKey);
            Assert.Equal(new[] { "config.json: MenuKey is set to 9999, which is not a key; using the default F8 instead." }, result.Warnings);
        }

        // "Chest-marking key set to None".
        [Fact]
        public void ChestMarkKeyNone_StaysNone_AndWarns()
        {
            KeyCheckResult result = Check(menuKey: F8, chestMarkKey: None);

            Assert.Equal(None, result.ChestMarkKey);
            Assert.Equal(new[] { "config.json: ChestMarkKey is set to None, so chest marking is turned off." }, result.Warnings);
        }

        // "Both keys are the same".
        [Fact]
        public void BothKeysK_Warns()
        {
            KeyCheckResult result = Check(menuKey: K, chestMarkKey: K);

            Assert.Equal(new[] { "config.json: MenuKey and ChestMarkKey are both K; that key opens the mod menu, so chest marking will not work." }, result.Warnings);
        }

        // "Invalid key whose default collides with the other key".
        [Fact]
        public void InvalidMenuKeyWhoseDefaultCollides_WarnsTwice()
        {
            KeyCheckResult result = Check(menuKey: 9999, chestMarkKey: F8);

            Assert.Equal(F8, result.MenuKey);
            Assert.Equal(new[]
            {
                "config.json: MenuKey is set to 9999, which is not a key; using the default F8 instead.",
                "config.json: MenuKey and ChestMarkKey are both F8; that key opens the mod menu, so chest marking will not work.",
            }, result.Warnings);
        }

        // "Valid, different keys".
        [Fact]
        public void ValidDifferentKeys_NoWarnings_Unchanged()
        {
            KeyCheckResult result = Check(menuKey: F8, chestMarkKey: G);

            Assert.Equal(F8, result.MenuKey);
            Assert.Equal(G, result.ChestMarkKey);
            Assert.Empty(result.Warnings);
        }
    }
}
