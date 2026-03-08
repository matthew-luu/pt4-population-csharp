using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed record ActionPartitionResult(
    string SpotKey,
    IReadOnlyDictionary<string, ApproximationResult> Results
);