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

var profile = requestedProfileName is null
    ? profiles.First()
    : profiles.FirstOrDefault(p => p.Name.Equals(requestedProfileName, StringComparison.OrdinalIgnoreCase));

if (profile is null)
{
    Console.WriteLine($"Ranking profile not found: {requestedProfileName}");
    Console.WriteLine("Available profiles:");
    foreach (var p in profiles)
        Console.WriteLine($"  {p.Name}");
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

var engine = new ApproximationEngine(
    resolver,
    requestBuilder,
    approximator,
    ordering);

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

void RunSingle(List<PopulationNode> nodes, string? nodeKey)
{
    if (string.IsNullOrWhiteSpace(nodeKey))
    {
        Console.WriteLine("single mode requires a node key.");
        return;
    }

    var lookup = new PopulationNodeLookup(nodes);
    var node = lookup.Get(nodeKey);

    if (node is null)
    {
        Console.WriteLine($"Node not found: {nodeKey}");
        return;
    }

    var results = engine.RunAll(nodes, profile);

    if (!results.TryGetValue(node.NodeId.ToKey(), out var result))
    {
        Console.WriteLine($"No result generated for {node.NodeId.ToKey()}");
        return;
    }

    Write(node, result);
}

void RunRfi(List<PopulationNode> nodes)
{
    var rfiNodes = nodes
        .Where(n => n.NodeId.Action == "rfi")
        .OrderBy(n => n.NodeId.Actor)
        .ToList();

    var results = engine.RunAll(rfiNodes, profile);

    Console.WriteLine($"Generating {rfiNodes.Count} RFI ranges\n");

    foreach (var node in rfiNodes)
    {
        if (!results.TryGetValue(node.NodeId.ToKey(), out var result))
            continue;

        Write(node, result);
    }
}

void RunAll(List<PopulationNode> nodes)
{
    var results = engine.RunAll(nodes, profile);

    Console.WriteLine($"Generating {results.Count} ranges\n");

    foreach (var node in nodes)
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