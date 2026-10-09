# IDM Trial Reset Modernization Plan

## Important scope

This plan does **not** modernize trial-reset, fake-registration, license bypass, or registry-permission-locking behavior.

The refreshed project will become a legitimate Windows diagnostics and cleanup utility for users with a valid IDM installation or license.

## Before executing any implementation step

Create the development branch from the current `main` branch:

```bash
git fetch origin
git switch main
git pull --ff-only origin main
git switch -c development
git push --set-upstream origin development
```

All implementation work must happen on `development`. Do not commit modernization changes directly to `main`.

## Phase 1 — Baseline and repository cleanup

- Record the current project behavior and supported Windows versions.
- Add `SECURITY.md`, `CHANGELOG.md`, and contributor documentation.
- Remove obsolete release/build artifacts that are not required for the new utility.
- Preserve the license and attribution information.
- Document the exact supported scope: diagnostics, backup, cleanup, and official IDM links.

Acceptance criteria:

- A clean checkout explains how to build and run the project.
- No documentation claims that the tool resets trials or registers IDM.
- `main` remains unchanged; all work is on `development`.

## Phase 2 — Replace bypass functionality

Remove from the product:

- Trial reset logic.
- Silent `/trial` autorun mode.
- Fake registration logic.
- `idm_trial.reg` and `idm_reg.reg` payloads.
- Registry ownership changes to `S-1-0-0`.
- SetACL-based registry permission locking.
- Automatic trial-reset startup entries.

Retain only narrowly scoped operations that are necessary for the utility’s supported features.

Acceptance criteria:

- The application cannot reset an IDM trial.
- The application cannot fake an IDM registration.
- The application does not change CLSID ownership or ACLs.
- The application does not create a startup task for trial resetting.

## Phase 3 — Implement diagnostics

Add read-only diagnostics for:

- IDM installation detection.
- IDM executable path.
- Installed IDM version, when available.
- Windows version and architecture.
- Whether this utility has its own startup entry or temporary files.
- Relevant configuration presence without exposing or logging sensitive values.

Add an exportable diagnostic report with:

- Tool version.
- Timestamp.
- OS and architecture.
- Detected IDM information.
- Per-check status and error details.

Acceptance criteria:

- Diagnostics do not modify IDM state.
- Missing IDM, inaccessible registry data, and unexpected versions are reported as errors or warnings rather than crashes.
- Sensitive values are redacted from reports and logs.

## Phase 4 — Add safe backup and cleanup

Implement only reversible, user-visible actions:

- Back up supported settings before modifying them.
- Use timestamped backup directories.
- Provide restore from a selected backup.
- Remove only temporary files created by this tool.
- Remove only this tool’s own startup entry.
- Provide a dry-run preview before cleanup.
- Require confirmation for every state-changing operation.

Acceptance criteria:

- Cleanup cannot target arbitrary registry paths.
- Backup and restore failures stop the operation and show the failed step.
- No destructive operation runs silently.
- Restore is tested against a generated backup.

## Phase 5 — Secure update handling

Replace the current unauthenticated Pastebin HTTP check:

```autoit
InetRead('http://pastebin.com/raw/uYr0cstV', 1)
```

Use:

- HTTPS only.
- GitHub Releases or a versioned release manifest.
- Request timeouts and clear offline behavior.
- SHA-256 verification for downloaded artifacts.
- Signed releases where the selected packaging method supports them.
- No automatic execution of downloaded files.

Acceptance criteria:

- HTTP endpoints are rejected.
- Network failure does not break diagnostics or cleanup.
- An update is never treated as valid solely because a remote version string is higher.

## Phase 6 — Modernize the Windows application

Preferred implementation:

- C# on the current supported .NET release.
- WPF or WinUI only if a graphical application is required.
- No new dependency where the Windows API or .NET standard library is sufficient.

UI sections:

- Status.
- Diagnostics.
- Backup and restore.
- Cleanup.
- Settings/about.

Required UX properties:

- High-DPI support.
- Keyboard navigation.
- Clear success, warning, and failure states.
- No unconditional “success” message after an unverified operation.
- Links to official IDM purchase, registration, and support pages.

## Phase 7 — Testing and release

Add focused tests for:

- Version parsing.
- Installation detection.
- Backup naming and restore boundaries.
- Cleanup path restrictions.
- Update manifest validation.
- Redaction of sensitive values.
- Failure handling and rollback.

Run the actual Windows application manually for:

- Fresh install.
- IDM absent.
- IDM present.
- Normal diagnostics.
- Failed network connection.
- Backup, cleanup, and restore.
- Standard user and administrator execution where supported.

Release requirements:

- Reproducible build instructions.
- Signed release binaries where available.
- SHA-256 checksums.
- Changelog entry.
- Security policy.
- No trial-reset or fake-registration behavior in source, payloads, documentation, or releases.

## First implementation milestone

The first milestone should be intentionally small:

1. Create and push `development` from `main`.
2. Replace the current README with the supported scope.
3. Remove bypass and ACL-locking entry points.
4. Add read-only IDM installation diagnostics.
5. Add a basic diagnostic report.
6. Add a smoke check covering IDM-present and IDM-absent cases.
7. Build and manually verify the Windows executable.

Do not add auto-reset, license emulation, registry ACL manipulation, telemetry, or a plugin system.
