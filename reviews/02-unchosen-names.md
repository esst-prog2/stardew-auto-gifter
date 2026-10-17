# HW6 review 02: unchosen names and constants

## What we looked for

Names and constants in the code that nobody recorded a decision for: they are neither in the spec nor in PLANNING_LOG.md, so the user never chose them. This covers number values, timings, distances, defaults, colors, texts, and file and folder names.

We reviewed the whole repository, not a diff:
- every string literal, constant, number, color and name in `StardewAutoGifter/`, plus `manifest.json`;
- each one was looked up in the three specs, `design.md`, and `PLANNING_LOG.md`.

Line numbers refer to the code at commit `549e3a7`.

## What we found

Already decided by the user (nothing to do):
- `F8` and `G` keys;
- the sentinel color `Color(13, 202, 91)`;
- the roughly once-per-second check;
- at least 2 units;
- the weekly cap `2` and the daily `< 1`;
- `IncludeLikedItems = true`;
- the "Tomorrow's Plan" label;
- the inactive, paused and conflict texts (01d);
- `(O)` items only (01a);
- SMAPI day numbering (01b);
- the `hw5/`, `spike/` and `reviews/` folders.

Chosen by the assistant and not recorded anywhere:

| Id | Value | Where |
|---|---|---|
| C1 | Hand-off distance `1.5` tiles (the user decided "about 1-2 tiles") | `Delivery/ProximityDeliveryService.cs:20` |
| C2 | Sparkle every 4 s, 1000 ms long, a particle every 100 ms, white. The conflict check (01d) uses the same 4 s timer, while the spec only said "every few seconds". | `ModEntry.cs:39`, `Visuals/AnchorChestSparkleService.cs:26-28` |
| C3 | Player-facing texts, exact wording (see below) | listed below |
| C4 | "G" and "F8" written into texts, although both keys are configurable. If the keys are changed in `config.json`, the texts name the wrong key. | `Core/MarkingConflict.cs:11,23-24`, `ModEntry.cs:161,164`, spec texts from 01d |
| C5 | Default of the "anchor chest only" setting: `false` | `Settings/ModSettingsData.cs:6` |
| C6 | The "2 units" rule duplicated in two constants and one log text | `Core/GiftSelector.cs:18`, `GiftSelection/FarmItemPool.cs:33`, the "fewer than 2 units" text at `Delivery/ProximityDeliveryService.cs:113` |
| C7 | `manifest.json`: Name `Auto-Gifter`, Version `0.1.0`, the Description text, UniqueID `TamaraT0811.AutoGifter`, MinimumApiVersion `4.0.0`. The log says the target is SMAPI 4.5.x. | `manifest.json` |
| C8 | Save-data keys `blocklist`, `configured-npcs`, `onboarding`, `settings`, `next-plan` | the five store classes |
| C9 | Menu look (see below) | `Menus/ModMainMenu.cs`, `ModEntry.cs:244` |
| C10 | Internal names (see below) | listed below |

C3, the player-facing texts:
- `Gave {item} to {npc}.`;
- the two in-person dialogue lines;
- the two Plan-row reasons;
- "No NPCs configured yet - pick some in the NPCs tab.";
- the two Settings texts;
- the four Warnings paragraphs and the "Active warning: …" line;
- the 8 welcome pages;
- the tab labels;
- `[x]` / `[ ]` and the `...` truncation mark.

C9, the menu look:
- 800×600, centered;
- row 56, tab 145×48, letter box 28, list start 140, bottom margin 40, which gives 7 visible rows;
- paddings 16/32/96/160, line height 28;
- a 50% dark overlay; gray for inactive tabs, letters and rows; red for the active warning;
- the `drumkit6` click sound;
- the conflict HUD message uses the error style.

C10, the internal names:
- the chest name "Auto-Gifter Chest" (the log only gave it as "e.g.") and "Chest" on unmark;
- the starter chest item `(BC)130`;
- the source folder names;
- the review file name `01-spec-drift`;
- the log levels: Trace for a skipped delivery, Debug for diagnostics.

## What we decided

| Id | Decision | Step |
|---|---|---|
| C1 | Approved; written into the spec and the log | 02a |
| C2 | Approved: 4 s, 1000 ms, 100 ms, white, and the conflict check on the same 4 s timer; written into the spec and the log | 02a |
| C3 | The texts are accepted as they are; recorded in the log | 02a |
| C4 | Fix: texts name the keys set in the config. The text building is a pure Core function with unit tests; the spec is updated. | 02d |
| C5 | Off by default, approved; recorded in the log | 02a |
| C6 | One shared Core constant for all three places, with no behavior change | 02b |
| C7 | MinimumApiVersion becomes `4.5.0`. Name, Version, Description and UniqueID stay and are recorded in the log; UniqueID is not touched. | 02c |
| C8, C9, C10 | Stay as they are; recorded in the log. The save-data keys must not change. | 02a |

## What we changed (step 02a: record decisions, no code)

- `proximity-delivery/spec.md`, "Automatic hand-off within a short distance": the distance is 1.5 tiles, measured between the player's tile and the NPC's tile. This is what the code does with `Vector2.Distance(player.Tile, npc.Tile)`.
- `gift-selection/spec.md`, "Anchor chest sparkles periodically for visibility": every 4 seconds; each sparkle lasts 1000 ms, with a particle every 100 ms, in white.
- `daily-plan-review/spec.md`, "Warnings and info panel": the periodic conflict check runs every 4 seconds, on the same timer as the sparkle. It used to say "every few seconds".
- `PLANNING_LOG.md`: one line recording C1, C2, C3, C5, C8, C9 and C10.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 45/45 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 02b: C6, one shared constant)

- `Core/StockRule.cs` (new): `StockRule.MinimumUnitsForGift = 2`.
- `Core/GiftSelector.cs`: the private `MinimumUnitsToBeEligible` is removed, and selection uses `StockRule.MinimumUnitsForGift`.
- `GiftSelection/FarmItemPool.cs`: the private `MinimumUnitsBeforeRemoval` is removed, and the removal check uses `StockRule.MinimumUnitsForGift`.
- `Delivery/ProximityDeliveryService.cs`: the Trace log text reads the number from `StockRule.MinimumUnitsForGift` instead of a written "2". The text it logs is the same.
- No behavior change: the value is still 2 in all three places. The existing selection tests (`GiftSelectorTests`) still pass unchanged.
- `PLANNING_LOG.md`: a line recording that the 02a entry is an exception to the one-line rule, as the user decided (option 1). From now on it is one line per decision again.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 45/45 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 02c: C7, MinimumApiVersion)

- `StardewAutoGifter/manifest.json`: `MinimumApiVersion` changed from `4.0.0` to `4.5.0`, to match the logged target of SMAPI 4.5.x. The installed SMAPI is 4.5.2, so the mod still loads.
- Unchanged, and recorded in the log: Name `Auto-Gifter`, Version `0.1.0`, the Description text, and UniqueID `TamaraT0811.AutoGifter`. UniqueID was not touched, because changing it would lose the mod's saved data.
- The build copied the new manifest into the game's Mods folder; this was checked.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 45/45 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

To check in-game: start the game with SMAPI. In the SMAPI console, "Auto-Gifter 0.1.0" should still be listed among the loaded mods, with no "requires a newer version of SMAPI" error.

## What we changed (step 02d: C4, configured key names in texts)

Spec, written before the code:
- `daily-plan-review`: the inactive and paused Plan-tab messages, and the conflict HUD message, use `<chest-marking key>`. This is the name of the key currently set in the configuration file (G by default), never a fixed key name. The same rule applies to every player-facing text that names `<chest-marking key>` or `<menu key>`. New scenarios cover a chest-marking key set to K.
- `gift-selection`, "Starter chest and welcome message on first farm exit": wherever the welcome message names a key, it uses the configured keys (G and F8 by default). Two new scenarios: the exact default texts, and remapped keys K and F9 with no page naming G or F8.

Tests, written before the implementation:
- `StardewAutoGifter.Tests/MarkingConflictTests.cs`: the existing text tests now pass the key name `"G"` to the new functions. Their expected texts did not change. Three new tests check the same texts with the key set to `"K"`.
- `StardewAutoGifter.Tests/WelcomeTextTests.cs` (new), 2 tests:
  - the default keys give the exact marking and menu pages;
  - K and F9 give "press K" and "Press F9", and no page has "press G" or "F8".

Code:
- `Core/MarkingConflict.cs`: `HudMessage` is now `HudMessage(chestMarkKey)`, and `InactivePlanMessage` also takes `chestMarkKey`.
- `Core/WelcomeText.cs` (new, pure): `Pages(menuKey, chestMarkKey)`. These are the 8 welcome pages, moved out of `ModEntry` and otherwise unchanged.
- `ModEntry.cs` passes `_config.MenuKey.ToString()` and `_config.ChestMarkKey.ToString()` to the welcome text, the HUD message and the menu.
- `Menus/ModMainMenu.cs` takes the chest-marking key name for the Plan-tab message.
- No player-facing string in the code names G or F8 any more. A search for "press G", "with G" and "F8" in string literals finds nothing.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 50/50 green (45 + 5).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

To check in-game:
1. With the game closed, edit `D:\SteamLibrary\steamapps\common\Stardew Valley\Mods\StardewAutoGifter\config.json` and set `"ChestMarkKey": "K"` and `"MenuKey": "F9"`.
2. Start the game. Expect F9 to open the menu and F8 to do nothing.
3. Unmark every chest and open F9 → Plan tab. Expect "… press K to mark it.".
4. Mark two chests with K. Expect the HUD message "… Unmark all but one with K." and the Plan tab saying "… press K to unmark it …".
5. Welcome text: it shows only once per save, so start a new save with the remapped keys. Expect "press K" and "Press F9" on the pages.
6. Afterwards, set both keys back to `"G"` and `"F8"`.

**In-game status (2026-10-17):** not run, in-game verification pending: the 02c load check (MinimumApiVersion) and C4 (remapped keys in texts, 02d). The homework does not require running it.
