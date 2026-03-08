namespace Poker.RangeApprox.Core.Domain;

public static class NodeIdExtensions
{
    public static string ToKey(this NodeId nodeId)
    {
        return nodeId.Opponent is null
            ? $"{nodeId.Action}_{nodeId.Actor}"
            : $"{nodeId.Action}_{nodeId.Actor}_vs_{nodeId.Opponent}";
    }
}