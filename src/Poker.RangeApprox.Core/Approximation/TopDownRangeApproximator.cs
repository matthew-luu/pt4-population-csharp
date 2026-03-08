﻿using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed class TopDownRangeApproximator
{
    public const int TotalPreflopCombos = 1326;

    public ApproximationResult Approximate(
        double targetPercent,
        RankingProfile profile,
        SelectionDirection direction = SelectionDirection.StrongestFirst)
    {
        if (targetPercent < 0 || targetPercent > 100)
            throw new ArgumentOutOfRangeException(nameof(targetPercent), "Target percent must be between 0 and 100.");

        ArgumentNullException.ThrowIfNull(profile);

        var targetCombos = TotalPreflopCombos * (targetPercent / 100.0);

        return ApproximateToComboTarget(
            targetCombos: targetCombos,
            profile: profile,
            candidateUniverse: null,
            direction: direction);
    }

    public ApproximationResult ApproximateToComboTarget(
        double targetCombos,
        RankingProfile profile,
        IReadOnlyList<RangeCell>? candidateUniverse,
        SelectionDirection direction = SelectionDirection.StrongestFirst)
    {
        if (targetCombos < 0)
            throw new ArgumentOutOfRangeException(nameof(targetCombos), "Target combos must be >= 0.");

        ArgumentNullException.ThrowIfNull(profile);

        var orderedCandidates = GetOrderedCandidates(profile, candidateUniverse, direction);
        var universeCombos = orderedCandidates.Sum(GetWeightedCombos);

        if (targetCombos > universeCombos)
        {
            targetCombos = universeCombos;
        }

        var selected = new List<RangeCell>();
        double actualCombos = 0.0;

        foreach (var candidate in orderedCandidates)
        {
            var candidateWeightedCombos = GetWeightedCombos(candidate);
            var nextCombos = actualCombos + candidateWeightedCombos;

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

        var targetPercent = universeCombos == 0
            ? 0
            : (targetCombos / universeCombos) * 100.0;

        var actualPercent = universeCombos == 0
            ? 0
            : (actualCombos / universeCombos) * 100.0;

        return new ApproximationResult(
            TargetPercent: targetPercent,
            ActualPercent: actualPercent,
            TargetCombos: targetCombos,
            ActualCombos: actualCombos,
            Cells: selected);
    }

    public List<RangeCell> GetOrderedCandidates(
        RankingProfile profile,
        IReadOnlyList<RangeCell>? candidateUniverse,
        SelectionDirection direction = SelectionDirection.StrongestFirst)
    {
        ArgumentNullException.ThrowIfNull(profile);

        List<RangeCell> ordered;

        if (candidateUniverse is null)
        {
            ordered = profile.OrderedHands
                .Select(h => new RangeCell(h, 1.0))
                .ToList();
        }
        else
        {
            var candidateMap = candidateUniverse
                .Where(c => c.Weight > 0)
                .GroupBy(c => c.HandClass.ToEquilabToken(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);
            ordered = new List<RangeCell>();

            foreach (var hand in profile.OrderedHands)
            {
                var token = hand.ToEquilabToken();

                if (candidateMap.TryGetValue(token, out var candidate))
                {
                    ordered.Add(candidate);
                }
            }
        }

        if (direction == SelectionDirection.WeakestFirst)
        {
            ordered.Reverse();
        }

        return ordered;
    }

    private static double GetWeightedCombos(RangeCell cell)
    {
        return cell.HandClass.ComboCount * cell.Weight;
    }
}