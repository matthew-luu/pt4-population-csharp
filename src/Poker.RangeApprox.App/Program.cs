using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;
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

var engine = new ApproximationEngine(
    resolver,
    requestBuilder,
    approximator,
    ordering,
    rankingProfileSelector);

var writer = new RangeFileWriter();

switch (mode)
{
    case "single":
        RunSingle(nodes, nodeKey);
        break;

    case "all":
        RunAll(nodes);
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
}

void Write(PopulationNode node, ApproximationResult result)
{
    var equilab = EquilabFormatter.FormatExplicit(result.Cells);
    var matrix = MatrixFormatter.Format(result.Cells);

    var dir = Path.Combine("output", node.NodeId.ToKey());

    writer.WriteEquilab(Path.Combine(dir, "range.txt"), result.Cells);
    writer.WriteMatrix(Path.Combine(dir, "range_matrix.txt"), result.Cells);

    Console.WriteLine($"Node: {node.NodeId.ToKey()}");
    Console.WriteLine($"Freq: {node.Frequency:0.##}%");
    Console.WriteLine($"Target: {result.TargetCombos:0.##} combos");
    Console.WriteLine($"Actual: {result.ActualCombos:0.##} combos");
    Console.WriteLine($"Output: {dir}");
    Console.WriteLine();
}