using StardewValley;
using StardewAutoGifter.Core;

namespace StardewAutoGifter.GiftSelection
{
    /// <summary>Wraps the game's own NPC gift-taste lookup for <see cref="GiftSelector"/>.</summary>
    public class NpcGiftTasteProvider : IGiftTasteProvider
    {
        public GiftTaste GetTaste(string npcName, string qualifiedItemId)
        {
            NPC? npc = Game1.getCharacterFromName(npcName);
            if (npc == null)
                return GiftTaste.Neutral;

            Item? item = ItemRegistry.Create(qualifiedItemId, allowNull: true);
            if (item == null)
                return GiftTaste.Neutral;

            int taste = npc.getGiftTasteForThisItem(item);
            return taste switch
            {
                NPC.gift_taste_love => GiftTaste.Love,
                NPC.gift_taste_like => GiftTaste.Like,
                NPC.gift_taste_dislike => GiftTaste.Dislike,
                NPC.gift_taste_hate => GiftTaste.Hate,
                _ => GiftTaste.Neutral,
            };
        }
    }
}
