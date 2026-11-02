namespace BadPig.Shop.Core;

/// <summary>
/// One action of the board on the shop. It keeps no personal data about members: the subject is the name of an article or variant
/// (copied, so that it stays readable once the article is deleted), the target the identifier of the article, variant or order.
/// Only the name of the board member who acts is kept (proof of who did what). The table is immutable: a database trigger refuses any update or delete.
/// </summary>
public sealed class ShopAuditEntry
{
    public long Id { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Guid ActorId { get; set; }
    public required string ActorName { get; set; }

    /// <summary>Action code, e.g. "variant.restocked".</summary>
    public required string Action { get; set; }

    public Guid? TargetId { get; set; }

    /// <summary>Name of the article or "article – variant" concerned.</summary>
    public string? Subject { get; set; }

    /// <summary>What changed, in a short sentence for the board (quantity, price…).</summary>
    public string? Details { get; set; }
}
