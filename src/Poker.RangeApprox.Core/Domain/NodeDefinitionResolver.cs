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
        if (action == "rfi")
        {
            return new NodeDefinition(
                NodeId: new NodeId(action, actor, null),
                FrequencyBasis: FrequencyBasis.FullUniverse,
                ParentNodeId: null
            );
        }

        // Fourbet nodes are part of the defense-vs-3bet family and should be
        // measured against the actor's original open range.
        //
        // Example:
        // fourbet_lj_vs_hj  -> parent = rfi_lj
        if (action == "fourbet")
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

        // Nodes vs 3bet are percentages of the actor's prior opening range.
        //
        // Examples:
        // call_btn_vs_3bet_sb
        // fold_co_vs_3bet_btn
        if (!string.IsNullOrWhiteSpace(opponent) &&
            opponent.StartsWith("3bet_", StringComparison.OrdinalIgnoreCase))
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

        // Nodes vs open are chosen from the full hand universe.
        //
        // Examples:
        // call_co_vs_lj
        // threebet_btn_vs_co
        // fold_sb_vs_btn
        if (!string.IsNullOrWhiteSpace(opponent))
        {
            return new NodeDefinition(
                NodeId: new NodeId(action, actor, opponent),
                FrequencyBasis: FrequencyBasis.FullUniverse,
                ParentNodeId: null
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