namespace Poker.RangeApprox.Core.Domain;

public static class NodeSpotHelper
{
    public static bool IsPartitionAction(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        var action = Normalize(nodeId.Action);

        return action is "call" or "fold" or "threebet" or "fourbet";
    }

    public static bool IsAggressiveAction(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        var action = Normalize(nodeId.Action);
        return action is "threebet" or "fourbet";
    }

    public static bool IsCallAction(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);
        return Normalize(nodeId.Action) == "call";
    }

    public static bool IsFoldAction(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);
        return Normalize(nodeId.Action) == "fold";
    }

    public static bool IsVsThreeBetSpot(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        var action = Normalize(nodeId.Action);
        var opponent = NormalizeNullable(nodeId.Opponent);

        if (action == "fourbet")
            return true;

        return !string.IsNullOrWhiteSpace(opponent)
            && opponent.StartsWith("3bet_", StringComparison.OrdinalIgnoreCase);
    }

    public static string ToSpotKey(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        var actor = Normalize(nodeId.Actor);
        var opponent = NormalizeNullable(nodeId.Opponent);

        if (string.IsNullOrWhiteSpace(actor))
            throw new InvalidOperationException(
                $"Node '{nodeId.ToKey()}' does not have an actor and cannot be grouped into a partition spot.");

        if (string.IsNullOrWhiteSpace(opponent))
            throw new InvalidOperationException(
                $"Node '{nodeId.ToKey()}' does not have an opponent and cannot be grouped into a partition spot.");

        // Normalize fourbet_lj_vs_hj to the same family as:
        // call_lj_vs_3bet_hj
        // fold_lj_vs_3bet_hj
        if (Normalize(nodeId.Action) == "fourbet" && !opponent.StartsWith("3bet_", StringComparison.OrdinalIgnoreCase))
        {
            opponent = $"3bet_{opponent}";
        }

        return $"{actor}_vs_{opponent}";
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim().ToLowerInvariant();
    }
}