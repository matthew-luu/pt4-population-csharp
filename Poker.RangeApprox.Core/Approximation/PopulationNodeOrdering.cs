using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed class PopulationNodeOrdering
{
    private readonly NodeDefinitionResolver _nodeDefinitionResolver;

    public PopulationNodeOrdering(NodeDefinitionResolver nodeDefinitionResolver)
    {
        _nodeDefinitionResolver = nodeDefinitionResolver ?? throw new ArgumentNullException(nameof(nodeDefinitionResolver));
    }

    public IReadOnlyList<PopulationNode> Order(IEnumerable<PopulationNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        var remaining = nodes.ToDictionary(
            n => n.NodeId.ToKey(),
            n => n,
            StringComparer.OrdinalIgnoreCase);

        var ordered = new List<PopulationNode>();
        var resolved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (remaining.Count > 0)
        {
            var progressMade = false;

            foreach (var kv in remaining.ToList())
            {
                var node = kv.Value;
                var definition = _nodeDefinitionResolver.Resolve(node.NodeId);

                var parentKey = definition.ParentNodeId?.ToKey();

                if (parentKey is null || resolved.Contains(parentKey))
                {
                    ordered.Add(node);
                    resolved.Add(kv.Key);
                    remaining.Remove(kv.Key);
                    progressMade = true;
                }
            }

            if (!progressMade)
            {
                var unresolved = string.Join(", ", remaining.Keys.OrderBy(x => x));
                throw new InvalidOperationException(
                    $"Could not resolve node ordering due to missing parent dependencies. Remaining nodes: {unresolved}");
            }
        }

        return ordered;
    }
}