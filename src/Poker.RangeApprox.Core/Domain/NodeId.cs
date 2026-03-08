namespace Poker.RangeApprox.Core.Domain;
public sealed record NodeId(
    string Action,
    string Actor,
    string? Opponent
);