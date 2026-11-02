namespace BadPig.Interclubs.Core;

/// <summary>A fixture (rencontre) of a team against another club, at home or away.</summary>
public sealed class Fixture
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public DateOnly PlayedOn { get; set; }
    public TimeOnly? StartTime { get; set; }

    /// <summary>The opposing club.</summary>
    public string Opponent { get; set; } = "";

    /// <summary>One of <see cref="FixtureVenues"/>.</summary>
    public string Venue { get; set; } = "";

    /// <summary>The gym or the address, when the captain gives it.</summary>
    public string? Place { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
