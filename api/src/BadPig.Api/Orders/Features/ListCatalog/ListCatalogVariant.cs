namespace BadPig.Orders.Features.ListCatalog;

/// <summary>One variant of an article for sale.</summary>
/// <param name="Id">Variant identifier, to give when ordering.</param>
/// <param name="Label">Label, e.g. "Vitesse 77" or "M - Homme".</param>
/// <param name="PriceCents">Price of one unit in euro cents.</param>
/// <param name="Available">Units that can be ordered now.</param>
/// <param name="InStock">False when there is nothing left to order.</param>
public sealed record ListCatalogVariant(string Id, string Label, int PriceCents, int Available, bool InStock);
