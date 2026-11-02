namespace BadPig.Administration.Features.UpdateSchoolHoliday;

/// <summary>JSON body of the change of a period of school holidays.</summary>
/// <param name="Name">Name of the period (1 to 60 characters).</param>
/// <param name="StartsOn">First day without session (included).</param>
/// <param name="EndsOn">Last day without session (included).</param>
public sealed record UpdateSchoolHolidayBody(string? Name, DateOnly? StartsOn, DateOnly? EndsOn);
