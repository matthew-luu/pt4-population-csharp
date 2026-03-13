using Poker.RangeApprox.Core.Domain;
using System.Data;

namespace Poker.RangeApprox.WinForms.Controls;

public sealed class RangeMatrixControl : UserControl
{
    private static readonly char[] Ranks = "AKQJT98765432".ToCharArray();

    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        AllowUserToResizeColumns = false,
        RowHeadersWidth = 60,
        SelectionMode = DataGridViewSelectionMode.CellSelect,
        MultiSelect = false
    };

    public RangeMatrixControl()
    {
        Controls.Add(_grid);
        BuildGrid();
    }

    private void BuildGrid()
    {
        _grid.Columns.Clear();

        foreach (var r in Ranks)
        {
            _grid.Columns.Add(r.ToString(), r.ToString());
            _grid.Columns[^1].Width = 50;
        }

        _grid.Rows.Clear();

        foreach (var r in Ranks)
        {
            int rowIndex = _grid.Rows.Add();
            _grid.Rows[rowIndex].HeaderCell.Value = r.ToString();
        }
    }

    public void LoadRange(IReadOnlyList<RangeCell> cells)
    {
        var map = cells.ToDictionary(
            c => c.HandClass.ToEquilabToken(),
            c => c.Weight);

        for (int row = 0; row < Ranks.Length; row++)
        {
            for (int col = 0; col < Ranks.Length; col++)
            {
                string token = BuildToken(row, col);

                double weight = map.TryGetValue(token, out var w)
                    ? w
                    : 0;

                var cell = _grid.Rows[row].Cells[col];

                cell.Value = weight.ToString("0.##");

                ColorizeCell(cell, weight);
            }
        }
    }

    private static string BuildToken(int row, int col)
    {
        char r1 = Ranks[row];
        char r2 = Ranks[col];

        if (row == col)
            return $"{r1}{r2}";

        if (row < col)
            return $"{r1}{r2}s";

        return $"{r2}{r1}o";
    }

    private static void ColorizeCell(DataGridViewCell cell, double weight)
    {
        if (weight <= 0)
        {
            cell.Style.BackColor = Color.White;
            return;
        }

        int intensity = Math.Min(255, (int)(weight * 255));

        cell.Style.BackColor = Color.FromArgb(255 - intensity, 255, 255 - intensity);
    }
}