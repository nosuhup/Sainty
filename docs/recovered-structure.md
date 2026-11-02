# SentinelMacro recovery map

## Extraction result

The supplied executable is a self-contained x64 Windows application targeting .NET 10. Its embedded primary managed module is named `MacroV2.dll`.

The module was recovered from the single-file bundle for static inspection:

- Original executable SHA-256: `beb4f31d4e7114b9ac70169fc7240750990a00fd656c2ac13673277aaf1b1158`
- Extracted `MacroV2.dll` size: 1,446,912 bytes
- Extracted module SHA-256: `d318edb77c1ca9ec7f15b6587ad2eb5e3eefd4154a0dc98663eadb70256af523`
- Metadata tables: 67 TypeDef rows and 456 MethodDef rows

This is metadata-level recovery, not a source decompilation. Names and signatures survive in the assembly; comments, source formatting, and the original project file do not.

## Main application types

| Original type | Recovered responsibility / key methods |
|---|---|
| `MacroV2.App` | WPF application entry point |
| `MacroV2.Loader` | Startup flow, Roblox account scan, loading sequence, update confirmation and launch |
| `MacroV2.MainWindow` | Main navigation, settings, profiles, overlays, game scan, account and donation UI |
| `MacroV2.MacroEngine` | Macro timing, input packet creation, global keyboard/mouse hooks, start/stop/toggle |
| `MacroV2.TriggerBotEngine` | Pixel scan loop, target color, activation state, reaction/cooldown timing and input |
| `MacroV2.GameDetector` | Detect running processes and resolve friendly game names |
| `MacroV2.GameInstance` | Game name, detection time and icon source |
| `MacroV2.RobloxGameInfo` | Roblox game name, developer and thumbnail URL |
| `MacroV2.AppSettings` | Persisted application and trigger settings |
| `MacroV2.MacroProfile` | Named macro/trigger settings snapshot |
| `MacroV2.ConfigData` | Last user, user data and installed version |
| `MacroV2.UserDonationData` | Donation total and gamepass state/date data |
| `MacroV2.DonationLeaderboardEntry` | Rank, username, avatar URL and donation total |

## Engine API recovered from method names

### Macro engine

The original `MacroEngine` exposes:

- `Start`, `Stop`, `Toggle`, `ClickLoop`
- `BuildInputPackets`, `FireInput`, `HandleTrigger`
- keyboard and mouse low-level hook callbacks
- `QueryPerformanceCounter` / `QueryPerformanceFrequency`
- `NtSetTimerResolution`, `timeBeginPeriod` / `timeEndPeriod`
- `SetThreadAffinityMask`
- `SendInput` and `MapVirtualKey`

This indicates a high-frequency timed input loop. Exact timing and event semantics still need to be independently reimplemented and tested.

### Trigger bot

The original `TriggerBotEngine` exposes:

- `Start`, `Stop`, `HandleTrigger`, `ScanLoop`
- `SetupInputBuffers`, `PreciseDelay`
- target RGB channels, color tolerance and scan radius
- activation key/mouse, action key/mouse and hold mode
- reaction time, press cooldown, duplicate-ignore window and click duration

The current Sainty UI stores these parameters only. The scanner and input path are not connected.

### Game and profile flow

The original `GameDetector` exposes `DetectGame`, `GetOpenInstances`, `GetFriendlyName`, `CreateGame` and `GetLastPlayedRobloxGameAsync`.

The original profile model includes the name, APS, action mode, hold mode, macro trigger key/mouse, and the trigger bot's color, activation, action and timing parameters.

## Recovered AppSettings field names

The assembly contains these persisted properties:

- `RunOnStartup`, `MinimizeToTray`, `PlayBeep`, `Topmost`, `Randomize`, `Theme`
- `Aps`, `ActionMode`, `HoldMode`, `LastProfileName`
- `TBTargetColorHex`, `TBKeyToPress`, `TBMouseToPress`
- `TBActivationKey`, `TBActivationMouse`, `TBHoldMode`
- `TBTolerance`, `TBReactionTime`, `TBPressCooldown`, `TBDuplicateIgnore`, `TBScanRadius`

Sainty keeps these names in its model to make the migration traceable. A few additional fields are used for a clearer standalone macro configuration.

## Integrations deliberately excluded

The original assembly contains code paths for Roblox user/game metadata, Discord Rich Presence, donation/gamepass services and a Codeberg-based updater. They are not needed for the local-first core and are not copied into this milestone.

The updater is a supply-chain concern: static inspection showed release downloads and replacement behavior but no obvious application-level signature verification. Sainty will require an independently verified update design before supporting self-update.

## Validation limits

- The executable was inspected statically; it was not run.
- The primary assembly metadata and type/method names were parsed.
- The WPF application has not yet been compiled in this Linux analysis environment because the .NET SDK/Windows Desktop targeting pack is unavailable here.
- The next validation gate is a Windows .NET 10 build and smoke test before implementing any low-level input or screen-capture engine.
