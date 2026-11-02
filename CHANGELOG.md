# Changelog

## 1.0.1

Hotfix release focused on making the Windows release package available directly from GitHub Releases.

- Attach the self-contained Windows x64 ZIP to the GitHub release automatically
- Publish a SHA-256 checksum alongside the package
- Keep the Actions artifact as a secondary build output

## 1.0.0

First release of Sainty.

- WPF .NET 10 x64 desktop application
- Macro scheduling/profile configuration
- Input-independent trigger scanner with deterministic validation
- Trigger tolerance, cooldown, duplicate filtering, and restart-state validation
- Game detection and local configuration/profile storage
- Windows CI and versioned self-contained release packaging

### Release scope

This release intentionally keeps the macro and trigger components input-independent: the scanner consumes supplied pixel samples and emits detection events rather than capturing the desktop or synthesizing OS input.
