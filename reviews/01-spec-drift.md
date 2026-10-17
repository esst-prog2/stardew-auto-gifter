# HW6 review 01: spec drift

## What we looked for

We looked for two kinds of mismatch between the code and the spec (`openspec/changes/add-auto-gifter-mvp/specs`):

- a feature in the code that the spec does not describe;
- a requirement in the spec that the code does not implement, or only partly implements.

We reviewed the whole repository, not a diff: every requirement in all three specs (`daily-plan-review`, `gift-selection`, `proximity-delivery`) against every source file in `StardewAutoGifter/`. We read `design.md` and the "Future work" list in `proposal.md` only to tell whether a mismatch was intentional. `StardewAutoGifter.Tests` and the root files hold no features, so there was nothing in them to compare.

## What we found

Line numbers below refer to the code as it was when the review started (branch `hw6-review`, created from `main` at `4c8dca1`).

### In the spec, but not (fully) implemented

- **A1. The "tomorrow" plan is delivered the same day.** `daily-plan-review/spec.md:15` says the plan is computed for the next day, and `gift-selection/spec.md:146` says automatic gifting only starts the next day. But `ModEntry.cs:139` sets `_currentPlan` and `ModEntry.cs:177-178` delivers from it every second, with no day check. The HW5 real-use run showed it: the delivery path ran on the same day the plan was recomputed.
- **A2. The edited plan is not carried into the next day.** `daily-plan-review/spec.md:103` says the player's edits are not recomputed or discarded. But:
  - every F8 open recomputes the plan and drops toggled-off rows (`ModEntry.cs:114-139`);
  - the plan is lost on save load (`ModEntry.cs:67`);
  - nothing resets `Delivered` when the day changes, so the "once per day" rules (`proximity-delivery/spec.md:54,73`) really mean "once per plan".
- **A3. Delivery runs while the mod is inactive.** `gift-selection/spec.md:8` says the mod only selects and delivers while exactly one chest is marked. But `ProximityDeliveryService.cs:86-91` still delivers when the marking is ambiguous or has been removed since the plan was computed.
- **A4. The marking-conflict warning is not "active".** `daily-plan-review/spec.md:110,117` and `gift-selection/spec.md:60` require an active warning. The only warning is a red line on the Warnings tab (`ModMainMenu.cs:361-368`). For an ambiguous marking the Plan tab says "open a chest… press G to mark it" (`ModMainMenu.cs:266`), which is misleading.
- **A5. The greenhouse is probably left out of the pool (not verified).** `gift-selection/spec.md:67` names the greenhouse, but `FarmChestScanner.cs:25` only reads `building.indoors.Value`. In 1.6 the greenhouse interior is most likely not an instanced building interior, so this value is probably null for it.
- **A6. The blocklist lists only `(O)` items.** `daily-plan-review/spec.md:70,74` says "full catalog of item types", but `ItemCatalog.cs:17` reads only `objectData`. In practice almost only object items can be gifted.
- **A7. The Plan tab does not scroll.** With more than about 7-8 configured NPCs, rows run past the bottom of the menu (`ModMainMenu.cs:276-307`).

### In the code, but not in the spec

- **B1. The daily gift limit at delivery time.** `GiftLimitPolicy.cs:21`, called from `ProximityDeliveryService.cs:76`, enforces the daily limit (the HW5 work). The spec (`proximity-delivery/spec.md:46-51`, `gift-selection/spec.md:156`) mentioned only the weekly limit. It also did not say that planning intentionally ignores the daily limit, or that the skip is silent. Separately, the `CanGiveGift` doc comment (`GiftLimitPolicy.cs:15-18`) was stale: it still said `giftsToday` was unused.
- **B2. Diagnostic logging.** The hw4-spike `OnMenuChanged` handler (`ModEntry.cs:46,53-59`) and the chest-mark Debug log lines (`ModEntry.cs:150,156,162,169`) are not in the spec.
- **B3. Small UI additions.** These are not in the spec:
  - the "No NPCs configured yet" message (`ModMainMenu.cs:272`);
  - the in-person notice is not shown while another menu is open (`ProximityDeliveryService.cs:54`);
  - the Settings/fallback pages in the welcome text.

## What we decided

All points are handled; none is skipped.

| Point | Decision | Step |
|---|---|---|
| B1 | Fix: update the spec to match the tested behavior, and fix the stale comment. | 01a |
| A6 | Fix: correct the spec wording (the blocklist lists giftable object items). | 01a |
| B2 | Keep on purpose: PLANNING_LOG.md records that the spike diagnostic is kept on purpose (spike evidence cites it), and the Debug lines are the same kind of diagnostic. | 01a |
| B3 | Keep on purpose: harmless UI additions that the spec does not forbid. | 01a |
| A1 + A2 | Fix, one shared root cause (the plan has no target day). Options are proposed first; nothing is implemented until the user picks one. | later |
| A3, A4, A7 | Fix. A proposal comes first; nothing is implemented until the user decides. | later |
| A5 | Verify in-game first, then decide. | later |

## What we changed (step 01a)

- `openspec/changes/add-auto-gifter-mvp/specs/proximity-delivery/spec.md`, "Gift-limit re-checked at delivery time":
  - the delivery is also cancelled if the NPC already received a gift today;
  - a delivery cancelled this way shows no confirmation message;
  - this is the only place the daily limit is checked;
  - new scenario: "NPC already received a gift today before delivery happens".
- `openspec/changes/add-auto-gifter-mvp/specs/gift-selection/spec.md`, "Gift-limit state respected during selection":
  - planning intentionally does not exclude an NPC who was gifted today, because the plan is for the next day;
  - new scenario: "NPC gifted today but under the weekly cap".
- `openspec/changes/add-auto-gifter-mvp/specs/daily-plan-review/spec.md`, "Blocklist editor lists the full item catalog": the editor now lists the game's giftable object items, not every item type.
- `StardewAutoGifter/Core/GiftLimitPolicy.cs`: rewrote the stale `CanGiveGift` doc comment. It now says the method enforces both the daily and the weekly limit, is used by delivery only, and that planning uses `CanPlanGiftForNextDay`. The new comment has the same number of lines as the old one on purpose, so the `CanGiveGift` expression stays on line 21 and `CanPlanGiftForNextDay` on line 29. PLANNING_LOG.md and the HW5 PR description cite those line numbers. Comment only, no behavior change.

Checks:
- `openspec validate add-auto-gifter-mvp --strict`: valid.
- `dotnet build` (StardewAutoGifter): 0 errors. The one warning, CS9057 (the ModBuildConfig analyzer's compiler version), also appears without these changes.
- `dotnet test`: 16/16 green.

## Decisions after step 01a

- The user approved keeping the `CanGiveGift` doc comment the same length, so lines 21 and 29 of `GiftLimitPolicy.cs` stay valid.
- A1 + A2: option 1 from the proposal, two plans with a target day, recomputed with toggled-off rows kept. If the player never opens the menu during a day, the mod computes the next day's plan automatically when the day ends.
- A3, A4 and A7: implement as proposed.
- Each fix is its own commit: 01b (A1+A2), 01c (A3), 01d (A4), 01e (A7).

## What we changed (step 01b: A1 + A2, plans carry a target day)

Spec, written before the code:
- `daily-plan-review`, "Plan computed for the next day when the menu is opened":
  - each plan records its target day, which is always the next day;
  - recomputing tomorrow's plan never changes today's plan;
  - reopening the menu recomputes tomorrow's plan but keeps toggled-off rows for NPCs that still have a planned item;
  - four new scenarios.
- `daily-plan-review`, "Edited plan carried into the next day":
  - the plan is stored with the save when the day ends;
  - at the start of a day it becomes that day's plan only if its target day matches;
  - three new scenarios.
- New requirement `daily-plan-review`, "Next day's plan computed automatically at the end of the day": if there is no plan for tomorrow, or only an inactive one, the mod computes one as the day ends. An active plan, with its edits, is kept. Three scenarios.
- New requirement `proximity-delivery`, "Plan delivered only on its target day": only the plan whose target day is today delivers or shows notices, and each day starts with nothing delivered or notified. Three scenarios.
- `design.md`, "Plan computation timing": a short note on the target-day design.

Tests, written before the implementation, with expected values from the scenarios above. The new file is `StardewAutoGifter.Tests/PlanTimingTests.cs`, with 12 tests:
- target day = today + 1;
- whether a plan is for today: a plan for tomorrow, a plan for today, a plan for yesterday, no plan;
- whether to compute at day end: no plan, an active plan, an inactive plan;
- keeping toggled-off rows after recompute: a still-planned NPC, an NPC with no item;
- the save round trip keeps the target day, rows and toggled-off rows;
- the save round trip starts with nothing delivered.

Code:
- Moved `DailyPlan`, `PlannedGift` and `GiftPlanStatus` from `Planning/` to `Core/` (namespace `StardewAutoGifter.Core`), so the test project compiles them. No behavior change.
- `Core/DailyPlan.cs`: new `TargetDay` property.
- `Core/PlanTiming.cs` (new, pure): `NextTargetDay`, `IsPlanForToday`, `ShouldComputeAtDayEnding`, `CarryExclusions`.
- `Core/StoredPlanData.cs` (new, pure): the save form of a plan. It does not store `Delivered`.
- `Planning/PlanStore.cs` (new): reads and writes the next-day plan with SMAPI save data (key `next-plan`).
- `Planning/PlanComputer.cs`: takes the target day.
- `ModEntry.cs`:
  - `_currentPlan` is replaced by `_todaysPlan` (the only plan delivered) and `_nextPlan` (shown and edited in the menu);
  - F8 recomputes only `_nextPlan` and keeps toggled-off rows;
  - `DayEnding` computes `_nextPlan` if needed, then stores it, and logs "No active plan for tomorrow; computed one automatically…" (Debug) when it computes one;
  - `SaveLoaded` and `DayStarted` make the stored plan today's plan when its target day is today. This is safe to run more than once.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 28/28 green (16 + 12).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

To check in-game: see the checklist at the end of this file.

## What we changed (step 01c: A3, delivery paused while inactive)

Spec, written before the code: new requirement `proximity-delivery`, "Delivery paused while the mod is inactive".
- Right before a hand-off or an in-person notice, the mod re-checks that exactly one chest is marked.
- If not, nothing is given or shown, and the gift stays pending for later that day.
- Three scenarios: the anchor is unmarked after planning, two chests are marked, the marking is restored later the same day.

Tests, written before the implementation: `StardewAutoGifter.Tests/AnchorRuleTests.cs`, 3 tests. Expected values come from the "exactly one chest" rule in `gift-selection` and the new scenarios: 0 marked → inactive, 1 → active, 2 → inactive.

Code:
- `Core/AnchorRule.cs` (new, pure): `IsActive(markedChestCount)`.
- `GiftSelection/FarmChestScanner.cs`: `FindAnchorChest` uses the same rule, so planning and the sparkle follow it too. No behavior change there.
- `Delivery/ProximityDeliveryService.cs`: `CheckAndDeliver` checks the rule once an NPC is in range, at most once per call. If the mod is inactive, it returns without giving anything, showing anything, or setting `Delivered`. `TryDeliver` no longer handles the ambiguous case itself, because it can no longer happen there.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 31/31 green (28 + 3).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 01d: A4, active warning and correct Plan-tab text)

Spec, written before the code:
- `daily-plan-review`, "Reason shown for configured NPCs with no gift": the inactive message names the fix for the cause. With no chest marked: "Mod inactive: open a chest on your farm and press G to mark it." With more than one marked: "Mod paused: more than one chest is marked. Open each extra chest and press G to unmark it until only one remains." Three new scenarios.
- `daily-plan-review`, "Warnings and info panel":
  - the active warning is the HUD message "Auto-Gifter paused: more than one chest is marked. Unmark all but one with G.";
  - it is shown once when a conflict starts (detected by the marking key, the menu opening, or the periodic check), not repeated while the conflict lasts, and shown again if the conflict starts again;
  - three new scenarios.

Tests, written before the implementation: `StardewAutoGifter.Tests/MarkingConflictTests.cs`, 8 tests. The expected values, including the exact texts, come from the spec:
- `ShouldNotify` for all four combinations of before and now;
- the HUD text;
- the Plan-tab message for 0, 1 and 2 marked chests.

Code:
- `Core/MarkingConflict.cs` (new, pure): `HudMessage`, `ShouldNotify(wasConflict, isConflict)`, `InactivePlanMessage(markedChestCount)`.
- `ModEntry.cs`:
  - new `_wasMarkingConflict` field, reset on save load;
  - new `CheckMarkingConflict`, which shows the HUD message (error style) only when a conflict starts;
  - it is called after the chest-marking key, when F8 opens, and on the 4-second sparkle check.
- `Menus/ModMainMenu.cs`: takes the marked-chest count instead of a bool, and the Plan tab shows `InactivePlanMessage`. The red line on the Warnings tab is unchanged.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 39/39 green (31 + 8).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 01e: A7, scrollable Plan tab)

Spec, written before the code: `daily-plan-review`, "Per-NPC preview with on/off toggle".
- With more rows than fit, the Plan tab scrolls one row per wheel step.
- It never goes above the first row, never past the point where the last row is visible, and does not scroll when every row fits.
- Rows are never drawn outside the menu, and a click acts on the row shown.
- Six new scenarios with concrete numbers: 20 rows, of which 7 fit.

Tests, written before the implementation: `StardewAutoGifter.Tests/ListScrollTests.cs`, 6 tests, with the expected values from those scenarios:
- scrolling: one step down, the top limit, the bottom limit, and every row fitting;
- clicking: the 7th visible row with row 13 at the top → row 19, and below the last row → no row.

Code:
- `Core/ListScroll.cs` (new, pure): `Scroll(topRow, rowsDown, itemCount, visibleRows)` and `ItemIndexAtRow(topRow, visibleRowIndex, itemCount)`.
- `Menus/ModMainMenu.cs`:
  - new `_planScrollIndex`;
  - the Plan tab draws and handles clicks only on the visible rows, both from the same scroll position;
  - the mouse wheel scrolls the Plan tab;
  - the NPCs and Blocklist tabs now use the same `ListScroll.Scroll` and `VisibleRows` (same results as before, no behavior change there).

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 45/45 green (39 + 6).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## In-game checklist (one session)

Not run yet: the user runs it in-game.

Setup:
- Start on a Monday, so that days 1-4 below stay in the same week.
- Haley and a second villager ("NPC2", who has a planned item, not "nothing to give") both have empty gift boxes this week on the Social tab, and it is not either one's birthday.
- The marked chest holds at least 10 Coconuts and NPC2's item (at least 2 units).
- "Only use the marked chest" is off.

A1 + A2 (target day, edits kept, save/load, automatic plan):
1. Day 1 (Mon), first day with the new build: select Haley and NPC2 on the NPCs tab, then open the Plan tab. Expect `[x] Haley -> Coconut` and `[x] NPC2 -> ...`.
2. Still day 1: walk past Haley within 1.5 tiles. Expect no gift and no "Gave … to Haley." message, and the Coconut count is unchanged, because the plan is for tomorrow.
3. Toggle NPC2 off, close F8, open it again. Expect NPC2 still `[ ]` and Haley still `[x]`. Sleep.
4. Day 2 (Tue): walk past Haley. Expect "Gave Coconut to Haley." and one fewer Coconut. Walk past NPC2. Expect no gift and no dialogue.
5. Day 2 evening: open F8, toggle NPC2 off again (tomorrow's plan is a new plan), sleep, then quit the game after it saves. Restart and load the save.
6. Day 3 (Wed), without opening F8: walk past Haley. Expect "Gave Coconut to Haley.". Walk past NPC2. Expect no gift. Do not open F8 at all on day 3, then sleep.
7. In `SMAPI-latest.txt` for the end of day 3, expect: `No active plan for tomorrow; computed one automatically as the day ended (target day …)`.

A3 + A4 (day 4, Thu, before going near NPC2):
8. Open the marked chest and press G to unmark it. Walk past NPC2 and Haley. Expect no gift and no dialogue.
9. Open F8, Plan tab. Expect "Mod inactive: open a chest on your farm and press G to mark it.".
10. Mark the chest again with G. Then open another farm chest and press G. Expect the HUD message "Auto-Gifter paused: more than one chest is marked. Unmark all but one with G." once. Wait 10+ seconds and open F8. Expect no repeated HUD message, the Plan tab showing "Mod paused: more than one chest is marked. …", and the red line on the Warnings tab.
11. Walk past NPC2. Expect no gift.
12. Unmark the second chest (no HUD message), mark it again (the HUD message appears again), then unmark it again.
13. Walk past NPC2. Expect "Gave … to NPC2.": the paused delivery resumes the same day, and the automatic plan has no toggled-off rows. Walk past Haley. Expect the dialogue "I've already gotten plenty from you this week. Thank you!".

A7 (any day):
14. Select at least 10 villagers and open the Plan tab. Expect at most 7 rows, none outside the menu border.
15. Scroll down to the end. Expect the last villager to be reachable, and scrolling further to do nothing. Scroll up to the top. Expect scrolling further up to do nothing.
16. Scroll down 3 steps and click the top visible `[x]` row. Expect that exact villager (check the name) to become `[ ]`. Close F8 and open it again. Expect it is still `[ ]`. Afterwards, deselect the extra villagers.

A5 (greenhouse; the greenhouse must be repaired):
17. Put a chest in the greenhouse, open it and press G. In the SMAPI console or log, find `Chest-mark key pressed on a Chest at … in Greenhouse; in farm scope: …`. `True` means the greenhouse is in the pool. `False` means it is not. If it was `True`, press G again on that chest to unmark it.
18. Confirmation: move every Coconut out of the other farm chests and put at least 3 into the greenhouse chest. Haley must still be under the weekly cap, which takes a new week after the steps above. Open F8. `[x] Haley -> Coconut` means the greenhouse is in the pool. Any other item, or "nothing to give", means it is not.
