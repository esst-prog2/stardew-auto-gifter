## Purpose

Lets the player preview and edit tomorrow's automatically computed gift plan, and manage the blocklist, from an in-game menu opened via a configurable keybind in the evening.

## ADDED Requirements

### Requirement: Menu opened via configurable keybind
The mod SHALL let the player open its menu at any time during gameplay by pressing a keybind. The keybind SHALL be configurable via the mod's configuration file, and SHALL default to a key that does not conflict with any of the game's default keybindings.

#### Scenario: Player opens the menu with the configured key
- **WHEN** the player presses the configured keybind while playing
- **THEN** the mod's menu opens, showing the daily plan preview and blocklist editor

### Requirement: Plan computed for the next day when the menu is opened
The mod SHALL compute the gift plan for the next in-game day at the moment the player opens the menu, using the farm-wide item pool's contents and the blocklist as they are at that moment. This plan SHALL be what is shown in the preview. The Plan tab SHALL always display a persistent label identifying it as tomorrow's plan (not today's), so the player is not left to infer the timing from memory or from a message they may have already dismissed.

#### Scenario: Plan reflects current farm-wide contents when menu opens
- **WHEN** the player opens the menu on a given evening
- **THEN** the mod computes and displays the gift plan for the following day based on the farm-wide item pool's contents and the blocklist at that moment

#### Scenario: Plan tab always labeled as the next day's plan
- **WHEN** the player opens the Plan tab, at any time
- **THEN** the tab displays a persistent label identifying the shown plan as tomorrow's, not today's

### Requirement: Per-NPC preview with on/off toggle
The menu SHALL show one row per configured NPC that has an item selected for the next day, naming the NPC and the item they are planned to receive. Each row SHALL have a toggle the player can use to exclude that NPC from the next day's automatic gifting.

#### Scenario: Player excludes an NPC from tomorrow's plan
- **WHEN** the player toggles off a row for an NPC in the preview
- **THEN** that NPC receives no automatic gift the next day, even though the mod had selected an eligible item for them

### Requirement: Reason shown for configured NPCs with no gift
For each configured NPC who does not have a planned item for the next day — either because no eligible loved or liked item is available in the farm-wide pool, or because they have already reached the weekly friendship-point-counting gift limit — the preview SHALL show a row explaining why, instead of silently omitting them. If the mod is inactive (no single valid anchor chest currently marked), the preview SHALL show one explanatory message instead of per-NPC rows.

#### Scenario: Configured NPC has no eligible item
- **WHEN** a configured NPC has no eligible loved or liked item anywhere in the farm-wide pool
- **THEN** the preview shows a row for that NPC stating no item is available, instead of omitting them from the preview

#### Scenario: Configured NPC already reached the weekly cap
- **WHEN** a configured NPC has already reached the weekly friendship-point-counting gift limit
- **THEN** the preview shows a row for that NPC stating they have already reached this week's gift limit

#### Scenario: Mod inactive
- **WHEN** no chest is currently marked as the anchor, or more than one is
- **THEN** the preview shows a single message explaining the mod is inactive, instead of per-NPC rows

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
The same menu SHALL open a blocklist editor that lists the game's full catalog of item types (not just items currently in the chest), each shown with a checkbox reflecting whether it is currently on the blocklist.

#### Scenario: Full catalog shown
- **WHEN** the player opens the blocklist editor
- **THEN** every enterable item type in the game's catalog is listed with its current blocklist checkbox state

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
Once the player closes the menu (or ends the day), the plan as edited (including any rows toggled off) SHALL be the plan executed during the next day; the mod SHALL NOT recompute or discard the player's edits when the new day starts.

#### Scenario: Toggled-off NPC stays excluded the next day
- **WHEN** the player toggled off an NPC's row before ending the day
- **THEN** that NPC is not given an automatic gift on the following day, regardless of the farm-wide pool's contents when the day starts

### Requirement: Warnings and info panel
The mod menu SHALL include a dedicated warnings/info panel, opened from the main menu, that documents conditions the player should avoid because they could interfere with the mod's operation — including applying the reserved sentinel color to any chest other than through the mod's own chest-marking action, and marking more than one chest as the mod's anchor at the same time. When the mod detects that more than one chest currently carries the sentinel color, it SHALL additionally show an active warning, not just the static panel, until the conflict is resolved.

#### Scenario: Player opens the warnings panel
- **WHEN** the player opens the warnings/info panel from the mod menu
- **THEN** the panel lists the documented conditions that could interfere with the mod's operation, including the sentinel-color and single-chest-marking guidance

#### Scenario: Active warning shown for a marking conflict
- **WHEN** the mod detects more than one chest currently carrying the reserved sentinel color
- **THEN** the mod shows the player an active warning about the conflict, in addition to the static info in the warnings panel
