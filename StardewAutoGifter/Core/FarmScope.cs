namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Which farm building interiors belong to the mod's farm scope: every interior except the
    /// farmhouse's (cabins included, since the game treats them as farmhouses). See
    /// specs/gift-selection - "Farm-wide item pool" (HW7b).
    /// </summary>
    public static class FarmScope
    {
        public static bool IncludesBuildingInterior(bool hasInterior, bool isFarmHouse) => hasInterior && !isFarmHouse;
    }
}
