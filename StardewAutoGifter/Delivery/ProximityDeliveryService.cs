using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewAutoGifter.GiftSelection;
using StardewAutoGifter.Planning;
using SObject = StardewValley.Object;

namespace StardewAutoGifter.Delivery
{
    /// <summary>
    /// Checks player-to-NPC proximity (called from a throttled, roughly-once-per-second event, never
    /// from raw per-tick updates - see design.md "Proximity detection") and performs automatic
    /// hand-offs. Implements specs/proximity-delivery.
    /// </summary>
    public static class ProximityDeliveryService
    {
        /// <summary>"Close enough" to trigger an automatic hand-off, in tiles. Fixed, not configurable - see design.md.</summary>
        private const float ProximityThresholdTiles = 1.5f;

        public static void CheckAndDeliver(DailyPlan plan, IMonitor monitor)
        {
            foreach (PlannedGift gift in plan.PendingDeliveries().ToList())
            {
                NPC? npc = Game1.getCharacterFromName(gift.NpcName);
                if (npc == null || npc.currentLocation != Game1.player.currentLocation)
                    continue;

                float distance = Vector2.Distance(Game1.player.Tile, npc.Tile);
                if (distance > ProximityThresholdTiles)
                    continue;

                TryDeliver(gift, npc, monitor);
            }

            // specs/proximity-delivery - "In-person notice when nothing is available for a configured NPC".
            foreach (PlannedGift gift in plan.PendingNoGiftNotifications().ToList())
            {
                NPC? npc = Game1.getCharacterFromName(gift.NpcName);
                if (npc == null || npc.currentLocation != Game1.player.currentLocation)
                    continue;

                float distance = Vector2.Distance(Game1.player.Tile, npc.Tile);
                if (distance > ProximityThresholdTiles)
                    continue;

                ShowNoGiftNotice(gift, npc);
            }
        }

        private static void ShowNoGiftNotice(PlannedGift gift, NPC npc)
        {
            if (Game1.activeClickableMenu != null)
                return; // don't interrupt whatever the player is already doing

            string message = gift.Status == GiftPlanStatus.WeeklyCapReached
                ? "I've already gotten plenty from you this week. Thank you!"
                : "Hmm, I don't think you have anything for me right now.";

            // Game1.drawDialogue(NPC, string) doesn't exist in 1.6 - push a Dialogue onto the NPC's
            // own stack first, then draw it, matching how the game's dialogue system now works.
            npc.CurrentDialogue.Push(new Dialogue(npc, null, message));
            Game1.drawDialogue(npc);

            gift.Delivered = true; // resolved (notified), so we don't show this again today
        }

        private static void TryDeliver(PlannedGift gift, NPC npc, IMonitor monitor)
        {
            // Only ever called for Status == Planned gifts (see DailyPlan.PendingDeliveries), so
            // QualifiedItemId is guaranteed non-null here.
            string qualifiedItemId = gift.QualifiedItemId!;

            // specs/proximity-delivery - "Gift-limit re-checked at delivery time".
            if (FriendshipGiftTracker.HasReachedWeeklyCap(gift.NpcName))
            {
                gift.Delivered = true; // resolved (skipped), so we stop re-checking it for the rest of the day
                return;
            }

            List<StardewValley.Objects.Chest> farmChests = FarmChestScanner.GetFarmChests().ToList();

            // specs/proximity-delivery - "Delivery prefers the anchor chest": TryRemoveOneUnit takes
            // the first chest in this list that holds the item, so put the anchor chest first.
            StardewValley.Objects.Chest? anchor = FarmChestScanner.FindAnchorChest(out bool isAmbiguous);
            if (!isAmbiguous && anchor != null)
            {
                farmChests.Remove(anchor);
                farmChests.Insert(0, anchor);
            }

            // specs/proximity-delivery - "Delivery skipped if the planned item is no longer available"
            // and "Delivery never drops an item's farm-wide total below one": TryRemoveOneUnit
            // refuses if fewer than 2 units remain, so either case lands here.
            if (!FarmItemPool.TryRemoveOneUnit(farmChests, qualifiedItemId))
            {
                gift.Delivered = true;
                monitor.Log($"Skipped delivery to {gift.NpcName}: fewer than 2 units of {qualifiedItemId} remain in the farm's chests.", LogLevel.Trace);
                return;
            }

            Item? deliveredItem = ItemRegistry.Create(qualifiedItemId, allowNull: true);
            if (deliveredItem is SObject deliveredObject)
            {
                // receiveGift applies friendship-point changes and updates the game's own
                // GiftsToday/GiftsThisWeek tracking, so the mod never duplicates that state.
                npc.receiveGift(deliveredObject, Game1.player, updateGiftLimitInfo: true);
            }

            gift.Delivered = true;

            string itemName = deliveredItem?.DisplayName ?? qualifiedItemId;
            Game1.addHUDMessage(new HUDMessage($"Gave {itemName} to {npc.displayName}."));
        }
    }
}
