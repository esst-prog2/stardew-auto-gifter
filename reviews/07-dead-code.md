# HW6 review 07: dead code

## What we looked for

Dead code, across the whole of `StardewAutoGifter/` and `StardewAutoGifter.Tests/`:
- functions, classes, fields, constants, parameters and `using`s that nothing calls or uses;
- code put into comments;
- remains of the TextBox search box;
- parts left behind by the renames and merges of 04a-04g;
- config or spec elements that the code no longer reads.

How we searched, as of commit `d250a52`:
- **The compiler's own rules.** `dotnet format style --verify-no-changes` (check only, nothing changed) ran IDE0005 (unused using), IDE0051/IDE0052 (unused private member), IDE0060 (unused parameter) and IDE0059 (unused value) on both projects.
- **A script.** It counted every declared type, method, property, field and constant name in the comment-free code, separately for the main project, the declaring file and the tests.
- **Text searches.** For commented-out code, `TextBox`/`keyboardDispatcher`/`Options*`/`ClickableComponent`, the old names from 04a-04g, and the config fields against `ModConfig`.

## What we found

Code:
- **K1.** `DailyPlan.FindFor` (`Core/DailyPlan.cs:22`) was used only by the tests: 0 calls in the main project, 8 in the tests.
- **K2.** `GiftLimitPolicy.CanPlanGiftForNextDay` (`Core/GiftLimitPolicy.cs:28-29`) is called by nothing in the mod, only by the two HW5 tests, and its `giftsToday` parameter is unused (IDE0060, line 28). It is kept on purpose since 04a, and its comment at lines 24-26 says so.
- **K3.** Public members used only in their own file:
  - `FarmChestScanner.AnchorChestName` (`GiftSelection/FarmChestScanner.cs:90`);
  - `TargetDayGiftLimit.WeekOf` and `StartsNewWeek` (`Core/TargetDayGiftLimit.cs:25,28`).
- **K4.** `GiftTaste.Dislike` and `GiftTaste.Hate` (`Core/GiftTaste.cs:9-10`) are produced by `NpcGiftTasteProvider` but never read, because selection only looks at `Love` and `Like`.

Not found in the code:
- unused `using`s;
- unused private members or parameters, except K2;
- commented-out code (the three candidates are explanatory comments);
- TextBox remains;
- leftovers of the 04a-04g names;
- config fields the code does not read: `config.json` has exactly `MenuKey`, `ChestMarkKey` and `IncludeLikedItems`, the three `ModConfig` properties.

Documents:
- **S1.** `design.md:51-53` still describes the removed TextBox search field, without saying that `design.md:104` superseded it.
- **S2.** `design.md:157-161`, "Selection UI: build on the game's own menu option elements". The code is a custom-drawn `IClickableMenu` that uses none of the game's option or text-input elements, so the alternative the section rejects is the one that was built.
- **S3.** `design.md:171` mentions "search + per-row checkbox".
- **S4.** `design.md:72` cites `Game1.drawDialogue(NPC, string)`, which does not exist in 1.6. The code pushes a `Dialogue` onto the NPC's stack and calls `Game1.drawDialogue(npc)`.
- **S5.** `Delivered`, renamed `Resolved` in 04e, remains in `design.md:74`, `design.md:144` and `tasks.md:67` (task 6.15).
- **S6.** `proposal.md:21` says "searchable", and `proposal.md:11` "full item catalog".
- **S7.** `tasks.md` already marks 3.5 and 6.1 as superseded by 6.6. Task 1.2 strikes through the old config NPC list. Task 6.4 (remove `ConfiguredNpcNames` from `ModConfig`) is done in code, with no trace of the field left, and is only waiting for in-game verification. An old `config.json` that still has that field is harmless, because SMAPI ignores unknown fields.

## What we decided

| Id | Decision | Step |
|---|---|---|
| K1 | Move `DailyPlan.FindFor` into the test project as a helper. The tests' expected values stay word for word. | 07a |
| K2 | Keep on purpose: the HW5 red-green checks rely on it. | recorded in 07a |
| K3 | `AnchorChestName` becomes `private`. `WeekOf` and `StartsNewWeek` stay public, as part of the week formula. | 07a |
| K4 | Keep on purpose: the enum mirrors the game's taste tiers. Without `Dislike` and `Hate`, those tiers would merge into `Neutral`. | recorded in 07a |
| S1-S3 | The old `design.md` sections get a "Superseded by …" note and one sentence on what they became (custom-drawn rows, `ListScroll`, `TextLayout`, the letter filter). The old text is not deleted. | 07b |
| S4 | Fix the `drawDialogue` reference to the real call. | 07b |
| S5 | `Delivered` → `Resolved` in `design.md`. In `tasks.md` 6.15, only the note "(since 04e: Resolved)" is added. | 07b |
| S6 | `proposal.md` stays the frozen text of the original proposal, with one line at the top saying that `specs/` describes the current behavior. | 07b |
| S7 | Record: nothing to change, only the in-game verification of 6.4 is still open. | recorded in 07a |

## What we changed (step 07a: K1, K3)

- `Core/DailyPlan.cs`: removed `FindFor`.
- `StardewAutoGifter.Tests/DailyPlanTestExtensions.cs` (new): an `internal` extension method `FindFor(this DailyPlan plan, string npcName)` with the same body. The tests still call `plan.FindFor("Haley")`, so no existing test file changed; `git status` showed only the new file in the test project.
- `GiftSelection/FarmChestScanner.cs`: `AnchorChestName` is now `private const`.
- No behavior change.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build --no-incremental`: 0 errors, 0 warnings.
- `dotnet test`: 80/80 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 07b: S1-S6)

All in `openspec/changes/add-auto-gifter-mvp/`. No old text was deleted.
- **S1.** `design.md`, "TextBox search field: explicit keyboard-dispatcher subscription": a note "Superseded by "Letter-filter checkboxes replace free-text search" (below)". It says the TextBox field was removed, and the blocklist editor and the NPCs tab filter with A-Z letter boxes.
- **S2.** `design.md`, "Selection UI: build on the game's own menu option elements": a note "Superseded by what was built". It says the menu is a custom-drawn `IClickableMenu`:
  - rows drawn with `drawTextureBox`/`DrawString` and hit-tested by hand;
  - `Core/ListScroll` for scrolling and `Core/TextLayout` for wrapping;
  - the A-Z letter filter instead of a search field;
  - none of the game's option-row or text-input elements.
- **S3.** `design.md`, the blocklist-editor risk: a "Superseded" sub-point says there is no search field and no option-row elements, and points to "Selection UI".
- **S4.** `design.md`, "In-person "nothing to give" notice": the call is now given as it really is, `npc.CurrentDialogue.Push(new Dialogue(npc, null, message))` then `Game1.drawDialogue(npc)`, noting that there is no `Game1.drawDialogue(NPC, string)` overload in 1.6.
- **S5.** `design.md`: `PlannedGift.Delivered` → `Resolved` in the same section, with "(called `Delivered` until HW6 review 04e)" at its first mention, and in the "Why reuse …" point and the target-day note. `tasks.md` 6.15: only "(since 04e: Resolved)" after `Delivered`. The rest of the task text is unchanged.
- **S6.** `proposal.md`: one line at the top saying it is the frozen text of the original proposal and that the specs in `specs/` describe the current behavior. The proposal text itself is unchanged.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, 0 warnings.
- `dotnet test`: 80/80 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.
