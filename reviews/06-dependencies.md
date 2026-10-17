# HW6 review 06: dependencies

## What we looked for

For every dependency in the two `.csproj` files and in `manifest.json`:
- does the package exist;
- does the code actually use it;
- is it pinned to an exact version?

We reviewed both project files and the manifest as of commit `214b567`, with these tools:
- `dotnet list package` for the resolved versions;
- `--vulnerable` and `--outdated` against nuget.org;
- a search of the code.

How "pinned" is meant here: in NuGet, `Version="4.3.2"` means "at least 4.3.2". The lowest match is restored, so it is pinned in practice as long as that version exists. `[4.3.2]` would be a strict pin. There is no `packages.lock.json`.

## What we found

- **F1.** `Pathoschild.Stardew.ModBuildConfig` 4.3.2 (`StardewAutoGifter.csproj:14`):
  - it exists (4.4.0 is also available), it is used (game and SMAPI references, deploy to `Mods`), it is pinned in practice, and it has no known vulnerabilities;
  - **but its analyzer does not run.** Warning CS9057 says the analyzer needs compiler 4.9, while the only installed SDK, 6.0.428, has compiler 4.3. So the package's own warnings are silently skipped.
- **F2.** No `global.json`, so the SDK floats. `<LangVersion>latest</LangVersion>` floats with it, currently C# 10.
- **F3.** Test packages:
  - `Microsoft.NET.Test.Sdk` 17.8.0 (latest 18.11.0), `xunit` 2.6.2 (latest 2.9.3), `xunit.runner.visualstudio` 2.5.4 (latest 4.0.1);
  - all exist, all are used (the test host, the test framework, and test discovery for `dotnet test`), all are pinned in practice, and none has a known vulnerability.
- **F4.** The test project compiles `Core/**/*.cs` directly instead of referencing the mod project.
- **F5.** `EnableHarmony=false`, and no Harmony is used.
- **F6.** `manifest.json`:
  - `MinimumApiVersion` `4.5.0` (since 02c);
  - `Dependencies` and `UpdateKeys` are empty;
  - the minimum game version is not declared anywhere, although the log targets 1.6.14+.
- **F7.** The mod project builds only where the game and SMAPI are installed, and every build copies the mod into `Mods`. This previously caused a DLL lock while the game was running.

## What we decided

| Id | Decision | Step |
|---|---|---|
| F1 | The user installs the .NET 8 SDK. After that, build again and report whether CS9057 is gone and which new warnings appear, without fixing them. | after 06b |
| F2 | Add a `global.json` that asks for at least the 6.0 SDK and allows newer ones (`rollForward: latestMajor`), and set `LangVersion` to exactly 10. The README says that the analyzer needs the .NET 8+ SDK and that the tests are fine with 6. | 06b |
| F3 | The test packages stay at their current versions. | 06a |
| F4 | Kept on purpose. | 06a |
| F5 | Kept on purpose. | 06a |
| F6 | Check the SMAPI documentation for a minimum-game-version manifest field. If there is one, set it to 1.6.14; if not, write it only in the README. Record the source. | 06a |
| F7 | Document in the README: the mod project builds only with the game and SMAPI installed, and the build copies into `Mods`, so do not build while the game is running. | 06a |

The reasons:
- **F3:** the current versions work with the `net6.0` target and the 6.0 SDK, the whole suite is green, and no package has a known vulnerability. Newer majors may drop `net6.0` support; this was not checked. Upgrading is a separate decision, to be taken if the target or the SDK changes.
- **F4:** the tests then build with only the .NET SDK, without the game. The consequence is that everything testable must live in `Core/`.
- **F5:** the mod patches no game code, so Harmony is not needed.

## Status of F1 when this file was written

At first, on 2026-10-17, `dotnet --list-sdks` showed only `6.0.428`. Version 8 was present only as runtimes (`Microsoft.NETCore.App 8.0.15`, `Microsoft.AspNetCore.App 8.0.15`, `Microsoft.WindowsDesktop.App 8.0.15`), not as an SDK. Before step 06a was committed, the user had installed the .NET 8 SDK. `dotnet --list-sdks` now shows `6.0.428` and `8.0.426`, and `dotnet --version` gives `8.0.426`. The F1 build check (a full rebuild, then a list of new warnings) comes after 06b.

## What we changed (step 06a: F3-F7)

F6, how the field was confirmed:
- The SMAPI wiki page (stardewvalleywiki.com, Modding:Modder_Guide/APIs/Manifest) returned HTTP 403 to the fetch.
- So the field was confirmed from SMAPI's own source, the interface `IManifest` in `src/SMAPI.Toolkit.CoreInterfaces/IManifest.cs` (github.com/Pathoschild/SMAPI, `develop` branch). It declares `ISemanticVersion? MinimumGameVersion`, documented as "The minimum Stardew Valley version required by this mod, if any."
- The installed SMAPI 4.5.2 has it too: `smapi-internal/SMAPI.Toolkit.CoreInterfaces.xml` documents `P:StardewModdingAPI.IManifest.MinimumGameVersion` with the same summary.

Changes:
- `StardewAutoGifter/manifest.json`: added `"MinimumGameVersion": "1.6.14"`. The README states the minimum as well.
- `README.md`, new section "6. Building and running the mod":
  - the minimum game and SMAPI versions;
  - the mod project builds only with the game and SMAPI installed;
  - every build copies into `Mods`, so do not build while the game runs;
  - the tests need no game.
- F3, F4 and F5 are recorded in this file and the log. No change.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, 0 warnings. While this step was being made, the user finished installing the .NET 8 SDK (`dotnet --list-sdks` now shows `6.0.428` and `8.0.426`). With no `global.json` yet, this build already used 8.0.426, which is why CS9057 no longer appeared. It was an incremental build that probably did not recompile the C# code, so it says nothing yet about the analyzer's own warnings. That is the F1 check, a full rebuild after 06b.
- `dotnet test`: 80/80 green.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

To check in-game: start the game. SMAPI should still load "Auto-Gifter 0.1.0", with no version error.


## What we changed (step 06b: F2, and the F1 check)

- `global.json` (new, repository root): `"sdk": { "version": "6.0.100", "rollForward": "latestMajor" }`. This asks for at least the 6.0 SDK and uses the newest installed one.
- `StardewAutoGifter/StardewAutoGifter.csproj`: `<LangVersion>latest</LangVersion>` → `<LangVersion>10</LangVersion>`. The C# language level no longer moves when the SDK changes. The test project sets no `LangVersion`, so it uses the default for `net6.0`, which is also C# 10.
- `README.md`, section 6: `global.json` and the fixed C# 10. The build package's analyzer needs the .NET 8 SDK or newer; with only the 6.0 SDK the build shows CS9057 and the analyzer's checks are skipped. The unit tests are fine with the 6.0 SDK.

F1 check, with the .NET 8 SDK installed:
- `dotnet --list-sdks`: `6.0.428`, `8.0.426`.
- `dotnet --version` in the project folder: `8.0.426`. With `global.json` and `rollForward: latestMajor`, the newest SDK is picked.
- `dotnet build StardewAutoGifter/StardewAutoGifter.csproj --no-incremental`, a full rebuild: **0 errors, 0 warnings. CS9057 is gone.**
- The analyzer really runs. A rebuild with `-p:ReportAnalyzer=true -v:d` shows:
  - the compiler `C:\Program Files\dotnet\sdk\8.0.426\Roslyn\bincore\csc.dll` with `/langversion:10`;
  - `/analyzer:…\pathoschild.stardew.modbuildconfig\4.3.2\analyzers\dotnet\cs\SMAPI.ModBuildConfig.Analyzer.dll`;
  - the analyzer report lists `StardewModdingAPI.ModBuildConfig.Analyzer.NetFieldAnalyzer (AvoidNetField)` and `StardewModdingAPI.ModBuildConfig.Analyzer.ObsoleteFieldAnalyzer (AvoidObsoleteField)`, both executed, with a total analyzer time of 0.043 s.
  - So "0 warnings" means the analyzer ran and found nothing, not that it was skipped.
- No new warnings appeared, so there is nothing to list or decide.

Checks:
- `openspec validate --strict`: valid.
- `dotnet build`: 0 errors, 0 warnings, with SDK 8.0.426.
- `dotnet test`: 80/80 green, with SDK 8.0.426.
- `GiftLimitPolicy.cs` lines 21 and 29 unchanged.

**In-game status (2026-10-17):** not run, in-game verification pending: the 06a load check (MinimumGameVersion). The homework does not require running it.
