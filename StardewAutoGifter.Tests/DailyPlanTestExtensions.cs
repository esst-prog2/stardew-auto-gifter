using System.Linq;
using StardewAutoGifter.Core;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Test-only helpers for reading a <see cref="DailyPlan"/>. <c>FindFor</c> used to live on
    /// <see cref="DailyPlan"/> itself, but only the tests called it, so it moved here (HW6 review
    /// 07a). As an extension method, test code still reads <c>plan.FindFor("Haley")</c>.
    /// </summary>
    internal static class DailyPlanTestExtensions
    {
        public static PlannedGift? FindFor(this DailyPlan plan, string npcName)
            => plan.Gifts.FirstOrDefault(g => g.NpcName == npcName);
    }
}
