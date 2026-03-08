using NUnit.Framework;
using Poker.RangeApprox.Equity.OMPEval;

namespace Poker.RangeApprox.Tests.Approximation;

[TestFixture]
public class NativeInteropSmokeTests
{
    [Test]
    public void NativeDll_CanBeCalled()
    {
        var probe = new NativeInteropSmokeProbe();

        var result = probe.TestAdd(2.0, 3.0);

        Assert.That(result, Is.EqualTo(5.0));
    }
}