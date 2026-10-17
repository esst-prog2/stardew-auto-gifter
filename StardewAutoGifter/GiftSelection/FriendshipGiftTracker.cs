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

        /// <summary>
        /// Planning-time check for a plan's target day, used by plan computation (menu and day end).
        /// See <see cref="TargetDayGiftLimit.CanGiveGiftOn"/> (HW6 review 03, R1).
        /// </summary>
        public static bool CanGiveGiftOn(string npcName, int targetDay, int today)
        {
            if (!Game1.player.friendshipData.TryGetValue(npcName, out Friendship? friendship) || friendship == null)
                return true;

            return TargetDayGiftLimit.CanGiveGiftOn(targetDay, today, friendship.GiftsToday, friendship.GiftsThisWeek);
        }
    }
}
