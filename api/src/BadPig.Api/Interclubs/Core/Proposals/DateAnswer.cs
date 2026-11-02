namespace BadPig.Interclubs.Core;

/// <summary>The answer of a person to a proposed date (one per person and date). Erased with the account.</summary>
public sealed class DateAnswer
{
    public Guid ProposedDateId { get; set; }
    public Guid AccountId { get; set; }

    /// <summary>One of <see cref="DateAnswers"/>.</summary>
    public string Answer { get; set; } = "";

    public DateTimeOffset AnsweredAt { get; set; }
}
