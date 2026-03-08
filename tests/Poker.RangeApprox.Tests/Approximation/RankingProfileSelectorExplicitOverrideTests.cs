using NUnit.Framework;
using Poker.RangeApprox.Core.Approximation;
using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Tests.Approximation;

[TestFixture]
public class RankingProfileSelectorExplicitOverrideTests
{
    private static IReadOnlyList<RankingProfile> CreateProfiles() =>
        new List<RankingProfile>
        {
            new("HU all-in equity", new List<HandClass>()),
            new("Pokerstove", new List<HandClass>()),
            new("Sklansky-Chubukov", new List<HandClass>())
        };

    [Test]
    public void ExplicitProfileName_UsesRequestedProfile()
    {
        var selector = new RankingProfileSelector();
        var profiles = CreateProfiles();

        var result = selector.Select(
            new NodeId("rfi", "btn", null),
            profiles,
            explicitProfileName: "Pokerstove");

        Assert.That(result.Profile.Name, Is.EqualTo("Pokerstove"));
    }

    [Test]
    public void ExplicitProfileName_StillUsesCorrectDirection()
    {
        var selector = new RankingProfileSelector();
        var profiles = CreateProfiles();

        var result = selector.Select(
            new NodeId("fold", "btn", "co"),
            profiles,
            explicitProfileName: "Pokerstove");

        Assert.That(result.Profile.Name, Is.EqualTo("Pokerstove"));
        Assert.That(result.Direction, Is.EqualTo(SelectionDirection.WeakestFirst));
    }

    [Test]
    public void MissingExplicitProfile_Throws()
    {
        var selector = new RankingProfileSelector();
        var profiles = CreateProfiles();

        Assert.Throws<InvalidOperationException>(() =>
            selector.Select(
                new NodeId("rfi", "btn", null),
                profiles,
                explicitProfileName: "DoesNotExist"));
    }

    [Test]
    public void MissingRecommendedProfile_FallsBackToFirstAvailable()
    {
        var selector = new RankingProfileSelector();
        var profiles = new List<RankingProfile>
    {
        new("No limit", new List<HandClass>()),
        new("Sklansky-Malmuth", new List<HandClass>())
    };

        var result = selector.Select(
            new NodeId("threebet", "btn", "co"),
            profiles);

        Assert.That(result.Profile.Name, Is.EqualTo("No limit"));
    }
}