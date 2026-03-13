using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Core.Visualization;

namespace Poker.RangeApprox.App.Execution;

public sealed class RangeVisualizationBuilder
{
    public RangeVisualizationDocument Build(
        IReadOnlyDictionary<string, ExploitSpotResult> openResults,
        IReadOnlyDictionary<string, ExploitThreeBetSpotResult> threeBetResults)
    {
        var scenarios = new List<RangeVisualizationScenario>
        {
            BuildOpenScenario(openResults),
            BuildVsOpenScenario(threeBetResults)
        };

        return new RangeVisualizationDocument(
            "1.0",
            DateTime.UtcNow,
            scenarios);
    }

    private RangeVisualizationScenario BuildOpenScenario(
        IReadOnlyDictionary<string, ExploitSpotResult> spots)
    {
        var views = new List<RangeVisualizationView>();

        foreach (var spot in spots.Values)
        {
            var cells = new List<RangeVisualizationCell>();

            foreach (var hand in spot.RankedHands)
            {
                var token = hand.HeroHand.ToEquilabToken();

                var (row, col) =
                    HandMatrixCoordinateMapper.GetCoordinates(hand.HeroHand);

                var (actionId, label) =
                    RangeVisualizationActionClassifier.ClassifyOpen(hand);

                cells.Add(new RangeVisualizationCell(
                    token,
                    row,
                    col,
                    actionId,
                    label,
                    hand.TotalEv,
                    hand.FoldBranchEv,
                    hand.CallBranchEv,
                    hand.ThreeBetBranchEv,
                    null));
            }

            var legend = RangeVisualizationLegendBuilder.Build(cells);

            views.Add(new RangeVisualizationView(
                spot.OpenPosition,
                spot.OpenPosition.ToUpperInvariant(),
                spot.OpenPosition,
                null,
                cells,
                legend));
        }

        return new RangeVisualizationScenario(
            "open",
            "Open",
            views);
    }

    private RangeVisualizationScenario BuildVsOpenScenario(
        IReadOnlyDictionary<string, ExploitThreeBetSpotResult> spots)
    {
        var views = new List<RangeVisualizationView>();

        foreach (var spot in spots.Values)
        {
            var cells = new List<RangeVisualizationCell>();

            foreach (var hand in spot.RankedHands)
            {
                var token = hand.HeroHand.ToEquilabToken();

                var (row, col) =
                    HandMatrixCoordinateMapper.GetCoordinates(hand.HeroHand);

                var (actionId, label) =
                    RangeVisualizationActionClassifier.ClassifyVsOpen(hand);

                cells.Add(new RangeVisualizationCell(
                    token,
                    row,
                    col,
                    actionId,
                    label,
                    hand.TotalEv,
                    hand.FoldBranchEv,
                    hand.CallBranchEv,
                    null,
                    hand.FourBetBranchEv));
            }

            var legend = RangeVisualizationLegendBuilder.Build(cells);

            views.Add(new RangeVisualizationView(
                $"{spot.ThreeBettorPosition}_vs_{spot.OpenPosition}",
                $"{spot.ThreeBettorPosition.ToUpper()} vs {spot.OpenPosition.ToUpper()}",
                spot.ThreeBettorPosition,
                spot.OpenPosition,
                cells,
                legend));
        }

        return new RangeVisualizationScenario(
            "vs_open",
            "vs open",
            views);
    }
}