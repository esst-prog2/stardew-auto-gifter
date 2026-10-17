using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec scenarios (HW6 review 01e, reviews/01-spec-drift.md):
    /// specs/daily-plan-review - "Per-NPC preview with on/off toggle" (scrollable Plan tab).
    /// </summary>
    public class ListScrollTests
    {
        // "Scrolling down one step".
        [Fact]
        public void ScrollDownOneStep_FromTop_MovesToRow1()
        {
            Assert.Equal(1, ListScroll.Scroll(topRow: 0, rowsDown: 1, itemCount: 20, visibleRows: 7));
        }

        // "Cannot scroll above the first row".
        [Fact]
        public void ScrollUp_AtTop_StaysAtRow0()
        {
            Assert.Equal(0, ListScroll.Scroll(topRow: 0, rowsDown: -1, itemCount: 20, visibleRows: 7));
        }

        // "Cannot scroll past the last row".
        [Fact]
        public void ScrollDown_WithLastRowVisible_StaysAtRow13()
        {
            Assert.Equal(13, ListScroll.Scroll(topRow: 13, rowsDown: 1, itemCount: 20, visibleRows: 7));
        }

        // "No scrolling when every row fits".
        [Fact]
        public void ScrollDown_WhenEveryRowFits_StaysAtRow0()
        {
            Assert.Equal(0, ListScroll.Scroll(topRow: 0, rowsDown: 1, itemCount: 5, visibleRows: 7));
        }

        // "Clicking a row after scrolling".
        [Fact]
        public void ClickSeventhVisibleRow_WithRow13AtTop_ActsOnRow19()
        {
            Assert.Equal(19, ListScroll.ItemIndexAtRow(topRow: 13, visibleRowIndex: 6, itemCount: 20));
        }

        // "Clicking below the last row".
        [Fact]
        public void ClickBelowLastRow_ActsOnNoRow()
        {
            Assert.Equal(-1, ListScroll.ItemIndexAtRow(topRow: 13, visibleRowIndex: 7, itemCount: 20));
        }
    }
}
