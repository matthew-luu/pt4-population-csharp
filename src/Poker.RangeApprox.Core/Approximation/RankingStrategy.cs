using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed record RankingStrategy(
    RankingProfile Profile,
    SelectionDirection Direction
);