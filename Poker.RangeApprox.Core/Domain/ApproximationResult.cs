namespace Poker.RangeApprox.Core.Domain;

public sealed record ApproximationResult(
    double TargetPercent,
    double ActualPercent,
    double TargetCombos,
    double ActualCombos,
    IReadOnlyList<RangeCell> Cells);