using System.Text.Json;
using Poker.RangeApprox.Core.Visualization;

namespace Poker.RangeApprox.Infrastructure.Writing;

public sealed class RangeVisualizationFileWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public void Write(string filePath, RangeVisualizationDocument document)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(document, Options);

        File.WriteAllText(filePath, json);
    }

    public RangeVisualizationDocument Read(string filePath)
    {
        var json = File.ReadAllText(filePath);

        var doc = JsonSerializer.Deserialize<RangeVisualizationDocument>(json, Options);

        if (doc == null)
            throw new InvalidOperationException("Failed to deserialize range visualization document.");

        return doc;
    }
}