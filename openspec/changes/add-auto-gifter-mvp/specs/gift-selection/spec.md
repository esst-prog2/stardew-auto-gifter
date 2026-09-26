## Purpose

Determines which item, if any, each configured NPC should receive for a given day, based on the game's own gift preferences, the combined contents of the player's farm chests, the blocklist, and the game's gift-limit state.

## ADDED Requirements

### Requirement: Marked chest acts as the mod's on/off anchor
The mod SHALL be active (selecting and delivering gifts) only while exactly one chest on the player's farm carries a fixed sentinel tint color reserved by the mod. This color SHALL only ever be applied or removed through the mod's own chest-marking action (see the next requirement) — never through the game's built-in chest color picker, which offers a fixed set of preset swatch colors and cannot produce the reserved sentinel value. The marked chest continues to function as an ordinary storage chest, and its own contents are included in the farm-wide item pool like any other farm chest. If no chest carries the sentinel color, the mod SHALL treat every configured NPC as having no eligible item for that day.

#### Scenario: Mod active once a farm chest is marked
- **WHEN** a chest on the player's farm carries the mod's reserved sentinel color
- **THEN** the mod is active and includes that chest's contents in the farm-wide pool of items available for gifting

#### Scenario: No chest currently marked
- **WHEN** no chest on the farm currently carries the sentinel color
- **THEN** the mod is inactive and selects no item for any configured NPC that day

#### Scenario: Sentinel color unreachable through the vanilla color picker
- **WHEN** the player uses the game's built-in chest color picker on any chest
- **THEN** the player cannot produce the mod's reserved sentinel color, because that picker only offers its fixed set of preset swatch colors

### Requirement: Chest marked via a dedicated keybind while its inventory is open
The mod SHALL let the player mark the currently open chest as the mod's anchor by pressing a dedicated, configurable keybind while that chest's inventory menu is open. This keybind SHALL only take effect on chests located on the player's farm; pressing it while a chest outside the farm is open SHALL have no effect. The keybind SHALL default to a key that does not conflict with any of the game's default keybindings. Pressing the same keybind again while the current anchor chest's inventory is open SHALL unmark it.

#### Scenario: Player marks a farm chest as the anchor
- **WHEN** the player has a chest located on the farm open and presses the configured chest-marking keybind
- **THEN** the mod applies the reserved sentinel color to that chest, and the mod becomes active

#### Scenario: Player unmarks the current anchor chest
- **WHEN** the player has the current anchor chest's inventory open and presses the configured chest-marking keybind again
- **THEN** the mod removes the sentinel color from that chest, and the mod becomes inactive until another chest is marked

#### Scenario: Marking attempted on a non-farm chest has no effect
- **WHEN** the player has a chest located outside the farm open and presses the configured chest-marking keybind
- **THEN** the mod does not apply the sentinel color to that chest, and its active/inactive state is unchanged

### Requirement: Anchor chest is renamed for visibility
When the mod marks a chest as the anchor, it SHALL set that chest's display name to a distinctive value identifying it as the mod's chest. When unmarking a chest, the mod SHALL reset its name to the game's plain default chest name.

#### Scenario: Chest renamed on marking
- **WHEN** the player marks a farm chest as the anchor
- **THEN** that chest's display name becomes a distinctive, recognizable name identifying it as the Auto-Gifter chest

#### Scenario: Chest name reset on unmarking
- **WHEN** the player unmarks the current anchor chest
- **THEN** that chest's display name reverts to the game's plain default chest name

### Requirement: Anchor chest sparkles periodically for visibility
While the mod is active and the player is in the same location as the anchor chest, the mod SHALL periodically emit a brief sparkle effect at the anchor chest, reusing the game's own sparkle animation, to make it easier to spot at a glance. It SHALL NOT emit this effect while inactive or while the player is elsewhere.

#### Scenario: Sparkle shown while active and in view
- **WHEN** the mod is active and the player is in the same location as the anchor chest
- **THEN** the anchor chest periodically emits a brief sparkle effect

#### Scenario: No sparkle when inactive or out of view
- **WHEN** the mod is inactive, or the player is not in the same location as the anchor chest
- **THEN** no sparkle effect is emitted for the anchor chest

### Requirement: Ambiguous marking treated as unresolved
If more than one chest on the farm simultaneously carries the reserved sentinel color, the mod SHALL NOT guess which one is the intended anchor. It SHALL treat the situation as inactive — selecting no item for any configured NPC — until only one sentinel-colored chest remains, and SHALL warn the player about the conflict.

#### Scenario: Two farm chests carry the sentinel color
- **WHEN** more than one chest on the farm currently carries the reserved sentinel color
- **THEN** the mod selects no item for any configured NPC, and surfaces a warning about the conflicting chests, until the conflict is resolved

### Requirement: Farm-wide item pool
While the mod is active, the pool of items available for gift selection SHALL be the combined contents of every chest located on the player's outdoor farm map and inside farm buildings placed on it (such as the barn, coop, shed, and greenhouse), including the anchor chest itself. The mod SHALL NOT consider chests outside the farm, including chests inside the farmhouse or its cellar.

#### Scenario: Items pooled across multiple farm chests
- **WHEN** the player's farm has several chests, each containing some of an NPC's loved items
- **THEN** the mod treats each item as available in the sum of its quantity across all of those farm chests

#### Scenario: Chests outside the farm are ignored
- **WHEN** a chest located outside the player's farm (for example, in the mines or another location) contains an item an NPC loves
- **THEN** the mod does not count that chest's contents toward the item pool

#### Scenario: Farmhouse and cellar chests are ignored
- **WHEN** a chest inside the farmhouse or its cellar contains an item an NPC loves
- **THEN** the mod does not count that chest's contents toward the item pool

### Requirement: Minimum one unit reserved per item
The mod SHALL only consider an item eligible for selection if the farm-wide pool contains at least two units of it, so that giving one away always leaves at least one unit remaining in the farm's chests. An item present in exactly one unit SHALL be treated the same as an item that is entirely absent, for both the loved and liked tiers.

#### Scenario: Item present as a single unit is not selected
- **WHEN** the farm-wide pool contains exactly one unit of an NPC's loved item, and no other eligible loved item exists
- **THEN** the mod does not select that item, and falls back to liked items (if enabled) or selects nothing for that NPC, the same as if the item were absent entirely

#### Scenario: Item present in two or more units remains eligible
- **WHEN** the farm-wide pool contains two or more units of an NPC's loved item
- **THEN** the mod may select that item normally, subject to the other selection rules

### Requirement: Anchor chest prioritized before the wider farm pool
When selecting an item for an NPC, the mod SHALL first apply the tiered, quantity-based selection rule (below) using only the anchor chest's own contents as the pool. If that yields no eligible item and the "anchor chest only" setting (see specs/daily-plan-review) is disabled, the mod SHALL then apply the same rule using the full farm-wide pool (every farm chest, including the anchor). If the "anchor chest only" setting is enabled, the mod SHALL NOT fall back to the farm-wide pool at all. When delivering, if the planned item is present in both the anchor chest and another farm chest, the mod SHALL remove the unit from the anchor chest.

#### Scenario: Anchor chest alone has an eligible item
- **WHEN** the anchor chest alone contains an eligible loved or liked item (at least two units) for an NPC
- **THEN** the mod selects that item without needing to consult the rest of the farm-wide pool

#### Scenario: Anchor chest alone has nothing eligible, another farm chest does, fallback enabled
- **WHEN** the "anchor chest only" setting is disabled, the anchor chest alone has no eligible item for an NPC, but the combined farm-wide pool does
- **THEN** the mod falls back to selecting from the full farm-wide pool

#### Scenario: Anchor-chest-only setting enabled, nothing eligible in the anchor
- **WHEN** the "anchor chest only" setting is enabled and the anchor chest alone has no eligible item for an NPC, even though another farm chest does
- **THEN** the mod does not fall back to the farm-wide pool and treats that NPC as having no eligible item

#### Scenario: Delivery prefers the anchor chest
- **WHEN** the planned item is present in both the anchor chest and at least one other farm chest at delivery time
- **THEN** the mod removes the unit from the anchor chest rather than the other chest

### Requirement: Tiered, quantity-based item selection
Given a pool (either the anchor-only pool or the farm-wide pool, per the requirement above), the mod SHALL select the item to gift by first considering the NPC's loved items present in that pool, with at least two units available, and not on the blocklist, picking the one with the largest total quantity. Whether liked items are considered at all is controlled by the mod's "include liked items" configuration setting (default: enabled). When that setting is enabled and no eligible loved item exists in the pool, the mod SHALL fall back to the NPC's liked items in the same pool, again picking the largest quantity. When that setting is disabled, the mod SHALL only ever consider loved items. Items on the blocklist SHALL never be selected, regardless of preference tier or quantity.

#### Scenario: Multiple loved items at different total quantities
- **WHEN** the pool being considered contains several items the NPC loves, at different total quantities, none of them blocklisted
- **THEN** the mod selects the loved item with the largest total quantity

#### Scenario: Same loved item split across multiple chests within the farm-wide pool
- **WHEN**, while considering the farm-wide pool, an NPC's loved item is split across several farm chests (for example, 5 in one chest and 7 in another, totaling 12) and a different loved item exists as a single stack of 10 in one chest
- **THEN** the mod compares the summed quantities (12 vs. 10) and selects the item with the higher total

#### Scenario: Favorite item blocklisted, another loved item available
- **WHEN** the NPC's most-loved, most-plentiful (by total quantity) item in the pool being considered is on the blocklist, but another non-blocklisted loved item is also present
- **THEN** the mod skips the blocklisted item and selects the next-best non-blocklisted loved item by total quantity

#### Scenario: No eligible loved item, eligible liked item exists, liked items enabled
- **WHEN** the "include liked items" setting is enabled, and the pool being considered has no loved item for the NPC that is both present and not blocklisted, but does have a liked item that is present and not blocklisted
- **THEN** the mod selects the liked item with the largest total quantity

#### Scenario: Liked-item fallback disabled by configuration
- **WHEN** the "include liked items" setting is disabled and an NPC has no eligible loved item in the pool being considered
- **THEN** the mod selects no item for that NPC from that pool, even if an eligible liked item is present

#### Scenario: Blocklisted item never selected
- **WHEN** an item is on the blocklist
- **THEN** the mod never selects that item for any NPC, even if it is that NPC's favorite and the most plentiful matching item in the pool being considered

### Requirement: No selection when nothing is eligible
When an NPC has no loved or liked item in the farm-wide pool that is not blocklisted, the mod SHALL select no item for that NPC for that day, and the NPC SHALL NOT appear as receiving a gift in that day's plan.

#### Scenario: No loved or liked item available
- **WHEN** the farm-wide pool contains none of the NPC's loved or liked items, or only blocklisted ones
- **THEN** the NPC is not assigned any item for that day

### Requirement: Starter chest and welcome message on first farm exit
The first time the player exits the farmhouse onto the farm in a save, the mod SHALL add one unplaced chest to the player's inventory and show a one-time welcome message, in English, explaining how to place and mark a chest and how to open the mod menu. This message SHALL clearly call out, as important information (not buried in passing), that selecting an NPC or editing the plan does not affect the current day - automatic gifting only begins the following day. The mod SHALL NOT repeat this for that save once shown.

#### Scenario: First exit from the farmhouse
- **WHEN** the player warps from the farmhouse onto the farm for the first time in a save
- **THEN** the mod adds a chest item to the player's inventory and displays a one-time English welcome message that clearly explains gifting only starts the next day

#### Scenario: Welcome message and chest given only once per save
- **WHEN** the player exits the farmhouse onto the farm again later in the same save
- **THEN** the mod does not show the welcome message or add another chest

### Requirement: Gift-limit state respected during selection
The mod SHALL exclude an NPC from receiving a selection for a given day if the game's own per-NPC gift tracking shows that NPC has already reached the weekly limit of friendship-point-counting gifts, even if an otherwise-eligible item exists in the pool.

#### Scenario: NPC already reached the weekly friendship-gift cap
- **WHEN** the game's own tracking shows the NPC has already received the maximum number of friendship-point-counting gifts for the current week
- **THEN** the mod does not select any item for that NPC for that day, even if an eligible loved or liked item is present in the pool
