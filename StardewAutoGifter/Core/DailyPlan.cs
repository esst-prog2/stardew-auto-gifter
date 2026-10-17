using System.Collections.Generic;
using System.Linq;

namespace StardewAutoGifter.Core
{
    /// <summary>
    /// The gift plan for a single in-game day: computed for the following day (when the menu is
    /// opened, or automatically when the day ends), editable via the preview menu, stored with the
    /// save, and executed unchanged on its target day. See design.md - "Plan computation timing"
    /// and <see cref="PlanTiming"/>.
    /// </summary>
    public class DailyPlan
    {
        public List<PlannedGift> Gifts { get; } = new();

        /// <summary>The in-game day this plan is for (SMAPI's days-since-start count); always the day after it was computed.</summary>
        public int TargetDay { get; init; }

        /// <summary>False when no single valid anchor chest was marked at computation time (specs/gift-selection - "Marked chest acts as the mod's on/off anchor").</summary>
        public bool IsActive { get; init; } = true;

        public PlannedGift? FindFor(string npcName) => Gifts.FirstOrDefault(g => g.NpcName == npcName);

        /// <summary>Gifts still eligible for automatic delivery today: planned, not toggled off, not yet delivered.</summary>
        public IEnumerable<PlannedGift> PendingDeliveries() =>
            Gifts.Where(g => g.Status == GiftPlanStatus.Planned && !g.Excluded && !g.Resolved);

        /// <summary>
        /// Configured NPCs with no planned gift (no eligible item, or the weekly cap is reached)
        /// who haven't yet been given the in-person "nothing to give" notice today. See
        /// specs/proximity-delivery - "In-person notice when nothing is available for a configured NPC".
        /// </summary>
        public IEnumerable<PlannedGift> PendingNoGiftNotifications() =>
            Gifts.Where(g => g.Status != GiftPlanStatus.Planned && !g.Resolved);
    }
}
