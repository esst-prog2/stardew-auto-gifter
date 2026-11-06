using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewAutoGifter.Core;
using StardewAutoGifter.GiftSelection;

namespace StardewAutoGifter.Planning
{
    /// <summary>
    /// Computes a <see cref="DailyPlan"/> from the current anchor-only and farm-wide pools, the
    /// blocklist, and settings. Called when the player opens the mod menu, or as the day ends if no
    /// next-day plan exists yet. See
    /// specs/daily-plan-review - "Plan computed for the next day when the menu is opened" and
    /// "Reason shown for configured NPCs with no gift"; specs/gift-selection - "Anchor chest
    /// prioritized before the wider farm pool".
    /// </summary>
    public static class PlanComputer
    {
        public static DailyPlan Compute(
            IEnumerable<string> configuredNpcNames,
            IReadOnlyDictionary<string, int> anchorOnlyPool,
            IReadOnlyDictionary<string, int> farmWidePool,
            ISet<string> blocklist,
            bool includeLikedItems,
            bool anchorChestOnly,
            IGiftTasteProvider tasteProvider,
            int targetDay,
            int today,
            IMonitor monitor)
        {
            var plan = new DailyPlan { IsActive = true, TargetDay = targetDay };

            foreach (string npcName in configuredNpcNames)
            {
                // specs/gift-selection - "Villagers not met yet are not gifted" and "Gift-limit state
                // respected during selection", in the order of Core/PlanPrecheck: found in the game,
                // then met by the player, then the limit judged for the target day (so Saturday
                // evening's weekly count does not block Sunday).
                bool npcFound = Game1.getCharacterFromName(npcName) != null;
                bool hasMet = npcFound && FriendshipGiftTracker.HasMet(npcName);
                GiftPlanStatus? stopStatus = PlanPrecheck.StatusBeforeSelection(
                    npcFound,
                    hasMet,
                    withinGiftLimit: hasMet && FriendshipGiftTracker.CanGiveGiftOn(npcName, targetDay, today));

                if (stopStatus != null)
                {
                    // specs/daily-plan-review - "Reason shown for configured NPCs with no gift": an
                    // NPC the game does not know gets its own reason, not a false "nothing to give".
                    if (stopStatus == GiftPlanStatus.NpcNotFound)
                        monitor.LogOnce($"Configured villager '{npcName}' was not found in the game; the plan shows them as 'not found in the game'.", LogLevel.Warn);

                    plan.Gifts.Add(new PlannedGift(npcName, stopStatus.Value));
                    continue;
                }

                // specs/gift-selection - "Anchor chest prioritized before the wider farm pool":
                // try the anchor chest alone first. Fall back to the full farm-wide pool only when
                // the "anchor chest only" setting is disabled.
                string? itemId = GiftSelector.SelectItem(npcName, anchorOnlyPool, blocklist, includeLikedItems, tasteProvider);
                if (itemId == null && !anchorChestOnly)
                    itemId = GiftSelector.SelectItem(npcName, farmWidePool, blocklist, includeLikedItems, tasteProvider);

                plan.Gifts.Add(itemId != null
                    ? new PlannedGift(npcName, itemId)
                    : new PlannedGift(npcName, GiftPlanStatus.NoEligibleItem));
            }

            return plan;
        }
    }
}
