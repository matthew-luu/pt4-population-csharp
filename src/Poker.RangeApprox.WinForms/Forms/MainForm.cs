using Poker.RangeApprox.App;
using Poker.RangeApprox.App.Execution;
using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.WinForms;

public sealed partial class MainForm : Form
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
    private readonly TextBox _outputRootTextBox = new()
    {
        ReadOnly = true,
        Cursor = Cursors.Hand
    };

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
    private readonly Button _openVisualizationButton = new() { Text = "Open Visualization...", Width = 150, Height = 32 };
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
        InitializeModeOptions();
        InitializeRequestedProfileOptions();

        BindHelpText();
        RefreshRankingFileState();
        RefreshCsvFileState();
        UpdateDynamicFieldVisibility();
        SetInitialHelpText();
    }

    private void InitializeModeOptions()
    {
        _modeComboBox.Items.AddRange([
            "rfi",
            "single",
            "all",
            "rank-supercalls",
            "exploit-open",
            "exploit-3bet",
            "range-visualization"
        ]);

        _modeComboBox.SelectedItem = "rfi";
    }

    private void InitializeRequestedProfileOptions()
    {
        _requestedProfileComboBox.Items.Add("(auto)");
        _requestedProfileComboBox.SelectedIndex = 0;
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

    private static string ToNodeKey(NodeId nodeId)
    {
        if (string.Equals(nodeId.Action, "rfi", StringComparison.OrdinalIgnoreCase))
            return $"rfi_{nodeId.Actor}";

        return $"{nodeId.Action}_{nodeId.Actor}_vs_{nodeId.Opponent}";
    }
}