# HW6 review 05: swallowed errors

## What we looked for

Swallowed errors:
- `catch (Exception)`;
- empty `catch` blocks;
- a `return null`, a silent `return`, or a default value right after something failed;
- ignored results.

We reviewed the whole of `StardewAutoGifter/` as of commit `e995503`, not a diff. We searched for `catch`, `try`, `throw`, `allowNull`, `?? <default>`, `return null/false/true/;` and `TryGetValue`, and read each hit.

## What we found

There is no `try`/`catch` anywhere in the code, neither empty nor `catch (Exception)`. An exception in a SMAPI event handler is caught and logged by SMAPI, so it is not swallowed, but the handler stops where it failed. The findings are therefore silent defaults and ignored results:

- **E1.** Delivery removes the item from the chest first, and only then creates the gift object (`ItemRegistry.Create(…, allowNull: true)`). If that gives null or not an `SObject`, `receiveGift` is skipped, but the gift is still marked resolved and the "Gave … to …" HUD message is still shown. The item is lost, with a false confirmation (`Delivery/ProximityDeliveryService.cs:115-133`).
- **E2.** The result of `addItemToInventory` for the starter chest is ignored. With a full backpack the chest can be lost, while the welcome text still says it was added and the welcome is marked as shown, so it never comes back (`ModEntry.cs:152-160`).
- **E3.** A configured NPC who is not found in the game makes every taste `Neutral` (`GiftSelection/NpcGiftTasteProvider.cs:11-17`). The Plan tab then gives a false reason: "nothing to give".
- **E4.** A stored plan row that is `Planned` but has no item id silently becomes `NoEligibleItem` (`Core/StoredPlanData.cs:39`), which hides the damage.
- **E5.** With no friendship data, the gift-limit check answers "can give" (`GiftSelection/FriendshipGiftTracker.cs:21-22`). This is probably right, but undocumented.
- **E6.** A delivery skipped because of the daily limit is silent: no log line (`Delivery/ProximityDeliveryService.cs:96`).
- **E7.** The chest-mark key on a chest outside the farm gives only a Debug log, no in-game sign (`GiftSelection/FarmChestScanner.cs:98-99`).
- **E8.** Silent skips that hide nothing:
  - `ItemCatalog` skips items that cannot be created;
  - the Plan tab shows the raw id when an item cannot be created, which is visible to the player;
  - the stores' `?? new …` is a default for first use, when nothing is stored yet;
  - the sparkle skips a chest it cannot find.
- **Loud on purpose (a good example):** `SentinelColor` throws right away if the reserved color would match a vanilla swatch.

## What we decided

| Id | Decision | Step |
|---|---|---|
| E1 | Fix. Create and check the gift object first, and only then remove one unit. If it cannot be handed over, nothing is removed, there is no HUD message, the row is not marked Resolved (so it can be tried again later), and a Warn log line is written. Pure logic goes to `Core/` with unit tests. | 05b |
| E2 | Fix. If the starter chest does not fit in the backpack, it drops at the player's feet (`Game1.createItemDebris`), and the welcome text says so correctly. The spec is updated. | 05c |
| E3 | Fix. A separate status and Plan row, "not found in the game", plus a log line. Spec and test. | 05d |
| E4 | Fix. A damaged stored plan row gives a warning in the log, and the pure function returns the warning. Tested. | 05e |
| E5 | No change now. It goes on the in-game checklist: select a villager you have not met yet and try a delivery. We decide based on the result. | checklist |
| E6 | Fix: a Debug log line when a delivery is skipped because of the daily limit. | 05f |
| E7 | Keep on purpose: correct per the spec ("no effect"). | 05a |
| E8 | Keep on purpose: none of these hides a failure from the player or loses data. | 05a |

## What we changed (step 05a: record E7, E8)

- This file, and the PLANNING_LOG.md lines for the approval of the 04g sentence and for the review 05 decisions.
- No code or spec change.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 68/68 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 05b: E1)

Spec, written before the code: new requirement `proximity-delivery`, "Delivery never removes an item it cannot hand over".
- The mod creates and checks the gift object before removing anything.
- If it cannot be handed over: nothing is removed, there is no confirmation, the gift stays pending, and a warning naming the NPC and the item is logged.
- The checks run in this order: gift limit, then whether the item can be handed over, then stock.
- Four scenarios.

Tests, written before the implementation: `StardewAutoGifter.Tests/DeliveryRuleTests.cs`, 5 tests, with the scenarios' values:
- not giftable with 10 units → `SkipNotGiftable`, and that step does not resolve the gift;
- limit reached → `SkipGiftLimit`, which resolves;
- 1 unit → `SkipLowStock`, which resolves;
- 5 units → `Deliver`.

Code:
- `Core/DeliveryRule.cs` (new, pure): the `DeliveryStep` enum, `Decide(withinGiftLimit, itemCanBeHandedOver, unitsOnFarm)` and `ResolvesGift(step)`.
- `Delivery/ProximityDeliveryService.cs`, `TryDeliver`:
  - it creates the gift object (`ItemRegistry.Create(…, allowNull: true) as SObject`) and counts the stock before anything is removed, then follows `DeliveryRule.Decide`;
  - only `Deliver` removes one unit, calls `receiveGift`, and shows "Gave … to …". The HUD message now names the created object's `DisplayName`, never a raw id;
  - `SkipNotGiftable` writes a Warn line through `Monitor.LogOnce`, because the gift stays pending and is retried every second while the NPC is in range, and leaves the gift unresolved;
  - the other skips resolve the gift as before.
- What stays the same: a skip for the limit or for low stock resolves the gift and shows nothing, as before.
- What changes on purpose: an item that cannot be handed over is no longer removed and no longer gives a false "Gave …" message.
- A side effect: the farm's chests are now scanned once even when the gift limit already says no. This only costs time, and it happens only when an NPC is in range.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 73/73 green (68 + 5).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 05c: E2)

Spec, written before the code: `gift-selection`, "Starter chest and welcome message on first farm exit".
- The chest goes into the inventory, or, if the inventory is full, it is dropped on the ground at the player's feet.
- New scenario "Inventory full at the first farm exit", with the exact page-2 text: "Your inventory was full, so a chest has been dropped at your feet. Pick it up and place it anywhere on your farm (or inside the barn, coop, shed, or greenhouse) - not in the farmhouse or cellar." The other pages stay unchanged. The wording is the assistant's and still needs the user's approval.

Tests, written before the implementation: `StardewAutoGifter.Tests/WelcomeTextTests.cs`, 1 new test, which checks page 2 with the chest dropped and the other 7 pages unchanged. The existing welcome tests are unchanged, because the new parameter defaults to `false`.

Code:
- `Core/WelcomeText.cs`: `Pages(…, bool chestDroppedAtFeet = false)` chooses page 2.
- `ModEntry.cs`, `OnWarped`:
  - it adds the chest with `Game1.player.addItemToInventoryBool`, whose result is now checked;
  - if that returns false, it drops the chest with `Game1.createItemDebris(chest, player.getStandingPosition(), player.FacingDirection)` and writes a Debug log line;
  - it passes the result to the welcome text.
  - The build confirms that both game methods exist with these signatures.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 74/74 green (73 + 1).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 05d: E3)

Spec, written before the code: `daily-plan-review`, "Reason shown for configured NPCs with no gift".
- There is a third reason, "not found in the game".
- The row is "<name>: not found in the game", never "nothing to give", and a warning naming the NPC is logged.
- New scenario with "Sophia".

Tests, written before the implementation: `StardewAutoGifter.Tests/PlanRowTextTests.cs`, 2 tests:
- the exact row text "Sophia: not found in the game";
- the row does not contain "nothing to give".

Code:
- `Core/GiftPlanStatus.cs`: new `NpcNotFound`.
- `Core/PlanRowText.cs` (new, pure): `ReasonLine(status, npcName)` for all three reasons. The two existing texts moved there word for word.
- `Planning/PlanComputer.cs`: takes `IMonitor`. A configured name that `Game1.getCharacterFromName` cannot find gets `NpcNotFound` and a Warn line through `Monitor.LogOnce`, because the plan is recomputed on every F8 open and the line would repeat. This check comes before the gift-limit and selection steps.
- `Menus/ModMainMenu.cs`: every non-planned row uses `PlanRowText.ReasonLine`, in gray.
- `ModEntry.cs` passes `Monitor`.
- No notice is ever shown for a not-found NPC: there is no NPC to stand next to, and `IsInRange` is false.

Noticed, not changed: the NPCs tab lists only villagers the game currently has. A configured NPC who is not found therefore cannot be deselected from the menu, and the "not found" row stays until the NPC exists again. This is a possible follow-up.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 76/76 green (74 + 2).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

## What we changed (step 05e: E4)

Spec, written before the code: `daily-plan-review`, "Edited plan carried into the next day".
- A damaged stored row (planned, without an item) is loaded as having no eligible item.
- A warning naming the NPC is logged instead of hiding the damage.
- Two scenarios: a damaged row for Haley, and an undamaged plan.

Tests, written before the implementation: `StardewAutoGifter.Tests/StoredPlanWarningsTests.cs`, 2 tests:
- the damaged row becomes `NoEligibleItem`, with exactly one warning that names Haley;
- complete rows give no warnings.

Code:
- `Core/StoredPlanData.cs`: `ToPlan(List<string>? warnings = null)` adds a warning for each damaged row. The default `null` keeps the existing round-trip tests unchanged.
- `Planning/PlanStore.cs`: `Load(IMonitor)` logs each warning at Warn.
- `ModEntry.cs` passes `Monitor` on save load.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, the known CS9057 warning.
- `dotnet test`: 78/78 green (76 + 2).
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.
