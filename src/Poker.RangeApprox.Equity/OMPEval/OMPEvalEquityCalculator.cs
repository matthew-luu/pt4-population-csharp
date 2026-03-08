using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Core.Equity;

namespace Poker.RangeApprox.Equity.OMPEval;

public sealed class OMPEvalEquityCalculator : IHandVsRangeEquityCalculator
{
    private readonly int _iterations;

    public OMPEvalEquityCalculator(int iterations = 20000)
    {
        if (iterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations must be greater than 0.");

        _iterations = iterations;
    }

    public double CalculateEquity(
        HandClass heroHand,
        IReadOnlyList<RangeCell> villainRange)
    {
        ArgumentNullException.ThrowIfNull(heroHand);
        ArgumentNullException.ThrowIfNull(villainRange);

        if (villainRange.Count == 0)
            throw new ArgumentException("Villain range cannot be empty.", nameof(villainRange));

        var hero = heroHand.ToEquilabToken();
        var villain = WeightedRangeSerializer.Serialize(villainRange);

        var equity = OMPEvalNative.pra_calc_equity_vs_weighted_range(
            hero,
            villain,
            _iterations,
            out var errorCode);

        if (errorCode != 0)
        {
            throw new InvalidOperationException(
                $"Native equity calculation failed for hero '{hero}' with error code {errorCode}.");
        }

        return equity;
    }
}