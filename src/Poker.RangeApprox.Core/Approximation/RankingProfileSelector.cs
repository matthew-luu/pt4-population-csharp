using Poker.RangeApprox.Core.Domain;

namespace Poker.RangeApprox.Core.Approximation;

public sealed class RankingProfileSelector
{
    public RankingStrategy Select(
        NodeId nodeId,
        IReadOnlyList<RankingProfile> availableProfiles,
        string? explicitProfileName = null)
    {
        ArgumentNullException.ThrowIfNull(nodeId);
        ArgumentNullException.ThrowIfNull(availableProfiles);

        if (availableProfiles.Count == 0)
            throw new InvalidOperationException("No ranking profiles were provided.");

        if (!string.IsNullOrWhiteSpace(explicitProfileName))
        {
            var explicitProfile = FindProfile(availableProfiles, explicitProfileName);
            if (explicitProfile is null)
            {
                throw new InvalidOperationException(
                    $"Requested ranking profile '{explicitProfileName}' was not found. " +
                    $"Available profiles: {string.Join(", ", availableProfiles.Select(p => p.Name))}");
            }

            return new RankingStrategy(
                Profile: explicitProfile,
                Direction: GetDirection(nodeId));
        }

        var recommendedProfileName = GetRecommendedProfileName(nodeId);

        var recommendedProfile = FindProfile(availableProfiles, recommendedProfileName);
        if (recommendedProfile is not null)
        {
            return new RankingStrategy(
                Profile: recommendedProfile,
                Direction: GetDirection(nodeId));
        }

        // Fallback to first available profile if the preferred one doesn't exist.
        return new RankingStrategy(
            Profile: availableProfiles.First(),
            Direction: GetDirection(nodeId));
    }

    private static string GetRecommendedProfileName(NodeId nodeId)
    {
        var action = Normalize(nodeId.Action);
        var opponent = NormalizeNullable(nodeId.Opponent);

        var isVsThreeBet = !string.IsNullOrWhiteSpace(opponent)
            && opponent.StartsWith("3bet_", StringComparison.OrdinalIgnoreCase);

        // With the profiles you currently have:
        // - HU all-in equity
        // - Pokerstove
        // - Sklansky-Malmuth
        // - Sklansky-Chubukov
        // - No limit
        //
        // This is a heuristic mapping, not a solved one.

        if (action == "rfi")
            return "HU all-in equity";

        if (action == "fold")
        {
            // Folding still uses a ranking, but we will traverse weakest-first.
            // Pokerstove is a reasonable generic baseline for now.
            return "Pokerstove";
        }

        if (action == "call" && !isVsThreeBet)
            return "Pokerstove";

        if (action == "threebet" && !isVsThreeBet)
            return "Pokerstove";

        if (action == "call" && isVsThreeBet)
            return "Sklansky-Chubukov";

        if (action == "fourbet")
            return "Sklansky-Chubukov";

        // Conservative fallback
        return "HU all-in equity";
    }

    private static SelectionDirection GetDirection(NodeId nodeId)
    {
        var action = Normalize(nodeId.Action);

        return action == "fold"
            ? SelectionDirection.WeakestFirst
            : SelectionDirection.StrongestFirst;
    }

    private static RankingProfile? FindProfile(
        IReadOnlyList<RankingProfile> profiles,
        string profileName)
    {
        return profiles.FirstOrDefault(p =>
            p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim().ToLowerInvariant();
    }
}