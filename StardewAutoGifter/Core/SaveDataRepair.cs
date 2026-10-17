using System.Collections.Generic;

namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Repairs damaged saved lists instead of letting them fail later. A JSON <c>null</c> replaces a
    /// collection's default value when SMAPI loads save data, so it has to be caught on load. See
    /// specs/daily-plan-review - "Damaged saved lists reset with a warning" (HW6 review 08c).
    /// </summary>
    public static class SaveDataRepair
    {
        /// <summary>The saved set, or a new empty one plus a warning naming the field if it was stored as null.</summary>
        public static HashSet<string> SetOrEmpty(HashSet<string>? saved, string fieldName, List<string> warnings)
        {
            if (saved != null)
                return saved;

            warnings.Add($"Saved data field '{fieldName}' was missing (null); it was loaded as an empty list.");
            return new HashSet<string>();
        }
    }
}
