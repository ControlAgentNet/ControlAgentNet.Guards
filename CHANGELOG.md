# Changelog — ControlAgentNet.Guards

All notable changes to this package are documented here.

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Versioning follows [Semantic Versioning](https://semver.org/).

---

## [Unreleased]

### Added
- `RiskDenyGuardTests` — unit tests covering all risk level thresholds, exemptions (by Id and Name), case-insensitive matching, null descriptor, and configurable `Order`.
- `ToolAllowlistGuardTests` — unit tests covering allowlist matching, denial of unlisted tools, empty-allowlist modes (`TreatEmptyAllowlistAsAllowAll`), case-insensitive matching, null descriptor, and configurable `Order`.

---

## [0.1.0-alpha] — TBD

### Added
- `RiskDenyGuard` — denies tool execution when `CapabilityRiskLevel` meets or exceeds a configurable threshold. Supports per-tool exemptions via `ExemptToolIds`. Uses `FrozenSet` for O(1) look-ups.
- `ToolAllowlistGuard` — only permits execution of explicitly listed tool IDs. Supports `TreatEmptyAllowlistAsAllowAll` flag for development convenience.
- `GuardExtensions` — `AddRiskDenyGuard()` and `AddToolAllowlistGuard()` extension methods on `IServiceCollection`.
