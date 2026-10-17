using System.Collections.Generic;
using System.Linq;

namespace StardewAutoGifter.Core
{
    /// <summary>One row of the NPCs tab: the NPC's internal name (what is stored and toggled) and the label shown.</summary>
    public sealed record NpcTabRow(string Name, string Label, bool NotFound);

    /// <summary>
    /// Builds the NPCs tab's rows. Selected NPCs the game does not have come first, labeled
    /// "(not found)", so they can be unchecked; then every villager the game has. See
    /// specs/daily-plan-review - "Configured NPCs selected from the mod menu" (HW6 review 05g).
    /// </summary>
    public static class NpcTabList
    {
        public static List<NpcTabRow> Build(IEnumerable<string> selectedNames, IReadOnlyList<(string Name, string DisplayName)> villagers)
        {
            var known = new HashSet<string>(villagers.Select(v => v.Name));

            var rows = selectedNames
                .Where(name => !known.Contains(name))
                .OrderBy(name => name)
                .Select(name => new NpcTabRow(name, $"{name} (not found)", NotFound: true))
                .ToList();

            rows.AddRange(villagers.Select(v => new NpcTabRow(v.Name, v.DisplayName, NotFound: false)));
            return rows;
        }
    }
}
