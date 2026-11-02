namespace BadPig.Administration.Core;

/// <summary>
/// A period of school holidays of a season: the club closes, there is no session.
/// Both days are included (a holiday from the 18th to the 2nd closes the club on the 18th and on the 2nd).
/// </summary>
public sealed class SchoolHoliday
{
    public Guid Id { get; set; }

    /// <summary>The season these holidays belong to; they disappear with it.</summary>
    public Guid SeasonId { get; set; }

    /// <summary>What the board calls them, e.g. "Vacances de la Toussaint".</summary>
    public required string Name { get; set; }

    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
