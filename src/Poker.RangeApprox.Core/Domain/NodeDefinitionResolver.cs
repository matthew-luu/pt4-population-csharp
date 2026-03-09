namespace Poker.RangeApprox.Core.Domain;

public sealed class NodeDefinitionResolver
{
    public NodeDefinition Resolve(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        var action = Normalize(nodeId.Action);
        var actor = Normalize(nodeId.Actor);
        var opponent = NormalizeNullable(nodeId.Opponent);

        // ----------------------------------------
        // RFI
        // ----------------------------------------

        if (action == "rfi")
        {
            return new NodeDefinition(
                NodeId: new NodeId(action, actor, null),
                FrequencyBasis: FrequencyBasis.FullUniverse,
                ParentNodeId: null
            );
        }

        if (string.IsNullOrWhiteSpace(opponent))
        {
            throw new InvalidOperationException(
                $"Node '{nodeId.ToKey()}' does not specify an opponent.");
        }

        // ----------------------------------------
        // VS FOURBET
        // call_sb_vs_fourbet_btn
        // fivebet_sb_vs_fourbet_btn
        // fold_sb_vs_fourbet_btn
        // ----------------------------------------

        if (opponent.StartsWith("fourbet_", StringComparison.OrdinalIgnoreCase))
        {
            var opener = opponent.Replace("fourbet_", "");

            var parentNode = new NodeId(
                Action: "threebet",
                Actor: actor,
                Opponent: opener
            );

            return new NodeDefinition(
                NodeId: new NodeId(action, actor, opponent),
                FrequencyBasis: FrequencyBasis.ParentRange,
                ParentNodeId: parentNode
            );
        }

        // ----------------------------------------
        // VS THREEBET
        // call_btn_vs_threebet_sb
        // fourbet_btn_vs_threebet_sb
        // fold_btn_vs_threebet_sb
        // ----------------------------------------

        if (opponent.StartsWith("threebet_", StringComparison.OrdinalIgnoreCase))
        {
            var opener = opponent.Replace("threebet_", "");

            var parentNode = new NodeId(
                Action: "rfi",
                Actor: actor,
                Opponent: null
            );

            return new NodeDefinition(
                NodeId: new NodeId(action, actor, opponent),
                FrequencyBasis: FrequencyBasis.ParentRange,
                ParentNodeId: parentNode
            );
        }

        // ----------------------------------------
        // VS OPEN
        // call_sb_vs_btn
        // threebet_sb_vs_btn
        // fold_sb_vs_btn
        // ----------------------------------------

        return new NodeDefinition(
            NodeId: new NodeId(action, actor, opponent),
            FrequencyBasis: FrequencyBasis.FullUniverse,
            ParentNodeId: null
        );
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim().ToLowerInvariant();
    }
}