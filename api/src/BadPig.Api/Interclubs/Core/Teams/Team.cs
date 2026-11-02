namespace BadPig.Interclubs.Core;

/// <summary>A team of the club in the interclub competition for one season (e.g. "D4" in "2026-2027"), led by a captain.</summary>
public sealed class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";

    /// <summary>The season the team plays in (a season of the club, managed by the board).</summary>
    public Guid SeasonId { get; set; }

    /// <summary>Account identifier of the captain; null until the board designates one (or when the captain's account is deleted).</summary>
    public Guid? CaptainId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
