using IdmDiag.Core.Abstractions;

namespace IdmDiag.Tests;

internal sealed class FakeRegistry : IRegistryReader
{
    public Dictionary<string, Dictionary<string, string>> Keys { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool Throw { get; set; }

    /// <summary>Every value name requested via GetString, to assert sensitive values are never read.</summary>
    public List<string> ReadValueNames { get; } = new();

    private static string K(RegistryRoot r, string s) => $"{r}\\{s}";

    public FakeRegistry Add(RegistryRoot r, string subKey, params (string Name, string Value)[] values)
    {
        Keys[K(r, subKey)] = values.ToDictionary(v => v.Name, v => v.Value, StringComparer.OrdinalIgnoreCase);
        return this;
    }

    private void MaybeThrow()
    {
        if (Throw)
        {
            throw new UnauthorizedAccessException("Registry access denied (fake).");
        }
    }

    public bool KeyExists(RegistryRoot root, string subKey)
    {
        MaybeThrow();
        return Keys.ContainsKey(K(root, subKey));
    }

    public string? GetString(RegistryRoot root, string subKey, string valueName)
    {
        MaybeThrow();
        ReadValueNames.Add(valueName);
        return Keys.TryGetValue(K(root, subKey), out var d) && d.TryGetValue(valueName, out var v) ? v : null;
    }

    public IReadOnlyList<string> GetSubKeyNames(RegistryRoot root, string subKey)
    {
        MaybeThrow();
        var prefix = K(root, subKey) + "\\";
        return Keys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && !k[prefix.Length..].Contains('\\'))
            .Select(k => k[prefix.Length..])
            .ToList();
    }

    public IReadOnlyList<string> GetValueNames(RegistryRoot root, string subKey)
    {
        MaybeThrow();
        return Keys.TryGetValue(K(root, subKey), out var d) ? d.Keys.ToList() : new List<string>();
    }
}

internal sealed class FakeFileSystem : IFileSystem
{
    public Dictionary<string, string?> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool FileExists(string path) => Files.ContainsKey(path);

    public bool DirectoryExists(string path) => false;

    public string? GetFileVersion(string path) => Files.TryGetValue(path, out var v) ? v : null;

    public IReadOnlyList<string> EnumerateFiles(string directory, string searchPattern) => Array.Empty<string>();
}

internal sealed class FakeEnvironment : IEnvironmentInfo
{
    public string OsDescription => "Microsoft Windows 11 (fake)";
    public string OsVersion => "10.0.22631.0";
    public string OsArchitecture => "X64";
    public string ProcessArchitecture => "X64";
    public string? ProgramFiles => @"C:\Program Files";
    public string? ProgramFilesX86 => @"C:\Program Files (x86)";
    public string TempPath => @"C:\Temp\";
}
