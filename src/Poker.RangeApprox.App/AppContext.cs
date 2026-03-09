using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Core.Equity;
using Poker.RangeApprox.Infrastructure.Writing;

namespace Poker.RangeApprox.App;

public sealed record AppContext(
    AppOptions Options,
    string OutputRoot,
    IReadOnlyList<RankingProfile> Profiles,
    List<PopulationNode> Nodes,
    PopulationOpportunityProfile Opportunities,
    ApproximationEngine ApproximationEngine,
    RangeFileWriter RangeWriter,
    CallingSuperRangeBuilder CallingSuperRangeBuilder,
    WeightedSuperRangeFileWriter WeightedSuperRangeWriter,
    HandVsRangeRankingService HandRankingService,
    ExploitEngine ExploitEngine,
    ExploitSizingProfile ExploitSizing);