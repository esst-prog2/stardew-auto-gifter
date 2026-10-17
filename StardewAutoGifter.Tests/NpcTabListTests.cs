using System.Collections.Generic;
using System.Linq;
using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec scenarios (HW6 review 05g, reviews/05-swallowed-errors.md):
    /// specs/daily-plan-review - "Configured NPCs selected from the mod menu".
    /// </summary>
    public class NpcTabListTests
    {
        private static readonly List<(string Name, string DisplayName)> AbigailAndHaley = new()
        {
            ("Abigail", "Abigail"),
            ("Haley", "Haley"),
        };

        // "Selected villager not found in the game": "Sophia (not found)" at the top, then Abigail and Haley.
        [Fact]
        public void SelectedButNotFound_IsListedFirst_LabeledNotFound()
        {
            List<NpcTabRow> rows = NpcTabList.Build(new[] { "Sophia" }, AbigailAndHaley);

            Assert.Equal(new[] { "Sophia (not found)", "Abigail", "Haley" }, rows.Select(r => r.Label));
            Assert.Equal("Sophia", rows[0].Name);
            Assert.True(rows[0].NotFound);
        }

        // "Selected villager found in the game": each villager once, no "(not found)" row.
        [Fact]
        public void SelectedAndFound_IsListedOnce_WithoutNotFoundRow()
        {
            List<NpcTabRow> rows = NpcTabList.Build(new[] { "Haley" }, AbigailAndHaley);

            Assert.Equal(new[] { "Abigail", "Haley" }, rows.Select(r => r.Label));
            Assert.DoesNotContain(rows, r => r.NotFound);
        }
    }
}
