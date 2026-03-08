using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Formatting;

public static class EquilabFormatter
{
    public static string FormatExplicit(IReadOnlyList<RangeCell> cells)
    {
        return string.Join(",", cells
            .Where(c => c.Weight > 0)
            .Select(c => c.HandClass.ToEquilabToken()));
    }
}