namespace Poker.RangeApprox.Core.Visualization;

public sealed record RangeVisualizationView(
    string ViewId,
    string ViewLabel,
    string HeroPosition,
    string? VillainPosition,
    IReadOnlyList<RangeVisualizationCell> Cells,
    IReadOnlyList<RangeVisualizationLegendItem> Legend
);