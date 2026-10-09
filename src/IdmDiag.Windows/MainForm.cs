using System.Diagnostics;
using IdmDiag.Core;
using IdmDiag.Core.Reporting;

namespace IdmDiag.Windows;

internal sealed class MainForm : Form
{
    private static readonly (string Text, string Url)[] Links =
    {
        ("Buy IDM", "https://www.internetdownloadmanager.com/buy.html"),
        ("Register IDM", "https://www.internetdownloadmanager.com/register.html"),
        ("IDM support", "https://www.internetdownloadmanager.com/support.html"),
    };

    private readonly Label _status = new() { AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) };
    private readonly ListView _list = new()
    {
        View = View.Details,
        FullRowSelect = true,
        Dock = DockStyle.Fill,
        HideSelection = false,
    };
    private readonly Button _refresh = new() { Text = "&Refresh", AutoSize = true };
    private readonly Button _export = new() { Text = "&Export report...", AutoSize = true };
    private DiagnosticReport? _report;

    public MainForm()
    {
        Text = "IDM Diagnostics";
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(820, 480);
        MinimumSize = new Size(600, 360);

        _list.Columns.Add("Status", 90);
        _list.Columns.Add("Check", 200);
        _list.Columns.Add("Details", 500);

        var header = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
        header.Controls.Add(new Label { Text = "Status:", AutoSize = true });
        header.Controls.Add(_status);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8) };
        buttons.Controls.Add(_refresh);
        buttons.Controls.Add(_export);
        foreach (var (text, url) in Links)
        {
            var link = new LinkLabel { Text = text, AutoSize = true, Margin = new Padding(16, 6, 0, 0) };
            link.LinkClicked += (_, _) => OpenUrl(url);
            buttons.Controls.Add(link);
        }

        Controls.Add(_list);
        Controls.Add(header);
        Controls.Add(buttons);

        _refresh.Click += (_, _) => RunDiagnostics();
        _export.Click += (_, _) => ExportReport();
        Shown += (_, _) => RunDiagnostics();
    }

    private void RunDiagnostics()
    {
        try
        {
            var runner = new DiagnosticsRunner(
                new WindowsRegistryReader(), new WindowsFileSystem(), new WindowsEnvironmentInfo());
            _report = runner.Run();
        }
        catch (Exception ex)
        {
            _report = null;
            _status.Text = "Diagnostics failed: " + ex.Message;
            _export.Enabled = false;
            return;
        }

        _list.Items.Clear();
        foreach (var c in _report.Checks)
        {
            var item = new ListViewItem(c.Status.ToString());
            item.SubItems.Add(c.Name);
            item.SubItems.Add(c.Message);
            item.ForeColor = c.Status switch
            {
                CheckStatus.Ok => Color.DarkGreen,
                CheckStatus.Warning => Color.DarkGoldenrod,
                _ => Color.Firebrick,
            };
            _list.Items.Add(item);
        }

        var errors = _report.Checks.Count(c => c.Status == CheckStatus.Error);
        var warnings = _report.Checks.Count(c => c.Status == CheckStatus.Warning);
        _status.Text = _report.Idm.Installed
            ? $"IDM {_report.Idm.Version ?? "(version unknown)"} detected - {errors} error(s), {warnings} warning(s)"
            : "IDM not detected";
        _export.Enabled = true;
    }

    private void ExportReport()
    {
        if (_report is null)
        {
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Title = "Export diagnostic report",
            Filter = "JSON (*.json)|*.json|Text (*.txt)|*.txt",
            FileName = $"idm-diagnostics-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
            AddExtension = true,
            OverwritePrompt = true,
        };
        if (dlg.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var content = dlg.FilterIndex == 2 || dlg.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                ? ReportExporter.ToText(_report)
                : ReportExporter.ToJson(_report);
            File.WriteAllText(dlg.FileName, content);
            MessageBox.Show(this, "Report saved to:\n" + dlg.FileName, Text,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not save the report:\n" + ex.Message, Text,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void OpenUrl(string url) =>
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
}
