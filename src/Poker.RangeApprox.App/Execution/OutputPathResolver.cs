using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.App.Execution;

public static class OutputPathResolver
{
    public static (string Directory, string FileName) GetOutputPath(string outputRoot, NodeId nodeId)
    {
        var action = nodeId.Action.Trim().ToLowerInvariant();
        var actor = nodeId.Actor.Trim().ToLowerInvariant();
        var opponent = nodeId.Opponent?.Trim().ToLowerInvariant();

        if (action == "rfi")
        {
            return (
                Path.Combine(outputRoot, "rfi"),
                actor
            );
        }

        if (action is "call" or "fold" or "threebet")
        {
            if (!string.IsNullOrWhiteSpace(opponent) &&
                !opponent.StartsWith("threebet_", StringComparison.OrdinalIgnoreCase) &&
                !opponent.StartsWith("fourbet_", StringComparison.OrdinalIgnoreCase))
            {
                return (
                    Path.Combine(outputRoot, "facing open", $"vs {opponent}"),
                    $"{NormalizeFacingOpenAction(action)}_{actor}"
                );
            }
        }

        if (action is "call" or "fold" or "fourbet")
        {
            if (!string.IsNullOrWhiteSpace(opponent) &&
                opponent.StartsWith("threebet_", StringComparison.OrdinalIgnoreCase))
            {
                var opp = opponent.Replace("threebet_", "");

                return (
                    Path.Combine(outputRoot, "facing 3bet", $"vs {opp}"),
                    $"{NormalizeFacingThreeBetAction(action)}_{actor}"
                );
            }
        }

        if (action is "call" or "fold" or "fivebet")
        {
            if (!string.IsNullOrWhiteSpace(opponent) &&
                opponent.StartsWith("fourbet_", StringComparison.OrdinalIgnoreCase))
            {
                var opp = opponent.Replace("fourbet_", "");

                return (
                    Path.Combine(outputRoot, "facing 4bet", $"vs {opp}"),
                    $"{NormalizeFacingFourBetAction(action)}_{actor}"
                );
            }
        }

        return (
            Path.Combine(outputRoot, "misc"),
            nodeId.ToKey()
        );
    }

    private static string NormalizeFacingOpenAction(string action)
    {
        return action switch
        {
            "threebet" => "raise",
            "call" => "call",
            "fold" => "fold",
            _ => action
        };
    }

    private static string NormalizeFacingThreeBetAction(string action)
    {
        return action switch
        {
            "fourbet" => "raise",
            "call" => "call",
            "fold" => "fold",
            _ => action
        };
    }

    private static string NormalizeFacingFourBetAction(string action)
    {
        return action switch
        {
            "fivebet" => "raise",
            "call" => "call",
            "fold" => "fold",
            _ => action
        };
    }
}