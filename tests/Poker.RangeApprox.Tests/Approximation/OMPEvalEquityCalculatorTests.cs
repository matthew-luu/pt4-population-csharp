using NUnit.Framework;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Core.Equity;
using Poker.RangeApprox.Equity.OMPEval;

namespace Poker.RangeApprox.Tests.Approximation;

[TestFixture]
public class OMPEvalEquityCalculatorTests
{
    [Test]
    public void CalculateEquity_ComputesRealEquity_ForSimpleHeadsUpCase()
    {
        var calculator = new OMPEvalEquityCalculator(iterations: 100000);

        var villainRange = new List<RangeCell>
        {
            new(HandClass.ParseRankingToken("AKo"), 1.0)
        };

        var equity = calculator.CalculateEquity(
            HandClass.ParseRankingToken("QQp"),
            villainRange);

        Assert.That(equity, Is.GreaterThan(0.50));
        Assert.That(equity, Is.LessThan(0.60));
    }

    [Test]
    public void RankAllHands_ReturnsSortedEquities()
    {
        var calculator = new OMPEvalEquityCalculator(iterations: 50000);
        var rankingService = new HandVsRangeRankingService(calculator);

        var villainRange = new List<RangeCell>
        {
        new(HandClass.ParseRankingToken("AKo"), 1.0)
        };

        var ranking = rankingService.RankAllHands(villainRange);

        Assert.That(ranking.Count, Is.EqualTo(169));

        Assert.That(
            ranking.Zip(ranking.Skip(1))
                   .All(pair => pair.First.Equity >= pair.Second.Equity),
            Is.True);
    }
}