using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Equity;

public sealed record HandEquityResult(
    HandClass HeroHand,
    double Equity
);