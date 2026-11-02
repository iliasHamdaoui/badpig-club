namespace BadPig.Shop.Features.RestockVariant;

/// <summary>JSON body of a restocking.</summary>
/// <param name="Quantity">Units received (1 to 100,000).</param>
/// <param name="Note">Optional note (200 characters at most), e.g. the supplier or the delivery note.</param>
public sealed record RestockVariantBody(int? Quantity, string? Note = null);
