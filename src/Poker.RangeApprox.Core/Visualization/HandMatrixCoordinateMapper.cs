using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Visualization;

public static class HandMatrixCoordinateMapper
{
    private static readonly char[] Ranks = "AKQJT98765432".ToCharArray();

    public static (int Row, int Column) GetCoordinates(HandClass hand)
    {
        int r1 = Array.IndexOf(Ranks, hand.HighRank);
        int r2 = Array.IndexOf(Ranks, hand.LowRank);

        if (hand.Shape == HandShape.Pair)
            return (r1, r2);

        if (hand.Shape == HandShape.Suited)
            return (r1, r2);

        return (r2, r1);
    }
}