using StardewValley;
using StardewAutoGifter.Core;

namespace StardewAutoGifter.GiftSelection
{
    /// <summary>
    /// Reads the game's own per-NPC gift-tracking data (Friendship.GiftsToday / GiftsThisWeek -
    /// the same state that drives the Social Tab's gift checkboxes) instead of maintaining a
    /// separate mod-side counter, and delegates the actual decision to the pure
    /// <see cref="GiftLimitPolicy"/>. See design.md - "Gift-limit tracking".
    /// </summary>
    public static class FriendshipGiftTracker
    {
        public static bool HasReachedWeeklyCap(string npcName)
        {
            if (!Game1.player.friendshipData.TryGetValue(npcName, out Friendship? friendship) || friendship == null)
                return false;

            return !GiftLimitPolicy.CanGiveGift(friendship.GiftsToday, friendship.GiftsThisWeek);
        }

        /// <summary>Planning-time check (evening, for the next day); see <see cref="GiftLimitPolicy.CanPlanGiftForNextDay"/>.</summary>
        public static bool CanPlanGiftForNextDay(string npcName)
        {
            if (!Game1.player.friendshipData.TryGetValue(npcName, out Friendship? friendship) || friendship == null)
                return true;

            return GiftLimitPolicy.CanPlanGiftForNextDay(friendship.GiftsToday, friendship.GiftsThisWeek);
        }
    }
}
