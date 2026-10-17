namespace StardewAutoGifter.Core
{
    /// <summary>One configured NPC's gift status for the day the plan describes. See specs/daily-plan-review.</summary>
    public class PlannedGift
    {
        public string NpcName { get; }
        public GiftPlanStatus Status { get; }

        /// <summary>Only meaningful when <see cref="Status"/> is <see cref="GiftPlanStatus.Planned"/>.</summary>
        public string? QualifiedItemId { get; }

        /// <summary>True if the player toggled this row off in the preview; excluded ones are never delivered.</summary>
        public bool Excluded { get; set; }

        /// <summary>True once specs/proximity-delivery has resolved this gift for the day - delivered, skipped (gift limit or stock), or answered with the in-person notice - so it is not checked again.</summary>
        public bool Resolved { get; set; }

        public PlannedGift(string npcName, string qualifiedItemId)
        {
            NpcName = npcName;
            Status = GiftPlanStatus.Planned;
            QualifiedItemId = qualifiedItemId;
        }

        public PlannedGift(string npcName, GiftPlanStatus status)
        {
            NpcName = npcName;
            Status = status;
            QualifiedItemId = null;
        }
    }
}
