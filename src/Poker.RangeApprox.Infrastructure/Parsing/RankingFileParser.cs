using Poker.RangeApprox.Core.Domain;
using System.Text.RegularExpressions;

namespace Poker.RangeApprox.Infrastructure.Parsing;

public sealed class RankingFileParser
{
    private static readonly Regex HandTokenRegex =
        new(@"^[AKQJT98765432]{2}[pso]$", RegexOptions.Compiled);

    public IReadOnlyList<RankingProfile> Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Ranking file not found.", filePath);

        var lines = File.ReadAllLines(filePath)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var profileNames = new List<string>();
        var rankingLines = new List<List<HandClass>>();
        int unnamedIndex = 1;

        foreach (var line in lines)
        {
            if (line.StartsWith("@") && line.EndsWith("@"))
            {
                profileNames.Add(NormalizeProfileName(line));
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

            rankingLines.Add(hands);
        }

        var profiles = new List<RankingProfile>();

        for (int i = 0; i < rankingLines.Count; i++)
        {
            var name = i < profileNames.Count
                ? profileNames[i]
                : $"Profile {unnamedIndex++}";

            profiles.Add(new RankingProfile(name, rankingLines[i]));
        }

        if (profileNames.Count > 0 && rankingLines.Count == 0)
        {
            throw new InvalidOperationException(
                "Ranking file contained profile headers but no ranking lines.");
        }

        if (profileNames.Count > rankingLines.Count)
        {
            throw new InvalidOperationException(
                $"Ranking file contained {profileNames.Count} profile headers but only {rankingLines.Count} ranking lines.");
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