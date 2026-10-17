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
        /// Rebuilds the plan, adding a warning to <paramref name="warnings"/> for every kind of damage
        /// so the caller can log it (specs/daily-plan-review - "Edited plan carried into the next
        /// day"): a target day below 1 (the plan is then never used), a missing row list (loaded with
        /// no rows), a row without a villager name (skipped), and a planned row without an item
        /// (loaded as having no eligible item).
        /// </summary>
        public DailyPlan ToPlan(List<string>? warnings = null)
        {
            var plan = new DailyPlan { TargetDay = TargetDay, IsActive = IsActive };

            if (TargetDay < 1)
                warnings?.Add($"Stored plan has no valid target day ({TargetDay}); it will not be used for any day.");

            if (Gifts == null)
            {
                warnings?.Add("Stored plan had no row list (null); it was loaded with no rows.");
                return plan;
            }

            for (int i = 0; i < Gifts.Count; i++)
            {
                StoredGiftData gift = Gifts[i];
                if (gift == null || string.IsNullOrEmpty(gift.NpcName))
                {
                    warnings?.Add($"Stored plan row {i + 1} has no villager name; it was skipped.");
                    continue;
                }

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
