namespace IdmDiag.Core.Abstractions;

public enum RegistryRoot
{
    CurrentUser,
    LocalMachine,
}

/// <summary>Read-only registry access. Implementations must never write.</summary>
public interface IRegistryReader
{
    bool KeyExists(RegistryRoot root, string subKey);

    /// <summary>Returns the value as a string, or null when the key or value is missing.</summary>
    string? GetString(RegistryRoot root, string subKey, string valueName);

    IReadOnlyList<string> GetSubKeyNames(RegistryRoot root, string subKey);

    IReadOnlyList<string> GetValueNames(RegistryRoot root, string subKey);
}

public interface IFileSystem
{
    bool FileExists(string path);

    bool DirectoryExists(string path);

    /// <summary>Returns the file version string of an executable, or null when unavailable.</summary>
    string? GetFileVersion(string path);

    IReadOnlyList<string> EnumerateFiles(string directory, string searchPattern);
}

public interface IEnvironmentInfo
{
    string OsDescription { get; }

    string OsVersion { get; }

    string OsArchitecture { get; }

    string ProcessArchitecture { get; }

    /// <summary>%ProgramFiles%, or null when unknown.</summary>
    string? ProgramFiles { get; }

    /// <summary>%ProgramFiles(x86)%, or null when unknown.</summary>
    string? ProgramFilesX86 { get; }

    string TempPath { get; }
}
