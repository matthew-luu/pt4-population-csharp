namespace Poker.RangeApprox.Core.Visualization;

public sealed record RangeVisualizationScenario(
    string ScenarioId,
    string ScenarioLabel,
    IReadOnlyList<RangeVisualizationView> Views
);