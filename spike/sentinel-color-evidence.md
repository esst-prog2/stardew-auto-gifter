# Spike: does the anchor chest keep its sentinel color after its menu reopens / after save-reload?

## Question

Does a chest marked with the mod's reserved sentinel color `(13, 202, 91)` keep that exact color:

1. after its own `ItemGrabMenu` is closed and reopened, and
2. after the game is saved and the save is reloaded?

## Hypothesis

The vanilla `ItemGrabMenu` constructor recalculates the chest's displayed tint via
`DiscreteColorPicker.getSelectionFromColor` when the chest's menu opens. Since
`(13, 202, 91)` does not match any of the 21 vanilla swatch colors that function
recognizes, the hypothesis was that this lookup returns index 0 and the constructor
overwrites `chest.playerChoiceColor` to `(0, 0, 0)` - meaning simply opening a
marked chest's own menu (no keybind press, no pickup) could be enough to silently
un-mark it.

## Code-location argument (why this diagnostic can detect the hypothesized overwrite)

The diagnostic (`ModEntry.OnMenuChanged`, subscribed to SMAPI's `Display.MenuChanged`
event) logs `chest.playerChoiceColor.Value` from `e.NewMenu`, which is only available
to the handler once `Game1.activeClickableMenu` has already been reassigned to the new
menu instance - i.e. strictly after the `ItemGrabMenu` constructor has returned. In C#,
no code outside a constructor can hold a reference to the object before the constructor
completes, so the color read here reflects the state *after* construction, not before.

This means: a color overwrite happening inside the constructor, while the menu stays
open, or during its close routine would all show up as a changed value the *next* time
this log line fires for that chest (the next menu-open). The diagnostic has no blind
spot for any of those three moments relative to this test.

## Test 1: close and reopen the chest's menu (same play session)

Chest at `{X:70 Y:11}`, save `gagsag_450112948`, spring 2 Y1.

```
[12:58:43 TRACE SMAPI] Context: loaded save 'gagsag_450112948', starting spring 2 Y1, locale set to . Single-player.
[12:58:50 DEBUG Auto-Gifter] [hw4-spike] Chest menu opened at {X:70 Y:11}; color before: {R:13 G:202 B:91 A:255}.
[12:59:08 DEBUG Auto-Gifter] [hw4-spike] Chest menu opened at {X:70 Y:11}; color before: {R:13 G:202 B:91 A:255}.
```

First open (12:58:50) and the reopen 18 seconds later (12:59:08), on the same chest,
both log `(13, 202, 91)`. The color did not change across a close/reopen cycle.

Source: `spike/sentinel-test1-reopen.txt`

## Test 2: save and reload

Same chest at `{X:70 Y:11}`, same save `gagsag_450112948`; the save was reloaded between
the two tests (day advanced from spring 2 to spring 3).

```
[13:01:51 TRACE SMAPI] Context: loaded save 'gagsag_450112948', starting spring 3 Y1, locale set to . Single-player.
[13:02:01 DEBUG Auto-Gifter] [hw4-spike] Chest menu opened at {X:70 Y:11}; color before: {R:13 G:202 B:91 A:255}.
```

The first menu-open after reloading the save logs `(13, 202, 91)` - the color
survived the save/reload cycle.

Source: `spike/sentinel-test2-save-reload.txt`

## Privacy check

The raw logs in `spike/` were checked for personal data (Windows username, Steam ID, email address, machine name) before being committed; the only identifying string they contain is the mod's own public author id (`TamaraT0811`, already public in `manifest.json` and every commit in this repo), so the logs were committed unmodified.

## Result

**The hypothesis is not supported.** In both tests, on the same chest, the sentinel
color `(13, 202, 91)` was read back unchanged:

- after closing and reopening the chest's own menu (test 1), and
- after a save and reload (test 2).

The vanilla `ItemGrabMenu`/`DiscreteColorPicker` interaction described in the
hypothesis does not appear to overwrite the chest's `playerChoiceColor` in either
scenario. This specific mechanism is ruled out as an explanation for the earlier
"anchor chest color sometimes disappears" reports. See `design.md` (Risks section)
and `PLANNING_LOG.md` (2026-10-04 entries) for how this result is tracked going
forward.
