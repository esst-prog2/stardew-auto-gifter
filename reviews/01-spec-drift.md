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
