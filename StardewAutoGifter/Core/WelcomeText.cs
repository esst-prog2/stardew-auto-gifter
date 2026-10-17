using System.Collections.Generic;

namespace StardewAutoGifter.Core
{
    /// <summary>
    /// The one-time welcome message, one short string per dialogue page. Key names are passed in
    /// from the configuration, never written into the text. See specs/gift-selection - "Starter
    /// chest and welcome message on first farm exit".
    /// </summary>
    public static class WelcomeText
    {
        public static List<string> Pages(string menuKey, string chestMarkKey) => new()
        {
            "Welcome to Auto-Gifter!",
            "A chest has been added to your inventory. Place it anywhere on your farm (or inside the barn, coop, shed, or greenhouse) - not in the farmhouse or cellar.",
            $"Open the chest and press {chestMarkKey} to mark it as your gift chest - it will be renamed and sparkle occasionally so you can spot it.",
            "That chest is checked FIRST for gifts. If it doesn't have what's needed, the mod falls back to every other chest on your farm.",
            "You can turn that fallback off in the mod menu's Settings tab, to only ever use the marked chest.",
            $"Press {menuKey} anytime to open the mod menu: choose which villagers to gift, preview and edit tomorrow's plan, manage your blocklist, and adjust settings.",
            "IMPORTANT: automatic gifting always starts the NEXT day, never the same day.",
            "Selecting a villager or editing the plan today has no effect on today - check the \"Tomorrow's Plan\" label in the Plan tab to see what's actually queued up.",
        };
    }
}
