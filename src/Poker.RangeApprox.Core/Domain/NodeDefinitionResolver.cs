namespace Poker.RangeApprox.Core.Domain;

public sealed class NodeDefinitionResolver
{
    public NodeDefinition Resolve(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        var action = Normalize(nodeId.Action);
        var actor = Normalize(nodeId.Actor);
        var opponent = NormalizeNullable(nodeId.Opponent);

        // RFI nodes are always from the full 1326-hand universe.
        // Example: rfi_btn
        if (action == "rfi")
        {
            return new NodeDefinition(
                NodeId: new NodeId(action, actor, null),
                FrequencyBasis: FrequencyBasis.FullUniverse,
                ParentNodeId: null
            );
        }

        // Nodes vs open are still chosen from the full hand universe.
        // Examples:
        // call_co_vs_lj
        // 3bet_btn_vs_co
        // fold_sb_vs_btn
        if (!string.IsNullOrWhiteSpace(opponent) && !opponent.StartsWith("3bet_", StringComparison.OrdinalIgnoreCase))
        {
            return new NodeDefinition(
                NodeId: new NodeId(action, actor, opponent),
                FrequencyBasis: FrequencyBasis.FullUniverse,
                ParentNodeId: null
            );
        }

        // Nodes vs 3bet are percentages of the actor's prior opening range.
        // Examples:
        // call_btn_vs_3bet_sb
        // 4bet_lj_vs_3bet_hj
        // fold_co_vs_3bet_btn
        //
        // Parent is the actor's open range:
        // call_btn_vs_3bet_sb  -> parent = rfi_btn
        // 4bet_lj_vs_3bet_hj   -> parent = rfi_lj
        if (!string.IsNullOrWhiteSpace(opponent) && opponent.StartsWith("3bet_", StringComparison.OrdinalIgnoreCase))
        {
            var parentNodeId = new NodeId(
                Action: "rfi",
                Actor: actor,
                Opponent: null
            );

            return new NodeDefinition(
                NodeId: new NodeId(action, actor, opponent),
                FrequencyBasis: FrequencyBasis.ParentRange,
                ParentNodeId: parentNodeId
            );
        }

        throw new InvalidOperationException(
            $"Could not resolve node definition for node '{nodeId.ToKey()}'.");
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim().ToLowerInvariant();
    }
}