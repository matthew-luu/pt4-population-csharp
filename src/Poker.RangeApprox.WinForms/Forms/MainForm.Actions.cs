using System.Diagnostics;
using Poker.RangeApprox.App;
using Poker.RangeApprox.App.Execution;
using Poker.RangeApprox.Infrastructure.Writing;
using Poker.RangeApprox.WinForms.Dialogs;
using Poker.RangeApprox.WinForms.Visualization;

namespace Poker.RangeApprox.WinForms;

public sealed partial class MainForm
{
    private void ConfigureEvents()
    {
        _browseRankingButton.Click += (_, _) => BrowseRankingFile();
        _browseCsvButton.Click += (_, _) => BrowseCsvFile();
        _runButton.Click += async (_, _) => await RunAnalysisAsync();
        _openVisualizationButton.Click += (_, _) => OpenVisualizationFile();

        _modeComboBox.SelectedIndexChanged += (_, _) => UpdateDynamicFieldVisibility();

        _rankingPathTextBox.TextChanged += (_, _) => RefreshRankingFileState();
        _csvPathTextBox.TextChanged += (_, _) => RefreshCsvFileState();
        _outputRootTextBox.Click += (_, _) => OpenOutputFolder();
    }

    private void BrowseRankingFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Text files|*.txt|All files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _rankingPathTextBox.Text = dialog.FileName;
    }

    private void BrowseCsvFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "CSV files|*.csv|All files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
            _csvPathTextBox.Text = dialog.FileName;
    }

    private void OpenVisualizationFile()
    {
        try
        {
            var outputRoot = _outputRootTextBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(outputRoot) && Directory.Exists(outputRoot))
            {
                var currentRunFile = Path.Combine(outputRoot, "range_visualization.rangeviz");

                if (File.Exists(currentRunFile))
                {
                    OpenVisualizationDocument(currentRunFile);
                    return;
                }
            }

            using var dialog = new OpenFileDialog
            {
                Filter = "Range visualization files|*.rangeviz|JSON files|*.json|All files|*.*",
                CheckFileExists = true,
                RestoreDirectory = true
            };

            if (!string.IsNullOrWhiteSpace(outputRoot) && Directory.Exists(outputRoot))
                dialog.InitialDirectory = outputRoot;

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            OpenVisualizationDocument(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Open Visualization",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OpenVisualizationDocument(string filePath)
    {
        var reader = new RangeVisualizationFileWriter();
        var document = reader.Read(filePath);

        using var viewer = new RangeVisualizationViewerForm(document);
        viewer.ShowDialog(this);
    }

    private async Task RunAnalysisAsync()
    {
        _statusTextBox.Clear();
        _outputRootTextBox.Clear();
        _runButton.Enabled = false;

        try
        {
            ValidateInputsForRun(
                out var rakePercent,
                out var rakeCapBb,
                out var openSize,
                out var threeBetSize,
                out var fourBetSize,
                out var useDatabaseMode);

            var mode = _modeComboBox.SelectedItem?.ToString() ?? "rfi";

            var nodeKey = string.Equals(mode, "single", StringComparison.OrdinalIgnoreCase)
                ? _nodeKeyComboBox.SelectedItem?.ToString()
                : null;

            if (string.Equals(mode, "single", StringComparison.OrdinalIgnoreCase) &&
                !useDatabaseMode &&
                string.IsNullOrWhiteSpace(nodeKey))
            {
                throw new InvalidOperationException("single mode requires a node key in CSV mode.");
            }

            var requestedProfile = _requestedProfileComboBox.SelectedItem?.ToString();
            if (string.Equals(requestedProfile, "(auto)", StringComparison.OrdinalIgnoreCase))
                requestedProfile = null;

            string? connectionString = null;

            if (useDatabaseMode)
            {
                using var dialog = new DatabaseConnectionDialog();
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    AppendStatus("Run cancelled.");
                    return;
                }

                connectionString = dialog.ConnectionString;
            }

            var options = new AppOptions(
                RankingFilePath: _rankingPathTextBox.Text.Trim(),
                CsvPath: useDatabaseMode ? string.Empty : _csvPathTextBox.Text.Trim(),
                Mode: mode,
                NodeKey: nodeKey,
                RequestedProfileName: requestedProfile,
                RakePercent: rakePercent,
                RakeCapBb: rakeCapBb,
                OpenSize: openSize,
                ThreeBetSize: threeBetSize,
                FourBetSize: fourBetSize,
                OutputRoot: null);

            var statusWriter = new CallbackStatusWriter(AppendStatus);

            var exitCode = await Task.Run(() =>
            {
                var context = AppBootstrapper.Build(options, statusWriter, connectionString);
                BeginInvoke(() => _outputRootTextBox.Text = context.OutputRoot);
                return AppRunner.Run(context);
            });

            AppendStatus($"Completed with exit code {exitCode}.");
        }
        catch (Exception ex)
        {
            AppendStatus(ex.Message);
        }
        finally
        {
            _runButton.Enabled = true;
        }
    }

    private void AppendStatus(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendStatus(message));
            return;
        }

        _statusTextBox.AppendText(message + Environment.NewLine);
    }

    private void OpenOutputFolder()
    {
        var path = _outputRootTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(path))
            return;

        if (!Directory.Exists(path))
        {
            MessageBox.Show(
                this,
                "The output folder does not exist yet.",
                "Output Folder",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
    }
}