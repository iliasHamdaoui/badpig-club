namespace BadPig.Administration.Features.CreateSeason;

/// <summary>Body of the creation of a season. Only the start date and the two fees are required.</summary>
/// <param name="StartsOn">First day of the season (usually the 1st of September).</param>
/// <param name="EndsOn">Last day of the season. Default: one year after the start, minus a day (31 August).</param>
/// <param name="Name">Name of the season (1 to 60 characters, unique). Default: the years, for example 2026-2027.</param>
/// <param name="GraceDays">Days after the start during which the previous season's membership is still accepted. Default: 30.</param>
/// <param name="InternalFee">Yearly fee of a person licensed by the club, in euros.</param>
/// <param name="ExternalFee">Yearly fee of a person licensed in another club, in euros.</param>
public sealed record CreateSeasonRequest(
    DateOnly? StartsOn,
    DateOnly? EndsOn,
    string? Name,
    int? GraceDays,
    decimal? InternalFee,
    decimal? ExternalFee);
