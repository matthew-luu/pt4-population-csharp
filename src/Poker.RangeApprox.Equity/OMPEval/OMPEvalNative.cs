using System.Runtime.InteropServices;

namespace Poker.RangeApprox.Equity.OMPEval;

internal static class OMPEvalNative
{
    [DllImport("Poker.RangeApprox.Equity.Native.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern double pra_test_add(double a, double b);

    [DllImport("Poker.RangeApprox.Equity.Native.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern double pra_calc_equity_vs_weighted_range(
        string heroHandClass,
        string villainWeightedRange,
        int iterations,
        out int errorCode);
}