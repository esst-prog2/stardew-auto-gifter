using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected value taken from the spec (HW6 review 05d, reviews/05-swallowed-errors.md):
    /// specs/daily-plan-review - "Reason shown for configured NPCs with no gift", scenario
    /// "Configured NPC not found in the game".
    /// </summary>
    public class PlanRowTextTests
    {
        [Fact]
        public void NpcNotFound_RowSaysNotFoundInTheGame()
        {
            Assert.Equal("Sophia: not found in the game", PlanRowText.ReasonLine(GiftPlanStatus.NpcNotFound, "Sophia"));
        }

        // The same scenario: never the "nothing to give" reason.
        [Fact]
        public void NpcNotFound_RowIsNotTheNothingToGiveReason()
        {
            Assert.DoesNotContain("nothing to give", PlanRowText.ReasonLine(GiftPlanStatus.NpcNotFound, "Sophia"));
        }
    }
}
