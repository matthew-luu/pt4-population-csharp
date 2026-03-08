using Poker.RangeApprox.Core.Domain;
using System.Text.RegularExpressions;

namespace Poker.RangeApprox.Infrastructure.Parsing;

public sealed class RankingFileParser
{
    private static readonly Regex HandTokenRegex = new(@"^[AKQJT98765432]{2}[pso]$", RegexOptions.Compiled);

    public IReadOnlyList<RankingProfile> Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Ranking file not found.", filePath);

        var lines = File.ReadAllLines(filePath)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var profiles = new List<RankingProfile>();
        string? pendingProfileName = null;
        int unnamedIndex = 1;

        foreach (var line in lines)
        {
            if (line.StartsWith("@") && line.EndsWith("@"))
            {
                pendingProfileName = NormalizeProfileName(line);
                continue;
            }

            var tokens = SplitTokens(line);
            if (tokens.Count == 0)
                continue;

            if (!tokens.All(t => HandTokenRegex.IsMatch(t)))
                continue;

            var hands = tokens
                .Select(HandClass.ParseRankingToken)
                .ToList();

            string profileName = pendingProfileName ?? $"Profile {unnamedIndex++}";
            profiles.Add(new RankingProfile(profileName, hands));
            pendingProfileName = null;
        }

        return profiles;
    }

    private static List<string> SplitTokens(string line)
    {
        return line
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();
    }

    private static string NormalizeProfileName(string raw)
    {
        return raw.Trim('@').Replace("^", " ");
    }
}