namespace BadPig.Administration.Core;

/// <summary>
/// The federation licence number of a person (FFBaD), kept by the club: one row per account, erased with it.
/// The person changes it from their profile, the board from the list of the members.
/// </summary>
public sealed class MemberLicence
{
    public Guid AccountId { get; set; }

    /// <summary>Licence number as the federation writes it (letters, digits and dashes).</summary>
    public required string Number { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
