using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed class CallingSuperRangeBuilder
{
    public IReadOnlyDictionary<string, WeightedSuperRangeResult> Build(
        IEnumerable<PopulationNode> sourceNodes,
        IReadOnlyDictionary<string, ApproximationResult> approximationResults)
    {
        ArgumentNullException.ThrowIfNull(sourceNodes);
        ArgumentNullException.ThrowIfNull(approximationResults);

        var callVsOpenNodes = sourceNodes
            .Where(IsCallVsOpenNode)
            .ToList();

        var groupedByOpenPosition = callVsOpenNodes
            .GroupBy(
                n => NormalizeOpponent(n.NodeId.Opponent!),
                StringComparer.OrdinalIgnoreCase);

        var results = new Dictionary<string, WeightedSuperRangeResult>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in groupedByOpenPosition)
        {
            var openPosition = group.Key;
            var sourceRangeCount = group.Count();

            var accumulators = new Dictionary<string, CellAccumulator>(StringComparer.OrdinalIgnoreCase);

            foreach (var node in group)
            {
                var nodeKey = node.NodeId.ToKey();

                if (!approximationResults.TryGetValue(nodeKey, out var approximationResult))
                {
                    throw new InvalidOperationException(
                        $"Approximation result '{nodeKey}' was not found while building calling super-ranges.");
                }

                foreach (var cell in approximationResult.Cells)
                {
                    if (cell.Weight <= 0)
                        continue;

                    var token = cell.HandClass.ToEquilabToken();

                    if (!accumulators.TryGetValue(token, out var accumulator))
                    {
                        accumulator = new CellAccumulator(cell.HandClass);
                        accumulators[token] = accumulator;
                    }

                    accumulator.SumWeights += cell.Weight;
                    accumulator.SumContributionCombos += cell.HandClass.ComboCount * cell.Weight;
                }
            }

            var weightedCells = accumulators.Values
                .Select(a => new RangeCell(
                    HandClass: a.HandClass,
                    Weight: a.SumWeights / sourceRangeCount))
                .Where(c => c.Weight > 0)
                .OrderByDescending(c => c.Weight)
                .ThenBy(c => c.HandClass.ToEquilabToken(), StringComparer.OrdinalIgnoreCase)
                .ToList();

            var totalContributionCombos = accumulators.Values.Sum(a => a.SumContributionCombos);
            var averageCombosPerSourceRange = sourceRangeCount == 0
                ? 0
                : totalContributionCombos / sourceRangeCount;

            var key = $"call_super_vs_{openPosition}";

            results[key] = new WeightedSuperRangeResult(
                Key: key,
                OpenPosition: openPosition,
                SourceRangeCount: sourceRangeCount,
                TotalContributionCombos: totalContributionCombos,
                AverageCombosPerSourceRange: averageCombosPerSourceRange,
                Cells: weightedCells);
        }

        return results;
    }

    private static bool IsCallVsOpenNode(PopulationNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var nodeId = node.NodeId;

        if (!NodeSpotHelper.IsCallAction(nodeId))
            return false;

        if (string.IsNullOrWhiteSpace(nodeId.Opponent))
            return false;

        if (NodeSpotHelper.IsVsThreeBetSpot(nodeId))
            return false;

        return true;
    }

    private static string NormalizeOpponent(string opponent)
    {
        return opponent.Trim().ToLowerInvariant();
    }

    private sealed class CellAccumulator
    {
        public CellAccumulator(HandClass handClass)
        {
            HandClass = handClass;
        }

        public HandClass HandClass { get; }

        public double SumWeights { get; set; }

        public double SumContributionCombos { get; set; }
    }
}