using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values (including the exact texts) taken from the spec (HW6 review 01d,
    /// reviews/01-spec-drift.md): specs/daily-plan-review - "Reason shown for configured NPCs with
    /// no gift" and "Warnings and info panel".
    /// </summary>
    public class MarkingConflictTests
    {
        // "HUD message when a conflict starts".
        [Fact]
        public void ConflictStarts_ShowsHudMessage()
        {
            Assert.True(MarkingConflict.ShouldNotify(wasConflict: false, isConflict: true));
        }

        // "No repeated HUD message while the conflict lasts".
        [Fact]
        public void ConflictLasts_DoesNotRepeatHudMessage()
        {
            Assert.False(MarkingConflict.ShouldNotify(wasConflict: true, isConflict: true));
        }

        // "No HUD message without a conflict": the conflict was just resolved.
        [Fact]
        public void ConflictResolved_ShowsNoHudMessage()
        {
            Assert.False(MarkingConflict.ShouldNotify(wasConflict: true, isConflict: false));
        }

        // "No HUD message without a conflict": there was none before either.
        [Fact]
        public void NoConflict_ShowsNoHudMessage()
        {
            Assert.False(MarkingConflict.ShouldNotify(wasConflict: false, isConflict: false));
        }

        // "Warnings and info panel": the exact HUD text.
        [Fact]
        public void HudMessage_HasTheSpecifiedText()
        {
            Assert.Equal("Auto-Gifter paused: more than one chest is marked. Unmark all but one with G.", MarkingConflict.HudMessage);
        }

        // "Mod inactive because no chest is marked".
        [Fact]
        public void NoMarkedChest_PlanMessageTellsHowToMark()
        {
            Assert.Equal("Mod inactive: open a chest on your farm and press G to mark it.", MarkingConflict.InactivePlanMessage(markedChestCount: 0));
        }

        // "Mod paused because more than one chest is marked".
        [Fact]
        public void TwoMarkedChests_PlanMessageTellsHowToUnmark()
        {
            Assert.Equal("Mod paused: more than one chest is marked. Open each extra chest and press G to unmark it until only one remains.", MarkingConflict.InactivePlanMessage(markedChestCount: 2));
        }

        // "Exactly one chest is marked": no inactive message.
        [Fact]
        public void OneMarkedChest_NoInactivePlanMessage()
        {
            Assert.Null(MarkingConflict.InactivePlanMessage(markedChestCount: 1));
        }
    }
}
