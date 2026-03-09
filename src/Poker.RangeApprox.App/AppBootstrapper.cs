using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Core.Equity;
using Poker.RangeApprox.Infrastructure.Parsing;
using Poker.RangeApprox.Infrastructure.Writing;

namespace Poker.RangeApprox.App;

public static class AppBootstrapper
{
    public static AppContext Build(AppOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var runTimestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var outputRoot = Path.Combine("output", runTimestamp);
        Directory.CreateDirectory(outputRoot);

        Console.WriteLine($"Run directory: {outputRoot}");
        Console.WriteLine();

        var rankingParser = new RankingFileParser();
        var profiles = rankingParser.Parse(options.RankingFilePath);

        if (profiles.Count == 0)
            throw new InvalidOperationException("No ranking profiles found.");

        var csvReader = new PopulationCsvReader();
        var rows = csvReader.Read(options.CsvPath);

        if (rows.Count == 0)
            throw new InvalidOperationException("No CSV rows found.");

        var extractor = new PopulationNodeExtractor();
        var opportunityExtractor = new PopulationOpportunityExtractor();

        var nodes = extractor.Extract(rows[0]);
        var opportunities = opportunityExtractor.Extract(rows[0]);

        if (nodes.Count == 0)
            throw new InvalidOperationException("No nodes extracted from CSV.");

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
            Percent: 0.05,
            CapBb: 150.0);

        var realizationProfile = new ExploitRealizationProfile(
            SingleRaisedIp: 0.90,
            SingleRaisedOop: 0.78,
            ThreeBetCallIp: 0.84,
            ThreeBetCallOop: 0.72,
            FourBetCallIp: 0.87,
            FourBetCallOop: 0.76);

        var exploitSizing = new ExploitSizingProfile(
            OpenSize: 2.5,
            ThreeBetSize: 8.5,
            FourBetSize: 23.0,
            FiveBetSize: 100.0,
            SmallBlind: 0.5,
            BigBlind: 1.0,
            Rake: rakeProfile,
            Realization: realizationProfile);

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
            ExploitSizing: exploitSizing);
    }
}