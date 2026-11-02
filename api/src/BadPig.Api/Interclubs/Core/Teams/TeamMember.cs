namespace BadPig.Interclubs.Core;

/// <summary>A player of a team, added by its captain (or the board). Erased with the account.</summary>
public sealed class TeamMember
{
    public Guid TeamId { get; set; }
    public Guid AccountId { get; set; }
    public DateTimeOffset AddedAt { get; set; }
}
