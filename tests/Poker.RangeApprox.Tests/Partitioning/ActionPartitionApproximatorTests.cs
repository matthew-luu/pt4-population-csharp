using NUnit.Framework;
using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;
using System.Linq;

namespace Poker.RangeApprox.Tests.Partitioning;

[TestFixture]
public class ActionPartitionApproximatorTests
{
    [Test]
    public void Partition_ActionsAreDisjoint()
    {
        var approximator = new ActionPartitionApproximator(new TopDownRangeApproximator());

        var request = TestFixtures.CreateSimplePartitionRequest();

        var result = approximator.Approximate(request);

        var raiseKey = request.AggressiveRequest!.NodeId.ToKey();
        var callKey = request.CallRequest!.NodeId.ToKey();
        var foldKey = request.FoldRequest!.NodeId.ToKey();

        var raise = result.Results[raiseKey];
        var call = result.Results[callKey];
        var fold = result.Results[foldKey];

        var raiseHands = raise.Cells.Select(c => c.HandClass).ToHashSet();
        var callHands = call.Cells.Select(c => c.HandClass).ToHashSet();
        var foldHands = fold.Cells.Select(c => c.HandClass).ToHashSet();

        Assert.That(raiseHands.Overlaps(callHands), Is.False);
        Assert.That(raiseHands.Overlaps(foldHands), Is.False);
        Assert.That(callHands.Overlaps(foldHands), Is.False);
    }

    [Test]
    public void Partition_CoversEntireUniverse()
    {
        var approximator = new ActionPartitionApproximator(new TopDownRangeApproximator());

        var request = TestFixtures.CreateSimplePartitionRequest();

        var result = approximator.Approximate(request);

        var total = result.Results.Values.Sum(r => r.ActualCombos);

        var candidateUniverseCombos = request.CandidateUniverse!.Sum(
            c => c.HandClass.ComboCount * c.Weight);

        Assert.That(total, Is.EqualTo(candidateUniverseCombos).Within(0.001));
    }
    [Test]
    public void Partition_EachCandidateHandClassAppearsExactlyOnce()
    {
        var approximator = new ActionPartitionApproximator(new TopDownRangeApproximator());
        var request = TestFixtures.CreateSimplePartitionRequest();

        var result = approximator.Approximate(request);

        var allPartitionHands = result.Results.Values
            .SelectMany(r => r.Cells)
            .Select(c => c.HandClass)
            .ToList();

        var candidateHands = request.CandidateUniverse!
            .Select(c => c.HandClass)
            .ToList();

        Assert.That(allPartitionHands.Count, Is.EqualTo(candidateHands.Count));
        Assert.That(allPartitionHands.Distinct().Count(), Is.EqualTo(candidateHands.Count));
        Assert.That(allPartitionHands.All(candidateHands.Contains), Is.True);
    }
}