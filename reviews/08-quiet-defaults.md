# HW6 review 08: quiet defaults

## What we looked for

Places where a missing or invalid value quietly becomes a default instead of raising an error:
- `??` defaults, `GetValueOrDefault`, and defaults after `TryGetValue`;
- the defaults of the config fields: what happens when a `config.json` field is missing or invalid, for example an unknown key name;
- missing fields in the saved data.

Points that overlap review 05 are marked **[05: E…]**.

We reviewed the whole of `StardewAutoGifter/` as of commit `548ec73`: every `??`, `GetValueOrDefault`, `TryGetValue`, `TryParse`, `default(…)`, `allowNull` and property initializer, plus the config and save-data classes. `GetValueOrDefault`, `TryParse` and `default(…)` do not appear anywhere.

How SMAPI reads the files was checked in SMAPI's source (github.com/Pathoschild/SMAPI, `develop` branch):
- `ModHelper.ReadConfig` is `this.Data.ReadJsonFile<TConfig>("config.json") ?? new TConfig();` followed by `this.WriteConfig(config); // create file or fill in missing fields`.
- `JsonHelper.CreateDefaultSettings` uses `ObjectCreationHandling.Replace` and the converters `SemanticVersionConverter` and `StringEnumConverter`, with no `MissingMemberHandling`.
- `JsonHelper.ReadJsonFileIfExists` turns a deserialization failure into a `JsonReaderException`: "Can't parse JSON file at {path}. … Technical details: …".

## What we found

**A) config.json (`ModConfig.cs:9-15`)**
- **A1.** A missing field gets the C# default (`F8`, `G`, `true`), and SMAPI writes the file back with the default filled in.
- **A2.** An unknown key name (for example `"MenuKey": "Foo"`), or an invalid `IncludeLikedItems`, makes `StringEnumConverter` fail. SMAPI reports "Can't parse JSON file at …". Because `ReadConfig` is the first line of `Entry`, no event handler is registered, so the mod stays completely silent in the game: no menu, no gifts.
- **A3.** A number for a key (for example `"MenuKey": 9999`) is accepted by Newtonsoft's `StringEnumConverter` by default (not tested in-game). It becomes an `SButton` value that does not exist, so the key silently never works.
- **A4.** `"None"` is a valid `SButton`, but it can never be pressed, so the feature is silently off.
- **A5.** If `MenuKey == ChestMarkKey`, `OnButtonPressed` handles the menu first and returns (`ModEntry.cs:177-185`), so marking a chest never happens.

**B) Saved data.** It is read with the same JSON settings: missing fields get C# defaults, and an unknown enum name throws.
- **B1.** A stored plan without `TargetDay` gets 0. `IsPlanForToday(0, today)` is false, so the plan is silently dropped: no delivery that day and no log line.
- **B2.** A row without `Excluded` gets `false`, so a toggled-off NPC would be gifted. This only happens with a damaged or hand-edited save.
- **B3.** A row without `Status` gets 0, which is `Planned`. Without an item, E4 catches it; with an item, it loads as a normal planned row. **[05: E4]**
- **B4.** A row without `NpcName` gets `""`. `getCharacterFromName("")` is null, so the row never resolves, silently.
- **B5.** An unknown status name (for example a newer `"NpcNotFound"` read by an older build) throws in `SaveLoaded`. SMAPI logs it, but `PromotePlanForToday` does not run.
- **B6.** A JSON `null` for a collection (`"BlockedItemIds": null`, `"SelectedNpcNames": null`, `"Gifts": null`) replaces the default `new()`, because of `ObjectCreationHandling.Replace`, and later causes a `NullReferenceException`.
- **B7.** A missing `HasShownWelcome` gets `false`, so the welcome and the starter chest come again.
- **B8.** The stores' `?? new …` is the normal case of first use. **[05: E8]**

**C) `??`, `TryGetValue` and `allowNull` in the code**
- **C1.** With no friendship data, the gift-limit check says a gift can be given (`FriendshipGiftTracker.cs:21-22`). **[05: E5]**
- **C2.** An item that cannot be created gets the taste `Neutral` (`NpcGiftTasteProvider.cs:15-17`), so it is silently "not eligible". **[05: E3]**: the missing-NPC case has been handled separately since 05d.
- **C3.** The raw item id is shown as the name (`ModMainMenu.cs:289-290`). **[05: E8]**
- **C4.** The internal NPC name is shown when there is no display name (`ModMainMenu.cs:282`).
- **C5.** The Plan-tab fallback `?? InactivePlanMessage(0)` (`ModMainMenu.cs:261`) is unreachable today, because the plan and the count come from the same snapshot.
- **C6.** These need nothing:
  - `FarmItemPool.cs:26` uses `TryGetValue` for a starting sum of 0;
  - `ModEntry.cs:112`, `?.IsActive ?? false`, means "no plan, so not active";
  - `ProximityDeliveryService.cs:109`, `allowNull`, writes a Warn line since 05b **[05: E1]**;
  - `ItemCatalog.cs:19` **[05: E8]**;
  - the `?? "null"` inside log texts.

## What we decided

| Id | Decision | Step |
|---|---|---|
| A1 | Keep on purpose. This is normal SMAPI behavior, and the filled-in default is visible in the rewritten file. | 08a |
| A2 | Keep the loud error. The README says that if the mod does not react, look for the config.json error in the SMAPI console. | 08a |
| A3, A4, A5 | One pure Core function checks the loaded keys and returns warnings. A key that does not exist (A3) gets a Warn and the default. `None` (A4) gets a Warn, but `None` stays, as a way to turn the key off. Equal keys (A5) get a Warn. Unit tests, and `Entry` logs the warnings. | 08b |
| B1, B4 | A warning in `ToPlan` for `TargetDay < 1` and for an empty `NpcName`, following the E4 pattern, with tests. | 08c |
| B6 | In `Load`, `?? new()` for every collection, with a Warn line. | 08c |
| B3 | No change to the enum. Adding a first `Unknown` value would change the enum's order, which is a risk if any stored value or code relies on the numbers, and E4 already covers the planned-without-item case. | 08a |
| B2, B5, B7, B8, C5 | Keep on purpose. B2 and B7 only happen with damaged or hand-edited saves, and the mod always writes these fields. B5 is loud, and it only happens after a downgrade. B8 is the normal first-use case. C5 is unreachable while the plan and the count come from the same snapshot. | 08a |
| C2 | A Trace log line when an item cannot be created. | 08d |
| C1, C3, C4, C6 | Already recorded: C1 is review 05, E5 (on the in-game checklist); C3 and C6 are review 05, E8 and E1; C4 shows the internal name, visibly, for the "not found" rows. | 08a |

## What we changed (step 08a: records, and the A2 README sentence)

- `README.md`, section 6: a new point. If the mod does nothing in the game, look for a `config.json` error in the SMAPI console or `SMAPI-latest.txt`. An invalid value, such as an unknown key name, makes SMAPI report "Can't parse JSON file at …", and the mod stays inactive until the file is fixed.
- This file and one line in PLANNING_LOG.md record A1, A2, B2, B3, B5, B7, B8, C1, C3, C4, C5 and C6.
- No code or spec change.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, 0 warnings.
- `dotnet test`: 80/80 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 08b: A3, A4, A5)

Spec, written before the code: new requirement `daily-plan-review`, "Key settings checked when the mod starts".
- The exact warning texts for a key that is not a real key (it falls back to the default), for a key set to None (it stays None and is reported as turned off), and for both keys being the same key other than None, judged after any default.
- Five scenarios: menu key 9999, chest key None, both K, 9999 whose default F8 collides with chest key F8, and valid keys.

Tests, written before the implementation: `StardewAutoGifter.Tests/KeyConfigCheckTests.cs`, 5 tests, with the scenarios' exact texts. They use a fake key table with SButton's own numbers: None 0, G 71, K 75, F8 119.

Code:
- `Core/KeyConfigCheck.cs` (new, pure): `Validate(menuKey, chestMarkKey, isRealKey, noneKey, defaultMenuKey, defaultChestMarkKey, keyName)` returns `KeyCheckResult(MenuKey, ChestMarkKey, Warnings)`.
  - Keys are numbers, because the test project compiles only `Core/` and cannot see SMAPI's `SButton`.
- `ModEntry.cs`: `CheckConfiguredKeys()` runs right after `ReadConfig`. It passes `Enum.IsDefined(typeof(SButton), …)`, `SButton.None`, the `ModConfig` defaults and `SButton.ToString()`, applies the checked keys, and logs each warning at Warn.
- The fallback to the default is in memory only. `config.json` is not rewritten, so the bad value stays visible in the file until the player fixes it.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build --no-incremental`: 0 errors, 0 warnings.
- `dotnet test`: 85/85 green (80 + 5).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## In-game checklist additions (A3, A4, A5)

These come on top of the joint checklist in reviews 03, 04 and 05. Not run yet: the user runs it in-game.

Edit `D:\SteamLibrary\steamapps\common\Stardew Valley\Mods\StardewAutoGifter\config.json` with the game closed. Start the game each time, and read the SMAPI console (or `SMAPI-latest.txt`) right after the mod loads.

A3 (a number that names no key):
1. Set `"MenuKey": 9999`, without quotes, and keep `"ChestMarkKey": "G"`.
2. Expect the Warn line `config.json: MenuKey is set to 9999, which is not a key; using the default F8 instead.`
3. In the game, F8 opens the menu.
4. If SMAPI instead reports "Can't parse JSON file at …", the number is not accepted at all. That is A2's loud error, and the A3 assumption ("Newtonsoft accepts numbers") was wrong. Note which one happened.

A4 (None):
5. Set `"MenuKey": "F8"` and `"ChestMarkKey": "None"`.
6. Expect `config.json: ChestMarkKey is set to None, so chest marking is turned off.`
7. In the game, pressing G in a chest does nothing.

A5 (the same key twice):
8. Set `"MenuKey": "K"` and `"ChestMarkKey": "K"`.
9. Expect `config.json: MenuKey and ChestMarkKey are both K; that key opens the mod menu, so chest marking will not work.`
10. In the game, K opens the menu. With a chest open, K does not mark it.

Afterwards, set the keys back to `"F8"` and `"G"`, and check that no key warning is logged.
