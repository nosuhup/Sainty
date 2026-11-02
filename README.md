# Sainty

A clean-room reconstruction of the desktop functionality recovered from the supplied `SentinelMacro.exe`.

This is a new implementation, **not the original source code**. The executable contains a self-contained .NET 10 application and its primary managed assembly could be extracted and inspected for metadata. Original comments, formatting, project files and some implementation details are not recoverable from that metadata alone.

## Current build

- WPF desktop shell with Overview, Clicker, Trigger Bot, Games, Profiles and Settings sections.
- Local JSON settings and profile persistence under `%LOCALAPPDATA%\Sainty`.
- Local process-name game discovery.
- Recovered settings and profile field names used as the starting data model.
- No external service calls in this milestone.

## Next engineering milestones

1. Rebuild and verify the timing/input engine.
2. Reimplement the pixel scan pipeline behind an isolated, testable service.
3. Recreate the window overlay and theme behavior.
4. Review updater, Discord and account integrations independently before adding any network code.

The macro and trigger pages currently store configuration only. They do not send input or scan the screen yet.

## Build

Requires Windows and the .NET 10 SDK with the Windows Desktop workload.

~~~powershell
dotnet restore Sainty.csproj
dotnet build Sainty.csproj -c Release
~~~

## Recovery notes

See [docs/recovered-structure.md](docs/recovered-structure.md) for the extracted assembly map and the verified names of the main application types.

The original executable was unsigned. Its updater downloaded release artifacts without an obvious application-level signature check. Sainty intentionally does not silently replace itself or run downloaded code.

Roblox metadata, donation/account services and Discord RPC are intentionally omitted from this first pass. No unknown endpoint or credential flow is copied into the new project.
