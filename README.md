# Wandy

clean-room reconstruction of the functionality recovered from `SentinelMacro.exe`.

> this is **not** the original source code. the supplied executable was a self-contained .NET application, so original comments, project metadata, names, and some implementation details cannot be recovered exactly.

## current scope

- WPF desktop shell
- JSON configuration/profiles
- high-resolution macro timing
- keyboard/mouse input through Win32 `SendInput`
- pixel-based trigger scanner
- Roblox process detection
- optional Windows startup entry
- updater abstraction with explicit confirmation

## build

requires Windows and the .NET 10 Windows SDK/runtime.

```powershell
dotnet restore
dotnet build -c Release
```

## safety / migration notes

The original binary was unsigned. Its updater also downloaded release artifacts without an obvious application-level signature check. Wandy intentionally does **not** silently replace itself or execute downloaded code.

Network integrations from the binary (Roblox metadata, donation/account services, Discord RPC) are left out of this first clean-room pass rather than copying unknown endpoints into the new project.
