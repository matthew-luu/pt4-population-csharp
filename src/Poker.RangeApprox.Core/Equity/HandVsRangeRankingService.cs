using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Equity;

public sealed class HandVsRangeRankingService : IHandVsRangeRankingService
{
    private readonly IHandVsRangeEquityCalculator _equityCalculator;

    public HandVsRangeRankingService(IHandVsRangeEquityCalculator equityCalculator)
    {
        _equityCalculator = equityCalculator ?? throw new ArgumentNullException(nameof(equityCalculator));
    }

    public IReadOnlyList<HandEquityResult> RankAllHands(IReadOnlyList<RangeCell> villainRange)
    {
        ArgumentNullException.ThrowIfNull(villainRange);

        var allHands = HandClassCatalog.GetAll169();

        var results = allHands
            .Select(heroHand => new HandEquityResult(
                HeroHand: heroHand,
                Equity: _equityCalculator.CalculateEquity(heroHand, villainRange)))
            .OrderByDescending(x => x.Equity)
            .ThenBy(x => x.HeroHand.ToEquilabToken(), StringComparer.OrdinalIgnoreCase)
            .ToList();

        return results;
    }
}