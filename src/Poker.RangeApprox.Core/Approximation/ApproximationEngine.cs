using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed class ApproximationEngine
{
    private readonly NodeDefinitionResolver _nodeDefinitionResolver;
    private readonly ApproximationRequestBuilder _requestBuilder;
    private readonly TopDownRangeApproximator _approximator;
    private readonly PopulationNodeOrdering _ordering;
    private readonly RankingProfileSelector _rankingProfileSelector;
    private readonly ActionPartitionApproximator _actionPartitionApproximator;

    public ApproximationEngine(
        NodeDefinitionResolver nodeDefinitionResolver,
        ApproximationRequestBuilder requestBuilder,
        TopDownRangeApproximator approximator,
        PopulationNodeOrdering ordering,
        RankingProfileSelector rankingProfileSelector,
        ActionPartitionApproximator actionPartitionApproximator)
    {
        _nodeDefinitionResolver = nodeDefinitionResolver ?? throw new ArgumentNullException(nameof(nodeDefinitionResolver));
        _requestBuilder = requestBuilder ?? throw new ArgumentNullException(nameof(requestBuilder));
        _approximator = approximator ?? throw new ArgumentNullException(nameof(approximator));
        _ordering = ordering ?? throw new ArgumentNullException(nameof(ordering));
        _rankingProfileSelector = rankingProfileSelector ?? throw new ArgumentNullException(nameof(rankingProfileSelector));
        _actionPartitionApproximator = actionPartitionApproximator ?? throw new ArgumentNullException(nameof(actionPartitionApproximator));
    }

    public IReadOnlyDictionary<string, ApproximationResult> RunAll(
        IEnumerable<PopulationNode> nodes,
        IReadOnlyList<RankingProfile> availableProfiles,
        string? explicitProfileName = null)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(availableProfiles);

        var allNodes = nodes.ToList();
        var results = new Dictionary<string, ApproximationResult>(StringComparer.OrdinalIgnoreCase);

        var singleNodes = allNodes
            .Where(n => !NodeSpotHelper.IsPartitionAction(n.NodeId))
            .ToList();

        var orderedSingleNodes = _ordering.Order(singleNodes);

        foreach (var node in orderedSingleNodes)
        {
            var strategy = _rankingProfileSelector.Select(
                node.NodeId,
                availableProfiles,
                explicitProfileName);

            var request = _requestBuilder.Build(
                populationNode: node,
                existingResults: results,
                rankingProfileName: strategy.Profile.Name);

            var definition = _nodeDefinitionResolver.Resolve(node.NodeId);
            var candidateUniverse = GetCandidateUniverse(definition, results);

            var result = _approximator.ApproximateToComboTarget(
                targetCombos: request.TargetCombos,
                profile: strategy.Profile,
                candidateUniverse: candidateUniverse,
                direction: strategy.Direction);

            results[node.NodeId.ToKey()] = result;
        }

        var pendingPartitions = allNodes
            .Where(n => NodeSpotHelper.IsPartitionAction(n.NodeId))
            .GroupBy(n => NodeSpotHelper.ToSpotKey(n.NodeId), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        while (pendingPartitions.Count > 0)
        {
            var progressMade = false;

            foreach (var partition in pendingPartitions.ToList())
            {
                var spotKey = partition.Key;
                var spotNodes = partition.Value;

                if (!CanProcessPartition(spotNodes, results))
                    continue;

                var request = BuildPartitionRequest(
                    spotKey,
                    spotNodes,
                    results,
                    availableProfiles,
                    explicitProfileName);

                var partitionResult = _actionPartitionApproximator.Approximate(request);

                foreach (var kv in partitionResult.Results)
                {
                    results[kv.Key] = kv.Value;
                }

                pendingPartitions.Remove(spotKey);
                progressMade = true;
            }

            if (!progressMade)
            {
                var unresolved = string.Join(", ", pendingPartitions.Keys.OrderBy(x => x));
                throw new InvalidOperationException(
                    $"Could not resolve action partitions due to missing parent dependencies. Remaining spots: {unresolved}");
            }
        }

        return results;
    }

    public ApproximationResult RunOne(
        PopulationNode node,
        IReadOnlyList<RankingProfile> availableProfiles,
        IReadOnlyDictionary<string, ApproximationResult> existingResults,
        string? explicitProfileName = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(availableProfiles);
        ArgumentNullException.ThrowIfNull(existingResults);

        if (NodeSpotHelper.IsPartitionAction(node.NodeId))
        {
            throw new InvalidOperationException(
                $"Node '{node.NodeId.ToKey()}' belongs to a partitioned action family and should be run through RunAll().");
        }

        var strategy = _rankingProfileSelector.Select(
            node.NodeId,
            availableProfiles,
            explicitProfileName);

        var request = _requestBuilder.Build(
            populationNode: node,
            existingResults: existingResults,
            rankingProfileName: strategy.Profile.Name);

        var definition = _nodeDefinitionResolver.Resolve(node.NodeId);
        var candidateUniverse = GetCandidateUniverse(definition, existingResults);

        return _approximator.ApproximateToComboTarget(
            targetCombos: request.TargetCombos,
            profile: strategy.Profile,
            candidateUniverse: candidateUniverse,
            direction: strategy.Direction);
    }

    private bool CanProcessPartition(
        IReadOnlyList<PopulationNode> spotNodes,
        IReadOnlyDictionary<string, ApproximationResult> results)
    {
        var representativeNode = spotNodes.First();
        var definition = _nodeDefinitionResolver.Resolve(representativeNode.NodeId);

        if (definition.FrequencyBasis == FrequencyBasis.FullUniverse)
            return true;

        if (definition.ParentNodeId is null)
            return false;

        return results.ContainsKey(definition.ParentNodeId.ToKey());
    }

    private ActionPartitionRequest BuildPartitionRequest(
        string spotKey,
        IReadOnlyList<PopulationNode> spotNodes,
        IReadOnlyDictionary<string, ApproximationResult> results,
        IReadOnlyList<RankingProfile> availableProfiles,
        string? explicitProfileName)
    {
        var aggressiveNode = spotNodes.FirstOrDefault(n => NodeSpotHelper.IsAggressiveAction(n.NodeId));
        var callNode = spotNodes.FirstOrDefault(n => NodeSpotHelper.IsCallAction(n.NodeId));
        var foldNode = spotNodes.FirstOrDefault(n => NodeSpotHelper.IsFoldAction(n.NodeId));

        var representativeNode = aggressiveNode ?? callNode ?? foldNode
            ?? throw new InvalidOperationException(
                $"Could not determine representative node for spot '{spotKey}'.");

        var strategy = _rankingProfileSelector.Select(
            representativeNode.NodeId,
            availableProfiles,
            explicitProfileName);

        var definition = _nodeDefinitionResolver.Resolve(representativeNode.NodeId);
        var candidateUniverse = GetCandidateUniverse(definition, results);

        var aggressiveRequest = aggressiveNode is null
            ? null
            : _requestBuilder.Build(aggressiveNode, results, strategy.Profile.Name);

        var callRequest = callNode is null
            ? null
            : _requestBuilder.Build(callNode, results, strategy.Profile.Name);

        var foldRequest = foldNode is null
            ? null
            : _requestBuilder.Build(foldNode, results, strategy.Profile.Name);

        return new ActionPartitionRequest(
            SpotKey: spotKey,
            AggressiveRequest: aggressiveRequest,
            CallRequest: callRequest,
            FoldRequest: foldRequest,
            RankingProfile: strategy.Profile,
            CandidateUniverse: candidateUniverse);
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