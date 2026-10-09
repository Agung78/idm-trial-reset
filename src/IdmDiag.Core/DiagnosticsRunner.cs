using IdmDiag.Core.Abstractions;
using IdmDiag.Core.Checks;
using IdmDiag.Core.Reporting;

namespace IdmDiag.Core;

public sealed class DiagnosticsRunner
{
    public const string ToolVersion = "0.1.0";

    private readonly DiagnosticContext _context;
    private readonly IReadOnlyList<IDiagnosticCheck> _checks;
    private readonly Func<DateTimeOffset> _clock;

    public DiagnosticsRunner(
        IRegistryReader registry,
        IFileSystem fileSystem,
        IEnvironmentInfo environment,
        IEnumerable<IDiagnosticCheck>? checks = null,
        Func<DateTimeOffset>? clock = null)
    {
        _context = new DiagnosticContext(registry, fileSystem, environment);
        _checks = (checks ?? DefaultChecks()).ToList();
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public static IEnumerable<IDiagnosticCheck> DefaultChecks() => new IDiagnosticCheck[]
    {
        new IdmInstallCheck(),
        new IdmExePathCheck(),
        new IdmVersionCheck(),
        new OsCheck(),
        new LegacyArtifactsCheck(),
    };

    public DiagnosticReport Run()
    {
        var results = new List<CheckResult>();
        foreach (var check in _checks)
        {
            try
            {
                var r = check.Run(_context);
                results.Add(r with { Message = Redaction.Redact(r.Message) });
            }
            catch (Exception ex)
            {
                results.Add(new CheckResult(check.Id, check.Name, CheckStatus.Error,
                    Redaction.Redact($"Check failed: {ex.GetType().Name}: {ex.Message}")));
            }
        }

        var env = _context.Environment;
        var idm = BuildIdmInfo();
        return new DiagnosticReport(
            ToolVersion,
            _clock().ToUniversalTime(),
            new OsInfo(env.OsDescription, env.OsVersion, env.OsArchitecture, env.ProcessArchitecture),
            idm,
            results);
    }

    private IdmInfo BuildIdmInfo()
    {
        try
        {
            var (path, _) = _context.ResolveExePath();
            if (path is null)
            {
                return new IdmInfo(false, null, null);
            }

            var v = VersionParser.TryParse(_context.FileSystem.GetFileVersion(path))
                    ?? VersionParser.TryParse(_context.GetUninstallDisplayVersion());
            return new IdmInfo(true, path, v?.ToString());
        }
        catch
        {
            return new IdmInfo(false, null, null);
        }
    }
}
