## Why

Giving daily gifts to villagers in Stardew Valley requires remembering every NPC's favorite items and making a detour to hand them over in person, which is tedious and easy to forget. This change adds a SMAPI mod that automates the daily gift-giving loop by drawing from the player's farm storage, so the player only has to keep their farm chests stocked and review/edit a short daily plan.

## What Changes

- New SMAPI mod (C#, targeting Stardew Valley 1.6.14+ / SMAPI 4.5.x / .NET 6) with no prior code in this repo.
- The player marks one chest on their farm as the mod's on/off anchor with a dedicated keybind while that chest's inventory is open; the mod applies a reserved sentinel tint color to it that the game's own chest color picker cannot produce, so the marking can't happen by accident through normal play. The marked chest keeps working as a normal storage chest and looks like any other chest.
- While a chest is marked, the mod treats every chest on the player's farm (including the marked one and chests inside farm buildings) as a single combined pool of items — it is not limited to the marked chest's own contents.
- Each evening, when the player opens the mod menu (via a configurable keybind), the mod computes tomorrow's gift plan: for each configured NPC, it picks the loved item with the largest total quantity summed across the farm-wide pool that is not on the blocklist, falling back to liked items if no eligible loved item exists and the mod is configured to consider liked items (the default).
- The plan is shown as a per-NPC preview list with a toggle to exclude an NPC from tomorrow's automatic gifting; the same menu also opens a blocklist editor where the player can search the game's full item catalog and check/uncheck items to add or remove them from the blocklist. Since the pool now spans every farm chest rather than one curated bin, the blocklist becomes the primary safeguard against giving away things the player wants to keep.
- During the next day, when the player comes within a short distance of a planned NPC, the mod automatically removes one unit of the planned item from wherever it is found among the farm's chests and hands it to the NPC, showing a confirmation message, without requiring a button press.
- The daily/weekly gift limits (max 1 gift/day/NPC, max 2 gifts/week counted toward friendship points) are respected by relying on the game's own per-NPC friendship gift-tracking rather than a duplicate counter.
- Distance checks are throttled (not evaluated on every game tick) to avoid unnecessary performance cost.
- Both mod keybinds (menu-open and chest-marking) default to keys that don't collide with any of Stardew Valley's default keybindings, and remain configurable.

## Capabilities

### New Capabilities
- `gift-selection`: marking a farm chest as the mod's on/off anchor with a reserved sentinel color via a dedicated keybind, pooling every farm chest's contents into a single searchable item pool, and picking which item (if any) each configured NPC should receive, given that pool, the blocklist, the loved-only/loved+liked configuration setting, and the game's own gift-limit state.
- `daily-plan-review`: the evening menu that computes and previews tomorrow's plan per NPC with an on/off toggle per row, and a searchable, checkbox-based blocklist editor covering the full item catalog in the same menu.
- `proximity-delivery`: detecting player proximity to a planned NPC during the day (throttled, not per-tick) and automatically handing over the planned item with a confirmation message.

### Modified Capabilities
(none — this is a new mod with no pre-existing specs)

## Impact

- New C# SMAPI mod project (manifest, `.csproj`, mod entry point) — no existing code in this repo is affected.
- Depends on SMAPI's event APIs (menu/keybind handling, periodic update ticks) and the game's built-in NPC gift-preference and friendship-tracking data.
- Config additions: the reserved sentinel color constant, the loved-only/loved+liked toggle (`IncludeLikedItems`, default `true`), and the two keybind defaults (menu-open, chest-marking).
- No persistent custom save data beyond the blocklist and the sentinel-colored anchor chest itself; the daily plan lives in memory for the current day/evening cycle.
- Item selection and removal now scan every chest on the farm rather than a single chest, so gift removal touches whichever farm chest happens to hold the selected item.

## Future work (not in this change)

- Seasonal preference for selection (prioritizing fruits/vegetables that are currently in season).
- Automatic "rare item" detection (e.g. via AI trained on the player's own playthrough data), instead of a manually maintained blocklist.
- Combining multiple weighting factors for selection, beyond quantity alone.
- Multiplayer compatibility.
- Long-term history/statistics of past gifts.
- A configurable proximity threshold for automatic hand-off (currently fixed).
- A more distinctive visual marker for the anchor chest beyond its current color, name, and periodic sparkle (e.g. a floating icon or a genuinely two-toned chest sprite), which would require custom world-rendering.
