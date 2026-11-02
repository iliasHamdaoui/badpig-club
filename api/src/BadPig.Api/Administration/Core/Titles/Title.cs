namespace BadPig.Administration.Core;

/// <summary>
/// A title of the board (Président(e), Secrétaire, Trésorier(e)...). The list is managed by the board itself.
/// Two titles cannot have the same name, case ignored (unique index on lower(label) in the database).
/// </summary>
public sealed class Title
{
    public Guid Id { get; set; }
    public required string Label { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
