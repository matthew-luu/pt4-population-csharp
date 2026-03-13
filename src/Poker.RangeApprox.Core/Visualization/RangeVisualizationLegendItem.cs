namespace Poker.RangeApprox.Core.Visualization;

public sealed record RangeVisualizationLegendItem(
    string ActionId,
    string ActionLabel,
    int ComboCount,
    double Percentage
);