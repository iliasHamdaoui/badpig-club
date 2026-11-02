namespace BadPig.Administration.Features.DeleteSchoolHoliday;

/// <summary>Deletion of a period of school holidays.</summary>
/// <param name="SeasonId">Identifier of the season (from the route).</param>
/// <param name="Id">Identifier of the period (from the route).</param>
public sealed record DeleteSchoolHolidayRequest(string SeasonId, string Id);
