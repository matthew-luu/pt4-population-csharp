using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Visualization;

public static class RangeVisualizationLegendBuilder
{
    public static IReadOnlyList<RangeVisualizationLegendItem> Build(
        IEnumerable<RangeVisualizationCell> cells)
    {
        var grouped = cells.GroupBy(c => c.ActionId);

        int totalCombos = cells.Sum(c =>
            GetComboCount(c.Hand));

        var legend = new List<RangeVisualizationLegendItem>();

        foreach (var group in grouped)
        {
            int combos = group.Sum(c => GetComboCount(c.Hand));

            double pct = totalCombos == 0
                ? 0
                : (double)combos / totalCombos * 100.0;

            legend.Add(new RangeVisualizationLegendItem(
                group.Key,
                group.First().ActionLabel,
                combos,
                pct));
        }

        return legend
            .OrderByDescending(x => x.ComboCount)
            .ToList();
    }

    private static int GetComboCount(string token)
    {
        if (token.Length == 2)
            return 6;

        if (token.EndsWith("s"))
            return 4;

        return 12;
    }
}