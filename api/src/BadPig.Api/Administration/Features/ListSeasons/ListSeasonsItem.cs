namespace BadPig.Administration.Features.ListSeasons;

/// <summary>One season of the list.</summary>
/// <param name="Id">Season identifier.</param>
/// <param name="Name">Name of the season, for example 2026-2027.</param>
/// <param name="StartsOn">First day of the season.</param>
/// <param name="EndsOn">Last day of the season.</param>
/// <param name="GraceDays">Days after the start during which the previous season's membership is still accepted.</param>
/// <param name="InternalFee">Yearly fee of a person licensed by the club, in euros.</param>
/// <param name="ExternalFee">Yearly fee of a person licensed in another club, in euros.</param>
/// <param name="IsCurrent">True for the current season (the one memberships are validated for).</param>
public sealed record ListSeasonsItem(
    string Id,
    string Name,
    DateOnly StartsOn,
    DateOnly EndsOn,
    int GraceDays,
    decimal InternalFee,
    decimal ExternalFee,
    bool IsCurrent);
