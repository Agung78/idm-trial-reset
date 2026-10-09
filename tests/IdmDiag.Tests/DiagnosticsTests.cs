using IdmDiag.Core;
using IdmDiag.Core.Abstractions;
using IdmDiag.Core.Checks;
using IdmDiag.Core.Reporting;
using Xunit;

namespace IdmDiag.Tests;

public class DiagnosticsTests
{
    private const string Exe = @"C:\Program Files (x86)\Internet Download Manager\IDMan.exe";

    private static DiagnosticReport Run(FakeRegistry reg, FakeFileSystem fs) =>
        new DiagnosticsRunner(reg, fs, new FakeEnvironment(), clock: () => new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero)).Run();

    private static CheckResult Get(DiagnosticReport r, string id) => r.Checks.Single(c => c.Id == id);

    [Fact]
    public void IdmPresent_ReportsOkWithPathAndVersion()
    {
        var reg = new FakeRegistry().Add(RegistryRoot.CurrentUser, DiagnosticContext.UserKey,
            ("ExePath", Exe), ("Serial", "ABCDE-FGHIJ-KLMNO-PQRST"), ("Email", "a@b.com"));
        var fs = new FakeFileSystem();
        fs.Files[Exe] = "6.42.1.0";

        var report = Run(reg, fs);

        Assert.True(report.Idm.Installed);
        Assert.Equal(Exe, report.Idm.ExePath);
        Assert.Equal("6.42.1.0", report.Idm.Version);
        Assert.Equal(CheckStatus.Ok, Get(report, "idm.install").Status);
        Assert.Equal(CheckStatus.Ok, Get(report, "idm.version").Status);
        Assert.Equal(CheckStatus.Ok, Get(report, "legacy.artifacts").Status);
        Assert.Equal(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), report.TimestampUtc);
    }

    [Fact]
    public void IdmPresent_NeverReadsSensitiveValues()
    {
        var reg = new FakeRegistry().Add(RegistryRoot.CurrentUser, DiagnosticContext.UserKey,
            ("ExePath", Exe), ("Serial", "ABCDE-FGHIJ-KLMNO-PQRST"));
        var fs = new FakeFileSystem();
        fs.Files[Exe] = "6.42.1.0";

        var report = Run(reg, fs);

        Assert.DoesNotContain(reg.ReadValueNames, Redaction.IsSensitiveName);
        Assert.DoesNotContain("ABCDE-FGHIJ", ReportExporter.ToJson(report));
    }

    [Fact]
    public void IdmAbsent_ReportsErrorsWithoutCrash()
    {
        var report = Run(new FakeRegistry(), new FakeFileSystem());

        Assert.False(report.Idm.Installed);
        Assert.Null(report.Idm.ExePath);
        Assert.Equal(CheckStatus.Error, Get(report, "idm.install").Status);
        Assert.Equal(CheckStatus.Error, Get(report, "idm.exe").Status);
        Assert.Equal(CheckStatus.Warning, Get(report, "idm.version").Status);
        Assert.Equal(CheckStatus.Ok, Get(report, "os.info").Status);
    }

    [Fact]
    public void IdmFoundViaUninstallEntry_UsesDisplayVersionFallback()
    {
        var dir = @"C:\Apps\IDM";
        var reg = new FakeRegistry().Add(RegistryRoot.LocalMachine,
            DiagnosticContext.UninstallKeyWow + @"\IDM",
            ("DisplayName", "Internet Download Manager"), ("InstallLocation", dir), ("DisplayVersion", "6.41 Build 3"));
        var fs = new FakeFileSystem();
        fs.Files[dir + @"\IDMan.exe"] = null;

        var report = Run(reg, fs);

        Assert.True(report.Idm.Installed);
        Assert.Equal("6.41", report.Idm.Version);
    }

    [Fact]
    public void ThrowingRegistry_YieldsErrorNotCrash()
    {
        var report = Run(new FakeRegistry { Throw = true }, new FakeFileSystem());

        Assert.Equal(CheckStatus.Error, Get(report, "idm.install").Status);
        Assert.Contains("Check failed", Get(report, "idm.install").Message);
        Assert.False(report.Idm.Installed);
        Assert.Equal(CheckStatus.Ok, Get(report, "os.info").Status);
    }

    [Fact]
    public void LegacyArtifacts_AreReportedAsWarning()
    {
        var reg = new FakeRegistry().Add(RegistryRoot.CurrentUser, DiagnosticContext.RunKey,
            (LegacyArtifactsCheck.LegacyRunValueName, @"""C:\x\old.exe"" /old"));
        var fs = new FakeFileSystem();
        fs.Files[@"C:\Temp\idm_reg.reg"] = null;

        var r = Get(Run(reg, fs), "legacy.artifacts");

        Assert.Equal(CheckStatus.Warning, r.Status);
        Assert.Contains("startup entry", r.Message);
        Assert.Contains("idm_reg.reg", r.Message);
    }

    [Fact]
    public void Exporters_ContainRequiredFields()
    {
        var report = Run(new FakeRegistry(), new FakeFileSystem());

        var json = ReportExporter.ToJson(report);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        Assert.Equal(DiagnosticsRunner.ToolVersion, doc.RootElement.GetProperty("ToolVersion").GetString());
        Assert.Equal("Error", doc.RootElement.GetProperty("Checks")[0].GetProperty("Status").GetString());

        var text = ReportExporter.ToText(report);
        Assert.Contains("Tool version", text);
        Assert.Contains("2026-01-02T03:04:05Z", text);
        Assert.Contains("[ERROR  ]", text);
    }
}

public class VersionParserTests
{
    [Theory]
    [InlineData("6.42.1.0", "6.42.1.0")]
    [InlineData("6.42.1", "6.42.1")]
    [InlineData("6.42 Build 1", "6.42")]
    [InlineData("IDM v6.41.2", "6.41.2")]
    public void Parses(string input, string expected) =>
        Assert.Equal(Version.Parse(expected), VersionParser.TryParse(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown")]
    [InlineData("7")]
    [InlineData("99999999999.1")]
    public void ReturnsNullWhenUnparseable(string? input) => Assert.Null(VersionParser.TryParse(input));
}

public class RedactionTests
{
    [Theory]
    [InlineData("Serial=ABCDE-FGHIJ-KLMNO-PQRST", "ABCDE")]
    [InlineData("Email: user@example.com", "user@example.com")]
    [InlineData("FName=John LName=Doe", "John")]
    [InlineData("license key AAAAA-BBBBB-CCCCC-DDDDD end", "AAAAA")]
    [InlineData("contact someone@example.org", "someone@")]
    public void RemovesSensitiveData(string input, string leaked)
    {
        var o = Redaction.Redact(input);
        Assert.DoesNotContain(leaked, o);
        Assert.Contains(Redaction.Placeholder, o);
    }

    [Fact]
    public void LeavesOrdinaryTextAlone() =>
        Assert.Equal(@"C:\Program Files\IDM\IDMan.exe 6.42.1", Redaction.Redact(@"C:\Program Files\IDM\IDMan.exe 6.42.1"));

    [Theory]
    [InlineData("Serial", true)]
    [InlineData("email", true)]
    [InlineData("FName", true)]
    [InlineData("ExePath", false)]
    public void IsSensitiveName(string name, bool expected) => Assert.Equal(expected, Redaction.IsSensitiveName(name));
}
