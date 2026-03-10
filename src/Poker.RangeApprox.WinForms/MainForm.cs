using Poker.RangeApprox.App;
using Poker.RangeApprox.App.Execution;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Infrastructure.Parsing;

namespace Poker.RangeApprox.WinForms;

public sealed class MainForm : Form
{
    private const int RankingFileRow = 0;
    private const int CsvFileRow = 1;
    private const int ModeRow = 2;
    private const int NodeKeyRow = 3;
    private const int RequestedProfileRow = 4;
    private const int RakePercentRow = 5;
    private const int RakeCapRow = 6;
    private const int OpenSizeRow = 7;
    private const int ThreeBetSizeRow = 8;
    private const int FourBetSizeRow = 9;
    private const int OutputRootRow = 10;

    private readonly TextBox _rankingPathTextBox = new();
    private readonly TextBox _csvPathTextBox = new();
    private readonly ComboBox _modeComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _nodeKeyComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _requestedProfileComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _rakePercentTextBox = new() { Text = "0.05" };
    private readonly TextBox _rakeCapBbTextBox = new() { Text = "150" };
    private readonly TextBox _openSizeTextBox = new() { Text = "2.5" };
    private readonly TextBox _threeBetSizeTextBox = new() { Text = "8.5" };
    private readonly TextBox _fourBetSizeTextBox = new() { Text = "23" };
    private readonly TextBox _outputRootTextBox = new() { ReadOnly = true };

    private readonly TextBox _helpTextBox = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        BorderStyle = BorderStyle.FixedSingle,
        Dock = DockStyle.Fill
    };

    private readonly TextBox _statusTextBox = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        ReadOnly = true,
        BorderStyle = BorderStyle.FixedSingle,
        Dock = DockStyle.Fill
    };

    private readonly Button _runButton = new() { Text = "Run", Width = 120, Height = 32 };
    private readonly Button _browseRankingButton = new() { Text = "Browse...", Width = 90, Height = 30 };
    private readonly Button _browseCsvButton = new() { Text = "Browse...", Width = 90, Height = 30 };

    private readonly ToolTip _toolTip = new();
    private readonly ErrorProvider _errorProvider = new();

    private readonly Label _nodeKeyLabel = CreateFieldLabel("Node key");
    private readonly Label _requestedProfileLabel = CreateFieldLabel("Requested profile");

    private readonly Panel _nodeKeySidePanel = new() { Width = 1, Height = 1 };
    private readonly Panel _requestedProfileSidePanel = new() { Width = 1, Height = 1 };

    private TableLayoutPanel? _inputLayout;

    private bool _rankingFileValid;
    private bool _csvFileValid;

    public MainForm()
    {
        Text = "Poker Range Approx";
        Width = 1280;
        Height = 820;
        MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = SystemColors.Control;

        _toolTip.AutoPopDelay = 15000;
        _toolTip.InitialDelay = 250;
        _toolTip.ReshowDelay = 100;
        _toolTip.ShowAlways = true;

        _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        ConfigureControlSizes();
        ConfigureEvents();
        BuildLayout();

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

        BindHelpText();
        RefreshRankingFileState();
        RefreshCsvFileState();
        UpdateDynamicFieldVisibility();
        SetInitialHelpText();
    }

    private void ConfigureControlSizes()
    {
        foreach (var control in new Control[]
        {
            _rankingPathTextBox,
            _csvPathTextBox,
            _modeComboBox,
            _nodeKeyComboBox,
            _requestedProfileComboBox,
            _rakePercentTextBox,
            _rakeCapBbTextBox,
            _openSizeTextBox,
            _threeBetSizeTextBox,
            _fourBetSizeTextBox,
            _outputRootTextBox
        })
        {
            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(0, 2, 0, 2);
            control.Font = new Font("Segoe UI", 9);
        }

        _browseRankingButton.Margin = new Padding(8, 1, 0, 1);
        _browseCsvButton.Margin = new Padding(8, 1, 0, 1);
        _runButton.Margin = new Padding(8, 1, 0, 1);

        _helpTextBox.Font = new Font("Segoe UI", 10);
        _statusTextBox.Font = new Font("Consolas", 10);
    }

    private void ConfigureEvents()
    {
        _browseRankingButton.Click += (_, _) => BrowseRankingFile();
        _browseCsvButton.Click += (_, _) => BrowseCsvFile();
        _runButton.Click += async (_, _) => await RunAnalysisAsync();

        _modeComboBox.SelectedIndexChanged += (_, _) => UpdateDynamicFieldVisibility();

        _rankingPathTextBox.TextChanged += (_, _) => RefreshRankingFileState();
        _csvPathTextBox.TextChanged += (_, _) => RefreshCsvFileState();
    }

    private void BuildLayout()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(12)
        };

        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var leftPanel = BuildLeftPanel();
        var rightPanel = BuildRightPanel();

        leftPanel.Margin = new Padding(0, 0, 12, 0);
        rightPanel.Margin = new Padding(0);

        mainLayout.Controls.Add(leftPanel, 0, 0);
        mainLayout.Controls.Add(rightPanel, 1, 0);

        Controls.Add(mainLayout);
    }

    private Control BuildLeftPanel()
    {
        var scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        var contentPanel = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        var inputsGroup = new GroupBox
        {
            Text = "Inputs",
            Dock = DockStyle.Top,
            Padding = new Padding(12),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        _inputLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 11,
            Margin = new Padding(0)
        };

        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        for (int i = 0; i < 11; i++)
            _inputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddStandardRow(
            RankingFileRow,
            "Ranking file",
            _rankingPathTextBox,
            _browseRankingButton,
            "Text file containing hand ranking profiles used to approximate ranges.\r\n\r\nExample:\r\nprefloprankings.txt");

        AddStandardRow(
            CsvFileRow,
            "CSV file",
            _csvPathTextBox,
            _browseCsvButton,
            "Population CSV export from PokerTracker 4.\r\n\r\nLeave this blank to use database mode instead.");

        AddStandardRow(
            ModeRow,
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
            NodeKeyRow,
            _nodeKeyLabel,
            _nodeKeyComboBox,
            _nodeKeySidePanel,
            "Used only in single mode to identify the exact node to generate.\r\n\r\nExamples:\r\nrfi_btn\r\ncall_sb_vs_btn\r\nfourbet_btn_vs_threebet_sb");

        AddCustomRow(
            RequestedProfileRow,
            _requestedProfileLabel,
            _requestedProfileComboBox,
            _requestedProfileSidePanel,
            "Optional ranking profile override. Leave as (auto) to use automatic profile selection.\r\n\r\nExample:\r\nPokerstove");

        AddStandardRow(
            RakePercentRow,
            "Rake percent",
            _rakePercentTextBox,
            new Panel(),
            "Rake percentage applied to flop-seen branches.\r\n\r\nExample:\r\n0.05 for 5%");

        AddStandardRow(
            RakeCapRow,
            "Rake cap (bb)",
            _rakeCapBbTextBox,
            new Panel(),
            "Maximum rake cap measured in big blinds.\r\n\r\nExample:\r\n150");

        AddStandardRow(
            OpenSizeRow,
            "Open size",
            _openSizeTextBox,
            new Panel(),
            "Open raise size in big blinds used by exploit analysis.\r\n\r\nExample:\r\n2.5");

        AddStandardRow(
            ThreeBetSizeRow,
            "3bet size",
            _threeBetSizeTextBox,
            new Panel(),
            "3bet size in big blinds used by exploit analysis.\r\n\r\nExample:\r\n8.5");

        AddStandardRow(
            FourBetSizeRow,
            "4bet size",
            _fourBetSizeTextBox,
            new Panel(),
            "4bet size in big blinds used by exploit analysis.\r\n\r\nExample:\r\n23");

        AddStandardRow(
            OutputRootRow,
            "Output root",
            _outputRootTextBox,
            _runButton,
            "Run output directory generated by the app after execution starts.");

        inputsGroup.Controls.Add(_inputLayout);
        contentPanel.Controls.Add(inputsGroup);
        scrollPanel.Controls.Add(contentPanel);

        return scrollPanel;
    }

    private Control BuildRightPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };

        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));

        var helpGroup = new GroupBox
        {
            Text = "Field help",
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        BindHelp(helpGroup, "Descriptions and examples for the selected field.");
        BindHelp(_helpTextBox, "Descriptions and examples for the selected field.");
        helpGroup.Controls.Add(_helpTextBox);

        var statusGroup = new GroupBox
        {
            Text = "Status",
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        BindHelp(statusGroup, "Execution log and status messages for the current run.");
        BindHelp(_statusTextBox, "Execution log and status messages for the current run.");
        statusGroup.Controls.Add(_statusTextBox);

        panel.Controls.Add(helpGroup, 0, 0);
        panel.Controls.Add(statusGroup, 0, 1);

        return panel;
    }

    private void AddStandardRow(int row, string labelText, Control mainControl, Control sideControl, string helpText)
    {
        AddCustomRow(row, CreateFieldLabel(labelText), mainControl, sideControl, helpText);
    }

    private void AddCustomRow(int row, Label label, Control mainControl, Control sideControl, string helpText)
    {
        if (_inputLayout is null)
            return;

        label.Margin = new Padding(0, 4, 12, 4);
        mainControl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        sideControl.Anchor = AnchorStyles.Left;

        if (sideControl is Panel panel)
            panel.Margin = new Padding(0);

        BindHelp(label, helpText);
        BindHelp(mainControl, helpText);

        if (sideControl is not Panel)
            BindHelp(sideControl, helpText);

        _inputLayout.Controls.Add(label, 0, row);
        _inputLayout.Controls.Add(mainControl, 1, row);
        _inputLayout.Controls.Add(sideControl, 2, row);
    }

    private static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 4, 12, 4),
            Font = new Font("Segoe UI", 9)
        };
    }

    private void BindHelpText()
    {
        BindHelp(_browseRankingButton, "Browse for a ranking file.\r\n\r\nExample:\r\nprefloprankings.txt");
        BindHelp(_browseCsvButton, "Browse for a population CSV export.\r\n\r\nLeave blank to use database mode.");
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
            "Typical CSV run:\r\n" +
            "- choose a ranking file\r\n" +
            "- choose a CSV file\r\n" +
            "- leave Mode on rfi\r\n\r\n" +
            "Database mode:\r\n" +
            "- choose a ranking file\r\n" +
            "- leave CSV blank\r\n" +
            "- click Run and enter database connection details";
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

    private void RefreshRankingFileState()
    {
        ClearProfiles();
        _rankingFileValid = ValidateAndLoadRankingProfiles(_rankingPathTextBox.Text.Trim());
        UpdateDynamicFieldVisibility();
    }

    private void RefreshCsvFileState()
    {
        ClearNodeKeys();
        _csvFileValid = ValidateAndLoadNodeKeys(_csvPathTextBox.Text.Trim());
        UpdateDynamicFieldVisibility();
    }

    private void ClearProfiles()
    {
        _requestedProfileComboBox.Items.Clear();
        _requestedProfileComboBox.Items.Add("(auto)");
        _requestedProfileComboBox.SelectedIndex = 0;
    }

    private void ClearNodeKeys()
    {
        _nodeKeyComboBox.Items.Clear();
        _nodeKeyComboBox.Text = string.Empty;
    }

    private bool ValidateAndLoadRankingProfiles(string filePath)
    {
        _errorProvider.SetError(_rankingPathTextBox, string.Empty);

        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        if (!File.Exists(filePath))
        {
            _errorProvider.SetError(_rankingPathTextBox, "Ranking file does not exist.");
            return false;
        }

        try
        {
            var parser = new RankingFileParser();
            var profiles = parser.Parse(filePath);

            if (profiles.Count == 0)
            {
                _errorProvider.SetError(_rankingPathTextBox, "Ranking file did not contain any ranking profiles.");
                return false;
            }

            foreach (var profileName in profiles
                .Select(p => p.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                _requestedProfileComboBox.Items.Add(profileName);
            }

            _requestedProfileComboBox.SelectedIndex = 0;
            return true;
        }
        catch (Exception ex)
        {
            _errorProvider.SetError(_rankingPathTextBox, $"Invalid ranking file: {ex.Message}");
            return false;
        }
    }

    private bool ValidateAndLoadNodeKeys(string filePath)
    {
        _errorProvider.SetError(_csvPathTextBox, string.Empty);

        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        if (!File.Exists(filePath))
        {
            _errorProvider.SetError(_csvPathTextBox, "CSV file does not exist.");
            return false;
        }

        try
        {
            var reader = new PopulationCsvReader();
            var rows = reader.Read(filePath);

            if (rows.Count == 0)
            {
                _errorProvider.SetError(_csvPathTextBox, "CSV file contained no data rows.");
                return false;
            }

            var extractor = new PopulationNodeExtractor();
            var nodes = extractor.Extract(rows[0]);

            if (nodes.Count == 0)
            {
                _errorProvider.SetError(_csvPathTextBox, "CSV file did not contain any supported node columns.");
                return false;
            }

            foreach (var key in nodes
                .Select(n => ToNodeKey(n.NodeId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                _nodeKeyComboBox.Items.Add(key);
            }

            if (_nodeKeyComboBox.Items.Count > 0)
                _nodeKeyComboBox.SelectedIndex = 0;

            return true;
        }
        catch (Exception ex)
        {
            _errorProvider.SetError(_csvPathTextBox, $"Invalid CSV file: {ex.Message}");
            return false;
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

        SetRowVisible(
            NodeKeyRow,
            isSingleMode && _csvFileValid,
            _nodeKeyLabel,
            _nodeKeyComboBox,
            _nodeKeySidePanel);

        SetRowVisible(
            RequestedProfileRow,
            _rankingFileValid,
            _requestedProfileLabel,
            _requestedProfileComboBox,
            _requestedProfileSidePanel);
    }

    private void SetRowVisible(int rowIndex, bool visible, params Control[] controls)
    {
        if (_inputLayout is null)
            return;

        foreach (var control in controls)
            control.Visible = visible;

        _inputLayout.RowStyles[rowIndex].SizeType = visible ? SizeType.AutoSize : SizeType.Absolute;
        _inputLayout.RowStyles[rowIndex].Height = 0;
    }

    private bool ValidateInputsForRun(
        out double rakePercent,
        out double rakeCapBb,
        out double openSize,
        out double threeBetSize,
        out double fourBetSize,
        out bool useDatabaseMode)
    {
        rakePercent = 0;
        rakeCapBb = 0;
        openSize = 0;
        threeBetSize = 0;
        fourBetSize = 0;

        if (!_rankingFileValid)
            throw new InvalidOperationException("Select a valid ranking file.");

        useDatabaseMode = string.IsNullOrWhiteSpace(_csvPathTextBox.Text.Trim());

        if (!useDatabaseMode && !_csvFileValid)
            throw new InvalidOperationException("Select a valid CSV file or leave CSV blank to use database mode.");

        if (!double.TryParse(_rakePercentTextBox.Text, out rakePercent))
            throw new InvalidOperationException("Rake percent must be a valid number.");

        if (!double.TryParse(_rakeCapBbTextBox.Text, out rakeCapBb))
            throw new InvalidOperationException("Rake cap bb must be a valid number.");

        if (!double.TryParse(_openSizeTextBox.Text, out openSize))
            throw new InvalidOperationException("Open size must be a valid number.");

        if (!double.TryParse(_threeBetSizeTextBox.Text, out threeBetSize))
            throw new InvalidOperationException("3bet size must be a valid number.");

        if (!double.TryParse(_fourBetSizeTextBox.Text, out fourBetSize))
            throw new InvalidOperationException("4bet size must be a valid number.");

        if (rakePercent < 0)
            throw new InvalidOperationException("Rake percent cannot be negative.");

        if (rakeCapBb < 0)
            throw new InvalidOperationException("Rake cap bb cannot be negative.");

        if (openSize <= 0 || threeBetSize <= 0 || fourBetSize <= 0)
            throw new InvalidOperationException("Bet sizes must be greater than zero.");

        return true;
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
}