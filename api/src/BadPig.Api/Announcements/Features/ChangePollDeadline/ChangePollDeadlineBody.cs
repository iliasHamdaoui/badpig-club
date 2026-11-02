namespace BadPig.Announcements.Features.ChangePollDeadline;

/// <summary>JSON body of the change of the closing date of a poll.</summary>
/// <param name="ClosesOn">New last day the answers can be given or changed (yyyy-MM-dd, Paris date, not in the past).</param>
public sealed record ChangePollDeadlineBody(DateOnly? ClosesOn);
