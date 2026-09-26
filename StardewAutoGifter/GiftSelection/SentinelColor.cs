using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace StardewAutoGifter.GiftSelection
{
    /// <summary>
    /// The reserved tint color the mod uses to mark its anchor chest. Chosen because it does not
    /// match any of the 21 fixed swatches the vanilla chest color picker (DiscreteColorPicker) can
    /// produce, so a player can never set it on a chest by accident through normal play.
    /// See design.md - "Chest marking: a reserved sentinel color...".
    /// </summary>
    public static class SentinelColor
    {
        public static readonly Color Value = new Color(13, 202, 91);

        /// <summary>The 21 preset swatches offered by the game's built-in chest color picker.</summary>
        private static readonly Color[] VanillaSwatches =
        {
            new Color(0, 0, 0), new Color(85, 85, 255), new Color(119, 191, 255), new Color(0, 170, 170),
            new Color(0, 234, 175), new Color(0, 170, 0), new Color(159, 236, 0), new Color(255, 234, 18),
            new Color(255, 167, 18), new Color(255, 105, 18), new Color(255, 0, 0), new Color(135, 0, 35),
            new Color(255, 173, 199), new Color(255, 117, 195), new Color(172, 0, 198), new Color(143, 0, 255),
            new Color(89, 11, 142), new Color(64, 64, 64), new Color(100, 100, 100), new Color(200, 200, 200),
            new Color(254, 254, 254),
        };

        static SentinelColor()
        {
            if (VanillaSwatches.Any(swatch => swatch == Value))
                throw new InvalidOperationException("The reserved sentinel color must not match a vanilla chest color-picker swatch.");
        }

        public static bool Matches(Color color) => color == Value;
    }
}
