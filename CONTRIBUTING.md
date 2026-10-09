# Contributing

## Workflow

- Work on the `development` branch (or topic branches off it). Do not commit directly to `master`.
- Keep changes small and focused; include tests for new logic.
- Build and test before opening a pull request:

```bash
dotnet test tests/IdmDiag.Tests
dotnet build src/IdmDiag.Windows -p:EnableWindowsTargeting=true   # compile-only off Windows
```

## Scope

In scope: read-only diagnostics, exportable reports, official IDM links, and (later phases) reversible, user-confirmed backup/restore and cleanup of this tool's own files, plus HTTPS-only verified update checks. See [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md).

Out of scope (will not be accepted):

- Trial reset, license/registration emulation or any license bypass.
- Registry ownership or ACL changes, or permission locking.
- Autorun/startup entries that perform state changes.
- Telemetry, plugin systems, auto-reset behavior.
- Reading, logging or exporting serials, names, emails or other license/user data.

## Code guidelines

- Registry/file/environment access goes through `IRegistryReader`, `IFileSystem`, `IEnvironmentInfo` so Core stays testable cross-platform.
- Checks must catch failures and report Warning/Error rather than crash.
- No new dependencies where the .NET standard library suffices.
- UI changes must remain high-DPI and keyboard accessible; verify manually on Windows.

## License

Contributions are licensed under the project's [MIT license](LICENSE).
