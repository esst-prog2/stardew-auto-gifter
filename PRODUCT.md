# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Stack

Plain static HTML and CSS in the repository's `docs/` folder, published with GitHub Pages. No build step and no framework.

## Users

Stardew Valley players who browse mod sites (Nexus-style) looking for quality-of-life mods. They decide within a few seconds whether a mod is worth installing, and they want to know what it does, whether it is safe for their save and items, and how to install it.

## Product Purpose

Auto-Gifter is a SMAPI mod (version 0.1.0) that gives villagers their daily gift for the player. In the evening the player sees and can edit tomorrow's gift plan; the next day, when the player walks past a planned villager, the mod hands over the gift from the farm's chests. The promo page exists so a player understands this, trusts it, and tries it: its action is the v0.1.0 Release at https://github.com/esst-prog2/stardew-auto-gifter/releases/tag/v0.1.0.

## Positioning

No button to press: you walk past a villager and the gift is handed over (within 1.5 tiles), with the message "Gave Coconut to Haley." For each villager it picks the loved item (or, if enabled, a liked item) that the player has the most of, looking in the marked chest first and only then in the other farm chests. It never gives away the last unit of an item, respects the game's gift limits, and skips blocklisted items. Similar mods exist (some send gifts from a chest on a key press, others remind you who to gift); the page does not claim Auto-Gifter is the only gifting mod.

## Operating Context

- Requirements: Stardew Valley 1.6.14 or newer, SMAPI 4.5 or newer, single player.
- Install: unzip the Release zip into the game's `Mods` folder. On first stepping onto the farm the player gets a chest, places it on the farm (not in the farmhouse), opens it and presses G to mark it (it is renamed "Auto-Gifter Chest"); F8 opens the mod menu with NPCs, Plan, Blocklist and Settings tabs. Gifting starts the next day.
- The page content is fixed in `docs/designs/promo-page.md` (approved), with a source for every claim.

## Capabilities and Constraints

- Quoted in-game texts must match the code word for word: "Auto-Gifter Chest", "[x] Haley -> Coconut", "Emily: not met yet - talk to them first", "Gave Coconut to Haley.".
- Known limits the page must state: single player only; F8 does nothing while a chest or another menu is open; plans always start the next day; a Blocklist change only takes effect when a plan is next computed (reopen F8 to update tomorrow's plan; a gift already planned for today is not affected).
- Open product decision: whether the mod should apply Blocklist changes sooner (recompute tomorrow's plan, or re-check at delivery).

## Brand Commitments

- Name: "Auto-Gifter". All page text in English.
- The page does not imitate Stardew Valley's own art, logo, fonts or look (ConcernedApe's intellectual property). Only the player's own gameplay captures may be used.
- It must read as a mod made by a player for players, not as a tech or SaaS product page.

## Evidence on Hand

- Code-verified texts and behavior (see the design doc for sources), 101 unit tests, the OpenSpec specs, `PLANNING_LOG.md` and `reviews/`.
- The v0.1.0 Release (pre-release) with one asset, `StardewAutoGifter.0.1.0.zip`.
- Absent: in-game screenshots and the hero clip (planned for December), user reviews, download counts. None of these may be invented; until they exist, the page shows labelled placeholders.

## Product Principles

1. Show the walk with no button press before anything else.
2. Earn trust right after: the player's rare items stay safe, and the page says plainly what the mod does not do.
3. Every sentence is true and sourced; nothing is invented.
4. Getting it running takes a few steps, written in the order a player does them.
