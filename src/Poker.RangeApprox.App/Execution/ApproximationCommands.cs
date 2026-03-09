using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Infrastructure.Parsing;

namespace Poker.RangeApprox.App.Execution;

public static class ApproximationCommands
{
    public static void RunSingle(AppContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Options.NodeKey))
        {
            Console.WriteLine("single mode requires a node key.");
            return;
        }

        var lookup = new PopulationNodeLookup(context.Nodes);
        var node = lookup.Get(context.Options.NodeKey);

        if (node is null)
        {
            Console.WriteLine($"Node not found: {context.Options.NodeKey}");
            return;
        }

        var results = context.ApproximationEngine.RunAll(
            context.Nodes,
            context.Profiles,
            context.Options.RequestedProfileName);

        if (!results.TryGetValue(node.NodeId.ToKey(), out var result))
        {
            Console.WriteLine($"No result generated for {node.NodeId.ToKey()}");
            return;
        }

        OutputWriters.WriteRange(context, node, result);
    }

    public static void RunRfi(AppContext context)
    {
        var rfiNodes = context.Nodes
            .Where(n => n.NodeId.Action.Equals("rfi", StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n.NodeId.Actor)
            .ToList();

        var results = context.ApproximationEngine.RunAll(
            rfiNodes,
            context.Profiles,
            context.Options.RequestedProfileName);

        Console.WriteLine($"Generating {rfiNodes.Count} RFI ranges");
        Console.WriteLine();

        foreach (var node in rfiNodes)
        {
            if (!results.TryGetValue(node.NodeId.ToKey(), out var result))
                continue;

            OutputWriters.WriteRange(context, node, result);
        }
    }

    public static void RunAll(AppContext context)
    {
        var results = context.ApproximationEngine.RunAll(
            context.Nodes,
            context.Profiles,
            context.Options.RequestedProfileName);

        Console.WriteLine($"Generating {results.Count} ranges");
        Console.WriteLine();

        foreach (var node in context.Nodes)
        {
            if (!results.TryGetValue(node.NodeId.ToKey(), out var result))
                continue;

            OutputWriters.WriteRange(context, node, result);
        }

        var callingSuperRanges = context.CallingSuperRangeBuilder.Build(context.Nodes, results);
        context.WeightedSuperRangeWriter.WriteAll(context.OutputRoot, callingSuperRanges);

        Console.WriteLine($"Generating {callingSuperRanges.Count} weighted calling super-ranges");
        Console.WriteLine();

        foreach (var superRange in callingSuperRanges.Values.OrderBy(x => x.OpenPosition, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Super-range: {superRange.Key}");
            Console.WriteLine($"Output: {Path.Combine(context.OutputRoot, "calling super-ranges", superRange.Key)}");
            Console.WriteLine();
        }

        RankingCommands.WriteSuperCallRankings(context, callingSuperRanges);
    }

    public static void RunRankSuperCalls(AppContext context)
    {
        var results = context.ApproximationEngine.RunAll(
            context.Nodes,
            context.Profiles,
            context.Options.RequestedProfileName);

        var callingSuperRanges = context.CallingSuperRangeBuilder.Build(context.Nodes, results);

        context.WeightedSuperRangeWriter.WriteAll(context.OutputRoot, callingSuperRanges);
        RankingCommands.WriteSuperCallRankings(context, callingSuperRanges);

        Console.WriteLine($"Generated {callingSuperRanges.Count} calling super-ranges and rankings.");
        Console.WriteLine();
    }
}