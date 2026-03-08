using Poker.RangeApprox.Core.Domain;
using System.Text;

namespace Poker.RangeApprox.Core.Formatting;

public static class MatrixFormatter
{
    private static readonly char[] Ranks = "AKQJT98765432".ToCharArray();

    public static string Format(IReadOnlyList<RangeCell> cells)
    {
        var map = cells.ToDictionary(c => c.HandClass.ToEquilabToken(), c => c.Weight);

        var sb = new StringBuilder();

        sb.Append("    ");
        foreach (var r in Ranks)
            sb.Append($"{r,6}");
        sb.AppendLine();

        for (int i = 0; i < Ranks.Length; i++)
        {
            sb.Append($"{Ranks[i],3} ");
            for (int j = 0; j < Ranks.Length; j++)
            {
                string token = BuildToken(i, j);
                double weight = map.TryGetValue(token, out var w) ? w : 0.0;
                sb.Append($"{weight,6:0.##}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string BuildToken(int row, int col)
    {
        char r1 = Ranks[row];
        char r2 = Ranks[col];

        if (row == col)
            return $"{r1}{r2}";

        if (row < col)
            return $"{r1}{r2}s";

        return $"{r2}{r1}o";
    }
}