using System;
using System.Collections.Generic;

namespace StardewAutoGifter.Core
{
    /// <summary>The checked key settings: the keys to use, and the warnings to log.</summary>
    public sealed record KeyCheckResult(int MenuKey, int ChestMarkKey, IReadOnlyList<string> Warnings);

    /// <summary>
    /// Checks the two configured keys once at startup so a bad config.json value never fails
    /// silently. Keys are plain numbers (SMAPI's SButton values) so this stays game-independent;
    /// the caller says which numbers are real keys and how to name them. See
    /// specs/daily-plan-review - "Key settings checked when the mod starts" (HW6 review 08b).
    /// </summary>
    public static class KeyConfigCheck
    {
        public static KeyCheckResult Validate(
            int menuKey,
            int chestMarkKey,
            Func<int, bool> isRealKey,
            int noneKey,
            int defaultMenuKey,
            int defaultChestMarkKey,
            Func<int, string> keyName)
        {
            var warnings = new List<string>();

            menuKey = CheckOne("MenuKey", menuKey, "the mod menu", defaultMenuKey);
            chestMarkKey = CheckOne("ChestMarkKey", chestMarkKey, "chest marking", defaultChestMarkKey);

            if (menuKey == chestMarkKey && menuKey != noneKey)
                warnings.Add($"config.json: MenuKey and ChestMarkKey are both {keyName(menuKey)}; that key opens the mod menu, so chest marking will not work.");

            return new KeyCheckResult(menuKey, chestMarkKey, warnings);

            int CheckOne(string field, int key, string feature, int defaultKey)
            {
                if (!isRealKey(key))
                {
                    warnings.Add($"config.json: {field} is set to {key}, which is not a key; using the default {keyName(defaultKey)} instead.");
                    return defaultKey;
                }

                if (key == noneKey)
                    warnings.Add($"config.json: {field} is set to None, so {feature} is turned off.");

                return key;
            }
        }
    }
}
