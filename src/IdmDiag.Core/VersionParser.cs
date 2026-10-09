using System.Text.RegularExpressions;

namespace IdmDiag.Core;

public static class VersionParser
{
    private static readonly Regex Pattern = new(
        @"(?<!\d)(\d+)\.(\d+)(?:\.(\d+))?(?:\.(\d+))?(?!\d)", RegexOptions.Compiled);

    /// <summary>
    /// Extracts the first dotted numeric version from text such as "6.42.1", "6.42 Build 1" or "IDM 6.41.2.0".
    /// Returns null when none is present.
    /// </summary>
    public static Version? TryParse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var m = Pattern.Match(text);
        if (!m.Success)
        {
            return null;
        }

        try
        {
            var parts = new List<int>();
            for (var i = 1; i <= 4; i++)
            {
                if (m.Groups[i].Success)
                {
                    parts.Add(int.Parse(m.Groups[i].Value, System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            return parts.Count switch
            {
                2 => new Version(parts[0], parts[1]),
                3 => new Version(parts[0], parts[1], parts[2]),
                _ => new Version(parts[0], parts[1], parts[2], parts[3]),
            };
        }
        catch (OverflowException)
        {
            return null;
        }
    }
}
