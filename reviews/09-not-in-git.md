# HW6 review 09: what should not be in git

## What we looked for

Things that should not be in git, across the whole history (every commit, every branch):
- build output (`bin/`, `obj/`, `.dll`);
- large files;
- logs and what they contain (user name, Steam ID, paths);
- keys or `.env` files;
- personal data;
- IDE files;
- and whether `.gitignore` covers these.

Scope: all branches (`main`, `hw4-spike`, `hw5-usable`, `hw6-review`, and their `origin` copies) and every revision, as of commit `2e466b7`.

How we searched:
- `git rev-list --all --objects` for the 148 paths ever committed and the blob sizes;
- `git grep` over every revision for:
  - user and path strings (`horva`, `C:\Users`, `D:\SteamLibrary`, `AppData`, `OneDrive`);
  - personal data (the author's name and e-mail, `@gmail.com`);
  - Steam IDs (`Steam ID`, `SteamID`, the 64-bit form `7656119…`) and the save name;
  - secrets (`password`, `api key`, `secret`, `token`, private keys, `ghp_…`);
- `git log --all` for the commit identities;
- `git status --ignored` for what is ignored now.

## What we found

- **G1. Build output.** Never committed in any revision: no `bin/`, `obj/`, `.dll`, `.pdb`, `.exe`, `.nupkg` or `.binlog`. `.gitignore` covers `bin/` and `obj/`, and right now only those build folders are ignored.
- **G2. Large files.** None. The largest blob is `PLANNING_LOG.md`, at about 51 KB.
- **G3. Keys and secrets.** No `.env`, `.key`, `.pem` or `.pfx` file in any revision, and no password, API key, secret, token, private key or GitHub token in any content.
- **G4. IDE files.** None ever committed. `.gitignore` covers `.vs/`, `.vscode/`, `*.user` and `*.suo`.
- **G5.** The four raw HW5 test outputs (`hw5/red-run.txt`, `green-run.txt`, `red-run-planning.txt`, `green-run-planning.txt`, lines 3-4 of each) contain the Windows user name and the full path: `C:\Users\<user>\OneDrive\Desktop\stardew-auto-gifter\StardewAutoGifter.Tests\bin\Debug\net6.0\…`. They are on `origin/main` and `origin/hw5-usable`. HW5's rule says these raw outputs stay unmodified.
- **G6.** Every commit (65 in all refs) carries the author name `TamaraT0811` and the author's personal e-mail address. In 53 commits it is both the author and the committer; in the 12 GitHub merge commits the committer is `noreply@github.com`. The pushed commits already show it on GitHub.
- **G7.** The two raw SMAPI logs in `spike/` contain the game's install path (`D:\SteamLibrary\steamapps\common\Stardew Valley\Mods`) and the save name (`gagsag_450112948`: the farmer's name plus the game's random id). They contain no Windows user name and no Steam ID, so the claim in `spike/sentinel-color-evidence.md:66` holds for them. That check was never done for the HW5 outputs (G5).
- **G8.** The assistant's own checklists in `reviews/02`, `reviews/03` and `reviews/08` use the machine-specific path `D:\SteamLibrary\…\Mods\…`.
- **G9.** `.claude/` is tracked: 12 files, the OpenSpec-generated `commands/opsx/*.md` and `skills/*/SKILL.md`, with no personal data in them. A future `.claude/settings.local.json` (Claude Code's per-machine settings) was not ignored.
- **G10.** Patterns not in `.gitignore` yet: `.idea/` (Rider), `TestResults/` (`dotnet test --logger`), `*.binlog` (`dotnet build -bl`), `.env`. None of these was ever in the repository.

## What we decided

| Id | Decision |
|---|---|
| G1-G4 | Record: nothing to change. |
| G5 | Option a): the four HW5 outputs stay unmodified, and the risk is recorded here: a Windows account name and the local folder layout. No history rewrite. |
| G6 | The user sets the e-mail address for future commits personally. The assistant does not touch the git config. The existing history is not rewritten. |
| G7 | The spike logs stay. The risk is low: an install path and a save name, with no account name and no Steam ID. |
| G8 | From now on, the assistant writes game paths as `<game folder>\Mods\…`. The existing review files stay as they are. |
| G9, G10 | Add `.claude/settings.local.json`, `.idea/`, `TestResults/`, `*.binlog` and `.env` to `.gitignore`. `*.log` is not added, because the project's own logs are `.txt` files and are in git on purpose. |

## What we changed (step 09a)

- `.gitignore`: added `.idea/` to the IDE section, a new "Test and build logs" section (`TestResults/`, `*.binlog`), and a new "Local settings and secrets" section (`.claude/settings.local.json`, `.env`).
- This file, and one line in PLANNING_LOG.md.
- No code change, and no change to the git config or the history.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, 0 warnings.
- `dotnet test`: 90/90 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.
- `git status` shows no previously tracked file newly ignored.
