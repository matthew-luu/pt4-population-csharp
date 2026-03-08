namespace Poker.RangeApprox.Core.Domain;

public static class HandClassCatalog
{
    private static readonly char[] Ranks = "AKQJT98765432".ToCharArray();

    public static IReadOnlyList<HandClass> GetAll169()
    {
        var hands = new List<HandClass>(169);

        for (int row = 0; row < Ranks.Length; row++)
        {
            for (int col = 0; col < Ranks.Length; col++)
            {
                if (row == col)
                {
                    hands.Add(new HandClass(Ranks[row], Ranks[col], HandShape.Pair));
                }
                else if (row < col)
                {
                    hands.Add(new HandClass(Ranks[row], Ranks[col], HandShape.Suited));
                }
                else
                {
                    hands.Add(new HandClass(Ranks[col], Ranks[row], HandShape.Offsuit));
                }
            }
        }

        return hands;
    }
}