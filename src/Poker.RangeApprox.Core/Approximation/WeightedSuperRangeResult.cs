using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed record WeightedSuperRangeResult(
    string Key,
    string OpenPosition,
    int SourceRangeCount,
    double TotalContributionCombos,
    double AverageCombosPerSourceRange,
    IReadOnlyList<RangeCell> Cells
);