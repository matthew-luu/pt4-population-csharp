using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed class ApproximationEngine
{
    private readonly NodeDefinitionResolver _nodeDefinitionResolver;
    private readonly ApproximationRequestBuilder _requestBuilder;
    private readonly TopDownRangeApproximator _approximator;
    private readonly PopulationNodeOrdering _ordering;

    public ApproximationEngine(
        NodeDefinitionResolver nodeDefinitionResolver,
        ApproximationRequestBuilder requestBuilder,
        TopDownRangeApproximator approximator,
        PopulationNodeOrdering ordering)
    {
        _nodeDefinitionResolver = nodeDefinitionResolver ?? throw new ArgumentNullException(nameof(nodeDefinitionResolver));
        _requestBuilder = requestBuilder ?? throw new ArgumentNullException(nameof(requestBuilder));
        _approximator = approximator ?? throw new ArgumentNullException(nameof(approximator));
        _ordering = ordering ?? throw new ArgumentNullException(nameof(ordering));
    }

    public IReadOnlyDictionary<string, ApproximationResult> RunAll(
        IEnumerable<PopulationNode> nodes,
        RankingProfile defaultProfile)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(defaultProfile);

        var orderedNodes = _ordering.Order(nodes);
        var results = new Dictionary<string, ApproximationResult>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in orderedNodes)
        {
            var request = _requestBuilder.Build(
                populationNode: node,
                existingResults: results,
                rankingProfileName: defaultProfile.Name);

            var definition = _nodeDefinitionResolver.Resolve(node.NodeId);
            var candidateUniverse = GetCandidateUniverse(definition, results);

            var result = _approximator.ApproximateToComboTarget(
                targetCombos: request.TargetCombos,
                profile: defaultProfile,
                candidateUniverse: candidateUniverse);

            results[node.NodeId.ToKey()] = result;
        }

        return results;
    }

    public ApproximationResult RunOne(
        PopulationNode node,
        RankingProfile defaultProfile,
        IReadOnlyDictionary<string, ApproximationResult> existingResults)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(defaultProfile);
        ArgumentNullException.ThrowIfNull(existingResults);

        var request = _requestBuilder.Build(
            populationNode: node,
            existingResults: existingResults,
            rankingProfileName: defaultProfile.Name);

        var definition = _nodeDefinitionResolver.Resolve(node.NodeId);
        var candidateUniverse = GetCandidateUniverse(definition, existingResults);

        return _approximator.ApproximateToComboTarget(
            targetCombos: request.TargetCombos,
            profile: defaultProfile,
            candidateUniverse: candidateUniverse);
    }

    private static IReadOnlyList<RangeCell>? GetCandidateUniverse(
        NodeDefinition definition,
        IReadOnlyDictionary<string, ApproximationResult> results)
    {
        if (definition.FrequencyBasis == FrequencyBasis.FullUniverse)
            return null;

        if (definition.ParentNodeId is null)
        {
            throw new InvalidOperationException(
                $"Node '{definition.NodeId.ToKey()}' uses ParentRange basis but ParentNodeId is null.");
        }

        var parentKey = definition.ParentNodeId.ToKey();

        if (!results.TryGetValue(parentKey, out var parentResult))
        {
            throw new InvalidOperationException(
                $"Parent result '{parentKey}' not found for node '{definition.NodeId.ToKey()}'.");
        }

        return parentResult.Cells;
    }
}