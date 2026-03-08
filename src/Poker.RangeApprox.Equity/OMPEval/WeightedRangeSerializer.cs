using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Equity.OMPEval;

internal static class WeightedRangeSerializer
{
    public static string Serialize(IReadOnlyList<RangeCell> range)
    {
        ArgumentNullException.ThrowIfNull(range);

        return string.Join(",",
            range
                .Where(c => c.Weight > 0)
                .Select(c => c.HandClass.ToEquilabToken()));
    }
}