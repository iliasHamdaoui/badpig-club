namespace BadPig.Interclubs.Core;

/// <summary>
/// Two dates or more offered by the captain for the next fixture of a team (same opponent and venue), to see who is available.
/// Once the captain chooses a date, the fixture is created and the proposal is closed (<see cref="FixtureId"/> is set).
/// </summary>
public sealed class DateProposal
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string Opponent { get; set; } = "";

    /// <summary>One of <see cref="FixtureVenues"/>.</summary>
    public string Venue { get; set; } = "";

    public string? Place { get; set; }

    /// <summary>The fixture created from the chosen date; null while the proposal is open.</summary>
    public Guid? FixtureId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public List<ProposedDate> Dates { get; set; } = [];

    public bool IsOpen => FixtureId is null;
}
