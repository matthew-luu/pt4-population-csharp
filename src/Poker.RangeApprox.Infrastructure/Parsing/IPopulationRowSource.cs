namespace Poker.RangeApprox.Infrastructure.Parsing;

public interface IPopulationRowSource
{
    Dictionary<string, string> ReadSingleRow();
}