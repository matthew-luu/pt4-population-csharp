using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed class ApproximationRequestBuilder
{
    private readonly NodeDefinitionResolver _nodeDefinitionResolver;

    public ApproximationRequestBuilder(NodeDefinitionResolver nodeDefinitionResolver)
    {
        _nodeDefinitionResolver = nodeDefinitionResolver ?? throw new ArgumentNullException(nameof(nodeDefinitionResolver));
    }

    public ApproximationRequest Build(
        PopulationNode populationNode,
        IReadOnlyDictionary<string, ApproximationResult> existingResults,
        string? rankingProfileName = null)
    {
        ArgumentNullException.ThrowIfNull(populationNode);
        ArgumentNullException.ThrowIfNull(existingResults);

        var definition = _nodeDefinitionResolver.Resolve(populationNode.NodeId);

        double targetCombos = definition.FrequencyBasis switch
        {
            FrequencyBasis.FullUniverse => CalculateFromFullUniverse(populationNode.Frequency),
            FrequencyBasis.ParentRange => CalculateFromParentRange(
                populationNode,
                definition.ParentNodeId,
                existingResults),
            _ => throw new InvalidOperationException(
                $"Unsupported frequency basis '{definition.FrequencyBasis}'.")
        };

        return new ApproximationRequest(
            NodeId: populationNode.NodeId,
            LocalFrequencyPercent: populationNode.Frequency,
            FrequencyBasis: definition.FrequencyBasis,
            ParentNodeId: definition.ParentNodeId,
            TargetCombos: targetCombos,
            RankingProfileName: rankingProfileName
        );
    }

    private static double CalculateFromFullUniverse(double localFrequencyPercent)
    {
        ValidatePercent(localFrequencyPercent);
        return TopDownRangeApproximator.TotalPreflopCombos * (localFrequencyPercent / 100.0);
    }

    private static double CalculateFromParentRange(
        PopulationNode populationNode,
        NodeId? parentNodeId,
        IReadOnlyDictionary<string, ApproximationResult> existingResults)
    {
        if (parentNodeId is null)
        {
            throw new InvalidOperationException(
                $"Node '{populationNode.NodeId.ToKey()}' requires a parent range, but ParentNodeId was null.");
        }

        var parentKey = parentNodeId.ToKey();

        if (!existingResults.TryGetValue(parentKey, out var parentResult))
        {
            throw new InvalidOperationException(
                $"Parent result '{parentKey}' was not found for node '{populationNode.NodeId.ToKey()}'. " +
                "Generate the parent node first.");
        }

        ValidatePercent(populationNode.Frequency);

        var parentCombos = parentResult.ActualCombos;
        return parentCombos * (populationNode.Frequency / 100.0);
    }

    private static void ValidatePercent(double percent)
    {
        if (percent < 0 || percent > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(percent),
                $"Frequency percent must be between 0 and 100. Received: {percent}");
        }
    }
}