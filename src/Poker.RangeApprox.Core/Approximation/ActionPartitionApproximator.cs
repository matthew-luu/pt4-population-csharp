using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed class ActionPartitionApproximator
{
    private readonly TopDownRangeApproximator _approximator;

    public ActionPartitionApproximator(TopDownRangeApproximator approximator)
    {
        _approximator = approximator ?? throw new ArgumentNullException(nameof(approximator));
    }

    public ActionPartitionResult Approximate(ActionPartitionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.RankingProfile);

        var orderedCandidates = _approximator.GetOrderedCandidates(
            request.RankingProfile,
            request.CandidateUniverse,
            SelectionDirection.StrongestFirst);

        var universeCombos = orderedCandidates.Sum(GetWeightedCombos);

        var aggressiveTargetCombos = ClampTarget(
            request.AggressiveRequest?.TargetCombos ?? 0.0,
            universeCombos);

        var aggressiveCells = TakeFromTop(orderedCandidates, aggressiveTargetCombos);
        var remainingAfterAggressive = orderedCandidates
            .Skip(aggressiveCells.Count)
            .ToList();

        var remainingAfterAggressiveCombos = remainingAfterAggressive.Sum(GetWeightedCombos);

        var foldTargetCombos = ClampTarget(
            request.FoldRequest?.TargetCombos ?? 0.0,
            remainingAfterAggressiveCombos);

        var foldCells = TakeFromBottom(remainingAfterAggressive, foldTargetCombos);

        var middleCount = remainingAfterAggressive.Count - foldCells.Count;
        if (middleCount < 0)
            middleCount = 0;

        var callCells = remainingAfterAggressive
            .Take(middleCount)
            .ToList();

        var results = new Dictionary<string, ApproximationResult>(StringComparer.OrdinalIgnoreCase);

        if (request.AggressiveRequest is not null)
        {
            results[request.AggressiveRequest.NodeId.ToKey()] = BuildResult(
                request.AggressiveRequest,
                aggressiveCells,
                universeCombos);
        }

        if (request.CallRequest is not null)
        {
            results[request.CallRequest.NodeId.ToKey()] = BuildResult(
                request.CallRequest,
                callCells,
                universeCombos);
        }

        if (request.FoldRequest is not null)
        {
            results[request.FoldRequest.NodeId.ToKey()] = BuildResult(
                request.FoldRequest,
                foldCells,
                universeCombos);
        }

        return new ActionPartitionResult(
            SpotKey: request.SpotKey,
            Results: results);
    }

    private static ApproximationResult BuildResult(
        ApproximationRequest request,
        IReadOnlyList<RangeCell> cells,
        double universeCombos)
    {
        var actualCombos = cells.Sum(GetWeightedCombos);

        var targetPercent = universeCombos == 0
            ? 0
            : (request.TargetCombos / universeCombos) * 100.0;

        var actualPercent = universeCombos == 0
            ? 0
            : (actualCombos / universeCombos) * 100.0;

        return new ApproximationResult(
            TargetPercent: targetPercent,
            ActualPercent: actualPercent,
            TargetCombos: request.TargetCombos,
            ActualCombos: actualCombos,
            Cells: cells);
    }

    private static List<RangeCell> TakeFromTop(
        IReadOnlyList<RangeCell> orderedCandidates,
        double targetCombos)
    {
        var selected = new List<RangeCell>();
        double actualCombos = 0.0;

        foreach (var candidate in orderedCandidates)
        {
            var candidateCombos = GetWeightedCombos(candidate);
            var nextCombos = actualCombos + candidateCombos;

            var currentDistance = Math.Abs(actualCombos - targetCombos);
            var includeDistance = Math.Abs(nextCombos - targetCombos);

            if (includeDistance <= currentDistance)
            {
                selected.Add(candidate);
                actualCombos = nextCombos;
            }
            else
            {
                break;
            }
        }

        return selected;
    }

    private static List<RangeCell> TakeFromBottom(
        IReadOnlyList<RangeCell> orderedCandidates,
        double targetCombos)
    {
        var reversed = orderedCandidates.Reverse().ToList();
        var selectedReversed = TakeFromTop(reversed, targetCombos);

        selectedReversed.Reverse();
        return selectedReversed;
    }

    private static double ClampTarget(double targetCombos, double availableCombos)
    {
        if (targetCombos < 0)
            return 0;

        if (targetCombos > availableCombos)
            return availableCombos;

        return targetCombos;
    }

    private static double GetWeightedCombos(RangeCell cell)
    {
        return cell.HandClass.ComboCount * cell.Weight;
    }
}