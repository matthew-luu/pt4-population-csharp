using System.Globalization;
using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Infrastructure.Parsing;

public sealed class PopulationNodeExtractor
{
    private static readonly HashSet<string> MetadataColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "id_limit",
        "limit_name",
        "amt_sb",
        "amt_bb",
        "player_count",
        "total_hands"
    };

    public List<PopulationNode> Extract(Dictionary<string, string> row)
    {
        var nodes = new List<PopulationNode>();

        foreach (var kv in row)
        {
            var columnName = kv.Key.Trim();
            var rawValue = kv.Value.Trim();

            if (MetadataColumns.Contains(columnName))
                continue;

            if (columnName.EndsWith("_opp", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var frequency))
                continue;

            var nodeId = ParseNodeId(columnName);
            if (nodeId is null)
                continue;

            nodes.Add(new PopulationNode(
                NodeId: nodeId,
                Frequency: frequency
            ));
        }

        return nodes;
    }

    private static NodeId? ParseNodeId(string columnName)
    {
        var normalized = columnName.Trim().ToLowerInvariant();

        if (normalized.StartsWith("rfi_"))
        {
            var parts = normalized.Split('_', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                return new NodeId(
                    Action: "rfi",
                    Actor: parts[1],
                    Opponent: null
                );
            }

            return null;
        }

        var vsIndex = normalized.IndexOf("_vs_", StringComparison.Ordinal);
        if (vsIndex >= 0)
        {
            var left = normalized[..vsIndex];
            var right = normalized[(vsIndex + 4)..];

            var leftParts = left.Split('_', StringSplitOptions.RemoveEmptyEntries);

            if (leftParts.Length == 2)
            {
                var action = leftParts[0];
                var actor = leftParts[1];
                var opponent = right;

                return new NodeId(
                    Action: action,
                    Actor: actor,
                    Opponent: opponent
                );
            }

            return null;
        }

        return null;
    }
}