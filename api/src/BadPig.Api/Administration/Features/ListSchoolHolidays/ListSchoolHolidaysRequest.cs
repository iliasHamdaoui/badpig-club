using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListSchoolHolidays;

/// <summary>Holidays of one season: search (name), sort (startsOn, name; default startsOn ascending) and pagination.</summary>
public sealed record ListSchoolHolidaysRequest : ListQuery
{
    /// <summary>Identifier of the season whose holidays are listed (from the route).</summary>
    public string SeasonId { get; init; } = "";
}
