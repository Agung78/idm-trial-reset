using System.Text.RegularExpressions;

namespace IdmDiag.Core;

/// <summary>
/// Guards against sensitive data reaching reports. Checks never read these values;
/// redaction is a second layer applied to every message and exported string.
/// </summary>
public static class Redaction
{
    public const string Placeholder = "[REDACTED]";

    private static readonly string[] SensitiveNames =
    {
        "Serial", "Email", "FName", "LName", "Key", "License", "Password", "Token", "Secret",
    };

    // name=value / name: value, where name contains a sensitive word.
    private static readonly Regex NameValue = new(
        @"(?<name>\b\w*(?:serial|email|fname|lname|key|licen[sc]e|password|token|secret)\w*)\s*(?<sep>[=:])\s*(?<value>[^\s;,""\\]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Serial-number-like: 4+ groups of 4-6 alphanumerics separated by dashes (e.g. AAAAA-BBBBB-CCCCC-DDDDD).
    private static readonly Regex SerialLike = new(
        @"\b[A-Za-z0-9]{4,6}(?:-[A-Za-z0-9]{4,6}){3,}\b", RegexOptions.Compiled);

    private static readonly Regex EmailLike = new(
        @"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);

    public static bool IsSensitiveName(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        foreach (var s in SensitiveNames)
        {
            if (name.Contains(s, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static string Redact(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input ?? string.Empty;
        }

        var s = NameValue.Replace(input, m => $"{m.Groups["name"].Value}{m.Groups["sep"].Value}{Placeholder}");
        s = SerialLike.Replace(s, Placeholder);
        s = EmailLike.Replace(s, Placeholder);
        return s;
    }
}
