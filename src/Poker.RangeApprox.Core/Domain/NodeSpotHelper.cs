namespace Poker.RangeApprox.Core.Domain;

public static class NodeSpotHelper
{
    public static bool IsPartitionAction(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        var action = Normalize(nodeId.Action);

        return action is
            "call" or
            "fold" or
            "threebet" or
            "fourbet" or
            "fivebet";
    }

    public static bool IsAggressiveAction(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        var action = Normalize(nodeId.Action);

        return action is
            "threebet" or
            "fourbet" or
            "fivebet";
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

        var opponent = NormalizeNullable(nodeId.Opponent);

        return !string.IsNullOrWhiteSpace(opponent)
            && opponent.StartsWith("threebet_", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsVsFourBetSpot(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        var opponent = NormalizeNullable(nodeId.Opponent);

        return !string.IsNullOrWhiteSpace(opponent)
            && opponent.StartsWith("fourbet_", StringComparison.OrdinalIgnoreCase);
    }

    public static string ToSpotKey(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        var actor = Normalize(nodeId.Actor);
        var opponent = NormalizeNullable(nodeId.Opponent);

        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new InvalidOperationException(
                $"Node '{nodeId.ToKey()}' does not have an actor and cannot be grouped into a partition spot.");
        }

        if (string.IsNullOrWhiteSpace(opponent))
        {
            throw new InvalidOperationException(
                $"Node '{nodeId.ToKey()}' does not have an opponent and cannot be grouped into a partition spot.");
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