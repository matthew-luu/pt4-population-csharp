namespace Poker.RangeApprox.App.Execution;

public sealed class ConsoleStatusWriter : IAppStatusWriter
{
    public void WriteLine(string? message = null)
    {
        Console.WriteLine(message);
    }
}