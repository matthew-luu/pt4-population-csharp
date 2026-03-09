namespace Poker.RangeApprox.App;

public sealed record AppOptions(
    string RankingFilePath,
    string CsvPath,
    string Mode,
    string? NodeKey,
    string? RequestedProfileName,
    double RakePercent,
    double RakeCapBb,
    double OpenSize,
    double ThreeBetSize,
    double FourBetSize,
    string? OutputRoot = null)
{
    public static AppOptions Parse(string[] args)
    {
        var rankingFilePath = args.Length > 0 ? args[0] : "prefloprankings.txt";
        var csvPath = args.Length > 1 ? args[1] : "population.csv";
        var mode = args.Length > 2 ? args[2].ToLowerInvariant() : "rfi";
        var nodeKey = args.Length > 3 ? args[3] : null;
        var requestedProfileName = args.Length > 4 ? args[4] : null;

        var rakePercent = args.Length > 5 && double.TryParse(args[5], out var parsedPercent)
            ? parsedPercent
            : 0.05;

        var rakeCapBb = args.Length > 6 && double.TryParse(args[6], out var parsedCap)
            ? parsedCap
            : 150.0;

        var openSize = args.Length > 7 && double.TryParse(args[7], out var parsedOpen)
            ? parsedOpen
            : 2.5;

        var threeBetSize = args.Length > 8 && double.TryParse(args[8], out var parsedThreeBet)
            ? parsedThreeBet
            : 8.5;

        var fourBetSize = args.Length > 9 && double.TryParse(args[9], out var parsedFourBet)
            ? parsedFourBet
            : 23.0;

        var outputRoot = args.Length > 10 ? args[10] : null;

        return new AppOptions(
            RankingFilePath: rankingFilePath,
            CsvPath: csvPath,
            Mode: mode,
            NodeKey: nodeKey,
            RequestedProfileName: requestedProfileName,
            RakePercent: rakePercent,
            RakeCapBb: rakeCapBb,
            OpenSize: openSize,
            ThreeBetSize: threeBetSize,
            FourBetSize: fourBetSize,
            OutputRoot: outputRoot);
    }
}