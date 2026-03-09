using System.Globalization;
using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Infrastructure.Parsing;

public sealed class PopulationOpportunityExtractor
{
    public PopulationOpportunityProfile Extract(Dictionary<string, string> row)
    {
        ArgumentNullException.ThrowIfNull(row);

        var rfi = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var vsOpen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var vsThreeBet = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var vsFourBet = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in row)
        {
            var columnName = kv.Key.Trim().ToLowerInvariant();
            var rawValue = kv.Value.Trim();

            if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count))
                continue;

            if (TryParseSimplePositionColumn(columnName, "rfi_", "_opp", out var rfiPosition))
            {
                rfi[rfiPosition] = count;
                continue;
            }

            if (TryParseSimplePositionColumn(columnName, "vs_open_", "_opp", out var vsOpenPosition))
            {
                vsOpen[vsOpenPosition] = count;
                continue;
            }

            if (TryParseSimplePositionColumn(columnName, "vs_threebet_", "_opp", out var vsThreeBetPosition))
            {
                vsThreeBet[vsThreeBetPosition] = count;
                continue;
            }

            if (TryParseSimplePositionColumn(columnName, "vs_fourbet_", "_opp", out var vsFourBetPosition))
            {
                vsFourBet[vsFourBetPosition] = count;
            }
        }

        return new PopulationOpportunityProfile(
            rfiOpportunities: rfi,
            vsOpenOpportunities: vsOpen,
            vsThreeBetOpportunities: vsThreeBet,
            vsFourBetOpportunities: vsFourBet);
    }

    private static bool TryParseSimplePositionColumn(
        string columnName,
        string prefix,
        string suffix,
        out string position)
    {
        position = string.Empty;

        if (!columnName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !columnName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var inner = columnName[prefix.Length..^suffix.Length].Trim();
        if (string.IsNullOrWhiteSpace(inner))
            return false;

        position = inner.ToLowerInvariant();
        return true;
    }
}