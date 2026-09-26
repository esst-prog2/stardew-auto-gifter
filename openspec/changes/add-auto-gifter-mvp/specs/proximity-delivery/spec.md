## Purpose

Executes the finalized daily plan during gameplay by detecting when the player is near a planned NPC and automatically handing over the planned item, without requiring a button press.

## ADDED Requirements

### Requirement: Throttled proximity detection
The mod SHALL check the player's distance to each planned NPC on a throttled schedule (no more often than roughly once per second), rather than on every game tick, to avoid unnecessary performance cost.

#### Scenario: Distance checked on a throttled schedule
- **WHEN** the player is moving around during a day with a pending plan
- **THEN** the mod evaluates player-to-NPC distance on its throttled schedule rather than on every tick

### Requirement: Automatic hand-off within a short distance
When the player comes within a short, fixed distance of an NPC who has a pending, non-excluded item in that day's plan, the mod SHALL automatically remove one unit of that item — preferring the anchor chest if it holds one, otherwise any other farm chest that holds it (see specs/gift-selection - "Anchor chest prioritized before the wider farm pool") — and give it to the NPC, without requiring the player to press a button.

#### Scenario: Player walks near a planned NPC
- **WHEN** the player comes within the fixed proximity threshold of an NPC who has a pending planned item and has not been toggled off
- **THEN** the mod removes one unit of the planned item, preferring the anchor chest, and gives it to that NPC automatically

### Requirement: Delivery skipped if the planned item is no longer available
If, by the time delivery would happen, the planned item's total quantity across the farm's chests has dropped to zero (for example because the player removed or used it after the plan was computed), the mod SHALL skip that delivery without giving anything and without showing a confirmation message.

#### Scenario: Planned item no longer present anywhere on the farm
- **WHEN** the player comes within the proximity threshold of a planned NPC, but the planned item is no longer present in any farm chest
- **THEN** the mod does not hand over any item to that NPC and shows no confirmation message for that NPC

### Requirement: Delivery never drops an item's farm-wide total below one
Immediately before removing a unit of the planned item from a farm chest, the mod SHALL confirm that at least two units of that item are currently available across the farm's chests, so that removing one still leaves at least one behind. If only one unit (or none) remains at that moment — for example because other deliveries or the player's own actions reduced the stock after the plan was computed — the mod SHALL skip that delivery the same way as when the item is entirely unavailable: without giving anything and without showing a confirmation message.

#### Scenario: Only one unit remains at delivery time
- **WHEN** the player comes within the proximity threshold of a planned NPC, but only one unit of the planned item remains across the farm's chests at that moment
- **THEN** the mod does not hand over the item, and shows no confirmation message for that NPC

#### Scenario: Two or more units remain at delivery time
- **WHEN** the player comes within the proximity threshold of a planned NPC, and two or more units of the planned item remain across the farm's chests at that moment
- **THEN** the mod removes one unit and delivers it normally, leaving at least one unit behind

### Requirement: Confirmation message on hand-off
After an automatic hand-off, the mod SHALL show the player a message confirming which item was given to which NPC.

#### Scenario: Confirmation shown after delivery
- **WHEN** the mod automatically hands an item to an NPC
- **THEN** the mod displays a message naming the item and the NPC it was given to

### Requirement: Gift-limit re-checked at delivery time
Immediately before handing over a planned item, the mod SHALL re-check the game's own per-NPC gift tracking. If the NPC has, by that point, already reached the weekly limit of friendship-point-counting gifts (for example because the player gave them a gift by hand earlier that day), the mod SHALL cancel that delivery instead of handing over the item.

#### Scenario: Weekly cap reached before delivery happens
- **WHEN** the player walks near a planned NPC, but that NPC has already reached the weekly friendship-point-counting gift limit by the time the player gets close
- **THEN** the mod does not hand over the planned item to that NPC, even though the player walked past them

### Requirement: In-person notice when nothing is available for a configured NPC
When the player comes within the proximity threshold of a configured NPC who currently has no planned gift — because no eligible loved or liked item is available, or because the weekly friendship-point-counting gift limit is already reached — the mod SHALL show a one-time dialogue box, styled like a normal NPC conversation (the game's own NPC dialogue box, with that NPC as the speaker), explaining that nothing is being given to them right now. This SHALL happen at most once per configured NPC per day, and SHALL NOT happen for an NPC who does have a pending planned item.

#### Scenario: Approaching an NPC with no eligible item
- **WHEN** the player comes within the proximity threshold of a configured NPC who has no eligible loved or liked item available
- **THEN** the mod shows a dialogue box, as if talking to that NPC, explaining nothing is available to give them right now

#### Scenario: Approaching an NPC who already reached the weekly cap
- **WHEN** the player comes within the proximity threshold of a configured NPC who has already reached the weekly friendship-point-counting gift limit
- **THEN** the mod shows a dialogue box, as if talking to that NPC, explaining they've already received this week's gift limit

#### Scenario: Notice shown only once per day
- **WHEN** the player has already seen this notice for an NPC earlier that day and approaches them again
- **THEN** the mod does not show the notice again that day

#### Scenario: No notice for an NPC with a pending gift
- **WHEN** the player approaches a configured NPC who has a pending, non-excluded planned item
- **THEN** the mod does not show this notice (the normal automatic hand-off happens instead)

### Requirement: At most one delivery per NPC per day
Once the mod has delivered a gift to an NPC on a given day, it SHALL NOT deliver another automatic gift to that same NPC for the rest of that day, even if the player passes near them again.

#### Scenario: Player walks past the same NPC twice in one day
- **WHEN** the player has already received an automatic delivery confirmation for an NPC earlier that day, and walks near that NPC again the same day
- **THEN** the mod does not trigger a second automatic hand-off to that NPC that day
