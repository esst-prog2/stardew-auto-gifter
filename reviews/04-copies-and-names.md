# HW6 review 04: copies instead of changes, and misleading names

## What we looked for

Two units doing the same job (often a copy kept instead of changing the original), and names that promise something other than what the code does ("a copy instead of a change").

We reviewed the whole of `StardewAutoGifter/` and `StardewAutoGifter.Tests/` as of commit `63205fe`, not a diff.

## What we found

Two units for the same job:
- **D1.** The planning limit exists twice: `GiftLimitPolicy.CanPlanGiftForNextDay` and `FriendshipGiftTracker.CanPlanGiftForNextDay`, the old ones, next to `TargetDayGiftLimit.CanGiveGiftOn`, the new one since 03c.
  - The tracker method is called by nothing.
  - The policy method is called only by the HW5 tests.
  - The old name promises "can plan for the next day", which is wrong on Saturday.
  - The doc comments at `GiftLimitPolicy.cs:16-17` and `:24-25` still said that planning used it.
- **D2.** Two "can I give today" checks: delivery used `HasReachedWeeklyCap` → `CanGiveGift`, while `CanGiveGiftOn(today, today)` gives the same answer.
- **D3.** The anchor state was computed separately in many places (`FindAnchorChest`, `GetMarkedChests().Count`, `AnchorRule.IsActive`). Two of them scanned the chests twice in one call: `ModEntry.ComputeNextPlan`, and one `CheckAndDeliver` together with its `TryDeliver`.
- **D4.** Five near-identical store classes (`BlocklistStore`, `ConfiguredNpcStore`, `OnboardingStore`, `ModSettingsStore`, `PlanStore`), each "mirrors BlocklistStore".
- **D5.** Settings live in two places: `IncludeLikedItems` in `config.json`, which the menu does not show, and `AnchorChestOnly` per save, on the Settings tab.
- **D6.** The delivery loop repeated the same proximity check twice (`ProximityDeliveryService.cs:33-60`).

Misleading names:
- **D7.** `HasReachedWeeklyCap` is also true when only the daily limit is reached; HW5 changed what it does, and the name stayed.
- **D8.** `PlannedGift.Delivered` means "resolved": it is also set when a delivery was skipped or only a notice was shown.
- **D9.** `GiftSelector`'s parameter `farmWidePool` also receives the anchor-only pool, and its doc said "summed across all farm chests".
- **D10.** `ItemCatalog.BuildFullCatalog` returns only `(O)` items, and the spec requirement title still said "full item catalog".
- **D11.** `MarkingConflict` also holds the "no chest marked" text, which is not a conflict.
- **D12.** `ModMainMenu` kept `_hasMarkingConflict` next to `_markedChestCount`, the same fact twice.

Intentionally two units:
- The minimum stock is checked both at selection and live at delivery. `design.md` says this is on purpose ("check it twice"), and since 02b both use one constant.
- The `[hw4-spike]` diagnostic, kept on purpose as recorded in the log.

## What we decided

| Id | Decision | Step |
|---|---|---|
| D1 | Delete the unused `FriendshipGiftTracker.CanPlanGiftForNextDay`. `GiftLimitPolicy.CanPlanGiftForNextDay` stays for the HW5 tests, and its comments say so. Lines 21 and 29 and their numbers must not change. | 04a |
| D2 + D7 | Delivery also calls `CanGiveGiftOn(Today, Today)`, and `HasReachedWeeklyCap` goes away. No behavior change. | 04b |
| D3 | An `AnchorState` snapshot from a single scan, used everywhere. | 04c |
| D4 | A generic `SaveDataStore<T>`. The save keys `blocklist`, `configured-npcs`, `onboarding`, `settings` and `next-plan` must not change. | 04f |
| D5 | `IncludeLikedItems` stays in `config.json`. The Settings tab says in one sentence that the liked-items fallback is set in `config.json`. The spec is updated. | 04g |
| D6 | A shared `IsInRange` helper. | 04d |
| D8-D12 | `Delivered` → `Resolved`; `farmWidePool` → `pool`, with a doc that describes the anchor priority; `BuildFullCatalog` → `BuildGiftableCatalog`, with the spec requirement title; `MarkingConflict` → `AnchorStatusText`; only the count is kept. | 04e |

Where behavior must not change, the existing tests do not change. If a rename forces a test edit, the expected values stay word for word.

## What we changed (step 04a: D1)

- `GiftSelection/FriendshipGiftTracker.cs`: the unused `CanPlanGiftForNextDay(string npcName)` is deleted.
- `Core/GiftLimitPolicy.cs`, comments only, with the same number of lines:
  - Lines 16-18 now say that delivery checks `CanGiveGift` directly, and that planning reaches it through `TargetDayGiftLimit.CanGiveGiftOn`. Line 16 was stale too ("Used by the delivery path only"), so it changed along with the requested line 17.
  - Lines 24-26 now say that `CanPlanGiftForNextDay` is kept only for the HW5 tests, nothing in the mod calls it any more, planning uses `TargetDayGiftLimit.CanGiveGiftOn` instead, and it should not be used in new code.
  - The diff touches only lines 16-18 and 24-26. Lines 21 and 29 and their numbers are unchanged.
- Tests unchanged.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 67/67 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.
