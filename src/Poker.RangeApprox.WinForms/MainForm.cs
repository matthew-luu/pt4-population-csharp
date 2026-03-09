using Poker.RangeApprox.App;
using Poker.RangeApprox.App.Execution;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Infrastructure.Parsing;

namespace Poker.RangeApprox.WinForms;

public sealed class MainForm : Form
{
    private readonly TextBox _rankingPathTextBox = new() { Width = 500 };
    private readonly TextBox _csvPathTextBox = new() { Width = 500 };
    private readonly ComboBox _modeComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
    private readonly ComboBox _nodeKeyComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250 };
    private readonly ComboBox _requestedProfileComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250 };
    private readonly TextBox _rakePercentTextBox = new() { Width = 100, Text = "0.05" };
    private readonly TextBox _rakeCapBbTextBox = new() { Width = 100, Text = "150" };
    private readonly TextBox _openSizeTextBox = new() { Width = 100, Text = "2.5" };
    private readonly TextBox _threeBetSizeTextBox = new() { Width = 100, Text = "8.5" };
    private readonly TextBox _fourBetSizeTextBox = new() { Width = 100, Text = "23" };
    private readonly TextBox _outputRootTextBox = new() { ReadOnly = true };

    private readonly TextBox _helpTextBox = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Dock = DockStyle.Fill
    };

    private readonly TextBox _statusTextBox = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        ReadOnly = true,
        Dock = DockStyle.Fill
    };

    private readonly Button _runButton = new() { Text = "Run", Width = 120 };
    private readonly Button _browseRankingButton = new() { Text = "Browse..." };
    private readonly Button _browseCsvButton = new() { Text = "Browse..." };
    private readonly ToolTip _toolTip = new();

    private readonly Label _nodeKeyLabel = new() { Text = "Node key", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 8, 6) };
    private readonly Label _requestedProfileLabel = new() { Text = "Requested profile", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 8, 6) };

    private readonly Panel _nodeKeySidePanel = new() { Width = 1, Height = 1 };
    private readonly Panel _requestedProfileSidePanel = new() { Width = 1, Height = 1 };

    private TableLayoutPanel? _inputLayout;
    private TableLayoutPanel? _mainLayout;

    public MainForm()
    {
        Text = "Poker Range Approx";
        Width = 1200;
        Height = 780;
        StartPosition = FormStartPosition.CenterScreen;

        _toolTip.AutoPopDelay = 15000;
        _toolTip.InitialDelay = 250;
        _toolTip.ReshowDelay = 100;
        _toolTip.ShowAlways = true;

        _modeComboBox.Items.AddRange([
            "rfi",
            "single",
            "all",
            "rank-supercalls",
            "exploit-open"
        ]);
        _modeComboBox.SelectedItem = "rfi";

        _requestedProfileComboBox.Items.Add("(auto)");
        _requestedProfileComboBox.SelectedIndex = 0;

        _browseRankingButton.Click += (_, _) => BrowseRankingFile();
        _browseCsvButton.Click += (_, _) => BrowseCsvFile();
        _runButton.Click += async (_, _) => await RunAnalysisAsync();
        _modeComboBox.SelectedIndexChanged += (_, _) => UpdateDynamicFieldVisibility();

        BuildLayout();
        BindHelpText();
        UpdateDynamicFieldVisibility();
        SetInitialHelpText();
    }

    private void BuildLayout()
    {
        _mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(12)
        };

        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

        var leftPanel = BuildLeftPanel();
        var rightPanel = BuildRightPanel();

        _mainLayout.Controls.Add(leftPanel, 0, 0);
        _mainLayout.Controls.Add(rightPanel, 1, 0);

        Controls.Add(_mainLayout);
    }

    private Control BuildLeftPanel()
    {
        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };

        container.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 42));

        _inputLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 11,
            AutoScroll = true
        };

        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        AddStandardRow(
            0,
            "Ranking file",
            _rankingPathTextBox,
            _browseRankingButton,
            "Text file containing hand ranking profiles used to approximate ranges.\r\n\r\nExample:\r\nprefloprankings.txt");

        AddStandardRow(
            1,
            "CSV file",
            _csvPathTextBox,
            _browseCsvButton,
            "Population CSV export from PokerTracker 4. The app reads node frequencies and opportunity counts from it.\r\n\r\nExample:\r\npopulation.csv");

        AddStandardRow(
            2,
            "Mode",
            _modeComboBox,
            new Panel(),
            "Controls what the app generates.\r\n\r\n" +
            "rfi = generate only RFI ranges\r\n" +
            "single = generate one node using Node Key\r\n" +
            "all = generate all ranges, super-ranges, and rankings\r\n" +
            "rank-supercalls = build call super-ranges and rank hands against them\r\n" +
            "exploit-open = run exploitative open analysis");

        AddCustomRow(
            3,
            _nodeKeyLabel,
            _nodeKeyComboBox,
            _nodeKeySidePanel,
            "Used only in single mode to identify the exact node to generate.\r\n\r\nExamples:\r\nrfi_btn\r\ncall_sb_vs_btn\r\nfourbet_btn_vs_threebet_sb");

        AddCustomRow(
            4,
            _requestedProfileLabel,
            _requestedProfileComboBox,
            _requestedProfileSidePanel,
            "Optional ranking profile override. Leave as (auto) to use automatic profile selection.\r\n\r\nExample:\r\nPokerstove");

        AddStandardRow(
            5,
            "Rake percent",
            _rakePercentTextBox,
            new Panel(),
            "Rake percentage applied to flop-seen branches.\r\n\r\nExample:\r\n0.05 for 5%");

        AddStandardRow(
            6,
            "Rake cap (bb)",
            _rakeCapBbTextBox,
            new Panel(),
            "Maximum rake cap measured in big blinds.\r\n\r\nExample:\r\n150");

        AddStandardRow(
            7,
            "Open size",
            _openSizeTextBox,
            new Panel(),
            "Open raise size in big blinds used by exploit analysis.\r\n\r\nExample:\r\n2.5");

        AddStandardRow(
            8,
            "3bet size",
            _threeBetSizeTextBox,
            new Panel(),
            "3bet size in big blinds used by exploit analysis.\r\n\r\nExample:\r\n8.5");

        AddStandardRow(
            9,
            "4bet size",
            _fourBetSizeTextBox,
            new Panel(),
            "4bet size in big blinds used by exploit analysis.\r\n\r\nExample:\r\n23");

        AddStandardRow(
            10,
            "Output root",
            _outputRootTextBox,
            _runButton,
            "Run output directory generated by the app after execution starts.");

        var statusPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };

        statusPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statusPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var statusLabel = new Label
        {
            Text = "Status",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 0, 6)
        };

        BindHelp(statusLabel, "Execution log and status messages for the current run.");
        BindHelp(_statusTextBox, "Execution log and status messages for the current run.");

        statusPanel.Controls.Add(statusLabel, 0, 0);
        statusPanel.Controls.Add(_statusTextBox, 0, 1);

        container.Controls.Add(_inputLayout, 0, 0);
        container.Controls.Add(statusPanel, 0, 1);

        return container;
    }

    private Control BuildRightPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12, 0, 0, 0)
        };

        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var helpLabel = new Label
        {
            Text = "Field help",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 0, 0, 6)
        };

        panel.Controls.Add(helpLabel, 0, 0);
        panel.Controls.Add(_helpTextBox, 0, 1);

        BindHelp(helpLabel, "Descriptions and examples for the selected or hovered field.");
        BindHelp(_helpTextBox, "Descriptions and examples for the selected or hovered field.");

        return panel;
    }

    private void AddStandardRow(int row, string labelText, Control mainControl, Control sideControl, string helpText)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 8, 6)
        };

        AddCustomRow(row, label, mainControl, sideControl, helpText);
    }

    private void AddCustomRow(int row, Label label, Control mainControl, Control sideControl, string helpText)
    {
        if (_inputLayout is null)
            return;

        mainControl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        sideControl.Anchor = AnchorStyles.Left;

        BindHelp(label, helpText);
        BindHelp(mainControl, helpText);

        if (sideControl is not Panel)
            BindHelp(sideControl, helpText);

        _inputLayout.Controls.Add(label, 0, row);
        _inputLayout.Controls.Add(mainControl, 1, row);
        _inputLayout.Controls.Add(sideControl, 2, row);
    }

    private void BindHelpText()
    {
        BindHelp(_browseRankingButton, "Browse for a ranking file.\r\n\r\nExample:\r\nprefloprankings.txt");
        BindHelp(_browseCsvButton, "Browse for a population CSV export.\r\n\r\nExample:\r\npopulation.csv");
        BindHelp(_runButton, "Run the selected analysis using the current inputs.");
    }

    private void BindHelp(Control control, string helpText)
    {
        _toolTip.SetToolTip(control, helpText);
        control.MouseEnter += (_, _) => _helpTextBox.Text = helpText;
        control.Enter += (_, _) => _helpTextBox.Text = helpText;
    }

    private void SetInitialHelpText()
    {
        _helpTextBox.Text =
            "Select a field to see its description here.\r\n\r\n" +
            "Typical first run:\r\n" +
            "- choose a ranking file\r\n" +
            "- choose a CSV file\r\n" +
            "- leave Mode on rfi";
    }

    private void BrowseRankingFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Text files|*.txt|All files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _rankingPathTextBox.Text = dialog.FileName;
        LoadProfilesFromRankingFile(dialog.FileName);
        UpdateDynamicFieldVisibility();
    }

    private void BrowseCsvFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "CSV files|*.csv|All files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _csvPathTextBox.Text = dialog.FileName;
        LoadNodeKeysFromCsv(dialog.FileName);
        UpdateDynamicFieldVisibility();
    }

    private void LoadProfilesFromRankingFile(string filePath)
    {
        _requestedProfileComboBox.Items.Clear();
        _requestedProfileComboBox.Items.Add("(auto)");

        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                _requestedProfileComboBox.SelectedIndex = 0;
                return;
            }

            var parser = new RankingFileParser();
            var profiles = parser.Parse(filePath);

            foreach (var profile in profiles.Select(p => p.Name).Distinct(StringComparer.OrdinalIgnoreCase))
                _requestedProfileComboBox.Items.Add(profile);

            _requestedProfileComboBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            AppendStatus($"Failed to load ranking profiles: {ex.Message}");
            _requestedProfileComboBox.SelectedIndex = 0;
        }
    }

    private void LoadNodeKeysFromCsv(string filePath)
    {
        _nodeKeyComboBox.Items.Clear();

        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            var reader = new PopulationCsvReader();
            var rows = reader.Read(filePath);

            if (rows.Count == 0)
                return;

            var extractor = new PopulationNodeExtractor();
            var nodes = extractor.Extract(rows[0]);

            foreach (var key in nodes
                .Select(n => ToNodeKey(n.NodeId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                _nodeKeyComboBox.Items.Add(key);
            }

            if (_nodeKeyComboBox.Items.Count > 0)
                _nodeKeyComboBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            AppendStatus($"Failed to load node keys: {ex.Message}");
        }
    }

    private static string ToNodeKey(NodeId nodeId)
    {
        if (string.Equals(nodeId.Action, "rfi", StringComparison.OrdinalIgnoreCase))
            return $"rfi_{nodeId.Actor}";

        return $"{nodeId.Action}_{nodeId.Actor}_vs_{nodeId.Opponent}";
    }

    private void UpdateDynamicFieldVisibility()
    {
        var isSingleMode = string.Equals(_modeComboBox.SelectedItem?.ToString(), "single", StringComparison.OrdinalIgnoreCase);
        var hasRankingFile = !string.IsNullOrWhiteSpace(_rankingPathTextBox.Text) && File.Exists(_rankingPathTextBox.Text);

        SetRowVisible(_nodeKeyLabel, isSingleMode);
        SetRowVisible(_nodeKeyComboBox, isSingleMode);
        SetRowVisible(_nodeKeySidePanel, isSingleMode);

        SetRowVisible(_requestedProfileLabel, hasRankingFile);
        SetRowVisible(_requestedProfileComboBox, hasRankingFile);
        SetRowVisible(_requestedProfileSidePanel, hasRankingFile);

        if (!isSingleMode)
            _nodeKeyComboBox.SelectedIndex = _nodeKeyComboBox.Items.Count > 0 ? 0 : -1;

        if (!hasRankingFile)
        {
            _requestedProfileComboBox.Items.Clear();
            _requestedProfileComboBox.Items.Add("(auto)");
            _requestedProfileComboBox.SelectedIndex = 0;
        }
    }

    private static void SetRowVisible(Control control, bool visible)
    {
        control.Visible = visible;
    }

    private async Task RunAnalysisAsync()
    {
        _statusTextBox.Clear();
        _outputRootTextBox.Clear();
        _runButton.Enabled = false;

        try
        {
            if (!double.TryParse(_rakePercentTextBox.Text, out var rakePercent))
                throw new InvalidOperationException("Rake percent must be a valid number.");

            if (!double.TryParse(_rakeCapBbTextBox.Text, out var rakeCapBb))
                throw new InvalidOperationException("Rake cap bb must be a valid number.");

            if (!double.TryParse(_openSizeTextBox.Text, out var openSize))
                throw new InvalidOperationException("Open size must be a valid number.");

            if (!double.TryParse(_threeBetSizeTextBox.Text, out var threeBetSize))
                throw new InvalidOperationException("3bet size must be a valid number.");

            if (!double.TryParse(_fourBetSizeTextBox.Text, out var fourBetSize))
                throw new InvalidOperationException("4bet size must be a valid number.");

            var mode = _modeComboBox.SelectedItem?.ToString() ?? "rfi";
            var nodeKey = string.Equals(mode, "single", StringComparison.OrdinalIgnoreCase)
                ? _nodeKeyComboBox.SelectedItem?.ToString()
                : null;

            if (string.Equals(mode, "single", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(nodeKey))
                throw new InvalidOperationException("single mode requires a node key.");

            var requestedProfile = _requestedProfileComboBox.SelectedItem?.ToString();
            if (string.Equals(requestedProfile, "(auto)", StringComparison.OrdinalIgnoreCase))
                requestedProfile = null;

            var options = new AppOptions(
                RankingFilePath: _rankingPathTextBox.Text.Trim(),
                CsvPath: _csvPathTextBox.Text.Trim(),
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
                var context = AppBootstrapper.Build(options, statusWriter);
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
}