namespace IdmDiag.Core.Reporting;

public enum CheckStatus
{
    Ok,
    Warning,
    Error,
}

public sealed record CheckResult(string Id, string Name, CheckStatus Status, string Message);

public sealed record IdmInfo(bool Installed, string? ExePath, string? Version);

public sealed record OsInfo(string Description, string Version, string Architecture, string ProcessArchitecture);

public sealed record DiagnosticReport(
    string ToolVersion,
    DateTimeOffset TimestampUtc,
    OsInfo Os,
    IdmInfo Idm,
    IReadOnlyList<CheckResult> Checks);
