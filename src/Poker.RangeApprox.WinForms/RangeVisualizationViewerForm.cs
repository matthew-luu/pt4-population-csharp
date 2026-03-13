using Poker.RangeApprox.Core.Visualization;
using System.Reflection;

namespace Poker.RangeApprox.WinForms;

public sealed class RangeVisualizationViewerForm : Form
{
    private static readonly string[] RankOrder = ["A", "K", "Q", "J", "T", "9", "8", "7", "6", "5", "4", "3", "2"];

    private readonly RangeVisualizationDocument _document;
    private readonly Dictionary<string, TableLayoutPanel> _tableCache = new(StringComparer.OrdinalIgnoreCase);

    private readonly TabControl _scenarioTabControl = new()
    {
        Dock = DockStyle.Top,
        Height = 42
    };

    private readonly FlowLayoutPanel _heroPanel = new()
    {
        Dock = DockStyle.Top,
        Height = 44,
        AutoScroll = true,
        WrapContents = false,
        FlowDirection = FlowDirection.LeftToRight,
        Padding = new Padding(6, 6, 6, 6)
    };

    private readonly FlowLayoutPanel _villainPanel = new()
    {
        Dock = DockStyle.Top,
        Height = 44,
        AutoScroll = true,
        WrapContents = false,
        FlowDirection = FlowDirection.LeftToRight,
        Padding = new Padding(6, 6, 6, 6)
    };

    private readonly Label _heroLabel = new()
    {
        Text = "Hero",
        Dock = DockStyle.Top,
        Height = 22,
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        Padding = new Padding(8, 2, 0, 0)
    };

    private readonly Label _villainLabel = new()
    {
        Text = "Villain",
        Dock = DockStyle.Top,
        Height = 22,
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        Padding = new Padding(8, 2, 0, 0)
    };

    private readonly Panel _matrixHost = new()
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(8)
    };

    private readonly FlowLayoutPanel _legendPanel = new()
    {
        Dock = DockStyle.Top,
        Height = 42,
        AutoScroll = true,
        WrapContents = false,
        FlowDirection = FlowDirection.LeftToRight,
        Padding = new Padding(8, 8, 8, 8)
    };

    private readonly TextBox _detailsTextBox = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Font = new Font("Consolas", 10),
        BorderStyle = BorderStyle.FixedSingle
    };

    private readonly SplitContainer _contentSplit = new()
    {
        Dock = DockStyle.Fill,
        Orientation = Orientation.Horizontal,
        SplitterDistance = 600
    };

    private readonly ToolTip _toolTip = new();

    private string? _selectedScenarioId;
    private string? _selectedHero;
    private string? _selectedVillain;

    public RangeVisualizationViewerForm(RangeVisualizationDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));

        Text = "Range Visualization Viewer";
        Width = 1200;
        Height = 900;
        MinimumSize = new Size(980, 720);
        StartPosition = FormStartPosition.CenterParent;

        BuildLayout();
        PrebuildTables();
        LoadScenarios();
    }

    private void BuildLayout()
    {
        EnableDoubleBuffering(_matrixHost);
        EnableDoubleBuffering(_heroPanel);
        EnableDoubleBuffering(_villainPanel);
        EnableDoubleBuffering(_legendPanel);

        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 152
        };

        topPanel.Controls.Add(_villainPanel);
        topPanel.Controls.Add(_villainLabel);
        topPanel.Controls.Add(_heroPanel);
        topPanel.Controls.Add(_heroLabel);
        topPanel.Controls.Add(_scenarioTabControl);

        var legendGroup = new GroupBox
        {
            Text = "Legend",
            Dock = DockStyle.Top,
            Height = 72,
            Padding = new Padding(6),
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        legendGroup.Controls.Add(_legendPanel);

        var detailsGroup = new GroupBox
        {
            Text = "Selected Hand",
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        detailsGroup.Controls.Add(_detailsTextBox);

        _contentSplit.Panel1.Controls.Add(_matrixHost);
        _contentSplit.Panel2.Controls.Add(detailsGroup);

        Controls.Add(_contentSplit);
        Controls.Add(legendGroup);
        Controls.Add(topPanel);

        _scenarioTabControl.SelectedIndexChanged += (_, _) => OnScenarioChanged();
    }

    private void PrebuildTables()
    {
        foreach (var scenario in _document.Scenarios)
        {
            foreach (var view in scenario.Views)
            {
                _tableCache[view.ViewId] = BuildMatrixTable(view);
            }
        }
    }

    private void LoadScenarios()
    {
        _scenarioTabControl.TabPages.Clear();

        foreach (var scenario in _document.Scenarios)
        {
            var page = new TabPage(scenario.ScenarioLabel)
            {
                Tag = scenario.ScenarioId
            };

            _scenarioTabControl.TabPages.Add(page);
        }

        if (_scenarioTabControl.TabPages.Count > 0)
        {
            _scenarioTabControl.SelectedIndex = 0;
            _selectedScenarioId = _scenarioTabControl.SelectedTab?.Tag?.ToString();
            RefreshHeroButtons();
        }
    }

    private void OnScenarioChanged()
    {
        _selectedScenarioId = _scenarioTabControl.SelectedTab?.Tag?.ToString();
        _selectedHero = null;
        _selectedVillain = null;

        RefreshHeroButtons();
    }

    private void RefreshHeroButtons()
    {
        _heroPanel.SuspendLayout();
        _heroPanel.Controls.Clear();

        var scenario = GetSelectedScenario();
        if (scenario is null)
        {
            _heroPanel.ResumeLayout();
            RefreshVillainButtons();
            return;
        }

        var heroPositions = scenario.Views
            .Select(x => x.HeroPosition)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(GetPositionSortKey)
            .ToList();

        if (heroPositions.Count == 0)
        {
            _heroPanel.ResumeLayout();
            RefreshVillainButtons();
            return;
        }

        _selectedHero ??= heroPositions[0];

        foreach (var hero in heroPositions)
        {
            _heroPanel.Controls.Add(CreateSelectionButton(
                label: hero.ToUpperInvariant(),
                isSelected: string.Equals(hero, _selectedHero, StringComparison.OrdinalIgnoreCase),
                onClick: () =>
                {
                    _selectedHero = hero;
                    _selectedVillain = null;
                    RefreshHeroButtons();
                }));
        }

        _heroPanel.ResumeLayout();
        RefreshVillainButtons();
    }

    private void RefreshVillainButtons()
    {
        _villainPanel.SuspendLayout();
        _villainPanel.Controls.Clear();

        var scenario = GetSelectedScenario();
        if (scenario is null || string.IsNullOrWhiteSpace(_selectedHero))
        {
            _villainLabel.Visible = false;
            _villainPanel.Visible = false;
            _villainPanel.ResumeLayout();
            RenderCurrentView();
            return;
        }

        var matchingViews = scenario.Views
            .Where(x => string.Equals(x.HeroPosition, _selectedHero, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var villains = matchingViews
            .Select(x => x.VillainPosition)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(GetPositionSortKey)
            .ToList();

        var showVillains = villains.Count > 0;

        _villainLabel.Visible = showVillains;
        _villainPanel.Visible = showVillains;

        if (!showVillains)
        {
            _selectedVillain = null;
            _villainPanel.ResumeLayout();
            RenderCurrentView();
            return;
        }

        _selectedVillain ??= villains[0];

        foreach (var villain in villains)
        {
            _villainPanel.Controls.Add(CreateSelectionButton(
                label: villain.ToUpperInvariant(),
                isSelected: string.Equals(villain, _selectedVillain, StringComparison.OrdinalIgnoreCase),
                onClick: () =>
                {
                    _selectedVillain = villain;
                    RefreshVillainButtons();
                }));
        }

        _villainPanel.ResumeLayout();
        RenderCurrentView();
    }

    private Button CreateSelectionButton(string label, bool isSelected, Action onClick)
    {
        var button = new Button
        {
            Text = label,
            Width = 74,
            Height = 30,
            Margin = new Padding(4, 0, 4, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = isSelected ? SystemColors.Highlight : SystemColors.ControlLight,
            ForeColor = isSelected ? Color.White : Color.Black
        };

        button.Click += (_, _) => onClick();

        return button;
    }

    private void RenderCurrentView()
    {
        _matrixHost.SuspendLayout();
        _legendPanel.SuspendLayout();

        _matrixHost.Controls.Clear();
        _legendPanel.Controls.Clear();

        var view = GetSelectedView();

        if (view is null)
        {
            _detailsTextBox.Text = "No range visualization view is available.";
            _matrixHost.ResumeLayout();
            _legendPanel.ResumeLayout();
            return;
        }

        if (_tableCache.TryGetValue(view.ViewId, out var table))
            _matrixHost.Controls.Add(table);

        foreach (var legendItem in view.Legend)
            _legendPanel.Controls.Add(CreateLegendChip(legendItem));

        _detailsTextBox.Text =
            $"Scenario : {GetSelectedScenario()?.ScenarioLabel}{Environment.NewLine}" +
            $"Hero     : {view.HeroPosition.ToUpperInvariant()}{Environment.NewLine}" +
            $"Villain  : {(string.IsNullOrWhiteSpace(view.VillainPosition) ? "-" : view.VillainPosition!.ToUpperInvariant())}{Environment.NewLine}" +
            $"{Environment.NewLine}" +
            "Click a hand cell to inspect EV details.";

        _matrixHost.ResumeLayout();
        _legendPanel.ResumeLayout();
    }

    private Control CreateLegendChip(RangeVisualizationLegendItem item)
    {
        var panel = new Panel
        {
            Width = 170,
            Height = 26,
            Margin = new Padding(0, 0, 8, 0),
            BackColor = GetActionColor(item.ActionId),
            BorderStyle = BorderStyle.FixedSingle
        };

        var label = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Text = $"{item.ActionLabel} ({item.Percentage:0.0}%)"
        };

        panel.Controls.Add(label);
        return panel;
    }

    private TableLayoutPanel BuildMatrixTable(RangeVisualizationView view)
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 14,
            RowCount = 14,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            BackColor = Color.White
        };

        EnableDoubleBuffering(table);

        for (int i = 0; i < 14; i++)
        {
            table.ColumnStyles.Add(i == 0
                ? new ColumnStyle(SizeType.Absolute, 42)
                : new ColumnStyle(SizeType.Percent, 100f / 13f));

            table.RowStyles.Add(i == 0
                ? new RowStyle(SizeType.Absolute, 30)
                : new RowStyle(SizeType.Percent, 100f / 13f));
        }

        table.Controls.Add(CreateHeaderCell(string.Empty), 0, 0);

        for (int col = 0; col < 13; col++)
            table.Controls.Add(CreateHeaderCell(RankOrder[col]), col + 1, 0);

        for (int row = 0; row < 13; row++)
            table.Controls.Add(CreateHeaderCell(RankOrder[row]), 0, row + 1);

        var cellLookup = view.Cells.ToDictionary(
            x => (x.Row, x.Column),
            x => x);

        for (int row = 0; row < 13; row++)
        {
            for (int col = 0; col < 13; col++)
            {
                cellLookup.TryGetValue((row, col), out var cell);
                table.Controls.Add(CreateHandCell(cell), col + 1, row + 1);
            }
        }

        return table;
    }

    private Control CreateHeaderCell(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            BackColor = SystemColors.ControlLight
        };
    }

    private Control CreateHandCell(RangeVisualizationCell? cell)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Padding = new Padding(0),
            BackColor = cell is null ? Color.WhiteSmoke : GetActionColor(cell.ActionId),
            Cursor = cell is null ? Cursors.Default : Cursors.Hand
        };

        var label = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Text = cell?.Hand ?? string.Empty,
            BackColor = Color.Transparent
        };

        panel.Controls.Add(label);

        if (cell is not null)
        {
            var tooltip =
                $"{cell.Hand}{Environment.NewLine}" +
                $"Action: {cell.ActionLabel}{Environment.NewLine}" +
                $"Total EV: {cell.TotalEv:0.0000}";

            _toolTip.SetToolTip(panel, tooltip);
            _toolTip.SetToolTip(label, tooltip);

            void HandleClick()
            {
                _detailsTextBox.Text = BuildDetailsText(cell);
            }

            panel.Click += (_, _) => HandleClick();
            label.Click += (_, _) => HandleClick();
        }

        return panel;
    }

    private string BuildDetailsText(RangeVisualizationCell cell)
    {
        var lines = new List<string>
        {
            $"Hand        : {cell.Hand}",
            $"Action      : {cell.ActionLabel}",
            $"Total EV    : {cell.TotalEv:0.0000}"
        };

        if (cell.FoldBranchEv.HasValue)
            lines.Add($"Fold EV     : {cell.FoldBranchEv.Value:0.0000}");

        if (cell.CallBranchEv.HasValue)
            lines.Add($"Call EV     : {cell.CallBranchEv.Value:0.0000}");

        if (cell.ThreeBetBranchEv.HasValue)
            lines.Add($"3bet EV     : {cell.ThreeBetBranchEv.Value:0.0000}");

        if (cell.FourBetBranchEv.HasValue)
            lines.Add($"4bet EV     : {cell.FourBetBranchEv.Value:0.0000}");

        return string.Join(Environment.NewLine, lines);
    }

    private RangeVisualizationScenario? GetSelectedScenario()
    {
        if (string.IsNullOrWhiteSpace(_selectedScenarioId))
            return null;

        return _document.Scenarios.FirstOrDefault(x =>
            string.Equals(x.ScenarioId, _selectedScenarioId, StringComparison.OrdinalIgnoreCase));
    }

    private RangeVisualizationView? GetSelectedView()
    {
        var scenario = GetSelectedScenario();
        if (scenario is null || string.IsNullOrWhiteSpace(_selectedHero))
            return null;

        var matchingViews = scenario.Views
            .Where(x => string.Equals(x.HeroPosition, _selectedHero, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var hasVillains = matchingViews.Any(x => !string.IsNullOrWhiteSpace(x.VillainPosition));

        if (!hasVillains)
            return matchingViews.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(_selectedVillain))
            return matchingViews.FirstOrDefault();

        return matchingViews.FirstOrDefault(x =>
            string.Equals(x.VillainPosition, _selectedVillain, StringComparison.OrdinalIgnoreCase));
    }

    private static int GetPositionSortKey(string? position)
    {
        return position?.Trim().ToLowerInvariant() switch
        {
            "lj" => 1,
            "hj" => 2,
            "co" => 3,
            "btn" => 4,
            "sb" => 5,
            "bb" => 6,
            _ => int.MaxValue
        };
    }

    private static Color GetActionColor(string actionId)
    {
        return actionId.Trim().ToLowerInvariant() switch
        {
            "raise" => Color.FromArgb(140, 214, 140),
            "threebet" => Color.FromArgb(120, 190, 255),
            "fold" => Color.FromArgb(225, 225, 225),
            _ => Color.WhiteSmoke
        };
    }

    private static void EnableDoubleBuffering(Control control)
    {
        typeof(Control)
            .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(control, true, null);
    }
}