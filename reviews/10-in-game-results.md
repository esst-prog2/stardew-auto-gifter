# HW7: in-game results of the HW6 checklist

The joint in-game checklist left as "not run" in HW6 (C4, R1, R3, D4, D5, E2, E3, E5, E6, 05g, A3-A5, and the review 01 A5 greenhouse check) was run by the user on 2026-11-06, on branch `hw7-progress`, with the build from `main` at `6ddb7a5`. It took four game launches. The results below are as the user reported them. Where a result is unclear or contradicts another one, this file says so instead of choosing for the user.

## Results

| # | Point | What was checked | Reported result | Recorded as |
|---|---|---|---|---|
| 1 | E2 + C4 | Fill the backpack before the first exit (new save, config F9/K) | the chest did not fit and was dropped on the ground in front of the player | OK (the E2 behavior) |
| 2 | E2 + C4 | Welcome: chest at feet, page 2 "inventory was full", page 3 "press K", page 6 "Press F9", the SMAPI log line | OK | OK for E2; the key names conflict with 3-4 (see D1) |
| 3 | C4 | F8 does nothing, F9 opens the menu, Plan says "press K to mark it" | F8 opened the menu, and the text said to mark with G | **deviates** (D1) |
| 4 | C4 | G does nothing, K marks the chest | G marks, K does not | **deviates** (D1) |
| 5 | C4 + R3 | Conflict HUD "… with K" once; Plan "press K to unmark"; the Warnings tab fits, scrolls, red line at the end | OK | OK for R3; the key names conflict with 3-4 (see D1) |
| 6 | R3 + D5 | Settings: the liked-items sentence, clicking the setting line toggles it, clicking the explanation does nothing | OK | OK |
| 7 | — | Unmark the second chest, quit | OK | OK |
| 8 | A3 + A4 | Config `MenuKey: 9999`, `ChestMarkKey: None`: two Warn lines, F8 opens the menu, G does nothing | OK | OK: Newtonsoft does accept the number (the A3 assumption holds) |
| 9 | A5 (keys) | Config K/K: Warn line, K opens the menu, K in a chest does not mark | OK | OK |
| 10 | — | Config F8/G on an existing save: no key warning | OK | OK |
| 11 | D4 | Blocklist, NPCs and Settings kept after the store refactor | OK | OK |
| 12 | D4 | Today's stored plan is delivered | OK | OK |
| 13 | E3 + 05g | A not-found villager row on the NPCs and Plan tabs, Warn line, unchecking | ok | OK (as reported) |
| 14 | D4, E5, R1 | Thursday: Haley gift 1 by hand, the not-met villager selected, one more item blocked, Plan shows both | ok | OK |
| 15 | E5 | Friday: walk past a villager not met yet | the mod only works after the player has talked to the villager; before that, SMAPI logs `KeyNotFoundException: The given key 'Caroline' was not present in the dictionary` from `NPC.receiveGift`, through `ProximityDeliveryService.TryDeliver` line 144 (logged at least twice) | **deviates** (D2) |
| 16 | E6 | Unmark the chest with G before going near Haley, then gift by hand | OK | OK |
| 17 | E6 | Re-mark, walk past Haley: no gift, and the Debug line `Skipped delivery to Haley: the gift limit … is already reached.` once | (no result given) | not reported |
| 18 | R1 | Friday evening: Haley "already reached this week's gift limit" | Ok | OK |
| 19 | R1 | Saturday evening: `[x] Haley -> Coconut` for Sunday | OK | OK |
| 20 | R1 | Sunday: "Gave Coconut to Haley.", one box ticked in the new week | OK | OK |
| 21 | D4 | After reloading, the item blocked on Thursday is still blocked | OK | OK |
| 22 | A5 (greenhouse) | The chest-mark log line in the greenhouse: `in farm scope: True/False` | False | **deviates** (D3): confirms the review 01 suspicion |
| 23 | A5 (greenhouse) | Coconuts only in the greenhouse chest → `[x] Haley -> Coconut`? | "nothing to give" | **deviates** (D3) |
| 24 | — | Put the Coconuts back | OK | OK |

Implicitly confirmed:
- the 02c and 06a load checks (`MinimumApiVersion` 4.5.0, `MinimumGameVersion` 1.6.14): the mod loaded in all four launches, with no version error.

Not covered by this run:
- the R4 welcome variant with "anchor chest only" already on;
- the review 01 step-7 target-day question;
- E1, which is not reproducible in game.

## Deviations, likely causes, and proposed fixes (nothing changed yet)

**D1. C4: the remapped keys were not in effect in launch 1 (points 3-4).**
- What happened: F8 opened the menu, and G marked the chest, which means the defaults were used. Points 2 and 5 were reported OK, but their expected texts name K and F9, so they contradict 3-4.
- Likely cause: the config edit for launch 1 did not reach the file the game read. Perhaps the file was saved after the game had already started, the edit went to another copy of `config.json`, or it was not saved. The key handling itself works: launches 2 and 3 applied their configs (points 8-9).
- Proposal: no code change. Re-run points 2-5 with `"MenuKey": "F9", "ChestMarkKey": "K"` saved before starting the game, and note the exact texts of welcome pages 3 and 6 and of the conflict HUD message.

**D2. E5: delivering to a villager the player has not met crashes in `NPC.receiveGift`, and the item is lost each time (point 15).**
- Cause, confirmed in the code:
  - `FriendshipGiftTracker.CanGiveGiftOn` returns true when there is no friendship entry. This is the open review 05 E5 / review 08 C1 assumption.
  - In `ProximityDeliveryService.TryDeliver`, the `Deliver` step first removes one unit (`TryRemoveOneUnit`) and only then calls `npc.receiveGift` (line 144). The game's `receiveGift` reads `friendshipData[name]` and throws `KeyNotFoundException` for a villager with no entry.
  - The exception escapes before `gift.Resolved` is set, so the delivery is retried every second while the player is in range. Every retry removes one more unit and throws again. The log shows at least two such failures. So items were most likely lost from the farm's chests; the user did not report the count.
- Proposed fix, for the user to decide:
  - **a)** Close the open E5 question with "not met yet = cannot be gifted". In planning, a villager with no friendship entry gets a new status and Plan row, "<name>: not met yet - talk to them first", instead of a planned item. Delivery and notices skip them. Spec and tests come first, with a pure rule in `Core/`.
  - **b)** Independently of a), make delivery safe: call `receiveGift` before `TryRemoveOneUnit`. The gift object is a new item, not the chest's stack, so nothing is taken from a chest unless the game accepted the gift. Also catch an exception from `receiveGift`, log it once at Warn, and resolve that gift for the day, so it is not retried every second.
  - Recommended: both a) and b). a) removes the cause; b) makes sure no future game exception can drain a chest.

**D3. A5: chests in the greenhouse are not part of the farm scope (points 22-23).**
- Cause, confirmed in the code: `FarmChestScanner.GetFarmScopeLocations` adds only `building.indoors.Value`, which is set only for buildings with their own instance of an interior (barn, coop, shed). The greenhouse's interior is a shared, named location, so `indoors.Value` is null and the greenhouse is skipped. That gives both "in farm scope: False" and the missing Coconuts.
- Proposed fix: for each farm building, use the game's `building.GetIndoors()`, which also returns shared interiors. Keep excluding the farmhouse; in 1.6 it is also a building with a shared interior, so skip any `FarmHouse` location (this also covers cabins). The cellar is not a building on the farm, so it stays out.
  - The spec needs no change, because it already lists the greenhouse.
  - The exact method name has to be confirmed by the build against the game DLL. If it does not exist, the fallback is `Game1.getLocationFromName("Greenhouse")`.
- In-game check after the fix: repeat points 22-23. Expect `in farm scope: True`, then `[x] Haley -> Coconut`.

**Point 17 (E6) has no result.** It needs a re-run: re-mark the chest, walk past Haley, and check `SMAPI-latest.txt` for the Debug line.

## Decisions (2026-11-06)

| Id | Decision |
|---|---|
| D1 | No code change. The user re-runs points 2-5 with a correctly saved config. |
| D2 | Both a) and b). **a)** A villager not met yet is not gifted: a new status and Plan row "<name>: not met yet - talk to them first", and delivery and the in-person notice skip them. Spec and tests first, with the pure rule in `Core/`. **b)** `receiveGift` comes first, and `TryRemoveOneUnit` only after a successful hand-off. If `receiveGift` throws, write one Warn line and resolve the gift for the day, with no retry. |
| D3 | Fix `GetFarmScopeLocations` as proposed: include shared interiors, exclude `FarmHouse`, and use `Game1.getLocationFromName("Greenhouse")` as the fallback if `GetIndoors()` does not exist. Tested where possible. |
| 17 | The user re-runs it. |

Point 15 (E5), how many amethysts were lost: **not reported**. The user's message left this as a placeholder, so no number is recorded.
