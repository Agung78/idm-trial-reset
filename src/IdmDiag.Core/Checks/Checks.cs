using IdmDiag.Core.Abstractions;
using IdmDiag.Core.Reporting;

namespace IdmDiag.Core.Checks;

public sealed class IdmInstallCheck : IDiagnosticCheck
{
    public string Id => "idm.install";

    public string Name => "IDM installation";

    public CheckResult Run(DiagnosticContext c)
    {
        var (path, source) = c.ResolveExePath();
        if (path is not null)
        {
            return new CheckResult(Id, Name, CheckStatus.Ok, $"IDM detected via {source}.");
        }

        var hasUserKey = c.Registry.KeyExists(RegistryRoot.CurrentUser, DiagnosticContext.UserKey);
        return hasUserKey
            ? new CheckResult(Id, Name, CheckStatus.Error,
                "IDM settings key exists but the executable was not found (possibly uninstalled or moved).")
            : new CheckResult(Id, Name, CheckStatus.Error, "IDM was not detected on this computer.");
    }
}

public sealed class IdmExePathCheck : IDiagnosticCheck
{
    public string Id => "idm.exe";

    public string Name => "IDM executable path";

    public CheckResult Run(DiagnosticContext c)
    {
        var (path, _) = c.ResolveExePath();
        return path is null
            ? new CheckResult(Id, Name, CheckStatus.Error, "Executable path could not be resolved.")
            : new CheckResult(Id, Name, CheckStatus.Ok, path);
    }
}

public sealed class IdmVersionCheck : IDiagnosticCheck
{
    public string Id => "idm.version";

    public string Name => "IDM version";

    public CheckResult Run(DiagnosticContext c)
    {
        var (path, _) = c.ResolveExePath();
        string? raw = path is null ? null : c.FileSystem.GetFileVersion(path);
        var parsed = VersionParser.TryParse(raw);
        if (parsed is not null)
        {
            return new CheckResult(Id, Name, CheckStatus.Ok, parsed.ToString());
        }

        var display = c.GetUninstallDisplayVersion();
        parsed = VersionParser.TryParse(display);
        if (parsed is not null)
        {
            return new CheckResult(Id, Name, CheckStatus.Ok, parsed + " (from registry DisplayVersion)");
        }

        if (raw is not null || display is not null)
        {
            return new CheckResult(Id, Name, CheckStatus.Warning,
                $"Unexpected version format: {Redaction.Redact(raw ?? display)}");
        }

        return new CheckResult(Id, Name, CheckStatus.Warning, "Version is not available.");
    }
}

public sealed class OsCheck : IDiagnosticCheck
{
    public string Id => "os.info";

    public string Name => "Operating system";

    public CheckResult Run(DiagnosticContext c)
    {
        var e = c.Environment;
        return new CheckResult(Id, Name, CheckStatus.Ok,
            $"{e.OsDescription} (version {e.OsVersion}, {e.OsArchitecture}; process {e.ProcessArchitecture})");
    }
}

/// <summary>
/// Reports (as warnings only, never removing) leftovers from the removed legacy trial-reset tool.
/// Lists only value names under HKCU Run and file names in the temp directory.
/// </summary>
public sealed class LegacyArtifactsCheck : IDiagnosticCheck
{
    public const string LegacyRunValueName = "IDM trial reset";

    public static readonly string[] LegacyTempFiles =
    {
        "idm_reset.reg", "idm_trial.reg", "idm_reg.reg", "SetACLx32.exe", "SetACLx64.exe",
    };

    public string Id => "legacy.artifacts";

    public string Name => "Legacy tool leftovers";

    public CheckResult Run(DiagnosticContext c)
    {
        var findings = new List<string>();

        foreach (var valueName in c.Registry.GetValueNames(RegistryRoot.CurrentUser, DiagnosticContext.RunKey))
        {
            if (string.Equals(valueName, LegacyRunValueName, StringComparison.OrdinalIgnoreCase))
            {
                findings.Add($"startup entry \"{valueName}\" in HKCU Run");
            }
        }

        foreach (var file in LegacyTempFiles)
        {
            var p = System.IO.Path.Combine(c.Environment.TempPath, file);
            if (c.FileSystem.FileExists(p))
            {
                findings.Add($"temp file {p}");
            }
        }

        return findings.Count == 0
            ? new CheckResult(Id, Name, CheckStatus.Ok, "No legacy startup entry or temp files found.")
            : new CheckResult(Id, Name, CheckStatus.Warning,
                "Found leftovers from an older version of this project (not modified): " + string.Join("; ", findings));
    }
}
