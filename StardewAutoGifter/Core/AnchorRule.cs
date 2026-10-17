namespace StardewAutoGifter.Core
{
    /// <summary>
    /// The pure on/off rule shared by planning, delivery and the sparkle. See specs/gift-selection -
    /// "Marked chest acts as the mod's on/off anchor" and "Ambiguous marking treated as unresolved".
    /// </summary>
    public static class AnchorRule
    {
        /// <summary>The mod is active only while exactly one farm chest carries the sentinel color.</summary>
        public static bool IsActive(int markedChestCount) => markedChestCount == 1;
    }
}
