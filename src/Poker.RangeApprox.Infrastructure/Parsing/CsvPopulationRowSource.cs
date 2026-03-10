namespace Poker.RangeApprox.Infrastructure.Parsing;

public sealed class CsvPopulationRowSource : IPopulationRowSource
{
    private readonly string _path;

    public CsvPopulationRowSource(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("CSV path cannot be empty.", nameof(path));

        _path = path;
    }

    public Dictionary<string, string> ReadSingleRow()
    {
        var csvReader = new PopulationCsvReader();
        var rows = csvReader.Read(_path);

        if (rows.Count == 0)
            throw new InvalidOperationException($"No CSV rows found in '{_path}'.");

        return rows[0];
    }
}