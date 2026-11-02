namespace BadPig.Administration.Core;

/// <summary>
/// A season of the club (usually 1 September to 31 August) with its two yearly fees. The board manages the seasons;
/// the current season is the one that contains today's date (see <see cref="SeasonRules"/>).
/// </summary>
public sealed class Season
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }

    /// <summary>Number of days after the start during which the membership of the previous season is still accepted.</summary>
    public int GraceDays { get; set; } = SeasonRules.DefaultGraceDays;

    /// <summary>Yearly fee of a person licensed by the club.</summary>
    public decimal InternalFee { get; set; }

    /// <summary>Yearly fee of a person licensed in another club.</summary>
    public decimal ExternalFee { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
