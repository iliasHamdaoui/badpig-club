namespace BadPig.Administration.Core;

/// <summary>Who holds which title: a person has at most one title, and a title has only one holder.</summary>
public sealed class BoardTitle
{
    public Guid MemberId { get; set; }
    public Guid TitleId { get; set; }
    public Title? Title { get; set; }
}
