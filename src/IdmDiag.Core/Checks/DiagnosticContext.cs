using IdmDiag.Core.Abstractions;

namespace IdmDiag.Core.Checks;

public sealed class DiagnosticContext
{
    public DiagnosticContext(IRegistryReader registry, IFileSystem fileSystem, IEnvironmentInfo environment)
    {
        Registry = registry;
        FileSystem = fileSystem;
        Environment = environment;
    }

    public IRegistryReader Registry { get; }

    public IFileSystem FileSystem { get; }

    public IEnvironmentInfo Environment { get; }

    public const string ExeName = "IDMan.exe";
    public const string UserKey = @"Software\DownloadManager";
    public const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    public const string UninstallKeyWow = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
    public const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    public const string IdmDisplayName = "Internet Download Manager";

    /// <summary>
    /// Resolves the IDM executable path from read-only sources, in order:
    /// HKCU ExePath, HKLM uninstall DisplayIcon/InstallLocation, default Program Files folders.
    /// Returns the source label alongside the path. Never reads user/license values.
    /// </summary>
    public (string? Path, string? Source) ResolveExePath()
    {
        var fromUser = Registry.GetString(RegistryRoot.CurrentUser, UserKey, "ExePath");
        if (!string.IsNullOrWhiteSpace(fromUser) && FileSystem.FileExists(fromUser.Trim('"')))
        {
            return (fromUser.Trim('"'), "HKCU\\" + UserKey + "\\ExePath");
        }

        foreach (var unKey in new[] { UninstallKey, UninstallKeyWow })
        {
            foreach (var sub in Registry.GetSubKeyNames(RegistryRoot.LocalMachine, unKey))
            {
                var path = unKey + "\\" + sub;
                var name = Registry.GetString(RegistryRoot.LocalMachine, path, "DisplayName");
                if (name is null || !name.Contains(IdmDisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var loc = Registry.GetString(RegistryRoot.LocalMachine, path, "InstallLocation");
                if (!string.IsNullOrWhiteSpace(loc))
                {
                    var candidate = System.IO.Path.Combine(loc.Trim('"'), ExeName);
                    if (FileSystem.FileExists(candidate))
                    {
                        return (candidate, "HKLM uninstall entry");
                    }
                }
            }
        }

        foreach (var pf in new[] { Environment.ProgramFilesX86, Environment.ProgramFiles })
        {
            if (string.IsNullOrWhiteSpace(pf))
            {
                continue;
            }

            var candidate = System.IO.Path.Combine(pf, IdmDisplayName, ExeName);
            if (FileSystem.FileExists(candidate))
            {
                return (candidate, "default Program Files folder");
            }
        }

        return (null, null);
    }

    /// <summary>Registry DisplayVersion from the IDM uninstall entry, if any.</summary>
    public string? GetUninstallDisplayVersion()
    {
        foreach (var unKey in new[] { UninstallKey, UninstallKeyWow })
        {
            foreach (var sub in Registry.GetSubKeyNames(RegistryRoot.LocalMachine, unKey))
            {
                var path = unKey + "\\" + sub;
                var name = Registry.GetString(RegistryRoot.LocalMachine, path, "DisplayName");
                if (name is not null && name.Contains(IdmDisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    var v = Registry.GetString(RegistryRoot.LocalMachine, path, "DisplayVersion");
                    if (!string.IsNullOrWhiteSpace(v))
                    {
                        return v;
                    }
                }
            }
        }

        return null;
    }
}
