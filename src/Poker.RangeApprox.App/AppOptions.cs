namespace Poker.RangeApprox.App;

public sealed record AppOptions(
    string RankingFilePath,
    string CsvPath,
    string Mode,
    string? NodeKey,
    string? RequestedProfileName)
{
    public static AppOptions Parse(string[] args)
    {
        return new AppOptions(
            RankingFilePath: args.Length > 0 ? args[0] : "prefloprankings.txt",
            CsvPath: args.Length > 1 ? args[1] : "population.csv",
            Mode: args.Length > 2 ? args[2].ToLowerInvariant() : "rfi",
            NodeKey: args.Length > 3 ? args[3] : null,
            RequestedProfileName: args.Length > 4 ? args[4] : null);
    }
}