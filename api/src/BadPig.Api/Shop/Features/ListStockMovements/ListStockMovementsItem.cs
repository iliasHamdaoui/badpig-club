namespace BadPig.Shop.Features.ListStockMovements;

/// <summary>One line of the stock history.</summary>
/// <param name="Id">Identifier of the line.</param>
/// <param name="Change">Units added (positive) or removed (negative).</param>
/// <param name="Reason">restock (delivery), correction (count) or sale (order paid).</param>
/// <param name="Note">Note of the board member; empty for a sale.</param>
/// <param name="AuthorName">Board member who made the change; empty for a sale or once the account is deleted.</param>
/// <param name="OrderId">The order that was paid, for a sale.</param>
/// <param name="CreatedAt">Date.</param>
public sealed record ListStockMovementsItem(string Id, int Change, string Reason, string Note, string AuthorName, string? OrderId, DateTimeOffset CreatedAt);
