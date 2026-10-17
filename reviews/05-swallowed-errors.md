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
