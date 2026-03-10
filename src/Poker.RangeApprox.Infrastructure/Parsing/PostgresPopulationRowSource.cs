using System.Data;
using System.Globalization;
using Npgsql;

namespace Poker.RangeApprox.Infrastructure.Parsing;

public sealed class PostgresPopulationRowSource : IPopulationRowSource
{
    private readonly string _connectionString;
    private readonly short _idLimit;

    public PostgresPopulationRowSource(
        string connectionString,
        short idLimit)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));

        _connectionString = connectionString;
        _idLimit = idLimit;
    }

    public Dictionary<string, string> ReadSingleRow()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        RefreshPopulationViews(connection);

        using var command = connection.CreateCommand();
        command.CommandText = "select * from pop.get_population_preflop(@p_id_limit);";
        command.Parameters.AddWithValue("p_id_limit", _idLimit);

        using var reader = command.ExecuteReader(CommandBehavior.SingleRow);

        if (!reader.Read())
        {
            throw new InvalidOperationException(
                $"No population row was returned for id_limit = {_idLimit}.");
        }

        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);

            if (reader.IsDBNull(i))
            {
                row[columnName] = string.Empty;
                continue;
            }

            row[columnName] = ConvertToInvariantString(reader.GetValue(i));
        }

        return row;
    }

    private static void RefreshPopulationViews(NpgsqlConnection connection)
    {
        using var refreshCommand = connection.CreateCommand();
        refreshCommand.CommandText = "call pop.refresh_population_views();";
        refreshCommand.ExecuteNonQuery();
    }

    private static string ConvertToInvariantString(object value)
    {
        return value switch
        {
            string s => s,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}