using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewAutoGifter.Blocklist;
using StardewAutoGifter.Npcs;
using StardewAutoGifter.Core;
using StardewAutoGifter.Settings;

namespace StardewAutoGifter.Menus
{
    /// <summary>
    /// The mod's menu: an NPC-selection tab, a daily-plan preview tab, a checkbox-filtered
    /// blocklist editor tab, a settings tab, and a warnings/info panel tab. See specs/daily-plan-review.
    /// </summary>
    public class ModMainMenu : IClickableMenu
    {
        private enum Tab
        {
            Npcs,
            Plan,
            Blocklist,
            Settings,
            Warnings,
        }

        private const int RowHeight = 56;
        private const int TabWidth = 145;
        private const int TabHeight = 48;
        private const int LetterBoxHeight = 28;
        private const int ListStartOffset = 140;

        private readonly DailyPlan _plan;
        private readonly BlocklistStore _blocklistStore;
        private readonly ConfiguredNpcStore _configuredNpcStore;
        private readonly ModSettingsStore _settingsStore;
        private readonly List<NPC> _villagers;
        private readonly List<Item> _fullItemCatalog;
        private readonly bool _hasMarkingConflict;
        private readonly int _markedChestCount;
        private readonly string _chestMarkKeyName;

        private Tab _activeTab = Tab.Npcs;

        private readonly Rectangle _npcsTabBounds;
        private readonly Rectangle _planTabBounds;
        private readonly Rectangle _blocklistTabBounds;
        private readonly Rectangle _settingsTabBounds;
        private readonly Rectangle _warningsTabBounds;

        private readonly HashSet<char> _blocklistLetters = new();
        private readonly HashSet<char> _npcsLetters = new();
        private List<Item> _filteredCatalog;
        private List<NPC> _filteredVillagers;
        private int _blocklistScrollIndex;
        private int _npcsScrollIndex;
        private int _planScrollIndex;
        private int _warningsScrollIndex;
        private int _settingsScrollIndex;

        /// <summary>Top of the Warnings/Settings text, and its line height. See specs/daily-plan-review - "Warnings and Settings text fits the menu".</summary>
        private const int PanelTextOffset = 96;
        private const int PanelLineHeight = 28;

        /// <summary>How many text lines fit on the Warnings/Settings tabs, with the same bottom margin as the row lists.</summary>
        private int VisiblePanelLines => Math.Max(1, (height - PanelTextOffset - 40) / PanelLineHeight);

        /// <summary>How many list rows fit below the tab bar / letter bar; the same on every tab.</summary>
        private int VisibleRows => Math.Max(1, (height - ListStartOffset - 40) / RowHeight);

        public ModMainMenu(
            DailyPlan plan,
            BlocklistStore blocklistStore,
            ConfiguredNpcStore configuredNpcStore,
            ModSettingsStore settingsStore,
            List<NPC> villagers,
            List<Item> fullItemCatalog,
            int markedChestCount,
            string chestMarkKeyName)
            : base(Game1.uiViewport.Width / 2 - 400, Game1.uiViewport.Height / 2 - 300, 800, 600, showUpperRightCloseButton: true)
        {
            _plan = plan;
            _blocklistStore = blocklistStore;
            _configuredNpcStore = configuredNpcStore;
            _settingsStore = settingsStore;
            _villagers = villagers;
            _filteredVillagers = villagers;
            _fullItemCatalog = fullItemCatalog;
            _filteredCatalog = fullItemCatalog;
            _markedChestCount = markedChestCount;
            _chestMarkKeyName = chestMarkKeyName;
            _hasMarkingConflict = markedChestCount > 1;

            int tabY = yPositionOnScreen + 16;
            _npcsTabBounds = new Rectangle(xPositionOnScreen + 16, tabY, TabWidth, TabHeight);
            _planTabBounds = new Rectangle(xPositionOnScreen + 16 + TabWidth + 8, tabY, TabWidth, TabHeight);
            _blocklistTabBounds = new Rectangle(xPositionOnScreen + 16 + (TabWidth + 8) * 2, tabY, TabWidth, TabHeight);
            _settingsTabBounds = new Rectangle(xPositionOnScreen + 16 + (TabWidth + 8) * 3, tabY, TabWidth, TabHeight);
            _warningsTabBounds = new Rectangle(xPositionOnScreen + 16 + (TabWidth + 8) * 4, tabY, TabWidth, TabHeight);
        }

        private Rectangle GetLetterBoxBounds(int rowY, int letterIndex)
        {
            int letterWidth = (width - 64) / 26;
            return new Rectangle(xPositionOnScreen + 32 + letterIndex * letterWidth, rowY, letterWidth - 2, LetterBoxHeight);
        }

        private void DrawLetterBar(SpriteBatch b, int rowY, HashSet<char> selectedLetters)
        {
            for (int i = 0; i < 26; i++)
            {
                char letter = (char)('A' + i);
                Rectangle box = GetLetterBoxBounds(rowY, i);
                bool selected = selectedLetters.Contains(letter);
                drawTextureBox(b, box.X, box.Y, box.Width, box.Height, selected ? Color.White : Color.Gray);
                b.DrawString(Game1.smallFont, letter.ToString(), new Vector2(box.X + 4, box.Y + 2), Game1.textColor);
            }
        }

        /// <summary>Returns true if the click landed on a letter box (and toggled it), so the caller can stop processing the click.</summary>
        private bool TryHandleLetterBarClick(int x, int y, int rowY, HashSet<char> selectedLetters)
        {
            for (int i = 0; i < 26; i++)
            {
                if (!GetLetterBoxBounds(rowY, i).Contains(x, y))
                    continue;

                char letter = (char)('A' + i);
                if (!selectedLetters.Remove(letter))
                    selectedLetters.Add(letter);

                Game1.playSound("drumkit6");
                return true;
            }
            return false;
        }

        private void RefreshBlocklistFilter()
        {
            _filteredCatalog = _blocklistLetters.Count == 0
                ? _fullItemCatalog
                : _fullItemCatalog.Where(item => StartsWithAny(item.DisplayName, _blocklistLetters)).ToList();
            _blocklistScrollIndex = 0;
        }

        private void RefreshNpcsFilter()
        {
            _filteredVillagers = _npcsLetters.Count == 0
                ? _villagers
                : _villagers.Where(npc => StartsWithAny(npc.displayName, _npcsLetters)).ToList();
            _npcsScrollIndex = 0;
        }

        private static bool StartsWithAny(string name, HashSet<char> letters)
            => name.Length > 0 && letters.Contains(char.ToUpperInvariant(name[0]));

        /// <summary>Width available for a single row's text before it would run past the menu's right edge.</summary>
        private int ContentWidth => width - 64;

        /// <summary>Shortens a single-line string with a trailing "..." if it would otherwise overflow the given width, so fixed-height rows never spill past the menu border.</summary>
        private static string TruncateToWidth(SpriteFont font, string text, int maxWidth)
        {
            if (font.MeasureString(text).X <= maxWidth)
                return text;

            const string ellipsis = "...";
            string result = text;
            while (result.Length > 0 && font.MeasureString(result + ellipsis).X > maxWidth)
                result = result[..^1];

            return result + ellipsis;
        }

        /// <summary>Splits text into lines that each fit within the given width, breaking on word boundaries.</summary>
        private static List<string> WrapText(SpriteFont font, string text, int maxWidth)
            => TextLayout.Wrap(text, s => font.MeasureString(s).X, maxWidth);

        /// <summary>Draws word-wrapped text starting at (x, y), returning the y position just below the last line.</summary>
        private static int DrawWrappedText(SpriteBatch b, SpriteFont font, string text, int x, int y, int maxWidth, Color color, int lineHeight = 28)
        {
            foreach (string line in WrapText(font, text, maxWidth))
            {
                b.DrawString(font, line, new Vector2(x, y), color);
                y += lineHeight;
            }
            return y;
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            DrawTab(b, _npcsTabBounds, "NPCs", _activeTab == Tab.Npcs);
            DrawTab(b, _planTabBounds, "Plan", _activeTab == Tab.Plan);
            DrawTab(b, _blocklistTabBounds, "Blocklist", _activeTab == Tab.Blocklist);
            DrawTab(b, _settingsTabBounds, "Settings", _activeTab == Tab.Settings);
            DrawTab(b, _warningsTabBounds, "Warnings", _activeTab == Tab.Warnings);

            switch (_activeTab)
            {
                case Tab.Npcs:
                    DrawNpcsTab(b);
                    break;
                case Tab.Plan:
                    DrawPlanTab(b);
                    break;
                case Tab.Blocklist:
                    DrawBlocklistTab(b);
                    break;
                case Tab.Settings:
                    DrawSettingsTab(b);
                    break;
                case Tab.Warnings:
                    DrawWarningsTab(b);
                    break;
            }

            base.draw(b);
            drawMouse(b);
        }

        private static void DrawTab(SpriteBatch b, Rectangle bounds, string label, bool active)
        {
            drawTextureBox(b, bounds.X, bounds.Y, bounds.Width, bounds.Height, active ? Color.White : Color.Gray);
            b.DrawString(Game1.smallFont, label, new Vector2(bounds.X + 16, bounds.Y + 12), Game1.textColor);
        }

        private void DrawNpcsTab(SpriteBatch b)
        {
            DrawLetterBar(b, yPositionOnScreen + 96, _npcsLetters);

            int rowY = yPositionOnScreen + ListStartOffset;
            int visibleRows = VisibleRows;

            foreach (NPC npc in _filteredVillagers.Skip(_npcsScrollIndex).Take(visibleRows))
            {
                bool selected = _configuredNpcStore.Contains(npc.Name);
                string checkbox = selected ? "[x]" : "[ ]";
                string line = TruncateToWidth(Game1.smallFont, $"{checkbox} {npc.displayName}", ContentWidth);
                b.DrawString(Game1.smallFont, line, new Vector2(xPositionOnScreen + 32, rowY), Game1.textColor);
                rowY += RowHeight;
            }
        }

        private void DrawPlanTab(SpriteBatch b)
        {
            // Always shown, regardless of state below - see specs/daily-plan-review - "Plan tab
            // always labeled as the next day's plan": this is the only reminder of the plan's
            // timing that's visible every time the tab is opened, not just once at game start.
            b.DrawString(Game1.smallFont, "Tomorrow's Plan", new Vector2(xPositionOnScreen + 32, yPositionOnScreen + 96), Game1.textColor);

            int rowY = yPositionOnScreen + ListStartOffset;

            if (!_plan.IsActive)
            {
                // specs/daily-plan-review - "Reason shown for configured NPCs with no gift": the
                // message names the fix for the actual cause (nothing marked vs. a conflict).
                string inactiveMessage = MarkingConflict.InactivePlanMessage(_markedChestCount, _chestMarkKeyName) ?? MarkingConflict.InactivePlanMessage(0, _chestMarkKeyName)!;
                DrawWrappedText(b, Game1.smallFont, inactiveMessage, xPositionOnScreen + 32, rowY, ContentWidth, Game1.textColor);
                return;
            }

            if (_plan.Gifts.Count == 0)
            {
                DrawWrappedText(b, Game1.smallFont, "No NPCs configured yet - pick some in the NPCs tab.", xPositionOnScreen + 32, rowY, ContentWidth, Game1.textColor);
                return;
            }

            // specs/daily-plan-review - "Per-NPC preview with on/off toggle": only the rows that fit
            // are drawn, from the current scroll position, so none spill outside the menu.
            for (int row = 0; row < VisibleRows; row++)
            {
                int index = ListScroll.ItemIndexAtRow(_planScrollIndex, row, _plan.Gifts.Count);
                if (index < 0)
                    break;

                PlannedGift gift = _plan.Gifts[index];
                NPC? npc = Game1.getCharacterFromName(gift.NpcName);
                string npcName = npc?.displayName ?? gift.NpcName;
                string line;
                Color color;

                switch (gift.Status)
                {
                    case GiftPlanStatus.Planned:
                        Item? displayItem = ItemRegistry.Create(gift.QualifiedItemId!, allowNull: true);
                        string itemName = displayItem?.DisplayName ?? gift.QualifiedItemId!;
                        string checkbox = gift.Excluded ? "[ ]" : "[x]";
                        line = $"{checkbox} {npcName} -> {itemName}";
                        color = gift.Excluded ? Color.Gray : Game1.textColor;
                        break;
                    case GiftPlanStatus.NoEligibleItem:
                        line = $"{npcName}: nothing to give (no loved/liked item available)";
                        color = Color.Gray;
                        break;
                    case GiftPlanStatus.WeeklyCapReached:
                        line = $"{npcName}: already reached this week's gift limit";
                        color = Color.Gray;
                        break;
                    default:
                        line = npcName;
                        color = Game1.textColor;
                        break;
                }

                b.DrawString(Game1.smallFont, TruncateToWidth(Game1.smallFont, line, ContentWidth), new Vector2(xPositionOnScreen + 32, rowY), color);
                rowY += RowHeight;
            }
        }

        private void DrawBlocklistTab(SpriteBatch b)
        {
            DrawLetterBar(b, yPositionOnScreen + 96, _blocklistLetters);

            int rowY = yPositionOnScreen + ListStartOffset;
            int visibleRows = VisibleRows;

            foreach (Item item in _filteredCatalog.Skip(_blocklistScrollIndex).Take(visibleRows))
            {
                bool blocked = _blocklistStore.Contains(item.QualifiedItemId);
                string checkbox = blocked ? "[x]" : "[ ]";
                string line = TruncateToWidth(Game1.smallFont, $"{checkbox} {item.DisplayName}", ContentWidth);
                b.DrawString(Game1.smallFont, line, new Vector2(xPositionOnScreen + 32, rowY), Game1.textColor);
                rowY += RowHeight;
            }
        }

        /// <summary>The Settings tab as wrapped lines: the setting's own lines first (clickable), then an empty line and its explanation.</summary>
        private List<(string Text, Color Color)> BuildSettingsLines(out int settingLineCount)
        {
            string checkbox = _settingsStore.AnchorChestOnly ? "[x]" : "[ ]";
            List<string> settingLines = WrapText(Game1.smallFont, $"{checkbox} Only use the marked chest (ignore other farm chests)", ContentWidth);
            settingLineCount = settingLines.Count;

            var lines = new List<(string Text, Color Color)>();
            foreach (string line in settingLines)
                lines.Add((line, Game1.textColor));
            lines.Add(("", Game1.textColor));
            foreach (string line in WrapText(Game1.smallFont, "When unchecked (default), the mod tries the marked chest first and falls back to every chest on your farm if it doesn't have what's needed.", ContentWidth))
                lines.Add((line, Color.Gray));
            return lines;
        }

        /// <summary>The Warnings tab as wrapped lines: the paragraphs, then the red active-warning line while a marking conflict lasts.</summary>
        private List<(string Text, Color Color)> BuildWarningsLines()
        {
            string[] paragraphs =
            {
                "This mod marks a chest as its on/off anchor using a reserved color that the game's normal chest color picker cannot produce.",
                "Do not use another mod's chest-coloring feature to try to match it - doing so could make an unrelated chest act as an anchor too.",
                "Mark only one chest at a time. If more than one chest carries the reserved color at once, the mod pauses (gives no gifts) until you unmark all but one.",
                "Picking up the marked chest and placing it again resets its color (and name), which un-marks it - if that happens, just mark it again with the keybind.",
            };

            var lines = new List<(string Text, Color Color)>();
            foreach (string line in TextLayout.Paragraphs(paragraphs, s => Game1.smallFont.MeasureString(s).X, ContentWidth))
                lines.Add((line, Game1.textColor));

            if (_hasMarkingConflict)
            {
                lines.Add(("", Game1.textColor));
                foreach (string line in WrapText(Game1.smallFont, "Active warning: more than one chest currently carries the reserved color.", ContentWidth))
                    lines.Add((line, Color.Red));
            }
            return lines;
        }

        /// <summary>Draws only the lines that fit, from the given scroll position, so no text spills outside the menu.</summary>
        private void DrawPanelLines(SpriteBatch b, List<(string Text, Color Color)> lines, int topLine)
        {
            int y = yPositionOnScreen + PanelTextOffset;
            for (int row = 0; row < VisiblePanelLines; row++)
            {
                int index = ListScroll.ItemIndexAtRow(topLine, row, lines.Count);
                if (index < 0)
                    break;

                b.DrawString(Game1.smallFont, lines[index].Text, new Vector2(xPositionOnScreen + 32, y), lines[index].Color);
                y += PanelLineHeight;
            }
        }

        private void DrawSettingsTab(SpriteBatch b)
            => DrawPanelLines(b, BuildSettingsLines(out _), _settingsScrollIndex);

        private void DrawWarningsTab(SpriteBatch b)
            => DrawPanelLines(b, BuildWarningsLines(), _warningsScrollIndex);

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (_npcsTabBounds.Contains(x, y)) { _activeTab = Tab.Npcs; return; }
            if (_planTabBounds.Contains(x, y)) { _activeTab = Tab.Plan; return; }
            if (_blocklistTabBounds.Contains(x, y)) { _activeTab = Tab.Blocklist; return; }
            if (_settingsTabBounds.Contains(x, y)) { _activeTab = Tab.Settings; return; }
            if (_warningsTabBounds.Contains(x, y)) { _activeTab = Tab.Warnings; return; }

            if (_activeTab == Tab.Npcs)
                HandleNpcsClick(x, y);
            else if (_activeTab == Tab.Plan)
                HandlePlanClick(x, y);
            else if (_activeTab == Tab.Blocklist)
                HandleBlocklistClick(x, y);
            else if (_activeTab == Tab.Settings)
                HandleSettingsClick(x, y);
        }

        private void HandleSettingsClick(int x, int y)
        {
            // Only the setting's own lines toggle it, at the current scroll position.
            int top = yPositionOnScreen + PanelTextOffset;
            if (x < xPositionOnScreen + 32 || x > xPositionOnScreen + width - 32 || y < top)
                return;

            int row = (y - top) / PanelLineHeight;
            if (row >= VisiblePanelLines)
                return;

            List<(string Text, Color Color)> lines = BuildSettingsLines(out int settingLineCount);
            int index = ListScroll.ItemIndexAtRow(_settingsScrollIndex, row, lines.Count);
            if (index < 0 || index >= settingLineCount)
                return;

            _settingsStore.ToggleAnchorChestOnly();
            Game1.playSound("drumkit6");
        }

        private void HandleNpcsClick(int x, int y)
        {
            if (TryHandleLetterBarClick(x, y, yPositionOnScreen + 96, _npcsLetters))
            {
                RefreshNpcsFilter();
                return;
            }

            int rowY = yPositionOnScreen + ListStartOffset;
            int visibleRows = VisibleRows;

            foreach (NPC npc in _filteredVillagers.Skip(_npcsScrollIndex).Take(visibleRows))
            {
                var rowBounds = new Rectangle(xPositionOnScreen + 32, rowY, width - 64, RowHeight - 8);
                if (rowBounds.Contains(x, y))
                {
                    _configuredNpcStore.Toggle(npc.Name);
                    Game1.playSound("drumkit6");
                    return;
                }
                rowY += RowHeight;
            }
        }

        private void HandlePlanClick(int x, int y)
        {
            if (!_plan.IsActive)
                return;

            // Uses the same scroll position as DrawPlanTab, so a click acts on the row shown there.
            int rowY = yPositionOnScreen + ListStartOffset;
            for (int row = 0; row < VisibleRows; row++)
            {
                int index = ListScroll.ItemIndexAtRow(_planScrollIndex, row, _plan.Gifts.Count);
                if (index < 0)
                    return;

                PlannedGift gift = _plan.Gifts[index];
                var rowBounds = new Rectangle(xPositionOnScreen + 32, rowY, width - 64, RowHeight - 8);
                if (rowBounds.Contains(x, y) && gift.Status == GiftPlanStatus.Planned)
                {
                    gift.Excluded = !gift.Excluded;
                    Game1.playSound("drumkit6");
                    return;
                }
                rowY += RowHeight;
            }
        }

        private void HandleBlocklistClick(int x, int y)
        {
            if (TryHandleLetterBarClick(x, y, yPositionOnScreen + 96, _blocklistLetters))
            {
                RefreshBlocklistFilter();
                return;
            }

            int rowY = yPositionOnScreen + ListStartOffset;
            int visibleRows = VisibleRows;

            foreach (Item item in _filteredCatalog.Skip(_blocklistScrollIndex).Take(visibleRows))
            {
                var rowBounds = new Rectangle(xPositionOnScreen + 32, rowY, width - 64, RowHeight - 8);
                if (rowBounds.Contains(x, y))
                {
                    _blocklistStore.Toggle(item.QualifiedItemId);
                    Game1.playSound("drumkit6");
                    return;
                }
                rowY += RowHeight;
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            // The game reports wheel-up as a positive direction, so one step down is -Sign(direction).
            int rowsDown = -Math.Sign(direction);

            if (_activeTab == Tab.Blocklist)
                _blocklistScrollIndex = ListScroll.Scroll(_blocklistScrollIndex, rowsDown, _filteredCatalog.Count, VisibleRows);
            else if (_activeTab == Tab.Npcs)
                _npcsScrollIndex = ListScroll.Scroll(_npcsScrollIndex, rowsDown, _filteredVillagers.Count, VisibleRows);
            else if (_activeTab == Tab.Plan && _plan.IsActive)
                _planScrollIndex = ListScroll.Scroll(_planScrollIndex, rowsDown, _plan.Gifts.Count, VisibleRows);
            else if (_activeTab == Tab.Warnings)
                _warningsScrollIndex = ListScroll.Scroll(_warningsScrollIndex, rowsDown, BuildWarningsLines().Count, VisiblePanelLines);
            else if (_activeTab == Tab.Settings)
                _settingsScrollIndex = ListScroll.Scroll(_settingsScrollIndex, rowsDown, BuildSettingsLines(out _).Count, VisiblePanelLines);
        }
    }
}
