namespace BadPig.Shop.Features.UpdateProduct;

/// <summary>JSON body of the change of an article.</summary>
/// <param name="Name">Name (1 to 100 characters).</param>
/// <param name="Description">Description shown to the members (1,000 characters at most); empty to remove it.</param>
/// <param name="IsActive">True = offered to the members; false = hidden.</param>
public sealed record UpdateProductBody(string? Name, string? Description, bool? IsActive);
