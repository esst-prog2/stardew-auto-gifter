using System;
using System.Collections.Generic;

namespace StardewAutoGifter.Core
{
    /// <summary>
    /// Pure word-wrapping for the menu's text, decoupled from the game's fonts: the caller passes
    /// in how wide a string is. See specs/daily-plan-review - "Warnings and Settings text fits the
    /// menu".
    /// </summary>
    public static class TextLayout
    {
        /// <summary>Splits text into lines no wider than <paramref name="maxWidth"/>, breaking only between words; a word wider than a whole line gets a line of its own.</summary>
        public static List<string> Wrap(string text, Func<string, float> measure, int maxWidth)
        {
            var lines = new List<string>();
            var current = "";

            foreach (string word in text.Split(' '))
            {
                string candidate = current.Length == 0 ? word : current + " " + word;
                if (measure(candidate) > maxWidth && current.Length > 0)
                {
                    lines.Add(current);
                    current = word;
                }
                else
                {
                    current = candidate;
                }
            }

            if (current.Length > 0)
                lines.Add(current);

            return lines;
        }

        /// <summary>Wraps each paragraph and joins them with one empty line between paragraphs.</summary>
        public static List<string> Paragraphs(IEnumerable<string> paragraphs, Func<string, float> measure, int maxWidth)
        {
            var lines = new List<string>();
            foreach (string paragraph in paragraphs)
            {
                if (lines.Count > 0)
                    lines.Add("");

                lines.AddRange(Wrap(paragraph, measure, maxWidth));
            }
            return lines;
        }
    }
}
