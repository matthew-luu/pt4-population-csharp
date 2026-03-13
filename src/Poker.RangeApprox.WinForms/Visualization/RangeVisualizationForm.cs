using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.WinForms.Controls;

namespace Poker.RangeApprox.WinForms.Visualization;

public sealed class RangeVisualizationForm : Form
{
    private readonly TabControl _tabControl = new() { Dock = DockStyle.Fill };

    public RangeVisualizationForm()
    {
        Text = "Range Visualization";
        Width = 900;
        Height = 700;

        Controls.Add(_tabControl);

        BuildTabs();
    }

    private void BuildTabs()
    {
        var openTab = new TabPage("Open");
        var vsOpenTab = new TabPage("vs Open");

        _tabControl.TabPages.Add(openTab);
        _tabControl.TabPages.Add(vsOpenTab);

        openTab.Controls.Add(BuildHeroVillainTabs());
        vsOpenTab.Controls.Add(BuildHeroVillainTabs());
    }

    private Control BuildHeroVillainTabs()
    {
        var tab = new TabControl
        {
            Dock = DockStyle.Fill
        };

        tab.TabPages.Add(new TabPage("Hero"));
        tab.TabPages.Add(new TabPage("Villain"));

        return tab;
    }

    public void LoadRange(string tabGroup, string role, IReadOnlyList<RangeCell> cells)
    {
        foreach (TabPage group in _tabControl.TabPages)
        {
            if (group.Text != tabGroup)
                continue;

            var inner = group.Controls[0] as TabControl;
            if (inner == null)
                return;

            foreach (TabPage roleTab in inner.TabPages)
            {
                if (roleTab.Text != role)
                    continue;

                roleTab.Controls.Clear();

                var matrix = new RangeMatrixControl
                {
                    Dock = DockStyle.Fill
                };

                matrix.LoadRange(cells);

                roleTab.Controls.Add(matrix);
            }
        }
    }
}