using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed record ActionPartitionRequest(
    string SpotKey,
    ApproximationRequest? AggressiveRequest,
    ApproximationRequest? CallRequest,
    ApproximationRequest? FoldRequest,
    RankingProfile RankingProfile,
    IReadOnlyList<RangeCell>? CandidateUniverse
);