using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.ListProducts;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/products")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListProductsEndpoint(IHandler<ListProductsRequest, PagedResponse<ListProductsItem>> handler) : ControllerBase
{
    /// <param name="request">Search, filter, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of articles.</response>
    /// <response code="400">Unknown sort or invalid page (see the errors).</response>
    [HttpGet]
    [EndpointSummary("Lists the shop articles (board)")]
    [EndpointDescription("Every article, hidden ones included, with its variants, prices and stock (on the shelves, reserved by unpaid orders, available). Search on the names; lowStock=true keeps the articles that have a variant at or under its alert threshold; sort on name (default) or createdAt; paginated.")]
    [ProducesResponseType<PagedResponse<ListProductsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListProducts([FromQuery] ListProductsRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
