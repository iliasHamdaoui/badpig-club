namespace BadPig.Administration.Features.CreateSchoolHoliday;

/// <summary>JSON body of the creation of a period of school holidays.</summary>
/// <param name="Name">Name of the period (1 to 60 characters), e.g. "Vacances de la Toussaint".</param>
/// <param name="StartsOn">First day without session (included).</param>
/// <param name="EndsOn">Last day without session (included).</param>
public sealed record CreateSchoolHolidayBody(string? Name, DateOnly? StartsOn, DateOnly? EndsOn);
