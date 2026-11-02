using BadPig.Shared.Core;

namespace BadPig.Shop.Features.ListStockMovements;

/// <summary>Stock history of a variant: search (note, author), sort (createdAt; default most recent first) and pagination.</summary>

public sealed record ListStockMovementsRequest : ListQuery
{
    /// <summary>Identifier of the variant (taken from the route).</summary>
    public string Id { get; init; } = "";
}
