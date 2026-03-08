using NUnit.Framework;
using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Equity.OMPEval;

namespace Poker.RangeApprox.Tests.Approximation;

[TestFixture]
public class OMPEvalEquityCalculatorTests
{
    [Test]
    public void CalculateEquity_CallsNativeStubSuccessfully()
    {
        var calculator = new OMPEvalEquityCalculator(iterations: 1000);

        var villainRange = new List<RangeCell>
        {
            new(HandClass.ParseRankingToken("AKo"), 1.0),
            new(HandClass.ParseRankingToken("AQs"), 0.5)
        };

        var equity = calculator.CalculateEquity(
            HandClass.ParseRankingToken("QQp"),
            villainRange);

        Assert.That(equity, Is.EqualTo(0.5).Within(0.000001));
    }
}