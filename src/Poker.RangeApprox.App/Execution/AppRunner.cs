namespace Poker.RangeApprox.App.Execution;

public static class AppRunner
{
    public static int Run(AppContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            switch (context.Options.Mode)
            {
                case "single":
                    ApproximationCommands.RunSingle(context);
                    break;

                case "all":
                    ApproximationCommands.RunAll(context);
                    break;

                case "rank-supercalls":
                    ApproximationCommands.RunRankSuperCalls(context);
                    break;

                case "exploit-open":
                    ExploitCommands.RunExploitOpen(context);
                    break;

                case "exploit-3bet":
                    ExploitCommands.RunExploitThreeBet(context);
                    break;

                case "range-visualization":
                    ExploitCommands.RunRangeVisualization(context);
                    break;

                default:
                    ApproximationCommands.RunRfi(context);
                    break;
            }

            return 0;
        }
        catch (Exception ex)
        {
            context.Status.WriteLine(ex.Message);
            return 1;
        }
    }
}