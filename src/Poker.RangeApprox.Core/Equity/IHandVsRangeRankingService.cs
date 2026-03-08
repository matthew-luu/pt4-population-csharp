using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Equity;

public interface IHandVsRangeRankingService
{
    IReadOnlyList<HandEquityResult> RankAllHands(
        IReadOnlyList<RangeCell> villainRange);
}