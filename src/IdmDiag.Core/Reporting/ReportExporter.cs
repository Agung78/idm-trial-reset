using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdmDiag.Core.Reporting;

public static class ReportExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string ToJson(DiagnosticReport report) =>
        JsonSerializer.Serialize(Sanitize(report), JsonOptions);

    public static string ToText(DiagnosticReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("IDM Diagnostics Report");
        sb.AppendLine($"Tool version : {report.ToolVersion}");
        sb.AppendLine($"Timestamp UTC: {report.TimestampUtc:yyyy-MM-ddTHH:mm:ssZ}");
        sb.AppendLine($"OS           : {report.Os.Description} ({report.Os.Version}, {report.Os.Architecture}; process {report.Os.ProcessArchitecture})");
        sb.AppendLine($"IDM installed: {(report.Idm.Installed ? "yes" : "no")}");
        sb.AppendLine($"IDM path     : {report.Idm.ExePath ?? "n/a"}");
        sb.AppendLine($"IDM version  : {report.Idm.Version ?? "n/a"}");
        sb.AppendLine();
        sb.AppendLine("Checks");
        foreach (var c in report.Checks)
        {
            sb.AppendLine($"[{c.Status.ToString().ToUpperInvariant(),-7}] {c.Name}: {c.Message}");
        }

        return Redaction.Redact(sb.ToString());
    }

    private static DiagnosticReport Sanitize(DiagnosticReport r) => r with
    {
        Checks = r.Checks.Select(c => c with { Message = Redaction.Redact(c.Message) }).ToList(),
    };
}
