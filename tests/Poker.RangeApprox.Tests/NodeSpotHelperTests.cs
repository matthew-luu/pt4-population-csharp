using NUnit.Framework;
using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Tests.Domain;

[TestFixture]
public class NodeSpotHelperTests
{
    [Test]
    public void FourBetAndCallVs3Bet_MapToSameSpot()
    {
        var a = NodeSpotHelper.ToSpotKey(new NodeId("fourbet", "btn", "sb"));
        var b = NodeSpotHelper.ToSpotKey(new NodeId("call", "btn", "3bet_sb"));

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void ThreeBetCallFold_MapToSameSpot()
    {
        var a = NodeSpotHelper.ToSpotKey(new NodeId("threebet", "btn", "co"));
        var b = NodeSpotHelper.ToSpotKey(new NodeId("call", "btn", "co"));
        var c = NodeSpotHelper.ToSpotKey(new NodeId("fold", "btn", "co"));

        Assert.That(a, Is.EqualTo(b));
        Assert.That(b, Is.EqualTo(c));
    }
}