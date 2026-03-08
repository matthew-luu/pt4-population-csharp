using Poker.RangeApprox.Core.Domain;
using Poker.RangeApprox.Core.Formatting;

namespace Poker.RangeApprox.Infrastructure.Writing;

public sealed class RangeFileWriter
{
    public void WriteEquilab(string filePath, IReadOnlyList<RangeCell> cells)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var content = EquilabFormatter.FormatExplicit(cells);
        File.WriteAllText(filePath, content);
    }

    public void WriteMatrix(string filePath, IReadOnlyList<RangeCell> cells)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var content = MatrixFormatter.Format(cells);
        File.WriteAllText(filePath, content);
    }
}