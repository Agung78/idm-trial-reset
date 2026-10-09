using System.Diagnostics;
using System.Runtime.InteropServices;
using IdmDiag.Core.Abstractions;
using Microsoft.Win32;

namespace IdmDiag.Windows;

/// <summary>Read-only registry access. Keys are opened without write rights; only read APIs are used.</summary>
internal sealed class WindowsRegistryReader : IRegistryReader
{
    private static RegistryKey? Open(RegistryRoot root, string subKey)
    {
        var hive = root == RegistryRoot.CurrentUser ? RegistryHive.CurrentUser : RegistryHive.LocalMachine;
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        return baseKey.OpenSubKey(subKey, writable: false);
    }

    public bool KeyExists(RegistryRoot root, string subKey)
    {
        using var k = Open(root, subKey);
        return k is not null;
    }

    public string? GetString(RegistryRoot root, string subKey, string valueName)
    {
        using var k = Open(root, subKey);
        return k?.GetValue(valueName) switch
        {
            null => null,
            string s => s,
            var other => Convert.ToString(other, System.Globalization.CultureInfo.InvariantCulture),
        };
    }

    public IReadOnlyList<string> GetSubKeyNames(RegistryRoot root, string subKey)
    {
        using var k = Open(root, subKey);
        return k?.GetSubKeyNames() ?? Array.Empty<string>();
    }

    public IReadOnlyList<string> GetValueNames(RegistryRoot root, string subKey)
    {
        using var k = Open(root, subKey);
        return k?.GetValueNames() ?? Array.Empty<string>();
    }
}

internal sealed class WindowsFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public string? GetFileVersion(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var v = FileVersionInfo.GetVersionInfo(path);
        return v.FileVersion ?? v.ProductVersion;
    }

    public IReadOnlyList<string> EnumerateFiles(string directory, string searchPattern) =>
        Directory.Exists(directory) ? Directory.GetFiles(directory, searchPattern) : Array.Empty<string>();
}

internal sealed class WindowsEnvironmentInfo : IEnvironmentInfo
{
    public string OsDescription => RuntimeInformation.OSDescription;

    public string OsVersion => Environment.OSVersion.Version.ToString();

    public string OsArchitecture => RuntimeInformation.OSArchitecture.ToString();

    public string ProcessArchitecture => RuntimeInformation.ProcessArchitecture.ToString();

    public string? ProgramFiles => Environment.GetEnvironmentVariable("ProgramW6432")
                                   ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    public string? ProgramFilesX86 => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

    public string TempPath => Path.GetTempPath();
}
