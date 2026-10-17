# HW6 review 03: patched where it broke, not where it's wrong

## What we looked for

The same problem fixed more than once in the git history, which is a sign that the underlying rule may be wrong ("patched where it broke, not where it's wrong").

We reviewed the whole history: all 38 commits on all branches (`git log --all`), together with PLANNING_LOG.md. The whole MVP is one commit (`af02acc`), so the iterations of 2026-09-26 show up only in the log. Each finding says whether it is visible in git or only in the log.

## What we found

- **R1. The gift-limit rule was fixed four times in a row (visible in git).**
  - `d71ed25` put the daily limit into the shared `CanGiveGift`.
  - That broke planning. `810793b` and `97c3900` split the check into two functions.
  - `6b45c26` (01a) brought the spec in line with the code afterwards.
  - `88c696d` (01b) gave plans a target day.
  - What is wrong underneath: the limit rule still does not know which day it is asked about. `CanPlanGiftForNextDay` applies today's weekly count to tomorrow. That is wrong on Saturday evening for Sunday, the week rollover left open since HW5, and the 01b day-end automatic plan has the same flaw.
- **R2. The anchor chest color was investigated three times, each time only treating symptoms (log and git).**
  - Log 2026-09-26: the diagnostic log was extended with the tile location.
  - Log 2026-09-26: picking up a chest loses its color, so a Warnings-tab note was added.
  - `c5c71ab` (HW4 spike): the menu does not reset the color; the hypothesis was rejected.
  - `ffac39f` and `3f6f2f6` (01c, 01d): re-checks and warnings in every place where the marking can break.
  - What is wrong underneath: the anchor is defined only by the chest's color, re-read live everywhere, with no stored flag. So it is lost on pick-up, can be ambiguous, and has to be re-checked in four places.
- **R3. Text or rows that do not fit, fixed three times in three places (log and git).**
  - Log 2026-09-26: truncation and wrapping in the menu.
  - Log 2026-09-26: the paged welcome message.
  - `003b59d` (01e): the scrollable Plan tab.
  - What is wrong underneath: the menu uses a fixed pixel layout with no shared rule for content that does not fit. The Warnings tab (4 wrapped paragraphs plus the red line) and the Settings tab (fixed y=160) can still overflow.
- **R4. The welcome text was extended four times (log and git).**
  - The log: the "next day" emphasis, then paging.
  - `4a873ad`: two pages about the anchor priority and the Settings toggle.
  - `9475ad2` (02d): key names from the config.
  - What is wrong underneath: the text repeats by hand what the code and the spec decide elsewhere, so every behavior change needs a manual text fix.
- **R5. The search box: two fixes, then the approach was replaced (log only).** The `TextBox` failed after two fix attempts, and it was replaced by the A-Z letter filter. Here the base was replaced, not patched.
- **R6. The plan lifetime: solved at the root (visible in git).** The HW5 real-use workarounds (toggle off and on, reopen, the `Delivered` flag) were symptoms of plans having no day. This was fixed at the root in `88c696d` (01b).
- **Process note.** Because `af02acc` is one large commit, git cannot show the 2026-09-26 repeats (R2 part 1, R3 part 1, R5). Since HW5, commits are small and step by step, so git now shows repeats.

## What we decided

| Id | Decision | Step |
|---|---|---|
| R1 | Fix, without touching `GiftLimitPolicy.cs` lines 21 and 29 (the HW5 red-green checks rely on them). A new pure function only adjusts the counts: if the target day starts a new week, the weekly count is 0; if the target day is not today, the daily count is 0. It then calls the existing `CanGiveGift`. Planning and the day-end automatic plan use it. The expected value's source (the Stardew Valley Wiki Friendship page, "the week begins on Sunday") is recorded in its own commit before the tests are written. | 03b, 03c |
| R2 | Keep on purpose. The HW4 spike showed the color survives reopening the menu and saving and loading; the Warnings tab warns about losing it on pick-up; and a stored anchor would need new spec behavior. | 03a |
| R3 | Fix: the Warnings and Settings tabs measure, wrap, and scroll when needed, built on `ListScroll`, with the testable part in `Core/`. | 03d |
| R4 | Fix: every value that comes from a setting is passed into `WelcomeText` as a parameter, with a test per page against the spec content. | 03e |
| R5 | Record, as a good example: when patches did not hold, the approach itself was replaced. | 03a |
| R6 | Record: solved at the root in 01b. | 03a |

## What we changed (step 03a: record R2, R5, R6)

- This file, and one line in `PLANNING_LOG.md` with the decisions for R1-R6.
- No code or spec change.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 50/50 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 03b: R1 expected-value source)

- `reviews/03-week-rollover-source.md` (new), committed before any R1 test:
  - the Stardew Valley Wiki Friendship quote "the week begins on Sunday";
  - the weekday and week formula on SMAPI's day count;
  - the three expected values;
  - what is not covered.

## What we changed (step 03c: R1 tests and implementation)

Spec, written before the code: `gift-selection`, "Gift-limit state respected during selection".
- The weekly limit is judged for the plan's target day. Weeks begin on Sunday, so planning on Saturday evening for Sunday treats the weekly count as 0.
- This applies to planning from the menu and to the day-end automatic plan.
- Two new scenarios: Saturday evening for Sunday, and a weekday for the next day.

Tests, written before the implementation: `StardewAutoGifter.Tests/TargetDayGiftLimitTests.cs`, 3 tests, using exactly the three expected values from `reviews/03-week-rollover-source.md`:
- day 6 → 7 with week count 2: true;
- day 2 → 3 with week count 2: false;
- day 3 → 3 with today count 1: false.

Code:
- `Core/TargetDayGiftLimit.cs` (new, pure): `CanGiveGiftOn(targetDay, today, giftsToday, giftsThisWeek)`.
  - The daily count becomes 0 unless the target day is today.
  - The weekly count becomes 0 when `StartsNewWeek(targetDay, today)`, that is, when `WeekOf(day) = day / 7` differs.
  - It then calls the unchanged `GiftLimitPolicy.CanGiveGift`.
- `GiftSelection/FriendshipGiftTracker.cs`: new `CanGiveGiftOn(npcName, targetDay, today)`, reading the game's counts.
- `Planning/PlanComputer.cs`: takes `today` as well, and uses `CanGiveGiftOn` instead of `CanPlanGiftForNextDay`.
- `ModEntry.cs`: `ComputeNextPlan` passes `Today`. The menu and the day-end automatic plan both go through it.
- `GiftLimitPolicy.cs` is not touched. Lines 21 (`CanGiveGift`) and 29 (`CanPlanGiftForNextDay`) are unchanged, so the HW5 red-green checks still rely on them. `CanPlanGiftForNextDay` is now used only by its HW5 tests and by `FriendshipGiftTracker.CanPlanGiftForNextDay`, which nothing calls any more. This is noted for review 04.
- Delivery is unchanged: it still checks today's counts through `CanGiveGift`.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 53/53 green (50 + 3).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged, and the file itself is unchanged.

## What we changed (step 03d: R3, Warnings and Settings text fits the menu)

Spec, written before the code: new requirement `daily-plan-review`, "Warnings and Settings text fits the menu".
- Text wraps only between words, and a word wider than a line gets a line of its own.
- Paragraphs are separated by one empty line.
- When the text is taller than the menu, the tab scrolls one line per wheel step, with the Plan tab's limits.
- No text is drawn outside the menu.
- On the Settings tab, only the setting's own lines toggle it, at the current scroll position.
- Four scenarios with concrete values.

Tests, written before the implementation: `StardewAutoGifter.Tests/TextLayoutTests.cs`, 4 tests, with the values from the scenarios. Each character is measured as 1 unit wide.
- "aaa bbb ccc" with room for 7 → "aaa bbb", "ccc";
- "abcdef gh" with room for 3 → "abcdef", "gh";
- the paragraphs "aa" and "bb" → "aa", "", "bb";
- 20 lines, 16 fit, top line 4, scroll down → stays at 4.

Code:
- `Core/TextLayout.cs` (new, pure): `Wrap(text, measure, maxWidth)` and `Paragraphs(paragraphs, measure, maxWidth)`. The menu's own `WrapText` now delegates to it, so there is one wrapping rule.
- `Menus/ModMainMenu.cs`:
  - The Warnings and Settings tabs are built as line lists (`BuildWarningsLines`, `BuildSettingsLines`). Only the lines that fit are drawn (`DrawPanelLines`, 16 lines from y+96, line height 28).
  - Both tabs scroll with the mouse wheel through `ListScroll.Scroll`.
  - The Settings click is a hit test on the scrolled line, true only for the setting's own lines. It replaces the fixed `_anchorChestOnlyRowBounds`.
- Two small visual changes come from the spec's "one empty line" between paragraphs:
  - the gap between Warnings paragraphs grows from 12 px to one line (28 px), and before the red line from 20 px to 28 px;
  - the Settings explanation starts at y+152 instead of y+160.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 57/57 green (53 + 4).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 03e: R4, setting-derived values in the welcome text)

Which welcome-text values come from a setting:
- the two keys (`MenuKey` and `ChestMarkKey` in `config.json`), already parameters since 02d;
- the save's "anchor chest only" setting, which pages 4 and 5 (the fallback pages) assumed was off.

That setting is stored per save and can already be on at the first farm exit, if the player turns it on from F8 inside the farmhouse. `IncludeLikedItems` is not mentioned in the welcome text, so it needs no parameter.

Spec, written before the code: `gift-selection`, "Starter chest and welcome message on first farm exit".
- Every setting-dependent part follows the setting's current value.
- New scenario "Welcome pages with the default settings": it lists all 8 pages exactly, in order.
- New scenario "Welcome pages when "anchor chest only" is already on": it gives new texts for pages 4 and 5. Their wording is the assistant's and still needs the user's approval.

Tests, written before the implementation: `StardewAutoGifter.Tests/WelcomeTextTests.cs`.
- A `[Theory]` with one case per page (8 tests) against the default-settings scenario.
- 2 tests for the "anchor chest only" variant: page 4, then page 5 plus the other pages unchanged.
- The two 02d key-name tests now pass `anchorChestOnly: false`, with unchanged expected values.

Code:
- `Core/WelcomeText.cs`: `Pages(menuKey, chestMarkKey, anchorChestOnly)`. Pages 4 and 5 depend on `anchorChestOnly`.
- `ModEntry.cs` passes `_settingsStore.AnchorChestOnly`.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 67/67 green (57 + 10).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## In-game checklist (C4, R1, R3)

Not run yet: the user runs it in-game.

C4 (remapped keys):
1. With the game closed, set `"ChestMarkKey": "K"` and `"MenuKey": "F9"` in `D:\SteamLibrary\steamapps\common\Stardew Valley\Mods\StardewAutoGifter\config.json`.
2. Start the game and load a save. Expect F9 to open the menu and F8 to do nothing. K marks a chest and G does nothing.
3. Unmark all chests, then open F9 → Plan tab. Expect "Mod inactive: open a chest on your farm and press K to mark it.".
4. Mark two chests with K. Expect the HUD message "… Unmark all but one with K." and the Plan tab saying "… press K to unmark it until only one remains.". Afterwards, unmark one of them.
5. Start a new save, which also checks R4. At the first farm exit, expect "press K" on page 3 and "Press F9" on page 6. Before leaving the farmhouse in a second new save, turn on "Only use the marked chest" in F9 → Settings. Expect pages 4 and 5 to show the "ONLY source" and "turn that fallback back on" texts.
6. Set both keys back to "G" and "F8".

R1 (Saturday evening plan for Sunday):
7. Pick a villager (for example Haley). By Saturday, she must have received 2 gifts this week: both Social-tab boxes are checked.
8. Saturday evening: open F8 → Plan tab. Expect `[x] Haley -> …`, not "already reached this week's gift limit", because the plan is for Sunday, a new week.
9. As a check of the other direction, do the same on a weekday evening with 2 gifts already this week. Expect "already reached this week's gift limit".
10. Sleep on Saturday. On Sunday, walk past Haley. Expect "Gave … to Haley.". On the Social tab, one box is checked again for the new week.
11. Optional, the day-end plan: on another Saturday with 2 gifts, do not open F8 that day. Sleep, and on Sunday walk past her. Expect "Gave … to Haley.".

R3 (Warnings and Settings tabs, worst case):
12. Mark two chests, so the red active-warning line is shown, and open F8 → Warnings. Expect no line outside the menu border. If the text is longer than the menu, the mouse wheel scrolls it one line at a time and stops at both ends. The red line is at the end.
13. Open the Settings tab. Click the setting's text. Expect `[x]`/`[ ]` to toggle. Click the gray explanation text. Expect nothing to change.
14. If the Settings or Warnings text does not need scrolling, expect the wheel to do nothing there.

**In-game status (2026-10-17):** not run, in-game verification pending: C4, R1 (Saturday-evening plan for Sunday), R3 (Warnings and Settings tabs) and the R4 welcome-page variants. The homework does not require running it.
