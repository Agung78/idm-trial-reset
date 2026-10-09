# Changelog

Format based on [Keep a Changelog](https://keepachangelog.com/).

## [Unreleased]

### Added
- C# / .NET 8 solution: `IdmDiag.Core` (read-only diagnostics, report model, JSON/text exporters), `IdmDiag.Windows` (WinForms UI, high-DPI), `IdmDiag.Tests` (xUnit).
- Checks: IDM installation, executable path, version, Windows version/architecture, legacy-tool leftovers (reported as warnings only).
- Redaction of sensitive values in all reports.
- README, SECURITY, CONTRIBUTING documentation.

### Removed
- Trial-reset, fake-registration and license-bypass functionality, including the AutoIt sources and the bundled `.reg` payloads.
- Registry ownership changes and SetACL-based permission locking (bundled `SetACLx32.exe` / `SetACLx64.exe`).
- The silent `/trial` autorun mode and its startup entry creation.
- The unauthenticated HTTP Pastebin update check.
- Bundled icon asset.

### Notes
- Original license (MIT) and J2TEAM / Juno_okyo attribution are preserved.
