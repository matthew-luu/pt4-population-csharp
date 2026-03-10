namespace Poker.RangeApprox.App.Execution;

public sealed class CallbackStatusWriter : IAppStatusWriter
{
    private readonly Action<string> _write;

    public CallbackStatusWriter(Action<string> write)
    {
        _write = write ?? throw new ArgumentNullException(nameof(write));
    }

    public void WriteLine(string? message = null)
    {
        _write(message ?? string.Empty);
    }
}