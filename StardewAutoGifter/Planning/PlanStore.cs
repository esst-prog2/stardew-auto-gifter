using StardewModdingAPI;
using StardewAutoGifter.Core;

namespace StardewAutoGifter.Planning
{
    /// <summary>
    /// Stores the next day's plan with the save (through <see cref="SaveDataStore{T}"/>).
    /// SMAPI writes save data to disk when the game saves, so a plan stored as the day ends is the
    /// one found again when the save is loaded. See specs/daily-plan-review - "Edited plan carried
    /// into the next day".
    /// </summary>
    public class PlanStore
    {
        private readonly SaveDataStore<StoredPlanData> _store;

        public PlanStore(IModHelper helper)
        {
            _store = new SaveDataStore<StoredPlanData>(helper, "next-plan");
        }

        public DailyPlan? Load() => _store.Read()?.ToPlan();

        public void Save(DailyPlan? plan) => _store.Write(plan == null ? null : StoredPlanData.FromPlan(plan));
    }
}
