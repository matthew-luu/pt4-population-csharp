using CsvHelper;
using System.Globalization;

namespace Poker.RangeApprox.Infrastructure.Parsing;
public sealed class PopulationCsvReader
{
    public List<Dictionary<string, string>> Read(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = new List<Dictionary<string, string>>();

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;

        while (csv.Read())
        {
            var row = new Dictionary<string, string>();

            foreach (var header in headers)
            {
                row[header] = csv.GetField(header);
            }

            records.Add(row);
        }

        return records;
    }
}