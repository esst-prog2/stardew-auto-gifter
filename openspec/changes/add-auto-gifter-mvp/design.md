## Context

This is a new SMAPI mod with no existing code (see proposal.md - Why). It targets Stardew Valley 1.6.14+ and SMAPI 4.5.x, built against .NET 6, which is the current stable modding target as of this change. There is no existing architecture to fit into; the decisions below establish the initial one.

## Goals / Non-Goals

**Goals:**
- Keep the first version's implementation surface small: one marked anchor chest over a farm-wide item pool, one blocklist, one menu, two keybinds (menu-open, chest-marking).
- Avoid duplicating game state the engine already tracks (gift limits), to prevent the mod's view of "gifts given" from drifting out of sync with the game's.
- Keep per-tick work cheap so the mod has no noticeable performance impact during normal play.
- Make the designated-chest marking impossible to trigger by accident through ordinary play (see Decisions).
- Give the player enough in-game context to recognize and recover from the reserved-color and single-chest-marking constraints, without depending on external documentation.

**Non-Goals:**
- Multiplayer support, seasonal weighting, rarity detection, or gift history/statistics (all explicitly deferred in proposal.md / the project README).
- A configurable proximity distance or a configurable plan-computation time; both are fixed for this version (see Decisions).
- Scanning chests outside the player's farm (for example, in the mines or other locations); the item pool is farm-only (see Decisions).

## Decisions

### Chest marking: a reserved sentinel color applied by the mod, not the vanilla color picker
The mod's anchor chest is identified by a fixed sentinel tint color, `Color(13, 202, 91)`, that the mod itself applies to (or removes from) a chest when the player presses a dedicated keybind while that chest's inventory is open. Marking no longer designates "the" source of items (see "Item pool" below) — it toggles the mod on or off.
- **Why a color, not tile coordinates**: a color travels with the chest if the player moves it; a coordinate-based reference would go stale and require detecting and recovering from a "chest not found at the saved location" state, since the game already supports tinting chests.
- **Why a reserved sentinel value, not a player-picked vanilla color**: the game's chest color picker (`DiscreteColorPicker`) is not a continuous color wheel — it offers exactly 21 fixed preset swatches: `(0,0,0)`, `(85,85,255)`, `(119,191,255)`, `(0,170,170)`, `(0,234,175)`, `(0,170,0)`, `(159,236,0)`, `(255,234,18)`, `(255,167,18)`, `(255,105,18)`, `(255,0,0)`, `(135,0,35)`, `(255,173,199)`, `(255,117,195)`, `(172,0,198)`, `(143,0,255)`, `(89,11,142)`, `(64,64,64)`, `(100,100,100)`, `(200,200,200)`, `(254,254,254)`. `(13, 202, 91)` matches none of these, so the vanilla picker cannot produce it on any other chest — the only way a chest ends up with this exact color is through the mod's own marking action. This is why marking is a dedicated mod action rather than "the player dyes a chest a color of their choosing": if the reserved value were reachable via the vanilla picker, a player could set it on the wrong chest by accident.
- **Why toggle rather than only mark**: pressing the keybind again on the current anchor chest removes the sentinel color, giving the player an explicit, discoverable way to turn the mod off without needing a separate "clear" command.
- **Why restrict marking to farm chests**: keeps the on/off switch physically located where its effect (farm-wide gifting) actually happens, avoiding a confusing setup where the switch lives in, say, a mine chest while the pool it controls is the farm.
- **Alternative considered**: tile-coordinate reference — rejected for the staleness reason above. Letting the player pick any of the 21 vanilla swatch colors themselves and configuring which one means "anchor" — rejected because any of those 21 values could also end up on an unrelated chest the player colors for their own storage-organization purposes, causing false-positive detection; a reserved, unreachable value avoids that collision entirely.

### Item pool: every farm chest, not just the anchor
While the mod is active, it draws gift items from the combined contents of every chest on the player's farm (including farm buildings), not only from the marked anchor chest. The anchor chest's own contents are included like any other farm chest, but marking it no longer restricts where items come from.
- **Why**: with a single dedicated source chest, the blocklist was largely redundant — the player already curated the pool by choosing what to put in that one chest. Pooling every farm chest makes the blocklist the actual safeguard against giving away something the player wants to keep, which is the point of having one. It also matches how players already store items in Stardew Valley: spread across several chests by category, not all in one bin.
- **Why farm-only, not every chest anywhere**: scanning every chest in every location (mines, town, other farms in multiplayer) would be slower to enumerate, and would risk pulling items from storage the player doesn't think of as "mine to give away" — chests in the mines, for instance, are often temporary or shared. Farm-only keeps the pool predictable: it is exactly the storage the player built.
- **Alternative considered**: keep the original single-source-chest design — superseded by explicit request; scanning every chest in every location — rejected for the predictability and performance reasons above.

### Quantity comparison: summed across farm chests, removal from any one
When comparing candidate items for an NPC, the mod sums an item's quantity across every farm chest that contains it, rather than comparing individual chests' stack sizes. When delivering, it removes one unit from whichever farm chest happens to hold it.
- **Why sum rather than compare per-chest stacks**: "how much of this do I have" is naturally a farm-wide question once the pool spans multiple chests; comparing only single-chest stack sizes would make the choice depend on how the player happened to distribute items across bins, rather than on how much they actually have overall.
- **Why removal source doesn't matter**: only one unit is ever needed per delivery, so which specific chest it comes from has no player-visible consequence; picking "any chest that has at least one" avoids adding a chest-preference rule the MVP doesn't need.
- **Alternative considered**: compare only the single largest stack per item (ignoring totals) — rejected as the less intuitive reading of "which item do I have the most of," and discussed explicitly with the user in favor of the summed approach.

### Starter chest delivered to inventory, not placed in the world
The one-time starter chest is added directly to the player's inventory rather than auto-placed at a computed farm tile.
- **Why**: placing an object in the world requires finding a valid, unobstructed tile, which adds real failure modes (no free tile found, placed somewhere awkward like blocking a path). Handing it to the player's inventory sidesteps all of that - they place it themselves, wherever they want, exactly like picking one up normally.
- **Alternative considered**: auto-place near the farmhouse door - rejected for the placement-failure risk above with no real benefit over just handing it over.

### Configured NPCs: in-game selection replaces the config-file list
Which NPCs the mod considers is now chosen from a checkbox tab in the mod menu, persisted per save (the same `IReadOnlyCollection`-backed save-data pattern as the blocklist), instead of the `ConfiguredNpcNames` config.json field from the original implementation.
- **Why**: editing a JSON file to list NPC names by hand is exactly the kind of friction the rest of this mod exists to remove; a checkbox list next to the blocklist and plan preview is consistent with how the mod already handles the blocklist.
- **Alternative considered**: keep both the config field and the in-game list, with the config field as a fallback/seed - rejected as unnecessary complexity; a fresh save simply starts with no NPCs selected until the player opens the menu and picks some.

### TextBox search field: explicit keyboard-dispatcher subscription
Clicking the blocklist search box sets `Game1.keyboardDispatcher.Subscriber` to the box explicitly, in addition to setting its `Selected` property.
- **Why**: initial testing showed the search box accepted clicks but not typed input. `TextBox.Selected`'s setter is documented to "trigger keyboard dispatcher assignment," but observed behavior in-game showed typed characters weren't reaching it; explicitly assigning the dispatcher subscriber on click is the same mechanism vanilla text boxes rely on and removes the dependency on that side effect happening the way assumed.

### Anchor chest regains meaning: priority source, not just a switch
The anchor chest is tried first, alone, for each NPC's selection; the farm-wide pool is only consulted as a fallback when the anchor chest alone has nothing eligible. Delivery prefers removing from the anchor chest when the planned item is present there.
- **Why**: after the earlier pivot to a farm-wide pool, the specific chest the player marked stopped mattering at all beyond being an on/off switch - the player raised this directly, since "which chest" had lost all meaning despite still requiring a real placement/marking step. Prioritizing it restores an intuitive mental model ("put what you actually want to give in here; the rest of the farm is just backup") without reverting the farm-wide fallback that made the blocklist worthwhile in the first place.
- **Why implemented as two sequential selection passes, not pool-merging with weights**: `GiftSelector` already takes a plain pool dictionary; calling it once against an anchor-only pool and, only on a miss, again against the farm-wide pool reuses that same pure logic unchanged - no new selection algorithm, just two calls with different inputs.
- **Alternative considered**: weight the anchor chest's quantities more heavily within a single merged pool (e.g., double-counted) - rejected as a less predictable rule than "try the anchor first, then everything," which is easy to explain and easy to verify by watching what's in that one chest.

### Welcome message paged, not a single long dialogue call
The welcome message is now a `List<string>` passed to `Game1.drawObjectDialogue(List<string>)`, one short message per page, instead of one long string with `\n\n` separators.
- **Why**: the single-string version visibly overflowed the dialogue box's fixed height (confirmed by the user with a screenshot), because `drawObjectDialogue(string)` doesn't paginate long text on its own. The list overload is the same mechanism the base game uses for ordinary multi-page NPC conversations - the game handles the page-break, the next-page arrow, and the click-to-advance interaction itself.
- **Why several short pages rather than fewer long ones**: without being able to measure the dialogue font's line-wrapping in this environment, several clearly-short pages is a safer margin than guessing how much text fits on one page and getting it wrong again.

### Plan-timing reminder made persistent, not one-time
The Plan tab now always shows a "Tomorrow's Plan" label, and the one-time welcome message calls out the next-day timing as important information, rather than relying on a single easy-to-miss mention buried in a sentence.
- **Why**: the only place this was ever explained was one clause in the welcome message, shown exactly once, ever, at the very start of a save - the user found there was no way to be reminded of it later, e.g. right after selecting a new NPC in the NPCs tab and wondering why nothing happened. A persistent label costs nothing to keep showing and doesn't depend on the player remembering something read once, potentially days or weeks earlier.
- **Alternative considered**: a one-time tooltip/popup the first time the NPCs tab is used - rejected as more implementation complexity (tracking a second onboarding flag) for a narrower fix than a label that's simply always there.

### In-person "nothing to give" notice reuses the game's own NPC dialogue box
Approaching a configured NPC with no planned gift shows a real NPC dialogue box (`Game1.drawDialogue(NPC, string)`, confirmed via the game's own source), not just a HUD message or a menu row, matching what the user asked for ("as if you were interacting"). It is gated the same way as delivery - same proximity check, same `PlannedGift.Delivered` flag reused as a generic "resolved for today" marker (already used this way for skipped deliveries) so it fires at most once per NPC per day.
- **Why the real dialogue box over a HUD message**: the user specifically wants it to feel like talking to the NPC, and `drawDialogue` is the exact function the game uses for that; a HUD message (used for successful deliveries) is deliberately lighter-weight/non-blocking, which is right for a routine confirmation but wrong for "explain why nothing happened."
- **Why reuse `Delivered` rather than add a new flag**: it already means "resolved for today, stop re-checking" for the skip cases in `ProximityDeliveryService` (weekly cap reached, item unavailable at delivery time) - a no-gift-notification is the same kind of one-time resolution, just for a `PlannedGift` that was never `Planned` to begin with.
- **Extended slightly beyond what was literally asked**: the user described the "no eligible item" case; the same notice is also shown for "already reached the weekly cap," since both are the same underlying situation from the player's perspective (walked up, nothing happened) and reuse the same code path with only the message text differing.

### Anchor chest sparkles via the game's own sparkle helper
A periodic sparkle effect on the anchor chest reuses `Utility.addSprinklesToLocation` (confirmed via the game's own decompiled source), called roughly every few seconds from the existing `OneSecondUpdateTicked` handler, only when the player is in the anchor chest's location.
- **Why this helper**: it is the exact function the game itself uses for this kind of brief particle burst, so it needs no new art asset and matches the game's native look; the alternative (hand-rolling a `TemporaryAnimatedSprite` loop) would just reimplement what this helper already does.
- **Why only in the player's current location**: `temporarySprites` are a per-location rendering collection; spawning them in a location the player isn't viewing has no visible effect and would be wasted work.
- **Why throttled to every few seconds rather than continuous**: a brief, occasional twinkle reads as "this chest is special" without being visually distracting or spawning particles indefinitely while the player is standing nearby.

### Menu text: truncate single-line rows, word-wrap paragraphs
Row text (NPC/item/plan rows) is shortened with a trailing "..." via `SpriteFont.MeasureString` if it would overflow the menu's content width; paragraph text (Warnings, Settings descriptions) is word-wrapped across multiple lines at the same width instead.
- **Why**: none of the draw calls measured text against the available width, so long NPC/item names or the longer status/explanation strings added over this session could render past the menu's border - reported directly by the user. Truncation suits single, fixed-height, clickable rows (wrapping them would break the row-height math the click handlers rely on); word-wrapping suits free-standing paragraphs that aren't click targets and can freely take more vertical space.
- **Alternative considered**: a larger fixed menu size to fit more text - rejected as not a real fix, since item/NPC names have no fixed maximum length regardless of menu size.

### Anchor-chest-only setting: per-save, menu-editable, disables the fallback outright
A new setting (`AnchorChestOnly`, default `false`) lets the player disable the farm-wide fallback entirely, persisted per save via the same `IModHelper.Data` save-data pattern as the blocklist and NPC selection, and toggled from a new "Settings" tab in the mod menu.
- **Why per-save save-data rather than config.json**: every other in-game-editable setting in this mod (blocklist, configured NPCs) already uses this pattern; config.json is edited outside the game and wouldn't give the "takes effect on the next plan computation" behavior the player can already see with the blocklist and NPC tabs.
- **Why a new tab rather than reusing Warnings**: Warnings is informational (pitfalls, active conflict state); a behavior-changing toggle belongs somewhere explicitly settings-shaped rather than mixed into that.
- **Implementation note**: this reuses the two-call `GiftSelector` pattern from the anchor-priority decision above almost unchanged - when the setting is enabled, `PlanComputer` simply skips the second (farm-wide) call instead of falling back to it.

### Anchor chest is renamed on marking
Marking a chest sets its display name to a fixed, distinctive value (e.g. "Auto-Gifter Chest"); unmarking resets it to the game's plain default chest name, discarding any custom name the player had set before marking.
- **Why**: the cheapest possible way to make the anchor chest recognizable at a glance (hover tooltip, chest menu title) - a one-line property change, no new rendering code.
- **Alternative considered**: preserve and restore the player's prior custom name across mark/unmark - rejected as unnecessary complexity for an edge case (a chest the player had already custom-named before deciding to use it as the anchor); resetting to the plain default is simple and predictable.

### Preview shows a reason, not silence, when a configured NPC gets nothing
A configured NPC who ends up with no planned item still gets a row in the preview, stating why (no eligible item, or the weekly gift cap already reached); an inactive mod (no valid anchor) shows one overall message instead of per-NPC rows.
- **Why**: silently omitting an NPC from the preview is indistinguishable from "the mod forgot about them" - the player has no way to tell "working as intended" apart from "something's broken" without this. This needed the plan's data model to track a reason instead of only "has a gift or doesn't."
- **Alternative considered**: a transient pop-up/HUD message per skipped NPC each time the menu opens - rejected as potentially very noisy (one message per configured NPC with nothing to give, every evening); a persistent row the player can check on demand is quieter and always available.

### Letter-filter checkboxes replace free-text search
Both the blocklist editor and the NPCs tab filter their lists with a row of checkable A-Z starting-letter boxes instead of a typed search term.
- **Why**: the free-text search box (`TextBox` + keyboard dispatcher) needed two separate fix attempts during playtesting and remained fragile; a checkbox-per-letter filter uses exactly the same simple rectangle-click handling already working reliably elsewhere in the menu (tabs, item rows), eliminating the keyboard-input code path entirely. It also naturally extends to the NPCs tab, which a typed search was never added to.
- **Alternative considered**: keep debugging the text box - rejected in favor of removing the risky code path altogether, per the user's own suggestion.

### Minimum reserve: never deliver an item down to zero
An item is only eligible for selection (and, separately, only actually removed at delivery time) if doing so leaves at least one unit of it behind across the farm's chests. In practice: selection requires a farm-wide total of at least 2, and the delivery-time removal re-checks that same "at least 2 before, at least 1 after" condition immediately before removing anything.
- **Why check it twice (planning and delivery)**: checking only at planning time isn't enough, because the stock can change between the evening plan and the next day's delivery (the player might sell or use most of it); checking only at delivery time isn't enough either, because then the preview could show a gift that silently never happens. Checking at both points keeps the preview accurate in the common case and still enforces the guarantee in the rare case where stock changes mid-day.
- **Why exactly one unit, not a configurable reserve**: matches what was asked for directly; a configurable reserve count would be a natural, low-risk follow-up if ever needed, but isn't required for this version.
- **Alternative considered**: enforce the reserve only at delivery time and let the preview occasionally show a gift that then gets silently skipped — rejected because a preview the player edited around (e.g. deciding not to toggle an NPC off because "they're getting something") that then silently fails is a worse experience than simply not proposing it in the first place.

### Ambiguous marking: treat as inactive, don't guess
If more than one chest on the farm carries the sentinel color at the same time (for example, if another mod's chest-coloring feature happens to reach it, or the player marks a second chest without noticing the first is still marked), the mod treats this the same as "no chest marked" — inactive, no item selected for any NPC — rather than picking one of the candidates, and it surfaces a warning to the player.
- **Why**: silently picking one of several sentinel-colored chests (e.g. "first one found while loading") would leave the player unsure which chest is actually controlling the mod, with nothing visibly wrong. Refusing to guess turns a silent, confusing state into a visible, self-explanatory "nothing is happening until you fix this" state.
- **Alternative considered**: pick the first chest found by load order — rejected because that order is not something the player can predict or control, and the ambiguity would be invisible until something unexpected happened.

### Player-facing warnings panel
The mod menu includes a dedicated warnings/info panel — a separate screen from the main menu — documenting pitfalls that could interfere with the mod's operation (the reserved-color scheme, marking only one chest at a time), plus an active, real-time warning when the mod detects a marking conflict (more than one sentinel-colored chest).
- **Why**: the reserved-sentinel-color scheme and the single-chest-marking assumption are non-obvious mechanics that a player has no vanilla-game reason to expect. A discoverable, in-game explanation is more likely to be read than external documentation (a mod page description), and the active warning catches the ambiguous-marking case in real time rather than relying on the player noticing that gifting silently stopped.
- **Alternative considered**: document these pitfalls only in the mod's external README/Nexus page — rejected because a player who doesn't read external docs would have no in-game signal when something goes wrong.

### Gift-limit tracking: read the game's own friendship data, don't duplicate it
The mod relies on the game's existing per-NPC friendship data (the same state that drives the Social Tab's daily/weekly gift checkboxes) to determine whether an NPC has already reached the weekly friendship-point-counting gift limit, instead of maintaining a separate counter.
- **Why**: the game already tracks this per NPC (visible in the Social Tab). A separate mod-side counter could drift from the true state — for example, if the player hand-gives a gift outside the mod's knowledge, a duplicate counter would not see it and could let the mod exceed the limit.
- **Alternative considered**: a mod-maintained counter, persisted per save — rejected due to the drift risk above; also adds save-data migration concerns a read-only check avoids.
- **Open implementation detail**: the exact field/property on the game's `Friendship`/NPC-friendship data that exposes today's and this week's gift counts needs to be located in the decompiled game code or SMAPI's typed API during implementation (see design's Open Questions).

### Proximity detection: throttled polling, not per-tick
Player-to-NPC distance for planned NPCs is checked on a throttled schedule (roughly once per second) rather than in the raw per-tick update event.
- **Why**: `UpdateTicked` fires 60 times/second; checking distance to every planned NPC that often is unnecessary work for a check whose result only needs to be accurate to about a second for a natural-feeling trigger. A once-per-second check is a well-established pattern in other SMAPI mods for this exact kind of proximity work and cuts the number of distance evaluations by roughly 95% relative to per-tick checking.
- **Alternative considered**: raw per-tick `UpdateTicked` distance checks — rejected as unnecessary cost with no player-visible benefit; the README itself flagged this as an open risk, which this decision resolves.

### Proximity threshold: fixed, narrow radius
The distance that counts as "close enough" to trigger an automatic hand-off is a small, fixed radius (on the order of 1-2 tiles - roughly the same distance at which the player could otherwise interact with the NPC directly), and is not exposed as a configuration option in this version.
- **Why**: keeps the trigger predictable and avoids the mod firing while the player is merely passing through an area at a distance; a fixed default avoids adding a tunable the MVP doesn't need.

### Plan computation timing: evening, when the menu is opened, for the next day
The gift plan is computed when the player opens the mod's menu (typically in the evening), using the farm-wide item pool's and blocklist's state at that moment, and describes the *next* day. Edits made in the menu (toggling an NPC off, editing the blocklist) are preserved into that next day; the plan is not recomputed when the new day actually starts.
- **Why**: this matches the intended player experience (review and adjust the plan the night before, then let it play out automatically), and resolves an inconsistency between the project README's "Shape" section (which describes computing "at the start of each day") and its worked "Demo" example (which shows the player reviewing "tomorrow's plan" the evening before and editing it before sleeping). The demo's behavior is treated as authoritative for this version.
- **Alternative considered**: recompute automatically at the `DayStarted` event each morning — rejected because it would silently discard the player's evening edits and no longer match the demo scenario in the README.

### Selection scope: configurable loved-only vs. loved+liked
Whether the fallback to "liked" items is available at all is a configuration setting, `IncludeLikedItems` (default: `true`).
- **Why default to including liked items**: this matches the earlier decision that an NPC with no eligible loved item should still receive something rather than being skipped; defaulting to the more permissive behavior means players who want the stricter loved-only behavior make one explicit change, rather than everyone needing to opt in to get the originally-intended fallback.
- **Alternative considered**: default to loved-only (matching the project README's literal MVP scope) — rejected because it would silently under-deliver relative to the fallback behavior already decided for this change; loved-only remains one config change away for players who prefer it.

### Keybind choices: verified against the vanilla default keybindings
The mod's two keybinds default to keys not used by any of Stardew Valley's default keybindings (movement WASD; C/left-click use-tool; X/right-click check-action; E/Esc menu; F journal; M map; Y emote; Tab shift-toolbar; Shift run; T chat; digits `1`-`0`, `-`, `=` for inventory hotkeys; F4 screenshot; Delete; N dialogue-no):
- **Menu-open keybind**: `F8` (default). Free in the vanilla keybinding set; function-row keys are a common, low-collision choice for mod hotkeys since vanilla only claims `F4`.
- **Chest-marking keybind**: `G` (default, active only while a chest's inventory menu is open). Free in the vanilla keybinding set; chosen as a letter mnemonic for "gift".
- **Why check at all**: a colliding default would either fail to fire (game consumes the key first) or interfere with an unrelated vanilla action, and would be a confusing first impression for a new mod.
- **Alternative considered**: reusing a function key already common among other SMAPI mods without checking — rejected in favor of explicitly cross-referencing the vanilla keybinding list first, since both chosen keys are confirmed free of it. (This does not rule out a collision with some other installed mod's own keybind choice, which no mod can fully control; both keys stay configurable for that reason.)

### Selection UI: build on the game's own menu option elements
The preview and blocklist-editor menus are built using the game's existing reusable option-row UI elements (the same kind of building blocks the game's own Options menu uses for toggles) plus its existing text-input control for the blocklist editor's search field, rather than hand-rolling checkbox rendering, text input, and hit-testing from scratch.
- **Why**: reduces the amount of new UI code needed for a first SMAPI mod, and reuses rendering/interaction behavior the game already provides and keeps visually consistent with the rest of the game's UI. This matters more for the blocklist editor than the preview list: the editor must list, filter, and scroll through the game's entire item catalog (several hundred entries), which is the single largest UI-complexity item in this change — reusing existing widgets for both the rows and the search box keeps that scope from also becoming a from-scratch text-input and scrolling implementation.
- **Alternative considered**: fully custom-drawn rows and a custom text box, with manual click/keystroke handling — rejected as substantially more implementation work for no player-facing benefit at this stage, and the riskiest place to introduce hand-rolled input handling in a first SMAPI mod.

## Risks / Trade-offs

- **[Risk] The anchor chest's color has been observed changing/disappearing during play, more than once.** → Mitigation: `playerChoiceColor` is confirmed (via the game's own source) to be a `NetColor` registered in `Chest.NetFields`, a normal persisted/networked field - the mod never writes to it except in `FarmChestScanner.ToggleMark`, so a change without a logged keybind press would have to come from the game itself, not the mod. Leading hypothesis, from reading the game's own chest-pickup code: picking up a placed chest converts it back into a plain inventory item keyed only by its base item id, which does not carry over per-instance `NetField` state such as `playerChoiceColor` or a custom `Name` - so picking up and re-placing the marked chest would silently un-mark it and reset its name, with no error or warning from the mod (since the mod always re-derives "is this the anchor" live from the chest's current color; there is no separate stored flag, so losing the color always means losing the anchor status too, consistently). This is now documented in the in-game Warnings panel. Not yet 100% confirmed against the full pickup code path; the chest-mark diagnostic log (with tile location) remains in place to confirm on a repeat occurrence.
- **[Risk] The exact game API for reading per-NPC weekly gift counts is not yet confirmed by name/type.** → Mitigation: confirm the field during implementation (first task in tasks.md); if it turns out not to be reliably readable, fall back to a mod-maintained counter as a documented follow-up change, not silently within this one.
- **[Risk] A fixed, non-configurable proximity threshold might feel too aggressive or too lax to some players.** → Mitigation: accepted for this version per the decision above; making it configurable is a natural, low-risk follow-up if needed.
- **[Risk] Evening-computed plans mean the preview can go stale if the player adds/removes items from any farm chest after closing the menu but before sleeping.** → Mitigation: acceptable trade-off for this version; the plan is a snapshot from when the menu was opened, consistent with the README's demo. If the planned item is gone entirely by delivery time, the delivery is silently skipped rather than erroring (see specs/proximity-delivery).
- **[Risk] Enumerating every chest on the farm each time the plan is computed could be slow on farms with very many chests.** → Mitigation: this only runs once per menu-open (an evening action), not per tick, so even a farm with dozens of chests is a one-time, infrequent scan; revisit only if real-world profiling shows otherwise.
- **[Risk] Another installed mod could give chests a continuous (non-discrete) color picker, in theory letting a player reach the reserved sentinel color by chance on an unrelated chest.** → Mitigation: accepted as a low-probability edge case (1 in ~16.7 million possible RGB combinations) outside this mod's control; the reserved-color scheme assumes the vanilla, unmodified 21-swatch chest color picker.
- **[Risk] The blocklist editor's full item-catalog browser (search + per-row checkbox over several hundred items) is the largest single UI-implementation item in this MVP, larger than the daily-plan preview list.** → Mitigation: build it directly on the game's existing option-row and text-input elements (see Decisions) rather than custom-drawn widgets, and give it focused implementation attention as the highest-risk task group during implementation.

## Open Questions

- Exact name/type of the game's per-NPC field(s) that expose "gifts given today" and "gifts given this week" (used to satisfy the gift-limit requirements) - to be confirmed against the decompiled game code or SMAPI's typed data APIs during implementation. Resolving this does not change the specs or the chosen approach, only which exact API call implements it.
