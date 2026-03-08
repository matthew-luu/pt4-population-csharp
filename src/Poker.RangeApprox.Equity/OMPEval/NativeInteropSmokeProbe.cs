namespace Poker.RangeApprox.Equity.OMPEval;

public sealed class NativeInteropSmokeProbe
{
    public double TestAdd(double a, double b)
    {
        return OMPEvalNative.pra_test_add(a, b);
    }
}