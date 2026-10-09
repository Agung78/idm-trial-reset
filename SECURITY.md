# Security Policy

## Reporting a vulnerability

Please report security issues privately via GitHub's "Report a vulnerability" (Security tab) for this repository. Do not open public issues for vulnerabilities. Include affected version, reproduction steps, and impact. Expect an initial response within a reasonable time; this is a volunteer project.

## Supported versions

Only the latest code on the `development` branch and the most recent release (when published) are supported.

## Design guarantees

- Diagnostics are read-only: registry keys are opened without write access.
- Sensitive IDM values (serial, name, email, key-like data) are never read, and report text is redacted.
- No network access and no update checks in the current version.
- The tool does not change registry ownership or ACLs, and does not create startup entries.

## Out of scope

Reports requesting trial-reset, license-bypass or registration-emulation features will be closed. Such functionality has been removed and will not be re-added.
