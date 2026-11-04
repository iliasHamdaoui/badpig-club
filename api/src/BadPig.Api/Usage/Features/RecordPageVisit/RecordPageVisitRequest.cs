namespace BadPig.Usage.Features.RecordPageVisit;

/// <summary>What the website reports when a person leaves a page, or hides the tab (nothing identifies the person).</summary>
/// <param name="Path">The page as a route template, for example <c>/announcements/:id</c> (never an address with identifiers).</param>
/// <param name="Views">1 the first time this opening of the page is reported, 0 for the following reports of the same opening.</param>
/// <param name="Seconds">Seconds the page stayed visible since the previous report (30 minutes at most).</param>
public sealed record RecordPageVisitRequest(string? Path, int Views, int Seconds);
