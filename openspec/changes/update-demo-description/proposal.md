## Why

README section 1 ("The demo") still describes the week-one idea: a plain menu with rows like "Abigail → Grape (12 in chest)" and the message "Gave Grape to Abigail". Since then the mod has gained:
- a marked "Auto-Gifter Chest" that acts as its on/off anchor and is used first;
- the F8 menu with NPCs, Plan and Blocklist tabs;
- plan rows in the form "[x] <NPC> -> <item>";
- the "not met yet - talk to them first" status.

The old demo's NPC-item pairs (Abigail → Grape, Sebastian → Beer, Penny → Strawberry) were never checked in the game. The demo should describe what the mod actually does today, using only texts the mod really shows.

## What Changes

- Replace the text of README section 1 ("The demo") with a new paragraph, fixed word for word by the user. It walks through:
  - the marked chest (10 Coconuts, 5 Amethysts, 12 Prismatic Shards), closed again before pressing F8;
  - the NPCs tab with Haley, Abigail and Emily ticked;
  - the Plan tab rows "[x] Haley -> Coconut", "[x] Abigail -> Amethyst" and "Emily: not met yet - talk to them first";
  - why the Blocklist matters (without it, Abigail would get a Prismatic Shard);
  - toggling Abigail's row off;
  - the next morning's automatic hand-off to Haley with "Gave Coconut to Haley.";
  - the chest counts afterwards.
- Nothing else in the README changes, nor any code, spec or test.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- None. This change only updates documentation and changes no spec-level behavior, so the change sets `skip_specs: true`.

## Impact

- **Affected file:** `README.md`, section 1 only.
- **No code, test, config or spec changes.**
- **Every quoted text is checked against the code** before the README is edited:
  - the chest name "Auto-Gifter Chest" (`FarmChestScanner`);
  - the Plan row format `[x] <NPC> -> <item>` (`ModMainMenu`);
  - the row "<NPC>: not met yet - talk to them first" (`Core/PlanRowText`);
  - the message "Gave <item> to <NPC>." (`ProximityDeliveryService`).
  - A villager not met yet can be selected on the NPCs tab (`VillagerCatalog` lists every villager).
  - The hand-off takes the Coconut from the marked chest first (the anchor chest is put first before `TryRemoveOneUnit`).
  - F8 does not open the menu while a chest menu is still open (`ModEntry.OpenModMenu`). This is why the demo says "close it again, and press F8".
- **Known unverified claims.** Two statements come from the game's own gift-taste data, not from the mod's code:
  - without the Blocklist, Abigail would get a Prismatic Shard (it is a universal "love", and 12 is more than 5 Amethysts);
  - Haley would not get one anyway, because she hates Prismatic Shards. The demo text no longer says this; it was the reason the user chose a wording that only names Abigail.

  The user will check both in-game when taking the December screenshots: untick the Prismatic Shard on the Blocklist tab and look at the Plan tab.
