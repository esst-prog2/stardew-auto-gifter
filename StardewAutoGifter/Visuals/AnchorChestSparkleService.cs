using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewAutoGifter.GiftSelection;

namespace StardewAutoGifter.Visuals
{
    /// <summary>
    /// Emits a brief sparkle on the anchor chest, reusing the game's own particle helper. See
    /// specs/gift-selection - "Anchor chest sparkles periodically for visibility".
    /// </summary>
    public static class AnchorChestSparkleService
    {
        public static void SparkleIfInView(Chest anchor)
        {
            GameLocation? location = FarmChestScanner.FindLocationOf(anchor);
            if (location == null || location != Game1.currentLocation)
                return;

            Utility.addSprinklesToLocation(
                location,
                (int)anchor.TileLocation.X,
                (int)anchor.TileLocation.Y,
                1,
                1,
                1000, // totalSprinkleDuration (ms)
                100,  // millisecondsBetweenSprinkles
                Color.White);
        }
    }
}
