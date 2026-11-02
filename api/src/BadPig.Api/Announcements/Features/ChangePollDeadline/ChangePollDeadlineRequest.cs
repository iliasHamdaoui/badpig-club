namespace BadPig.Announcements.Features.ChangePollDeadline;

/// <summary>New closing date of a poll: the route identifier and the date.</summary>
/// <param name="Id">Identifier of the announcement.</param>
/// <param name="Body">The new date.</param>
public sealed record ChangePollDeadlineRequest(string Id, ChangePollDeadlineBody Body);
