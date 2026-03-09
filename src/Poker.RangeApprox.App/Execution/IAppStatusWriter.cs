namespace Poker.RangeApprox.App.Execution;

public interface IAppStatusWriter
{
    void WriteLine(string? message = null);
}