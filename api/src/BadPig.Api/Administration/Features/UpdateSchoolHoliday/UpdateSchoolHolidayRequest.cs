namespace BadPig.Administration.Features.UpdateSchoolHoliday;

/// <summary>Change of a period of school holidays.</summary>
/// <param name="SeasonId">Identifier of the season (from the route).</param>
/// <param name="Id">Identifier of the period (from the route).</param>
/// <param name="Body">The new values.</param>
public sealed record UpdateSchoolHolidayRequest(string SeasonId, string Id, UpdateSchoolHolidayBody Body);
