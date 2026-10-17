## Purpose

Lets the player preview and edit tomorrow's automatically computed gift plan, and manage the blocklist, from an in-game menu opened via a configurable keybind in the evening.

## ADDED Requirements

### Requirement: Menu opened via configurable keybind
The mod SHALL let the player open its menu at any time during gameplay by pressing a keybind. The keybind SHALL be configurable via the mod's configuration file, and SHALL default to a key that does not conflict with any of the game's default keybindings.

#### Scenario: Player opens the menu with the configured key
- **WHEN** the player presses the configured keybind while playing
- **THEN** the mod's menu opens, showing the daily plan preview and blocklist editor

### Requirement: Plan computed for the next day when the menu is opened
The mod SHALL compute the gift plan for the next in-game day at the moment the player opens the menu, using the farm-wide item pool's contents and the blocklist as they are at that moment. This plan SHALL be what is shown in the preview. The Plan tab SHALL always display a persistent label identifying it as tomorrow's plan (not today's), so the player is not left to infer the timing from memory or from a message they may have already dismissed. Each plan SHALL record its target day (the in-game day it is for), which is always the day after the day it was computed on. Computing or recomputing the next day's plan SHALL NOT change or replace the plan being executed today. When the menu is opened again on the same day, the next day's plan SHALL be recomputed from the current contents, but every NPC whose row the player had toggled off SHALL stay toggled off, as long as that NPC still has a planned item in the recomputed plan.

#### Scenario: Plan reflects current farm-wide contents when menu opens
- **WHEN** the player opens the menu on a given evening
- **THEN** the mod computes and displays the gift plan for the following day based on the farm-wide item pool's contents and the blocklist at that moment

#### Scenario: Plan targets the day after it was computed
- **WHEN** the player opens the menu on in-game day 10
- **THEN** the computed plan's target day is day 11

#### Scenario: Reopening the menu keeps toggled-off rows
- **WHEN** the player toggles off an NPC's row, closes the menu, and opens it again on the same day, and that NPC still has a planned item
- **THEN** that NPC's row is still toggled off in the recomputed plan, and every row the player did not toggle off is still on

#### Scenario: Toggled-off NPC without a planned item after recomputing
- **WHEN** the player toggled off an NPC's row, and after reopening the menu that NPC no longer has a planned item (for example because the item ran out)
- **THEN** that NPC's row shows the reason it has no gift, and there is nothing left toggled off for that NPC

#### Scenario: Opening the menu does not change today's plan
- **WHEN** the player opens the menu during a day on which yesterday's plan is being delivered
- **THEN** today's deliveries keep following yesterday's plan, and only the next day's plan is recomputed

#### Scenario: Plan tab always labeled as the next day's plan
- **WHEN** the player opens the Plan tab, at any time
- **THEN** the tab displays a persistent label identifying the shown plan as tomorrow's, not today's

### Requirement: Per-NPC preview with on/off toggle
The menu SHALL show one row per configured NPC that has an item selected for the next day, naming the NPC and the item they are planned to receive. Each row SHALL have a toggle the player can use to exclude that NPC from the next day's automatic gifting. When there are more rows than fit in the menu, the Plan tab SHALL be scrollable with the mouse wheel, one row per wheel step, so that every row can be reached; it SHALL NOT scroll above the first row, or further down than the point where the last row is visible, and it SHALL NOT scroll at all when every row fits. Rows SHALL never be drawn outside the menu. Clicking a row SHALL act on the NPC shown in that row at the current scroll position.

#### Scenario: Player excludes an NPC from tomorrow's plan
- **WHEN** the player toggles off a row for an NPC in the preview
- **THEN** that NPC receives no automatic gift the next day, even though the mod had selected an eligible item for them

#### Scenario: Scrolling down one step
- **WHEN** the plan has 20 rows, 7 of them fit, the first row is at the top, and the player scrolls down one step
- **THEN** the second row (row 1, counting from 0) is at the top

#### Scenario: Cannot scroll above the first row
- **WHEN** the plan has 20 rows, 7 of them fit, the first row is at the top, and the player scrolls up one step
- **THEN** the first row (row 0) stays at the top

#### Scenario: Cannot scroll past the last row
- **WHEN** the plan has 20 rows, 7 of them fit, row 13 is at the top (so the last row, row 19, is visible), and the player scrolls down one step
- **THEN** row 13 stays at the top

#### Scenario: No scrolling when every row fits
- **WHEN** the plan has 5 rows, 7 rows fit, and the player scrolls down one step
- **THEN** the first row (row 0) stays at the top

#### Scenario: Clicking a row after scrolling
- **WHEN** the plan has 20 rows, row 13 is at the top, and the player clicks the 7th visible row (visible row 6, counting from 0)
- **THEN** the click acts on row 19, the NPC shown there

#### Scenario: Clicking below the last row
- **WHEN** the plan has 20 rows, row 13 is at the top, and the player clicks where an 8th visible row would be (visible row 7)
- **THEN** the click acts on no row

### Requirement: Reason shown for configured NPCs with no gift
For each configured NPC who does not have a planned item for the next day — either because no eligible loved or liked item is available in the farm-wide pool, or because they have already reached the weekly friendship-point-counting gift limit — the preview SHALL show a row explaining why, instead of silently omitting them. If the mod is inactive (no single valid anchor chest currently marked), the preview SHALL show one explanatory message instead of per-NPC rows, and that message SHALL tell the player how to fix the specific cause: when no chest is marked, "Mod inactive: open a chest on your farm and press G to mark it."; when more than one chest is marked, "Mod paused: more than one chest is marked. Open each extra chest and press G to unmark it until only one remains."

#### Scenario: Configured NPC has no eligible item
- **WHEN** a configured NPC has no eligible loved or liked item anywhere in the farm-wide pool
- **THEN** the preview shows a row for that NPC stating no item is available, instead of omitting them from the preview

#### Scenario: Configured NPC already reached the weekly cap
- **WHEN** a configured NPC has already reached the weekly friendship-point-counting gift limit
- **THEN** the preview shows a row for that NPC stating they have already reached this week's gift limit

#### Scenario: Mod inactive
- **WHEN** no chest is currently marked as the anchor, or more than one is
- **THEN** the preview shows a single message explaining the mod is inactive, instead of per-NPC rows

#### Scenario: Mod inactive because no chest is marked
- **WHEN** no chest is currently marked as the anchor
- **THEN** the preview's message is "Mod inactive: open a chest on your farm and press G to mark it."

#### Scenario: Mod paused because more than one chest is marked
- **WHEN** more than one chest currently carries the sentinel color
- **THEN** the preview's message is "Mod paused: more than one chest is marked. Open each extra chest and press G to unmark it until only one remains."

#### Scenario: Exactly one chest is marked
- **WHEN** exactly one chest currently carries the sentinel color
- **THEN** the preview shows the per-NPC rows, and no inactive message

### Requirement: Configured NPCs selected from the mod menu
The mod menu SHALL include a tab listing every villager in the game, each with a checkbox reflecting whether the mod should consider gifting them. Checking or unchecking a villager SHALL take effect starting with the next plan computation. This selection SHALL be the sole source of which NPCs the mod considers (no separate configuration-file list). The NPCs tab SHALL offer the same starting-letter filter described for the blocklist editor below.

#### Scenario: Player selects a villager
- **WHEN** the player checks a villager's checkbox in the NPCs tab
- **THEN** that villager becomes eligible to receive a planned gift starting with the next plan computation

#### Scenario: Player deselects a villager
- **WHEN** the player unchecks a previously-selected villager's checkbox
- **THEN** that villager no longer receives a planned gift starting with the next plan computation

### Requirement: Anchor-chest-only setting in the mod menu
The mod menu SHALL include a way to enable or disable restricting selection to the anchor chest alone (disabling the farm-wide fallback described in specs/gift-selection - "Anchor chest prioritized before the wider farm pool"). This setting SHALL take effect starting with the next plan computation.

#### Scenario: Player enables anchor-chest-only mode
- **WHEN** the player enables the anchor-chest-only setting
- **THEN** subsequent plan computations only consider the anchor chest's own contents, never falling back to other farm chests

#### Scenario: Player disables anchor-chest-only mode
- **WHEN** the player disables the anchor-chest-only setting
- **THEN** subsequent plan computations fall back to the farm-wide pool as usual when the anchor chest alone has nothing eligible

### Requirement: Blocklist editor lists the full item catalog
The same menu SHALL open a blocklist editor that lists the game's full catalog of giftable items - the game's object items, which are the items that can be given to villagers (not just items currently in the chest) - each shown with a checkbox reflecting whether it is currently on the blocklist.

#### Scenario: Full catalog shown
- **WHEN** the player opens the blocklist editor
- **THEN** every giftable object item in the game's catalog is listed with its current blocklist checkbox state

### Requirement: Blocklist filter by starting letter
The blocklist editor SHALL let the player filter the displayed item list by checking one or more starting-letter boxes (A-Z). When one or more letters are checked, only items whose name starts with one of the checked letters remain shown. When none are checked, all items are shown.

#### Scenario: Player filters by a starting letter
- **WHEN** the player checks a single letter's box
- **THEN** only items whose name starts with that letter remain shown

#### Scenario: Multiple letters checked
- **WHEN** the player checks more than one letter's box
- **THEN** items whose name starts with any of the checked letters remain shown

#### Scenario: No letters checked
- **WHEN** no letter boxes are checked
- **THEN** all items are shown

### Requirement: Toggling blocklist membership via checkbox
The player SHALL be able to check or uncheck an item's checkbox in the blocklist editor to add or remove it from the blocklist; the change SHALL take effect starting with the next plan computation.

#### Scenario: Player checks an item
- **WHEN** the player checks an unblocked item's checkbox in the blocklist editor
- **THEN** that item is added to the blocklist and excluded from selection starting with the next plan computation

#### Scenario: Player unchecks an item
- **WHEN** the player unchecks a blocklisted item's checkbox in the blocklist editor
- **THEN** that item is removed from the blocklist and becomes eligible for selection again starting with the next plan computation

### Requirement: Edited plan carried into the next day
Once the player closes the menu (or ends the day), the plan as edited (including any rows toggled off) SHALL be the plan executed during the next day; the mod SHALL NOT recompute or discard the player's edits when the new day starts. The next day's plan SHALL be stored with the save when the day ends, so that it is still the plan executed after the game is closed and the save is loaded again. When a new day starts, the stored plan SHALL become that day's plan only if its target day is the new day; otherwise the new day has no plan.

#### Scenario: Toggled-off NPC stays excluded the next day
- **WHEN** the player toggled off an NPC's row before ending the day
- **THEN** that NPC is not given an automatic gift on the following day, regardless of the farm-wide pool's contents when the day starts

#### Scenario: Plan survives saving and loading
- **WHEN** the player ends day 10 with a plan for day 11, quits the game, and later loads the save on day 11
- **THEN** the plan executed on day 11 is the plan from day 10, including the rows toggled off

#### Scenario: Plan becomes today's plan on its target day
- **WHEN** day 11 starts and the stored plan's target day is day 11
- **THEN** that plan becomes day 11's plan

#### Scenario: Plan for another day is not used
- **WHEN** a day starts and there is no stored plan, or the stored plan's target day is not that day
- **THEN** that day has no plan, and the mod delivers nothing that day

### Requirement: Next day's plan computed automatically at the end of the day
If, when the day ends, there is no plan for the next day, or the only one is inactive (it was computed while no single valid anchor chest was marked), the mod SHALL compute the next day's plan automatically at that moment, the same way as when the menu is opened, with no rows toggled off. If the player already has an active plan for the next day, the mod SHALL keep it unchanged, including the player's edits.

#### Scenario: Menu not opened during the day
- **WHEN** day 10 ends and the player never opened the menu on day 10
- **THEN** the mod computes a plan for day 11 when day 10 ends, and that plan is executed on day 11

#### Scenario: Plan already made during the day
- **WHEN** day 10 ends and the player already has an active plan for day 11
- **THEN** the mod does not recompute it, and the player's edits stay in place

#### Scenario: Only an inactive plan exists
- **WHEN** day 10 ends and the only plan for day 11 is inactive
- **THEN** the mod computes the plan for day 11 again when day 10 ends

### Requirement: Warnings and info panel
The mod menu SHALL include a dedicated warnings/info panel, opened from the main menu, that documents conditions the player should avoid because they could interfere with the mod's operation — including applying the reserved sentinel color to any chest other than through the mod's own chest-marking action, and marking more than one chest as the mod's anchor at the same time. When the mod detects that more than one chest currently carries the sentinel color, it SHALL additionally show an active warning, not just the static panel, until the conflict is resolved. The active warning SHALL be a HUD message, "Auto-Gifter paused: more than one chest is marked. Unmark all but one with G.", shown once at the moment the mod detects that a conflict has started (when the chest-marking key creates it, when the menu is opened, or at a periodic check every 4 seconds, the same timer as the anchor-chest sparkle), and SHALL NOT be repeated while the same conflict lasts. If the conflict is resolved and later starts again, the HUD message SHALL be shown again. While the conflict lasts, the warnings panel SHALL also show a line stating it.

#### Scenario: Player opens the warnings panel
- **WHEN** the player opens the warnings/info panel from the mod menu
- **THEN** the panel lists the documented conditions that could interfere with the mod's operation, including the sentinel-color and single-chest-marking guidance

#### Scenario: Active warning shown for a marking conflict
- **WHEN** the mod detects more than one chest currently carrying the reserved sentinel color
- **THEN** the mod shows the player an active warning about the conflict, in addition to the static info in the warnings panel

#### Scenario: HUD message when a conflict starts
- **WHEN** at the previous check at most one chest was marked, and now more than one is
- **THEN** the mod shows the HUD message "Auto-Gifter paused: more than one chest is marked. Unmark all but one with G."

#### Scenario: No repeated HUD message while the conflict lasts
- **WHEN** at the previous check more than one chest was marked, and more than one still is
- **THEN** the mod does not show the HUD message again

#### Scenario: No HUD message without a conflict
- **WHEN** at most one chest is marked now, whether or not more than one was marked at the previous check
- **THEN** the mod does not show the HUD message
