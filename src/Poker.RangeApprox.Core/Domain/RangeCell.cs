namespace Poker.RangeApprox.Core.Domain;

public sealed record RangeCell(HandClass HandClass, double Weight)
{
    public int WeightedCombos => (int)Math.Round(HandClass.ComboCount * Weight, MidpointRounding.AwayFromZero);
}