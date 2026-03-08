using NUnit.Framework;
using Poker.RangeApprox.Infrastructure.Parsing;
using System.IO;
using System.Linq;

namespace Poker.RangeApprox.Tests.Parsing;

[TestFixture]
public class RankingFileParserTests
{
    [Test]
    public void Parse_AssignsHeadersToRankingLinesInOrder()
    {
        var content = """
@No^limit@
@Pokerstove@
@Sklansky-Malmuth@
@Sklansky-Chubukov@
@HU^all-in^equity@
AAp,KKp,QQp
AKs,AQs,AJs
72o,83o,94o
KQo,QJo,JTo
A5s,A4s,A3s
""";

        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);

        var parser = new RankingFileParser();
        var profiles = parser.Parse(path);

        Assert.That(profiles.Count, Is.EqualTo(5));

        Assert.That(profiles[0].Name, Is.EqualTo("No limit"));
        Assert.That(profiles[1].Name, Is.EqualTo("Pokerstove"));
        Assert.That(profiles[2].Name, Is.EqualTo("Sklansky-Malmuth"));
        Assert.That(profiles[3].Name, Is.EqualTo("Sklansky-Chubukov"));
        Assert.That(profiles[4].Name, Is.EqualTo("HU all-in equity"));
    }

    [Test]
    public void Parse_MoreHeadersThanRankingLines_Throws()
    {
        var content = """
@No^limit@
@Pokerstove@
AAp,KKp,QQp
""";

        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);

        var parser = new RankingFileParser();

        Assert.Throws<InvalidOperationException>(() => parser.Parse(path));
    }
}