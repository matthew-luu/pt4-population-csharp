using Poker.RangeApprox.App.Execution;
using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Core.Equity;
using Poker.RangeApprox.Infrastructure.Parsing;
using Poker.RangeApprox.Infrastructure.Writing;

namespace Poker.RangeApprox.App;

public static class AppBootstrapper
{
    private const short DefaultIdLimit = 2;

    public static AppContext Build(
        AppOptions options,
        IAppStatusWriter? status = null,
        string? connectionStringOverride = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        status ??= new ConsoleStatusWriter();

        var runTimestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var outputRoot = string.IsNullOrWhiteSpace(options.OutputRoot)
            ? Path.Combine("output", runTimestamp)
            : Path.Combine(options.OutputRoot, runTimestamp);

        Directory.CreateDirectory(outputRoot);

        status.WriteLine($"Run directory: {outputRoot}");
        status.WriteLine($"Rake: {options.RakePercent:P2}, cap {options.RakeCapBb}bb");
        status.WriteLine($"Sizing: open {options.OpenSize}bb, 3bet {options.ThreeBetSize}bb, 4bet {options.FourBetSize}bb");
        status.WriteLine();

        var rankingParser = new RankingFileParser();
        var profiles = rankingParser.Parse(options.RankingFilePath);

        if (profiles.Count == 0)
            throw new InvalidOperationException("No ranking profiles found.");

        var row = ReadPopulationRow(options, status, connectionStringOverride);

        var extractor = new PopulationNodeExtractor();
        var opportunityExtractor = new PopulationOpportunityExtractor();

        var nodes = extractor.Extract(row);
        var opportunities = opportunityExtractor.Extract(row);

        if (nodes.Count == 0)
            throw new InvalidOperationException("No population nodes extracted from selected source.");

        var resolver = new NodeDefinitionResolver();
        var requestBuilder = new ApproximationRequestBuilder(resolver);
        var approximator = new TopDownRangeApproximator();
        var ordering = new PopulationNodeOrdering(resolver);
        var rankingProfileSelector = new RankingProfileSelector();
        var partitionApproximator = new ActionPartitionApproximator(approximator);

        var approximationEngine = new ApproximationEngine(
            resolver,
            requestBuilder,
            approximator,
            ordering,
            rankingProfileSelector,
            partitionApproximator);

        var rangeWriter = new RangeFileWriter();
        var callingSuperRangeBuilder = new CallingSuperRangeBuilder();
        var weightedSuperRangeWriter = new WeightedSuperRangeFileWriter(rangeWriter);

        var equityCalculator = new Poker.RangeApprox.Equity.OMPEval.OMPEvalEquityCalculator(iterations: 100000);
        var handRankingService = new HandVsRangeRankingService(equityCalculator);

        var exploitEngine = new ExploitEngine(equityCalculator);

        var rakeProfile = new ExploitRakeProfile(
            Percent: options.RakePercent,
            CapBb: options.RakeCapBb);

        var realizationProfile = new ExploitRealizationProfile(
            SingleRaisedIp: 0.90,
            SingleRaisedOop: 0.78,
            ThreeBetCallIp: 0.84,
            ThreeBetCallOop: 0.72,
            FourBetCallIp: 0.87,
            FourBetCallOop: 0.76);

        var exploitSizing = new ExploitSizingProfile(
            OpenSize: options.OpenSize,
            ThreeBetSize: options.ThreeBetSize,
            FourBetSize: options.FourBetSize,
            FiveBetSize: 100.0,
            SmallBlind: 0.5,
            BigBlind: 1.0,
            Rake: rakeProfile,
            Realization: realizationProfile);

        var rangeVisualizationBuilder = new RangeVisualizationBuilder();
        var rangeVisualizationFileWriter = new RangeVisualizationFileWriter();

        return new AppContext(
            Options: options,
            OutputRoot: outputRoot,
            Profiles: profiles,
            Nodes: nodes,
            Opportunities: opportunities,
            ApproximationEngine: approximationEngine,
            RangeWriter: rangeWriter,
            CallingSuperRangeBuilder: callingSuperRangeBuilder,
            WeightedSuperRangeWriter: weightedSuperRangeWriter,
            HandRankingService: handRankingService,
            ExploitEngine: exploitEngine,
            ExploitSizing: exploitSizing,
            RangeVisualizationBuilder: rangeVisualizationBuilder,
            RangeVisualizationFileWriter: rangeVisualizationFileWriter,
            Status: status);
    }

    private static Dictionary<string, string> ReadPopulationRow(
        AppOptions options,
        IAppStatusWriter status,
        string? connectionStringOverride)
    {
        if (!string.IsNullOrWhiteSpace(options.CsvPath) && File.Exists(options.CsvPath))
        {
            status.WriteLine($"Population source: CSV ({options.CsvPath})");
            status.WriteLine();

            IPopulationRowSource csvSource = new CsvPopulationRowSource(options.CsvPath);
            return csvSource.ReadSingleRow();
        }

        if (string.IsNullOrWhiteSpace(connectionStringOverride))
            throw new InvalidOperationException("Database mode requires a connection string.");

        status.WriteLine("Population source: PostgreSQL");
        status.WriteLine($"id_limit: {DefaultIdLimit}");
        status.WriteLine("Refreshing materialized views...");
        status.WriteLine();

        IPopulationRowSource postgresSource = new PostgresPopulationRowSource(
            connectionString: connectionStringOverride,
            idLimit: DefaultIdLimit);

        return postgresSource.ReadSingleRow();
    }
}