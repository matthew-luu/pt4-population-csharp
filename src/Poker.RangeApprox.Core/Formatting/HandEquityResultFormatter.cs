using Poker.RangeApprox.Core.Equity;

namespace Poker.RangeApprox.Core.Formatting;

public static class HandEquityResultFormatter
{
    public static string Format(IReadOnlyList<HandEquityResult> results)
    {
        return string.Join(
            Environment.NewLine,
            results.Select((r, i) => $"{i + 1,3}. {r.HeroHand.ToEquilabToken(),4}  {r.Equity:P2}"));
    }
}