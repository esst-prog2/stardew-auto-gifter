# Stardew-auto-gifter
Programming course Survey MA

# Stardew Valley Auto-Gifter

A SMAPI mod for the game Stardew Valley that automates daily gift-giving to villagers, so you don't have to remember everyone's favorite items or walk out of your way to deliver them by hand.

## 1. The demo

In the evening, before ending the day, I open the mod's menu and see tomorrow's plan: "Abigail → Grape (12 in chest), Sebastian → Beer (3 in chest), Penny → Strawberry (8 in chest)". I uncheck the Sebastian row because I don't want to give him anything tomorrow. I go to sleep, and a new day starts. The next day I play normally, mining, farming, and as I walk past Abigail in town, the mod automatically pulls a grape from the chest and hands it to her, a message confirms "Gave Grape to Abigail". The prismatic shard sitting in my chest is never touched, because I added it to the blocklist.

## 2. The shape

```
in     the contents of a designated chest (items + quantities) + a
       blocklist (item names that must never be taken) + the player's
       daily position/proximity to NPCs + the game's built-in list of
       each NPC's liked/loved/disliked items
out    automatic gift delivery to listed NPCs when the player gets
       close to them, plus a daily preview/plan that can be edited
in between   at the start of each day, picks for each NPC the item they
             love that exists in the largest quantity in the chest and
             is not on the blocklist, throughout the day, tracks the
             player's distance to NPCs, and if a gift is still due for
             that NPC today, triggers the hand-off
```

## 3. The size

**First useful version**
- SMAPI mod written in C#
- a designated chest (identified by color or placement)
- a blocklist, editable by hand (config file or a simple in-game menu)
- selection logic: among an NPC's loved items, pick the one present in the chest in the largest quantity that isn't blocklisted
- has a connection to a list of all the characters and the items they love/like/dislike
- respects the daily/weekly gifting limit (max 1 gift/day/NPC, max 2 that count toward friendship points/week)
- automatic hand-off when the player gets close to a listed NPC
- a daily preview list, with each row toggleable on/off before the day starts

**Not this term**
- seasonal preference (prioritizing fruits/vegetables in season)
- automatic detection of "rare" items (blocklist stays manual for now), the game itself has no rarity system, so a future version could learn one from an AI trained on my own playthrough data, based on how often an item has shown up in my inventory
- combining multiple weighting factors for selection (for now: quantity only)
- multiplayer compatibility
- long-term history/statistics of past gifts

## 4. How we would know it works

- If I go to an NPC with three loved items in the chest at different quantities (example: 2 grapes, 8 strawberries, 1 beer), the mod selects the strawberries, because that's what I have the most of.
- If an item is on the blocklist, even if it's the NPC's favorite and the most plentiful in the chest, the mod skips it and falls back to the next-best option.
- If an NPC has already received their two friendship-point-counting gifts for the week, no further automatic gift is given to them that day, even if the player walks past them.

## 5. What could stop this

- Similar mods already exist (example: "Easy Gifting," which delivers liked/loved gifts from a chest to a nearby NPC via a manual button press). These are not automatic and don't use quantity-based selection, so this project stays distinct but their source is worth reviewing for reference.
- C#/SMAPI is new for me, learning the modding API (fetching NPC gift preferences, tracking player position, handling item quantities) will take some time, so it's best started early.
- Open question: whether the SMAPI API offers an efficient way to check player-NPC distance on a tick event without a meaningful performance cost, worth testing with an early prototype.
- No personal or sensitive data is involved, everything comes from the player's own save file (in this case, mine).
