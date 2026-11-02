# Changelog

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
