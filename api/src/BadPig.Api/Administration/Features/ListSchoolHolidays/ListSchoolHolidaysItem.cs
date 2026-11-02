namespace BadPig.Administration.Features.ListSchoolHolidays;

/// <summary>One period of school holidays of the season.</summary>
/// <param name="Id">Identifier of the period.</param>
/// <param name="Name">Name of the period, e.g. "Vacances de la Toussaint".</param>
/// <param name="StartsOn">First day without session (included).</param>
/// <param name="EndsOn">Last day without session (included).</param>
public sealed record ListSchoolHolidaysItem(string Id, string Name, DateOnly StartsOn, DateOnly EndsOn);
