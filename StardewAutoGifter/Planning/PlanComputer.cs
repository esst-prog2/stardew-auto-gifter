using System.Collections.Generic;
using StardewAutoGifter.Core;
using StardewAutoGifter.GiftSelection;

namespace StardewAutoGifter.Planning
{
    /// <summary>
    /// Computes a <see cref="DailyPlan"/> from the current anchor-only and farm-wide pools, the
    /// blocklist, and settings. Called when the player opens the mod menu. See
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
            IGiftTasteProvider tasteProvider)
        {
            var plan = new DailyPlan { IsActive = true };

            foreach (string npcName in configuredNpcNames)
            {
                if (FriendshipGiftTracker.HasReachedWeeklyCap(npcName))
                {
                    plan.Gifts.Add(new PlannedGift(npcName, GiftPlanStatus.WeeklyCapReached));
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
