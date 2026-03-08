using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed record ApproximationRequest(
    NodeId NodeId,
    double LocalFrequencyPercent,
    FrequencyBasis FrequencyBasis,
    NodeId? ParentNodeId,
    double TargetCombos,
    string? RankingProfileName
);