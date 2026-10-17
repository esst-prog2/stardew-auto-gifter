using System.Collections.Generic;
using System.Linq;

namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Pure rules for which day a plan belongs to, decoupled from the game so they can be unit
    /// tested. Days are SMAPI's days-since-start count. See specs/daily-plan-review - "Plan computed
    /// for the next day when the menu is opened", "Edited plan carried into the next day", "Next
    /// day's plan computed automatically at the end of the day"; specs/proximity-delivery - "Plan
    /// delivered only on its target day".
    /// </summary>
    public static class PlanTiming
    {
        /// <summary>The target day of a plan computed today: always the next day.</summary>
        public static int NextTargetDay(int today) => today + 1;

        /// <summary>Whether a plan with the given target day (null: no plan) is the plan to execute today.</summary>
        public static bool IsPlanForToday(int? planTargetDay, int today) => planTargetDay == today;

        /// <summary>Whether the next day's plan has to be computed automatically as today ends: there is no plan for tomorrow, or only an inactive one.</summary>
        public static bool ShouldComputeAtDayEnding(int? nextPlanTargetDay, bool nextPlanIsActive, int today)
            => nextPlanTargetDay != NextTargetDay(today) || !nextPlanIsActive;

        /// <summary>
        /// Keeps the player's toggled-off rows when the next day's plan is recomputed: every NPC
        /// toggled off in <paramref name="previous"/> stays toggled off in <paramref name="recomputed"/>,
        /// as long as both plans are for the same day and the NPC still has a planned item.
        /// </summary>
        public static void CarryExclusions(DailyPlan previous, DailyPlan recomputed)
        {
            if (previous.TargetDay != recomputed.TargetDay)
                return;

            HashSet<string> excludedNames = previous.Gifts
                .Where(g => g.Excluded)
                .Select(g => g.NpcName)
                .ToHashSet();

            foreach (PlannedGift gift in recomputed.Gifts)
            {
                if (gift.Status == GiftPlanStatus.Planned && excludedNames.Contains(gift.NpcName))
                    gift.Excluded = true;
            }
        }
    }
}
