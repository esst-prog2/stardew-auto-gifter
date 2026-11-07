## 1. Update the README demo

- [x] 1.1 Replace only the paragraph under "## 1. The demo" in `README.md` with exactly the following text, and verify that `git diff README.md` changes that one paragraph and nothing else:

  On my farm I open my marked “Auto-Gifter Chest”, which holds 10 Coconuts, 5 Amethysts and 12 Prismatic Shards, close it again, and press F8. On the NPCs tab I have ticked Haley, Abigail and Emily, and the Plan tab shows tomorrow’s plan: “[x] Haley -> Coconut”, “[x] Abigail -> Amethyst” and “Emily: not met yet - talk to them first”, because I have never talked to Emily. Without the Blocklist, Abigail would get a Prismatic Shard, because there are more of them than Amethysts; the Shards are ticked on the Blocklist tab, so she gets an Amethyst instead. I click Abigail’s row to “[ ]”, because I want to give her something myself tomorrow, and go to sleep. The next morning I walk past Haley in town without pressing anything, a message says “Gave Coconut to Haley.”, and the chest now holds 9 Coconuts. When I walk past Abigail and Emily, nothing happens, and the chest still holds 5 Amethysts.

- [x] 1.2 Check every quoted in-game text against the current code once more, and verify that each matches word for word:
  - "Auto-Gifter Chest" (`FarmChestScanner.AnchorChestName`);
  - "[x] Haley -> Coconut" and "[ ]" (`ModMainMenu`, the `{checkbox} {npcName} -> {itemName}` format);
  - "Emily: not met yet - talk to them first" (`Core/PlanRowText`);
  - "Gave Coconut to Haley." (`ProximityDeliveryService`).

## 2. Record and deliver

- [x] 2.1 Add one line to `PLANNING_LOG.md` (Decided by the user). It records the new demo text, and that the claims "Abigail would get a Prismatic Shard without the Blocklist" and "Haley hates Prismatic Shards" come from the game's gift-taste data, not from the mod's code, to be checked in-game when the December screenshots are taken. Verify that the line is appended and no earlier line changed.
- [x] 2.2 Run `openspec validate --strict` for the change, and the build and tests, and verify they are valid and green, with no code change.
- [x] 2.3 Commit on `hw8-demo` and push `hw8-demo` to origin, and verify that `git status -sb` shows the branch tracking `origin/hw8-demo` with nothing to push.

## 3. Later in-game check (not part of this change's commit)

- [ ] 3.1 When taking the December screenshots: untick the Prismatic Shard on the Blocklist tab and open the Plan tab. Verify that Abigail's row shows "Prismatic Shard", and that Haley's row still shows "Coconut" (she hates Prismatic Shards). If either differs, the demo's third sentence must be revised.
