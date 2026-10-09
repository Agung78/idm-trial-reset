# IDM Diagnostics

[![license](https://img.shields.io/github/license/J2TeaM/idm-trial-reset.svg?maxAge=2592000)](LICENSE)

A small, read-only Windows diagnostics utility for [Internet Download Manager (IDM)](https://www.internetdownloadmanager.com/). It helps you collect and share accurate information when troubleshooting a valid IDM installation.

This project does **not** reset trials, register or license IDM, or modify IDM's registry data or permissions. A valid IDM license is required to use IDM; see the official pages below.

## What it does

- Detects whether IDM is installed, and where (`IDMan.exe` path).
- Reports the installed IDM version, when available.
- Reports Windows version and architecture.
- Warns about leftovers from older versions of this project (a legacy startup entry or temp files). It reports them only and never removes them.
- Exports a diagnostic report (JSON or plain text): tool version, UTC timestamp, OS/arch, IDM info, and per-check status.
- Never reads or reports license/user data (serial, name, email); report text is additionally redacted.

Scope is limited to diagnostics (and, in the future, safe backup/cleanup of this tool's own files). See [DEVELOPMENT_PLAN.md](DEVELOPMENT_PLAN.md).

## Official IDM links

- Buy: <https://www.internetdownloadmanager.com/buy.html>
- Register: <https://www.internetdownloadmanager.com/register.html>
- Support: <https://www.internetdownloadmanager.com/support.html>

## Requirements

- .NET 8 SDK (to build). The app itself targets Windows 10/11 (`net8.0-windows`, WinForms).
- Core library and tests build and run on any OS supported by .NET 8.

## Build, test, run

```bash
# Core library + tests (any OS)
dotnet test tests/IdmDiag.Tests

# Windows application (Windows, or other OS with targeting enabled for compile-only)
dotnet build src/IdmDiag.Windows -c Release
dotnet run --project src/IdmDiag.Windows

# Compile-only on macOS/Linux
dotnet build src/IdmDiag.Windows -p:EnableWindowsTargeting=true
```

Layout:

- `src/IdmDiag.Core`: checks, report model, exporters (no Windows dependencies).
- `src/IdmDiag.Windows`: WinForms UI and read-only Windows registry/file implementations.
- `tests/IdmDiag.Tests`: xUnit tests using fakes.

## Status

Early development on the `development` branch. No release binaries are published yet.

## History and credits

This repository was originally "IDM trial reset" by Juno_okyo (J2TEAM). The trial-reset, fake-registration and registry-permission-locking functionality has been removed. Original credits are preserved:

- Juno_okyo / J2TEAM (original project author)
- idmresettrial (original author of the legacy tool)

See [CHANGELOG.md](CHANGELOG.md).

## License

[MIT](LICENSE) License. Copyright (c) 2016 Juno_okyo.
