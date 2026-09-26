using StardewValley;

namespace StardewAutoGifter.GiftSelection
{
    /// <summary>
    /// Reads the game's own per-NPC gift-tracking data (Friendship.GiftsToday / GiftsThisWeek -
    /// the same state that drives the Social Tab's gift checkboxes) instead of maintaining a
    /// separate mod-side counter. See design.md - "Gift-limit tracking".
    /// </summary>
    public static class FriendshipGiftTracker
    {
        /// <summary>The vanilla cap on friendship-point-counting gifts per NPC per week.</summary>
        private const int WeeklyFriendshipGiftCap = 2;

        public static bool HasReachedWeeklyCap(string npcName)
        {
            if (!Game1.player.friendshipData.TryGetValue(npcName, out Friendship? friendship) || friendship == null)
                return false;

            return friendship.GiftsThisWeek >= WeeklyFriendshipGiftCap;
        }
    }
}
