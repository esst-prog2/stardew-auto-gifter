using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace StardewAutoGifter.Npcs
{
    /// <summary>Builds the full villager list for the mod menu's NPCs tab.</summary>
    public static class VillagerCatalog
    {
        public static List<NPC> BuildVillagerList()
        {
            return Utility.getAllCharacters()
                .Where(npc => npc.IsVillager)
                .OrderBy(npc => npc.displayName)
                .ToList();
        }
    }
}
