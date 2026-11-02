using BadPig.Orders.Core;
using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Orders.Features.GetProductImage;

/// <summary>The picture of an article: members see the pictures of the offered articles, the board those of every article.</summary>
internal sealed class GetProductImageHandler(IValidator<GetProductImageRequest> validator, OrderAccess access, IUserContext currentUser, IShopCatalog catalog)
    : IHandler<GetProductImageRequest, GetProductImageResponse>
{
    public async Task<Result<GetProductImageResponse>> HandleAsync(GetProductImageRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanOrderAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var image = await catalog.GetImageAsync(Guid.Parse(request.Id), IsOnTheBoard, ct);
        if (image is null)
            return Error.NotFound();

        return new GetProductImageResponse(image.Content, image.ContentType);
    }

    private bool IsOnTheBoard => currentUser.IsInGroup(ClubGroup.Board) || currentUser.IsInGroup(ClubGroup.Admin);
}
