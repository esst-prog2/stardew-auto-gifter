using System.Collections.Generic;

namespace StardewAutoGifter.Core
{
    /// <summary>
    /// The one-time welcome message, one short string per dialogue page. Every value that comes
    /// from a setting or from what happened is passed in (the configured keys, the save's "anchor
    /// chest only" setting, and whether the starter chest fit in the inventory), never written into
    /// the text. See specs/gift-selection - "Starter chest and welcome message on first farm exit".
    /// </summary>
    public static class WelcomeText
    {
        /// <param name="chestDroppedAtFeet">True when the starter chest did not fit in the inventory and was dropped at the player's feet instead.</param>
        public static List<string> Pages(string menuKey, string chestMarkKey, bool anchorChestOnly, bool chestDroppedAtFeet = false) => new()
        {
            "Welcome to Auto-Gifter!",
            chestDroppedAtFeet
                ? "Your inventory was full, so a chest has been dropped at your feet. Pick it up and place it anywhere on your farm (or inside the barn, coop, shed, or greenhouse) - not in the farmhouse or cellar."
                : "A chest has been added to your inventory. Place it anywhere on your farm (or inside the barn, coop, shed, or greenhouse) - not in the farmhouse or cellar.",
            $"Open the chest and press {chestMarkKey} to mark it as your gift chest - it will be renamed and sparkle occasionally so you can spot it.",
            anchorChestOnly
                ? "That chest is your ONLY source of gifts: the fallback to every other chest on your farm is turned off."
                : "That chest is checked FIRST for gifts. If it doesn't have what's needed, the mod falls back to every other chest on your farm.",
            anchorChestOnly
                ? "You can turn that fallback back on in the mod menu's Settings tab, to also use every other chest on your farm."
                : "You can turn that fallback off in the mod menu's Settings tab, to only ever use the marked chest.",
            $"Press {menuKey} anytime to open the mod menu: choose which villagers to gift, preview and edit tomorrow's plan, manage your blocklist, and adjust settings.",
            "IMPORTANT: automatic gifting always starts the NEXT day, never the same day.",
            "Selecting a villager or editing the plan today has no effect on today - check the \"Tomorrow's Plan\" label in the Plan tab to see what's actually queued up.",
        };
    }
}
