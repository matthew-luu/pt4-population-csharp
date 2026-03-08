using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Formatting;

public static class EquilabFormatter
{
    public static string FormatExplicit(IReadOnlyList<RangeCell> cells)
    {
        return FormatExplicit(cells, minimumWeight: 0.0);
    }

    public static string FormatExplicit(IReadOnlyList<RangeCell> cells, double minimumWeight)
    {
        return string.Join(",", cells
            .Where(c => c.Weight > minimumWeight)
            .Select(c => c.HandClass.ToEquilabToken()));
    }
}