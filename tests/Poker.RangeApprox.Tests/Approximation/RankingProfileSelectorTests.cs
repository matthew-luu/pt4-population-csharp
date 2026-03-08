using NUnit.Framework;
using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Tests.Approximation;

[TestFixture]
public class RankingProfileSelectorTests
{
    private static IReadOnlyList<RankingProfile> CreateProfiles() =>
        new List<RankingProfile>
        {
            new("HU all-in equity", new List<HandClass>()),
            new("Pokerstove", new List<HandClass>()),
            new("Sklansky-Malmuth", new List<HandClass>()),
            new("Sklansky-Chubukov", new List<HandClass>()),
            new("No limit", new List<HandClass>())
        };

    [Test]
    public void Rfi_UsesHuAllInEquity()
    {
        var selector = new RankingProfileSelector();
        var profiles = CreateProfiles();

        var result = selector.Select(
            new NodeId("rfi", "btn", null),
            profiles);

        Assert.That(result.Profile.Name, Is.EqualTo("HU all-in equity"));
    }

    [Test]
    public void ThreeBetVsOpen_UsesPokerstove()
    {
        var selector = new RankingProfileSelector();
        var profiles = CreateProfiles();

        var result = selector.Select(
            new NodeId("threebet", "btn", "co"),
            profiles);

        Assert.That(result.Profile.Name, Is.EqualTo("Pokerstove"));
    }

    [Test]
    public void FourBetVs3Bet_UsesSklanskyChubukov()
    {
        var selector = new RankingProfileSelector();
        var profiles = CreateProfiles();

        var result = selector.Select(
            new NodeId("fourbet", "btn", "sb"),
            profiles);

        Assert.That(result.Profile.Name, Is.EqualTo("Sklansky-Chubukov"));
    }

    [Test]
    public void Fold_UsesWeakestFirst()
    {
        var selector = new RankingProfileSelector();
        var profiles = CreateProfiles();

        var result = selector.Select(
            new NodeId("fold", "btn", "co"),
            profiles);

        Assert.That(result.Direction, Is.EqualTo(SelectionDirection.WeakestFirst));
    }
}