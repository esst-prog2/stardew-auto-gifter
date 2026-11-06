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
        /// <summary>
        /// Whether the player has met the villager: the game has a friendship entry for them. The
        /// game's own <c>NPC.receiveGift</c> needs that entry and throws without it (HW7a, found in
        /// the HW6 in-game run: <c>KeyNotFoundException</c> for an unmet villager).
        /// </summary>
        public static bool HasMet(string npcName)
            => Game1.player.friendshipData.TryGetValue(npcName, out Friendship? friendship) && friendship != null;

        /// <summary>
        /// Whether the NPC could receive a gift on <paramref name="targetDay"/>, judged from today's
        /// counts. Planning asks it for tomorrow; delivery asks it for today (targetDay == today).
        /// See <see cref="TargetDayGiftLimit.CanGiveGiftOn"/> (HW6 reviews 03c and 04b).
        /// </summary>
        public static bool CanGiveGiftOn(string npcName, int targetDay, int today)
        {
            if (!Game1.player.friendshipData.TryGetValue(npcName, out Friendship? friendship) || friendship == null)
                return true;

            return TargetDayGiftLimit.CanGiveGiftOn(targetDay, today, friendship.GiftsToday, friendship.GiftsThisWeek);
        }
    }
}
