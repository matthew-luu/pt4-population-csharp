using NUnit.Framework;
using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Tests.Approximation;

[TestFixture]
public class CallingSuperRangeBuilderTests
{
    [Test]
    public void Build_GroupsCallVsOpenNodes_ByOpenPosition()
    {
        var builder = new CallingSuperRangeBuilder();

        var nodes = new List<PopulationNode>
        {
            new(new NodeId("call", "btn", "co"), 10),
            new(new NodeId("call", "sb", "co"), 12),
            new(new NodeId("call", "bb", "lj"), 8)
        };

        var results = new Dictionary<string, ApproximationResult>(StringComparer.OrdinalIgnoreCase)
        {
            ["call_btn_vs_co"] = CreateResult("AKo"),
            ["call_sb_vs_co"] = CreateResult("AQs"),
            ["call_bb_vs_lj"] = CreateResult("KQs")
        };

        var superRanges = builder.Build(nodes, results);

        Assert.That(superRanges.Count, Is.EqualTo(2));
        Assert.That(superRanges.ContainsKey("call_super_vs_co"), Is.True);
        Assert.That(superRanges.ContainsKey("call_super_vs_lj"), Is.True);
    }

    [Test]
    public void Build_IgnoresCallVsThreeBetNodes()
    {
        var builder = new CallingSuperRangeBuilder();

        var nodes = new List<PopulationNode>
        {
            new(new NodeId("call", "btn", "co"), 10),
            new(new NodeId("call", "btn", "3bet_sb"), 40)
        };

        var results = new Dictionary<string, ApproximationResult>(StringComparer.OrdinalIgnoreCase)
        {
            ["call_btn_vs_co"] = CreateResult("AKo"),
            ["call_btn_vs_3bet_sb"] = CreateResult("72o")
        };

        var superRanges = builder.Build(nodes, results);

        Assert.That(superRanges.Count, Is.EqualTo(1));
        Assert.That(superRanges.ContainsKey("call_super_vs_co"), Is.True);
        Assert.That(superRanges.ContainsKey("call_super_vs_3bet_sb"), Is.False);
    }

    [Test]
    public void Build_AveragesWeightsAcrossSourceRanges()
    {
        var builder = new CallingSuperRangeBuilder();

        var nodes = new List<PopulationNode>
        {
            new(new NodeId("call", "btn", "co"), 10),
            new(new NodeId("call", "sb", "co"), 12)
        };

        var results = new Dictionary<string, ApproximationResult>(StringComparer.OrdinalIgnoreCase)
        {
            ["call_btn_vs_co"] = CreateResult("AKo", "AQs"),
            ["call_sb_vs_co"] = CreateResult("AKo", "KQs")
        };

        var superRanges = builder.Build(nodes, results);
        var superRange = superRanges["call_super_vs_co"];

        var weights = superRange.Cells.ToDictionary(
            c => c.HandClass.ToEquilabToken(),
            c => c.Weight,
            StringComparer.OrdinalIgnoreCase);

        Assert.That(superRange.SourceRangeCount, Is.EqualTo(2));

        Assert.That(weights["AKo"], Is.EqualTo(1.0).Within(0.001));
        Assert.That(weights["AQs"], Is.EqualTo(0.5).Within(0.001));
        Assert.That(weights["KQs"], Is.EqualTo(0.5).Within(0.001));
    }

    [Test]
    public void Build_ComputesContributionCombosCorrectly()
    {
        var builder = new CallingSuperRangeBuilder();

        var nodes = new List<PopulationNode>
        {
            new(new NodeId("call", "btn", "co"), 10),
            new(new NodeId("call", "sb", "co"), 12)
        };

        var results = new Dictionary<string, ApproximationResult>(StringComparer.OrdinalIgnoreCase)
        {
            ["call_btn_vs_co"] = CreateResult("AKo", "AQs"),
            ["call_sb_vs_co"] = CreateResult("AKo", "KQs")
        };

        var superRanges = builder.Build(nodes, results);
        var superRange = superRanges["call_super_vs_co"];

        // AKo = 12 combos in btn range + 12 combos in sb range = 24
        // AQs = 4 combos
        // KQs = 4 combos
        // Total = 32
        Assert.That(superRange.TotalContributionCombos, Is.EqualTo(32.0).Within(0.001));

        // 32 / 2 source ranges = 16 average combos per source range
        Assert.That(superRange.AverageCombosPerSourceRange, Is.EqualTo(16.0).Within(0.001));
    }

    [Test]
    public void Build_ThrowsWhenSourceNodeResultIsMissing()
    {
        var builder = new CallingSuperRangeBuilder();

        var nodes = new List<PopulationNode>
        {
            new(new NodeId("call", "btn", "co"), 10)
        };

        var results = new Dictionary<string, ApproximationResult>(StringComparer.OrdinalIgnoreCase);

        Assert.Throws<InvalidOperationException>(() => builder.Build(nodes, results));
    }

    private static ApproximationResult CreateResult(params string[] handTokens)
    {
        var cells = handTokens
            .Select(t => new RangeCell(HandClass.ParseRankingToken(t), 1.0))
            .ToList();

        var actualCombos = cells.Sum(c => c.HandClass.ComboCount * c.Weight);

        return new ApproximationResult(
            TargetPercent: 0,
            ActualPercent: 0,
            TargetCombos: actualCombos,
            ActualCombos: actualCombos,
            Cells: cells);
    }
}