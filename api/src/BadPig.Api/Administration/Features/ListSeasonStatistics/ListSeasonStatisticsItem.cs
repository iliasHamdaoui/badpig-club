namespace BadPig.Administration.Features.ListSeasonStatistics;

/// <summary>Memberships and fees collected for one season. The amounts are the ones recorded when each membership was validated.</summary>
/// <param name="SeasonId">Season identifier.</param>
/// <param name="Name">Name of the season, for example 2026-2027.</param>
/// <param name="StartsOn">First day of the season.</param>
/// <param name="EndsOn">Last day of the season.</param>
/// <param name="InternalCount">Number of validated memberships of people licensed by the club.</param>
/// <param name="ExternalCount">Number of validated memberships of people licensed in another club.</param>
/// <param name="TotalCount">Number of validated memberships.</param>
/// <param name="InternalRevenue">Fees collected from people licensed by the club, in euros.</param>
/// <param name="ExternalRevenue">Fees collected from people licensed in another club, in euros.</param>
/// <param name="TotalRevenue">All the fees collected, in euros.</param>
public sealed record ListSeasonStatisticsItem(
    string SeasonId,
    string Name,
    DateOnly StartsOn,
    DateOnly EndsOn,
    int InternalCount,
    int ExternalCount,
    int TotalCount,
    decimal InternalRevenue,
    decimal ExternalRevenue,
    decimal TotalRevenue);
