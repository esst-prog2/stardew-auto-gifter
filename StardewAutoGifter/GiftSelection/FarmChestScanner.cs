using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace StardewAutoGifter.GiftSelection
{
    /// <summary>
    /// Finds chests within the mod's farm-wide scope: the outdoor farm map plus the indoor
    /// locations of buildings placed on it (barn, coop, shed, greenhouse, ...). Deliberately
    /// excludes the farmhouse and its cellar. See specs/gift-selection - "Farm-wide item pool".
    /// </summary>
    public static class FarmChestScanner
    {
        /// <summary>All locations within the mod's farm scope: the farm map and its buildings' interiors.</summary>
        public static IEnumerable<GameLocation> GetFarmScopeLocations()
        {
            Farm farm = Game1.getFarm();
            yield return farm;

            foreach (Building building in farm.buildings)
            {
                GameLocation? indoors = building.indoors.Value;
                if (indoors != null)
                    yield return indoors;
            }
        }

        /// <summary>True if the given location is within the mod's farm scope (used to gate the chest-marking keybind).</summary>
        public static bool IsInFarmScope(GameLocation location)
        {
            foreach (GameLocation candidate in GetFarmScopeLocations())
            {
                if (candidate == location)
                    return true;
            }
            return false;
        }

        /// <summary>Every placed chest within the mod's farm scope.</summary>
        public static IEnumerable<Chest> GetFarmChests()
        {
            foreach (GameLocation location in GetFarmScopeLocations())
            {
                foreach (StardewValley.Object obj in location.Objects.Values)
                {
                    if (obj is Chest chest)
                        yield return chest;
                }
            }
        }

        /// <summary>Every farm chest currently carrying the reserved sentinel color.</summary>
        public static List<Chest> GetMarkedChests()
        {
            return GetFarmChests()
                .Where(chest => SentinelColor.Matches(chest.playerChoiceColor.Value))
                .ToList();
        }

        /// <summary>
        /// The single valid anchor chest, or null if none or more than one farm chest is marked.
        /// See specs/gift-selection - "Ambiguous marking treated as unresolved".
        /// </summary>
        public static Chest? FindAnchorChest(out bool isAmbiguous)
        {
            List<Chest> marked = GetMarkedChests();
            isAmbiguous = marked.Count > 1;
            return marked.Count == 1 ? marked[0] : null;
        }

        /// <summary>The farm-scope location that currently contains the given chest, or null if it isn't placed in one.</summary>
        public static GameLocation? FindLocationOf(Chest chest)
        {
            foreach (GameLocation location in GetFarmScopeLocations())
            {
                foreach (StardewValley.Object obj in location.Objects.Values)
                {
                    if (ReferenceEquals(obj, chest))
                        return location;
                }
            }
            return null;
        }

        /// <summary>The distinctive display name applied to the anchor chest. See specs/gift-selection - "Anchor chest is renamed for visibility".</summary>
        public const string AnchorChestName = "Auto-Gifter Chest";

        /// <summary>The game's plain default chest name, restored when a chest is unmarked.</summary>
        private const string DefaultChestName = "Chest";

        /// <summary>Marks the chest, or unmarks it if it is already the anchor. No-op if the chest isn't in farm scope.</summary>
        public static void ToggleMark(Chest chest, GameLocation chestLocation)
        {
            if (!IsInFarmScope(chestLocation))
                return;

            bool isCurrentlyMarked = SentinelColor.Matches(chest.playerChoiceColor.Value);

            chest.playerChoiceColor.Value = isCurrentlyMarked
                ? Color.Black // the game's "no tint selected" sentinel for chests
                : SentinelColor.Value;

            chest.Name = isCurrentlyMarked ? DefaultChestName : AnchorChestName;
        }
    }
}
