# Building and Verifying on Windows

## 1. Prerequisites

Run in PowerShell:

```powershell
winget install Microsoft.DotNet.SDK.8
winget install Git.Git
```

Open a new terminal, then verify:

```powershell
dotnet --version   # must start with 8.
```

## 2. Get the source

```powershell
git clone <repo-url> idm-trial-reset
cd idm-trial-reset
git switch development
```

## 3. Restore, test, build

```powershell
dotnet restore
dotnet test tests\IdmDiag.Tests
dotnet build src\IdmDiag.Windows -c Release
```

If `IdmDiag.sln` fails to load, regenerate it:

```powershell
del IdmDiag.sln
dotnet new sln -n IdmDiag
dotnet sln add src\IdmDiag.Core src\IdmDiag.Windows tests\IdmDiag.Tests
```

## 4. Run

```powershell
dotnet run --project src\IdmDiag.Windows
```

## 5. Publish a single executable

```powershell
dotnet publish src\IdmDiag.Windows -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
Get-FileHash publish\*.exe -Algorithm SHA256
```

Record the SHA-256 checksum in the release notes.

## 6. Manual verification checklist

| Case | Expected result |
|---|---|
| IDM installed | Install check is Ok; exe path and version are shown |
| IDM not installed | Warning or Error is reported; no crash |
| Legacy leftovers present (`HKCU\...\Run\IDM trial reset`, `%TEMP%\SetACLx64.exe`) | Reported as warnings only; nothing is modified |
| Export report (JSON and text) | Files are saved and contain no Serial, Email, or name values |
| Official links | Open the buy, register, and support pages on internetdownloadmanager.com |
| 150–200% display scaling | Text is sharp; no clipping |
| Keyboard navigation | Tab reaches every control |
| Standard user, then "Run as administrator" | Both work; no UAC prompt as a standard user |

### Read-only verification

Confirm that the app does not modify IDM registry state:

```powershell
reg export HKCU\Software\DownloadManager before.reg
# run the app and export a report
reg export HKCU\Software\DownloadManager after.reg
fc before.reg after.reg
```

`fc` must report no differences.
