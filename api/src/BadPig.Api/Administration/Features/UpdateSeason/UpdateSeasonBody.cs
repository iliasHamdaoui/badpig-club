namespace BadPig.Administration.Features.UpdateSeason;

/// <summary>JSON body of the request (the season id comes from the route). Every field is required: the season is replaced.</summary>
/// <param name="Name">Name of the season (1 to 60 characters, unique).</param>
/// <param name="StartsOn">First day of the season.</param>
/// <param name="EndsOn">Last day of the season (after the start).</param>
/// <param name="GraceDays">Days after the start during which the previous season's membership is still accepted (0 to 120).</param>
/// <param name="InternalFee">Yearly fee of a person licensed by the club, in euros.</param>
/// <param name="ExternalFee">Yearly fee of a person licensed in another club, in euros.</param>
public sealed record UpdateSeasonBody(
    string? Name,
    DateOnly? StartsOn,
    DateOnly? EndsOn,
    int? GraceDays,
    decimal? InternalFee,
    decimal? ExternalFee);
