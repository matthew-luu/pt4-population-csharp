using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Core.Equity;
using Poker.RangeApprox.Core.Formatting;

namespace Poker.RangeApprox.App.Execution;

public static class OutputWriters
{
    public static void WriteHandRanking(
        AppContext context,
        string key,
        IReadOnlyList<HandEquityResult> ranking)
    {
        var directory = Path.Combine(context.OutputRoot, "rankings", "vs call super");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, $"ranking_vs_{key}.txt");
        var content = HandEquityResultFormatter.Format(ranking);

        File.WriteAllText(path, content);
    }

    public static void WriteRange(
        AppContext context,
        PopulationNode node,
        ApproximationResult result)
    {
        var (directory, fileName) = OutputPathResolver.GetOutputPath(context.OutputRoot, node.NodeId);

        context.RangeWriter.WriteEquilab(Path.Combine(directory, $"{fileName}.txt"), result.Cells);

        context.Status.WriteLine($"Node: {node.NodeId.ToKey()}");
        context.Status.WriteLine($"Output: {Path.Combine(directory, fileName)}");
        context.Status.WriteLine();
    }

    public static void WriteExploitSpot(
        AppContext context,
        ExploitSpotResult result)
    {
        var directory = Path.Combine(context.OutputRoot, "exploit open", result.OpenPosition);
        Directory.CreateDirectory(directory);

        var rankingPath = Path.Combine(directory, "ranking.txt");
        var rangePath = Path.Combine(directory, "positive_ev_range.txt");

        var content = ExploitSpotResultFormatter.Format(result);
        File.WriteAllText(rankingPath, content);

        context.RangeWriter.WriteEquilab(rangePath, result.PositiveEvRange);
    }

    public static void WriteExploitThreeBetSpot(
        AppContext context,
        ExploitThreeBetSpotResult result)
    {
        var directory = Path.Combine(
            context.OutputRoot,
            "exploit 3bet",
            $"{result.ThreeBettorPosition} vs {result.OpenPosition}");

        Directory.CreateDirectory(directory);

        var rankingPath = Path.Combine(directory, "ranking.txt");
        var rangePath = Path.Combine(directory, "positive_ev_range.txt");

        var content = ExploitThreeBetSpotResultFormatter.Format(result);
        File.WriteAllText(rankingPath, content);

        context.RangeWriter.WriteEquilab(rangePath, result.PositiveEvRange);
    }
}