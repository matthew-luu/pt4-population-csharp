using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Equity;

public interface IHandVsRangeEquityCalculator
{
    double CalculateEquity(
        HandClass heroHand,
        IReadOnlyList<RangeCell> villainRange);
}
