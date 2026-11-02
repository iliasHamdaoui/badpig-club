namespace BadPig.Shop.Features.CreateProduct;

/// <summary>JSON body of the creation of an article.</summary>
/// <param name="Name">Name (1 to 100 characters), e.g. "Tube de volants".</param>
/// <param name="Description">Description shown to the members (1,000 characters at most); optional.</param>
public sealed record CreateProductRequest(string? Name, string? Description = null);
