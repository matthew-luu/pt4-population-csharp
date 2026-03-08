using NUnit.Framework;
using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Tests.Domain;

[TestFixture]
public class NodeDefinitionResolverTests
{
    private NodeDefinitionResolver _resolver = null!;

    [SetUp]
    public void Setup()
    {
        _resolver = new NodeDefinitionResolver();
    }

    [Test]
    public void RfiNode_UsesFullUniverse()
    {
        var node = new NodeId("rfi", "btn", null);

        var def = _resolver.Resolve(node);

        Assert.That(def.FrequencyBasis, Is.EqualTo(FrequencyBasis.FullUniverse));
        Assert.That(def.ParentNodeId, Is.Null);
    }

    [Test]
    public void CallVsOpen_UsesFullUniverse()
    {
        var node = new NodeId("call", "bb", "btn");

        var def = _resolver.Resolve(node);

        Assert.That(def.FrequencyBasis, Is.EqualTo(FrequencyBasis.FullUniverse));
        Assert.That(def.ParentNodeId, Is.Null);
    }

    [Test]
    public void CallVs3Bet_UsesParentRange()
    {
        var node = new NodeId("call", "btn", "3bet_sb");

        var def = _resolver.Resolve(node);

        Assert.That(def.FrequencyBasis, Is.EqualTo(FrequencyBasis.ParentRange));
        Assert.That(def.ParentNodeId!.ToKey(), Is.EqualTo("rfi_btn"));
    }

    [Test]
    public void FoldVs3Bet_UsesParentRange()
    {
        var node = new NodeId("fold", "btn", "3bet_sb");

        var def = _resolver.Resolve(node);

        Assert.That(def.FrequencyBasis, Is.EqualTo(FrequencyBasis.ParentRange));
        Assert.That(def.ParentNodeId!.ToKey(), Is.EqualTo("rfi_btn"));
    }

    [Test]
    public void FourBetVs3Bet_UsesParentRange()
    {
        var node = new NodeId("fourbet", "btn", "sb");

        var def = _resolver.Resolve(node);

        Assert.That(def.FrequencyBasis, Is.EqualTo(FrequencyBasis.ParentRange));
        Assert.That(def.ParentNodeId!.ToKey(), Is.EqualTo("rfi_btn"));
    }
}