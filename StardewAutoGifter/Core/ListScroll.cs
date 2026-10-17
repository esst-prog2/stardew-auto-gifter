using System;

namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Pure scrolling math for the menu's row lists (NPCs, Plan, Blocklist tabs). See
    /// specs/daily-plan-review - "Per-NPC preview with on/off toggle".
    /// </summary>
    public static class ListScroll
    {
        /// <summary>
        /// The new top row after scrolling <paramref name="rowsDown"/> rows (negative: up), kept
        /// between the first row and the last position where the final row is still visible.
        /// </summary>
        public static int Scroll(int topRow, int rowsDown, int itemCount, int visibleRows)
        {
            int maxTopRow = Math.Max(0, itemCount - visibleRows);
            return Math.Clamp(topRow + rowsDown, 0, maxTopRow);
        }

        /// <summary>The list index shown in the given visible row, or -1 if no item is shown there.</summary>
        public static int ItemIndexAtRow(int topRow, int visibleRowIndex, int itemCount)
        {
            int index = topRow + visibleRowIndex;
            return visibleRowIndex >= 0 && index < itemCount ? index : -1;
        }
    }
}
