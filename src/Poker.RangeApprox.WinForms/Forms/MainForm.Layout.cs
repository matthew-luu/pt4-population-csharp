namespace Poker.RangeApprox.WinForms;

public sealed partial class MainForm
{
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
        _runButton.Margin = new Padding(0, 0, 8, 0);
        _openVisualizationButton.Margin = new Padding(0);

        _helpTextBox.Font = new Font("Segoe UI", 10);
        _statusTextBox.Font = new Font("Consolas", 10);
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

        _inputLayout = CreateInputLayout();

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
            "exploit-open = run exploitative open analysis\r\n" +
            "exploit-3bet = run exploitative 3bet analysis\r\n" +
            "range-visualization = generate one .rangeviz file containing Open and vs open views");

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
            CreateOutputButtonsPanel(),
            "Run output directory generated by the app after execution starts.\r\n\r\nClick the output path to open the folder. Use Open Visualization to open the current run's .rangeviz file or browse for one.");

        inputsGroup.Controls.Add(_inputLayout);
        contentPanel.Controls.Add(inputsGroup);
        scrollPanel.Controls.Add(contentPanel);

        return scrollPanel;
    }

    private TableLayoutPanel CreateInputLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 11,
            Margin = new Padding(0)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        for (int i = 0; i < 11; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        return layout;
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

    private Control CreateOutputButtonsPanel()
    {
        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0)
        };

        panel.Controls.Add(_runButton);
        panel.Controls.Add(_openVisualizationButton);

        BindHelp(panel, "Run the selected analysis or open a saved .rangeviz file.");

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

    private void BindHelpText()
    {
        BindHelp(_browseRankingButton, "Browse for a ranking file.\r\n\r\nExample:\r\nprefloprankings.txt");
        BindHelp(_browseCsvButton, "Browse for a population CSV export.\r\n\r\nLeave blank to use database mode.");
        BindHelp(_runButton, "Run the selected analysis using the current inputs.");
        BindHelp(_openVisualizationButton, "Open the current run's .rangeviz file if one exists. Otherwise browse for a .rangeviz file.");
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
            "Visualization run:\r\n" +
            "- choose a ranking file\r\n" +
            "- choose a CSV file or use database mode\r\n" +
            "- set Mode to range-visualization\r\n" +
            "- click Run\r\n" +
            "- click Open Visualization to open the current run's range_visualization.rangeviz\r\n\r\n" +
            "Database mode:\r\n" +
            "- choose a ranking file\r\n" +
            "- leave CSV blank\r\n" +
            "- click Run and enter database connection details";
    }
}