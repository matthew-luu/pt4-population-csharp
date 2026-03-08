using System.Globalization;

namespace Poker.RangeApprox.Core.Domain;

public sealed record HandClass
{
    public char HighRank { get; }
    public char LowRank { get; }
    public HandShape Shape { get; }

    public HandClass(char highRank, char lowRank, HandShape shape)
    {
        HighRank = highRank;
        LowRank = lowRank;
        Shape = shape;
    }

    public bool IsPair => Shape == HandShape.Pair;

    public int ComboCount => Shape switch
    {
        HandShape.Pair => 6,
        HandShape.Suited => 4,
        HandShape.Offsuit => 12,
        _ => throw new InvalidOperationException("Unknown hand shape.")
    };

    public string ToEquilabToken() => Shape switch
    {
        HandShape.Pair => $"{HighRank}{LowRank}",
        HandShape.Suited => $"{HighRank}{LowRank}s",
        HandShape.Offsuit => $"{HighRank}{LowRank}o",
        _ => throw new InvalidOperationException("Unknown hand shape.")
    };

    public override string ToString() => ToEquilabToken();

    public static HandClass ParseRankingToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        token = token.Trim();

        // File format examples: AAp, AKs, AKo
        if (token.Length != 3)
            throw new FormatException($"Invalid ranking token '{token}'.");

        char r1 = token[0];
        char r2 = token[1];
        char suffix = char.ToLowerInvariant(token[2]);

        HandShape shape = suffix switch
        {
            'p' => HandShape.Pair,
            's' => HandShape.Suited,
            'o' => HandShape.Offsuit,
            _ => throw new FormatException($"Unknown hand suffix '{suffix}' in token '{token}'.")
        };

        if (shape == HandShape.Pair && r1 != r2)
            throw new FormatException($"Pair token '{token}' must have identical ranks.");

        if (shape != HandShape.Pair && r1 == r2)
            throw new FormatException($"Non-pair token '{token}' cannot have identical ranks.");

        return new HandClass(r1, r2, shape);
    }
}