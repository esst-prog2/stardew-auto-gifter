using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewAutoGifter.GiftSelection;
using StardewAutoGifter.Core;
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

        public static void CheckAndDeliver(DailyPlan plan, int today, IMonitor monitor)
        {
            // specs/proximity-delivery - "Delivery paused while the mod is inactive": checked only
            // once an NPC is in range (scanning the farm's chests every second would be wasted
            // work), and at most once per call. While inactive, nothing is resolved, so the gift
            // stays pending and can still be delivered later today once exactly one chest is marked.
            // The same snapshot is handed to TryDeliver, so one call scans the chests at most once.
            AnchorState? anchorState = null;
            AnchorState AnchorStateNow() => anchorState ??= FarmChestScanner.ReadAnchorState();

            foreach (PlannedGift gift in plan.PendingDeliveries().ToList())
            {
                NPC? npc = Game1.getCharacterFromName(gift.NpcName);
                if (!IsInRange(npc))
                    continue;

                if (!AnchorStateNow().IsActive)
                    return;

                TryDeliver(gift, npc, today, AnchorStateNow(), monitor);
            }

            // specs/proximity-delivery - "In-person notice when nothing is available for a configured NPC".
            foreach (PlannedGift gift in plan.PendingNoGiftNotifications().ToList())
            {
                NPC? npc = Game1.getCharacterFromName(gift.NpcName);
                if (!IsInRange(npc))
                    continue;

                if (!AnchorStateNow().IsActive)
                    return;

                ShowNoGiftNotice(gift, npc);
            }
        }

        /// <summary>
        /// Whether the NPC is close enough for a hand-off or notice: in the player's location and
        /// within <see cref="ProximityThresholdTiles"/> (player tile to NPC tile). See
        /// specs/proximity-delivery - "Automatic hand-off within a short distance".
        /// </summary>
        private static bool IsInRange([NotNullWhen(true)] NPC? npc)
            => npc != null
               && npc.currentLocation == Game1.player.currentLocation
               && Vector2.Distance(Game1.player.Tile, npc.Tile) <= ProximityThresholdTiles;

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

            gift.Resolved = true; // resolved (notified), so we don't show this again today
        }

        private static void TryDeliver(PlannedGift gift, NPC npc, int today, AnchorState anchorState, IMonitor monitor)
        {
            // Only ever called for Status == Planned gifts (see DailyPlan.PendingDeliveries), so
            // QualifiedItemId is guaranteed non-null here.
            string qualifiedItemId = gift.QualifiedItemId!;

            List<StardewValley.Objects.Chest> farmChests = FarmChestScanner.GetFarmChests().ToList();

            // specs/proximity-delivery - "Delivery prefers the anchor chest": TryRemoveOneUnit takes
            // the first chest in this list that holds the item, so put the anchor chest first.
            // (CheckAndDeliver already confirmed exactly one anchor exists, in this snapshot.)
            StardewValley.Objects.Chest? anchor = anchorState.Anchor;
            if (anchor != null)
            {
                farmChests.Remove(anchor);
                farmChests.Insert(0, anchor);
            }

            // specs/proximity-delivery - "Delivery never removes an item it cannot hand over": every
            // check runs before anything leaves a chest - the gift limit ("Gift-limit re-checked at
            // delivery time", the same rule as planning, asked for today), then whether the gift
            // object can be created, then the stock ("Delivery never drops an item's farm-wide total
            // below one").
            SObject? giftObject = ItemRegistry.Create(qualifiedItemId, allowNull: true) as SObject;
            DeliveryStep step = DeliveryRule.Decide(
                withinGiftLimit: FriendshipGiftTracker.CanGiveGiftOn(gift.NpcName, targetDay: today, today: today),
                itemCanBeHandedOver: giftObject != null,
                unitsOnFarm: FarmItemPool.GetTotalQuantity(farmChests, qualifiedItemId));

            switch (step)
            {
                case DeliveryStep.SkipGiftLimit:
                    break;

                case DeliveryStep.SkipNotGiftable:
                    // Not resolved, so a later attempt can still deliver it; LogOnce keeps the
                    // once-per-second retries from flooding the log with the same line.
                    monitor.LogOnce($"Could not deliver to {gift.NpcName}: the planned item {qualifiedItemId} cannot be created as a giftable object. Nothing was removed from the farm's chests.", LogLevel.Warn);
                    break;

                case DeliveryStep.SkipLowStock:
                    monitor.Log($"Skipped delivery to {gift.NpcName}: fewer than {StockRule.MinimumUnitsForGift} units of {qualifiedItemId} remain in the farm's chests.", LogLevel.Trace);
                    break;

                case DeliveryStep.Deliver:
                    if (!FarmItemPool.TryRemoveOneUnit(farmChests, qualifiedItemId))
                    {
                        // Cannot happen after the stock check above (same chests, same moment), but
                        // never give an item that was not actually removed.
                        monitor.Log($"Skipped delivery to {gift.NpcName}: {qualifiedItemId} could not be removed from the farm's chests.", LogLevel.Warn);
                        step = DeliveryStep.SkipLowStock;
                        break;
                    }

                    // receiveGift applies friendship-point changes and updates the game's own
                    // GiftsToday/GiftsThisWeek tracking, so the mod never duplicates that state.
                    npc.receiveGift(giftObject!, Game1.player, updateGiftLimitInfo: true);
                    Game1.addHUDMessage(new HUDMessage($"Gave {giftObject!.DisplayName} to {npc.displayName}."));
                    break;
            }

            if (DeliveryRule.ResolvesGift(step))
                gift.Resolved = true; // settled for the day, so it is not checked again
        }
    }
}
