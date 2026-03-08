namespace Poker.RangeApprox.Core.Domain;

public sealed record RankingProfile(string Name, IReadOnlyList<HandClass> OrderedHands);