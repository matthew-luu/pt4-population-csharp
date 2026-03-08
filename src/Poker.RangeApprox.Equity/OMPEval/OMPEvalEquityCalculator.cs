using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Core.Equity;

namespace Poker.RangeApprox.Equity.OMPEval;

public sealed class OMPEvalEquityCalculator : IHandVsRangeEquityCalculator
{
    public double CalculateEquity(
        HandClass heroHand,
        IReadOnlyList<RangeCell> villainRange)
    {
        ArgumentNullException.ThrowIfNull(heroHand);
        ArgumentNullException.ThrowIfNull(villainRange);

        if (villainRange.Count == 0)
            throw new ArgumentException("Villain range cannot be empty.", nameof(villainRange));

        throw new NotImplementedException(
            "OMPEval native interop has not been wired yet. " +
            "Next step is to implement the native wrapper and card/range translation.");
    }
}