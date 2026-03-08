using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Core.Equity;
using Poker.RangeApprox.Core.Formatting;
using Poker.RangeApprox.Infrastructure.Parsing;
using Poker.RangeApprox.Infrastructure.Writing;

var rankingFilePath = args.Length > 0 ? args[0] : "prefloprankings.txt";
var csvPath = args.Length > 1 ? args[1] : "population.csv";
var mode = args.Length > 2 ? args[2].ToLowerInvariant() : "rfi";
var nodeKey = args.Length > 3 ? args[3] : null;
var requestedProfileName = args.Length > 4 ? args[4] : null;

var rankingParser = new RankingFileParser();
var profiles = rankingParser.Parse(rankingFilePath);

if (profiles.Count == 0)
{
    Console.WriteLine("No ranking profiles found.");
    return;
}

var csvReader = new PopulationCsvReader();
var rows = csvReader.Read(csvPath);

if (rows.Count == 0)
{
    Console.WriteLine("No CSV rows found.");
    return;
}

var extractor = new PopulationNodeExtractor();
var nodes = extractor.Extract(rows[0]);

if (nodes.Count == 0)
{
    Console.WriteLine("No nodes extracted from CSV.");
    return;
}

var resolver = new NodeDefinitionResolver();
var requestBuilder = new ApproximationRequestBuilder(resolver);
var approximator = new TopDownRangeApproximator();
var ordering = new PopulationNodeOrdering(resolver);
var rankingProfileSelector = new RankingProfileSelector();
var partitionApproximator = new ActionPartitionApproximator(approximator);

var engine = new ApproximationEngine(
    resolver,
    requestBuilder,
    approximator,
    ordering,
    rankingProfileSelector,
    partitionApproximator);

var writer = new RangeFileWriter();
var callingSuperRangeBuilder = new CallingSuperRangeBuilder();
var weightedSuperRangeWriter = new WeightedSuperRangeFileWriter(writer);

var equityCalculator = new Poker.RangeApprox.Equity.OMPEval.OMPEvalEquityCalculator(iterations: 100000);
var handRankingService = new HandVsRangeRankingService(equityCalculator);

switch (mode)
{
    case "single":
        RunSingle(nodes, nodeKey);
        break;

    case "all":
        RunAll(nodes);
        break;

    case "rank-supercalls":
        RunRankSuperCalls(nodes);
        break;

    default:
        RunRfi(nodes);
        break;
}

void RunSingle(List<PopulationNode> populationNodes, string? requestedNodeKey)
{
    if (string.IsNullOrWhiteSpace(requestedNodeKey))
    {
        Console.WriteLine("single mode requires a node key.");
        return;
    }

    var lookup = new PopulationNodeLookup(populationNodes);
    var node = lookup.Get(requestedNodeKey);

    if (node is null)
    {
        Console.WriteLine($"Node not found: {requestedNodeKey}");
        return;
    }

    var results = engine.RunAll(populationNodes, profiles, requestedProfileName);

    if (!results.TryGetValue(node.NodeId.ToKey(), out var result))
    {
        Console.WriteLine($"No result generated for {node.NodeId.ToKey()}");
        return;
    }

    Write(node, result);
}

void RunRfi(List<PopulationNode> populationNodes)
{
    var rfiNodes = populationNodes
        .Where(n => n.NodeId.Action.Equals("rfi", StringComparison.OrdinalIgnoreCase))
        .OrderBy(n => n.NodeId.Actor)
        .ToList();

    var results = engine.RunAll(rfiNodes, profiles, requestedProfileName);

    Console.WriteLine($"Generating {rfiNodes.Count} RFI ranges");
    Console.WriteLine();

    foreach (var node in rfiNodes)
    {
        if (!results.TryGetValue(node.NodeId.ToKey(), out var result))
            continue;

        Write(node, result);
    }
}

void RunAll(List<PopulationNode> populationNodes)
{
    var results = engine.RunAll(populationNodes, profiles, requestedProfileName);

    Console.WriteLine($"Generating {results.Count} ranges");
    Console.WriteLine();

    foreach (var node in populationNodes)
    {
        if (!results.TryGetValue(node.NodeId.ToKey(), out var result))
            continue;

        Write(node, result);
    }

    var callingSuperRanges = callingSuperRangeBuilder.Build(populationNodes, results);
    weightedSuperRangeWriter.WriteAll("output", callingSuperRanges);

    Console.WriteLine($"Generating {callingSuperRanges.Count} weighted calling super-ranges");
    Console.WriteLine();

    foreach (var superRange in callingSuperRanges.Values.OrderBy(x => x.OpenPosition, StringComparer.OrdinalIgnoreCase))
    {
        Console.WriteLine($"Super-range: {superRange.Key}");
        Console.WriteLine($"Output: {Path.Combine("output", "calling super-ranges", superRange.Key)}");
        Console.WriteLine();
    }

    WriteSuperCallRankings(callingSuperRanges);
}

void RunRankSuperCalls(List<PopulationNode> populationNodes)
{
    var results = engine.RunAll(populationNodes, profiles, requestedProfileName);
    var callingSuperRanges = callingSuperRangeBuilder.Build(populationNodes, results);

    weightedSuperRangeWriter.WriteAll("output", callingSuperRanges);
    WriteSuperCallRankings(callingSuperRanges);

    Console.WriteLine($"Generated {callingSuperRanges.Count} calling super-ranges and rankings.");
    Console.WriteLine();
}

void WriteSuperCallRankings(IReadOnlyDictionary<string, WeightedSuperRangeResult> callingSuperRanges)
{
    foreach (var superRange in callingSuperRanges.Values.OrderBy(x => x.OpenPosition, StringComparer.OrdinalIgnoreCase))
    {
        var ranking = handRankingService.RankAllHands(superRange.Cells);
        WriteHandRanking(superRange.Key, ranking);

        Console.WriteLine($"Ranking: {superRange.Key}");
        Console.WriteLine($"Output: {Path.Combine("output", "rankings", "vs call super", $"{superRange.Key}.txt")}");
        Console.WriteLine();
    }
}

void WriteHandRanking(string key, IReadOnlyList<HandEquityResult> ranking)
{
    var directory = Path.Combine("output", "rankings", "vs call super");
    Directory.CreateDirectory(directory);

    var path = Path.Combine(directory, $"{key}.txt");
    var content = HandEquityResultFormatter.Format(ranking);

    File.WriteAllText(path, content);
}

void Write(PopulationNode node, ApproximationResult result)
{
    var (directory, fileName) = GetOutputPath(node.NodeId);

    writer.WriteEquilab(Path.Combine(directory, $"{fileName}.txt"), result.Cells);
    if (false) writer.WriteMatrix(Path.Combine(directory, $"{fileName}_matrix.txt"), result.Cells);

    Console.WriteLine($"Node: {node.NodeId.ToKey()}");
    Console.WriteLine($"Output: {Path.Combine(directory, fileName)}");
    Console.WriteLine();
}

(string Directory, string FileName) GetOutputPath(NodeId nodeId)
{
    var action = nodeId.Action.Trim().ToLowerInvariant();
    var actor = nodeId.Actor.Trim().ToLowerInvariant();
    var opponent = nodeId.Opponent?.Trim().ToLowerInvariant();

    if (action == "rfi")
    {
        return (
            Path.Combine("output", "rfi"),
            actor
        );
    }

    if (action is "call" or "fold" or "threebet")
    {
        if (!string.IsNullOrWhiteSpace(opponent) &&
            !opponent.StartsWith("3bet_", StringComparison.OrdinalIgnoreCase))
        {
            var actionName = NormalizeFacingOpenAction(action);

            return (
                Path.Combine("output", "facing open", $"vs {opponent}"),
                $"{actionName}_{actor}"
            );
        }
    }

    if (action == "fourbet")
    {
        return (
            Path.Combine("output", "facing 3bet", $"vs {opponent}"),
            $"raise_{actor}"
        );
    }

    if (action is "call" or "fold")
    {
        if (!string.IsNullOrWhiteSpace(opponent) &&
            opponent.StartsWith("3bet_", StringComparison.OrdinalIgnoreCase))
        {
            var opp = opponent.Replace("3bet_", "");

            return (
                Path.Combine("output", "facing 3bet", $"vs {opp}"),
                $"{action}_{actor}"
            );
        }
    }

    return (
        Path.Combine("output", "misc"),
        nodeId.ToKey()
    );
}

string NormalizeFacingOpenAction(string action)
{
    return action switch
    {
        "threebet" => "raise",
        "call" => "call",
        "fold" => "fold",
        _ => action
    };
}