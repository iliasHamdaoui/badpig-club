namespace BadPig.Administration.Features.CreateSchoolHoliday;

/// <summary>Creation of a period of school holidays in a season.</summary>
/// <param name="SeasonId">Identifier of the season (from the route).</param>
/// <param name="Body">The period.</param>
public sealed record CreateSchoolHolidayRequest(string SeasonId, CreateSchoolHolidayBody Body);
