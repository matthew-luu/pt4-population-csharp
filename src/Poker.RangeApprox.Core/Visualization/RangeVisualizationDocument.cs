namespace Poker.RangeApprox.Core.Visualization;

public sealed record RangeVisualizationDocument(
    string Version,
    DateTime CreatedUtc,
    IReadOnlyList<RangeVisualizationScenario> Scenarios
);