using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Infrastructure.Parsing;

public sealed class PopulationNodeLookup
{
    private readonly Dictionary<string, PopulationNode> _nodes;

    public PopulationNodeLookup(IEnumerable<PopulationNode> nodes)
    {
        _nodes = nodes.ToDictionary(
            n => n.NodeId.ToKey(),
            n => n,
            StringComparer.OrdinalIgnoreCase);
    }

    public PopulationNode? Get(string key)
    {
        return _nodes.TryGetValue(key, out var node) ? node : null;
    }

    public IReadOnlyCollection<PopulationNode> GetAll() => _nodes.Values.ToList();
}