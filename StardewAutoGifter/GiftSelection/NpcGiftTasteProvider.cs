using StardewModdingAPI;
using StardewValley;
using StardewAutoGifter.Core;

namespace StardewAutoGifter.GiftSelection
{
    /// <summary>Wraps the game's own NPC gift-taste lookup for <see cref="GiftSelector"/>.</summary>
    public class NpcGiftTasteProvider : IGiftTasteProvider
    {
        private readonly IMonitor _monitor;

        public NpcGiftTasteProvider(IMonitor monitor)
        {
            _monitor = monitor;
        }

        public GiftTaste GetTaste(string npcName, string qualifiedItemId)
        {
            NPC? npc = Game1.getCharacterFromName(npcName);
            if (npc == null)
                return GiftTaste.Neutral;

            Item? item = ItemRegistry.Create(qualifiedItemId, allowNull: true);
            if (item == null)
            {
                // Treated as Neutral, so the item is never selected; say so in the log (HW6 review
                // 08d). LogOnce, because planning asks this for every NPC and every pooled item.
                _monitor.LogOnce($"Item {qualifiedItemId} from a farm chest cannot be created by the game; it is treated as not giftable.", LogLevel.Trace);
                return GiftTaste.Neutral;
            }

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
