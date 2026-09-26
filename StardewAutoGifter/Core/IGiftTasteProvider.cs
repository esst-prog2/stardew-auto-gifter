namespace StardewAutoGifter.Core
{
    /// <summary>Looks up an NPC's taste for an item. Kept as an interface so <see cref="GiftSelector"/> stays game-independent and unit-testable.</summary>
    public interface IGiftTasteProvider
    {
        GiftTaste GetTaste(string npcName, string qualifiedItemId);
    }
}
