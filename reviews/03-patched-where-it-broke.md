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
