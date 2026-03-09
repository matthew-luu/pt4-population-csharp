using Poker.RangeApprox.App;
using Poker.RangeApprox.App.Execution;

namespace Poker.RangeApprox.App;

public static class Program
{
    public static int Main(string[] args)
    {
        var options = AppOptions.Parse(args);
        var context = AppBootstrapper.Build(options, new ConsoleStatusWriter());

        return AppRunner.Run(context);
    }
}