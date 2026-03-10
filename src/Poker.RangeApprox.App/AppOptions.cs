using System.Globalization;

namespace Poker.RangeApprox.App;

public sealed record AppOptions(
    string RankingFilePath,
    string? CsvPath,
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
        var rankingFilePath = args.Length > 0
            ? args[0]
            : "prefloprankings.txt";

        var csvPath = args.Length > 1
            ? NormalizeNullable(args[1])
            : "population.csv";

        var mode = args.Length > 2
            ? args[2].ToLowerInvariant()
            : "rfi";

        var nodeKey = args.Length > 3
            ? NormalizeNullable(args[3])
            : null;

        var requestedProfileName = args.Length > 4
            ? NormalizeNullable(args[4])
            : null;

        var rakePercent = args.Length > 5 &&
                          double.TryParse(args[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedPercent)
            ? parsedPercent
            : 0.05;

        var rakeCapBb = args.Length > 6 &&
                        double.TryParse(args[6], NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedCap)
            ? parsedCap
            : 150.0;

        var openSize = args.Length > 7 &&
                       double.TryParse(args[7], NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedOpen)
            ? parsedOpen
            : 2.5;

        var threeBetSize = args.Length > 8 &&
                           double.TryParse(args[8], NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedThreeBet)
            ? parsedThreeBet
            : 8.5;

        var fourBetSize = args.Length > 9 &&
                          double.TryParse(args[9], NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedFourBet)
            ? parsedFourBet
            : 23.0;

        var outputRoot = args.Length > 10
            ? NormalizeNullable(args[10])
            : null;

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

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        if (trimmed.Equals("null", StringComparison.OrdinalIgnoreCase))
            return null;

        if (trimmed.Equals("-", StringComparison.OrdinalIgnoreCase))
            return null;

        return trimmed;
    }
}