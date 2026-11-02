using BadPig.Orders.Core;
using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Orders.Features.ListCatalog;

/// <summary>The offered articles for the members.</summary>
internal sealed class ListCatalogHandler(IValidator<ListCatalogRequest> validator, OrderAccess access, OrderSettlement settlement, IShopCatalog catalog)
    : IHandler<ListCatalogRequest, PagedResponse<ListCatalogItem>>
{
    public async Task<Result<PagedResponse<ListCatalogItem>>> HandleAsync(ListCatalogRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanOrderAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        // Units held by orders whose time is over are available again before the stock is read.
        await settlement.ReleaseExpiredAsync(ct);

        var page = await catalog.ListOfferedAsync(request, ct);
        return page.Map(ToItem);
    }

    private static ListCatalogItem ToItem(OfferedProduct product) => new(product.Id.ToString(), product.Name, product.Description, product.ImageVersion,
        product.Variants.Select(v => new ListCatalogVariant(v.Id.ToString(), v.Label, v.PriceCents, v.Available, v.Available > 0)).ToList());
}
