using System.Collections.Generic;

namespace StardewAutoGifter.Core
{
    /// <summary>
    /// The save-data form of a <see cref="DailyPlan"/>. Deliberately does not store
    /// <see cref="PlannedGift.Resolved"/>, so each day starts with nothing delivered or notified.
    /// See specs/daily-plan-review - "Edited plan carried into the next day".
    /// </summary>
    public class StoredPlanData
    {
        public int TargetDay { get; set; }
        public bool IsActive { get; set; }
        public List<StoredGiftData> Gifts { get; set; } = new();

        public static StoredPlanData FromPlan(DailyPlan plan)
        {
            var data = new StoredPlanData { TargetDay = plan.TargetDay, IsActive = plan.IsActive };
            foreach (PlannedGift gift in plan.Gifts)
            {
                data.Gifts.Add(new StoredGiftData
                {
                    NpcName = gift.NpcName,
                    Status = gift.Status,
                    QualifiedItemId = gift.QualifiedItemId,
                    Excluded = gift.Excluded,
                });
            }
            return data;
        }

        /// <summary>
        /// Rebuilds the plan. A damaged row (planned, but without an item) is loaded as having no
        /// eligible item, and a warning naming the NPC is added to <paramref name="warnings"/> so the
        /// caller can log it (specs/daily-plan-review - "Edited plan carried into the next day").
        /// </summary>
        public DailyPlan ToPlan(List<string>? warnings = null)
        {
            var plan = new DailyPlan { TargetDay = TargetDay, IsActive = IsActive };
            foreach (StoredGiftData gift in Gifts)
            {
                if (gift.Status == GiftPlanStatus.Planned && gift.QualifiedItemId == null)
                    warnings?.Add($"Stored plan row for '{gift.NpcName}' was marked as planned but had no item; it was loaded as 'nothing to give'.");

                plan.Gifts.Add(gift.Status == GiftPlanStatus.Planned && gift.QualifiedItemId != null
                    ? new PlannedGift(gift.NpcName, gift.QualifiedItemId) { Excluded = gift.Excluded }
                    : new PlannedGift(gift.NpcName, gift.Status == GiftPlanStatus.Planned ? GiftPlanStatus.NoEligibleItem : gift.Status));
            }
            return plan;
        }
    }

    public class StoredGiftData
    {
        public string NpcName { get; set; } = "";
        public GiftPlanStatus Status { get; set; }
        public string? QualifiedItemId { get; set; }
        public bool Excluded { get; set; }
    }
}
