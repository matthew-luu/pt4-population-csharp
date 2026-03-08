namespace Poker.RangeApprox.Core.Domain;

public sealed record NodeDefinition(
    NodeId NodeId,
    FrequencyBasis FrequencyBasis,
    NodeId? ParentNodeId
);