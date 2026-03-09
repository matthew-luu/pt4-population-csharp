namespace Poker.RangeApprox.Core.Domain;

public sealed class PopulationOpportunityProfile
{
    private readonly Dictionary<string, int> _rfiOpportunities;
    private readonly Dictionary<string, int> _vsOpenOpportunities;
    private readonly Dictionary<string, int> _vsThreeBetOpportunities;
    private readonly Dictionary<string, int> _vsFourBetOpportunities;

    public PopulationOpportunityProfile(
        IReadOnlyDictionary<string, int>? rfiOpportunities = null,
        IReadOnlyDictionary<string, int>? vsOpenOpportunities = null,
        IReadOnlyDictionary<string, int>? vsThreeBetOpportunities = null,
        IReadOnlyDictionary<string, int>? vsFourBetOpportunities = null)
    {
        _rfiOpportunities = ToDictionary(rfiOpportunities);
        _vsOpenOpportunities = ToDictionary(vsOpenOpportunities);
        _vsThreeBetOpportunities = ToDictionary(vsThreeBetOpportunities);
        _vsFourBetOpportunities = ToDictionary(vsFourBetOpportunities);
    }

    public int GetRfiOpportunities(string position) => Get(_rfiOpportunities, position);

    public int GetVsOpenOpportunities(string openPosition) => Get(_vsOpenOpportunities, openPosition);

    public int GetVsThreeBetOpportunities(string openPosition) => Get(_vsThreeBetOpportunities, openPosition);

    public int GetVsFourBetOpportunities(string actorPosition) => Get(_vsFourBetOpportunities, actorPosition);

    private static int Get(IReadOnlyDictionary<string, int> map, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return map.TryGetValue(Normalize(key), out var value)
            ? value
            : 0;
    }

    private static Dictionary<string, int> ToDictionary(IReadOnlyDictionary<string, int>? source)
    {
        return source is null
            ? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            : source.ToDictionary(
                kv => Normalize(kv.Key),
                kv => kv.Value,
                StringComparer.OrdinalIgnoreCase);
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();
}