using Poker.RangeApprox.Core.Approximation;

namespace Poker.RangeApprox.App.Execution;

public static class RankingCommands
{
    public static void WriteSuperCallRankings(
        AppContext context,
        IReadOnlyDictionary<string, WeightedSuperRangeResult> callingSuperRanges)
    {
        foreach (var superRange in callingSuperRanges.Values.OrderBy(x => x.OpenPosition, StringComparer.OrdinalIgnoreCase))
        {
            var ranking = context.HandRankingService.RankAllHands(superRange.Cells);
            OutputWriters.WriteHandRanking(context, superRange.Key, ranking);

            context.Status.WriteLine($"Ranking: {superRange.Key}");
            context.Status.WriteLine($"Output: {Path.Combine(context.OutputRoot, "rankings", "vs call super", $"ranking_vs_{superRange.Key}.txt")}");
            context.Status.WriteLine();
        }
    }
}