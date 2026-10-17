using StardewModdingAPI;
using StardewAutoGifter.Core;

namespace StardewAutoGifter.Planning
{
    /// <summary>
    /// Stores the next day's plan with the save (mirrors <see cref="StardewAutoGifter.Blocklist.BlocklistStore"/>).
    /// SMAPI writes save data to disk when the game saves, so a plan stored as the day ends is the
    /// one found again when the save is loaded. See specs/daily-plan-review - "Edited plan carried
    /// into the next day".
    /// </summary>
    public class PlanStore
    {
        private const string SaveDataKey = "next-plan";

        private readonly IModHelper _helper;

        public PlanStore(IModHelper helper)
        {
            _helper = helper;
        }

        public DailyPlan? Load() => _helper.Data.ReadSaveData<StoredPlanData>(SaveDataKey)?.ToPlan();

        public void Save(DailyPlan? plan)
            => _helper.Data.WriteSaveData(SaveDataKey, plan == null ? null : StoredPlanData.FromPlan(plan));
    }
}
