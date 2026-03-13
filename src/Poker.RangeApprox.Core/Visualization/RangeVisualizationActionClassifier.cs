using Poker.RangeApprox.Core.Approximation;

namespace Poker.RangeApprox.Core.Visualization;

public static class RangeVisualizationActionClassifier
{
    public static (string id, string label) ClassifyOpen(ExploitHandResult hand)
    {
        if (hand.TotalEv > 0)
            return ("raise", "Raise");

        return ("fold", "Fold");
    }

    public static (string id, string label) ClassifyVsOpen(ExploitThreeBetHandResult hand)
    {
        if (hand.TotalEv > 0)
            return ("threebet", "3bet");

        return ("fold", "Fold");
    }
}