# Process log

One row per step, added together with that step's commit, starting with HW9 (branch `hw9-page`).

| Date | Step | What the tool did | What the user decided |
|---|---|---|---|
| 2026-11-12 | Create the HW9 branch | Switched to `main`, pulled (fast-forward `5466022..2915ca1`, bringing in the HW8 merge, PR #9), created `hw9-page` from `2915ca1`, and confirmed it with `git branch`. | Create `hw9-page` from an up-to-date `main`. |
| 2026-11-12 | Set up this process log | Created `PROCESS.md` with this table and added one line to `PLANNING_LOG.md`. | From now on, commit after each step, and add a row to `PROCESS.md` for each step: the step, what the tool did, and what the user decided. |
| 2026-11-12 | Install the gstack skill pack (Garry Tan) | Checked the prerequisites (Git and Node.js present, Bun missing) and stopped to ask. Then installed Bun 1.4.4 with the official script (`irm bun.sh/install.ps1 \| iex`), cloned gstack into the user-level skills folder and ran `./setup --no-timeline-stop-hook` (gstack 1.99.2.0, Claude host). The first run's Chromium download failed because drive C: was full (141 MB free); after the user freed space, a second run installed the bundled browser. The user's `settings.json` is unchanged (no hooks registered). `/office-hours` is available in this session without restarting VS Code. `/cso` is unavailable (it needs the Visual Studio 2022 C++ Build Tools). Nothing in this repository was changed by the install. | Option 1: install Bun with the official script, then gstack with `./setup --no-timeline-stop-hook`, without a `CLAUDE.md` section and without team mode; the Chromium download is fine. Freed disk space so that the browser could be installed. |
