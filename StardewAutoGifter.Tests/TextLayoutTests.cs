using System.Collections.Generic;
using StardewAutoGifter.Core;
using Xunit;

namespace StardewAutoGifter.Tests
{
    /// <summary>
    /// Expected values taken from the spec scenarios (HW6 review 03d,
    /// reviews/03-patched-where-it-broke.md): specs/daily-plan-review - "Warnings and Settings text
    /// fits the menu". "Room for N characters" is modeled by measuring each character as 1 unit.
    /// </summary>
    public class TextLayoutTests
    {
        private static float OneUnitPerCharacter(string text) => text.Length;

        // "Words that do not fit move to the next line".
        [Fact]
        public void WordsThatDoNotFit_MoveToNextLine()
        {
            Assert.Equal(new List<string> { "aaa bbb", "ccc" }, TextLayout.Wrap("aaa bbb ccc", OneUnitPerCharacter, maxWidth: 7));
        }

        // "A word wider than a whole line".
        [Fact]
        public void WordWiderThanLine_IsPutOnItsOwnLine_NotSplit()
        {
            Assert.Equal(new List<string> { "abcdef", "gh" }, TextLayout.Wrap("abcdef gh", OneUnitPerCharacter, maxWidth: 3));
        }

        // "Paragraphs separated by one empty line".
        [Fact]
        public void Paragraphs_AreSeparatedByOneEmptyLine()
        {
            Assert.Equal(new List<string> { "aa", "", "bb" }, TextLayout.Paragraphs(new[] { "aa", "bb" }, OneUnitPerCharacter, maxWidth: 10));
        }

        // "Cannot scroll past the last line": 20 lines, 16 fit, line 4 at the top.
        [Fact]
        public void ScrollDown_WithLastLineVisible_StaysAtLine4()
        {
            Assert.Equal(4, ListScroll.Scroll(topRow: 4, rowsDown: 1, itemCount: 20, visibleRows: 16));
        }
    }
}
