using Poker.RangeApprox.Core.Approximation;

namespace Poker.RangeApprox.Infrastructure.Writing;

public sealed class WeightedSuperRangeFileWriter
{
    private readonly RangeFileWriter _rangeFileWriter;

    public WeightedSuperRangeFileWriter(RangeFileWriter rangeFileWriter)
    {
        _rangeFileWriter = rangeFileWriter ?? throw new ArgumentNullException(nameof(rangeFileWriter));
    }

    public void WriteAll(
        string outputRoot,
        IReadOnlyDictionary<string, WeightedSuperRangeResult> superRanges)
    {
        ArgumentNullException.ThrowIfNull(outputRoot);
        ArgumentNullException.ThrowIfNull(superRanges);

        var baseDirectory = Path.Combine(outputRoot, "calling super-ranges");

        foreach (var superRange in superRanges.Values.OrderBy(x => x.OpenPosition, StringComparer.OrdinalIgnoreCase))
        {
            var matrixPath = Path.Combine(baseDirectory, $"{superRange.Key}.matrix.txt");
            var equilabPath = Path.Combine(baseDirectory, $"{superRange.Key}.txt");
            var summaryPath = Path.Combine(baseDirectory, $"{superRange.Key}.summary.txt");

            _rangeFileWriter.WriteMatrix(matrixPath, superRange.Cells);
            _rangeFileWriter.WriteEquilab(equilabPath, superRange.Cells);

            File.WriteAllText(summaryPath, BuildSummary(superRange));
        }
    }
    private static string BuildSummary(WeightedSuperRangeResult result)
    {
        return $"""
Key:                         {result.Key}
Open Position:               {result.OpenPosition}
Source Range Count:          {result.SourceRangeCount}
Total Contribution Combos:   {result.TotalContributionCombos:0.###}
Average Combos Per Range:    {result.AverageCombosPerSourceRange:0.###}
""";
    }

}