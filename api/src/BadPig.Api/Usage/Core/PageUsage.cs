namespace BadPig.Usage.Core;

/// <summary>
/// What was done with a page of the website on a day, all people together: the page is a route template
/// (<c>/announcements/:id</c>, never the address with its identifiers). Nothing identifies a person.
/// </summary>
public sealed class PageUsage
{
    /// <summary>Day in Paris.</summary>
    public DateOnly Day { get; set; }
    public string Path { get; set; } = "";

    /// <summary>How many times the page was opened.</summary>
    public int Views { get; set; }

    /// <summary>Total time the page stayed visible, in seconds.</summary>
    public long Seconds { get; set; }
}
