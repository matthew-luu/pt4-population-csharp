using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Tests;

public static class TestFixtures
{
    public static ActionPartitionRequest CreateSimplePartitionRequest()
    {
        var rankingProfile = new RankingProfile(
            Name: "Test Profile",
            OrderedHands: new List<HandClass>
            {
                HandClass.ParseRankingToken("AAp"),
                HandClass.ParseRankingToken("KKp"),
                HandClass.ParseRankingToken("QQp"),
                HandClass.ParseRankingToken("AKs"),
                HandClass.ParseRankingToken("AQs"),
                HandClass.ParseRankingToken("AKo")
            });

        var candidateUniverse = rankingProfile.OrderedHands
            .Select(h => new RangeCell(h, 1.0))
            .ToList();

        // Total combos in candidate universe:
        // AAp = 6
        // KKp = 6
        // QQp = 6
        // AKs = 4
        // AQs = 4
        // AKo = 12
        // Total = 38
        //
        // We will partition this as:
        // raise = 12 combos
        // fold  = 12 combos
        // call  = middle leftover (14 combos)

        var aggressiveRequest = new ApproximationRequest(
            NodeId: new NodeId("threebet", "btn", "co"),
            LocalFrequencyPercent: 0,
            FrequencyBasis: FrequencyBasis.FullUniverse,
            ParentNodeId: null,
            TargetCombos: 12.0,
            RankingProfileName: rankingProfile.Name);

        var callRequest = new ApproximationRequest(
            NodeId: new NodeId("call", "btn", "co"),
            LocalFrequencyPercent: 0,
            FrequencyBasis: FrequencyBasis.FullUniverse,
            ParentNodeId: null,
            TargetCombos: 14.0,
            RankingProfileName: rankingProfile.Name);

        var foldRequest = new ApproximationRequest(
            NodeId: new NodeId("fold", "btn", "co"),
            LocalFrequencyPercent: 0,
            FrequencyBasis: FrequencyBasis.FullUniverse,
            ParentNodeId: null,
            TargetCombos: 12.0,
            RankingProfileName: rankingProfile.Name);

        return new ActionPartitionRequest(
            SpotKey: "btn_vs_co",
            AggressiveRequest: aggressiveRequest,
            CallRequest: callRequest,
            FoldRequest: foldRequest,
            RankingProfile: rankingProfile,
            CandidateUniverse: candidateUniverse);
    }
}