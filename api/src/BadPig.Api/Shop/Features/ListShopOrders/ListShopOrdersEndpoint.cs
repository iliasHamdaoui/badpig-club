using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.ListShopOrders;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/orders")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListShopOrdersEndpoint(IHandler<ListShopOrdersRequest, PagedResponse<ListShopOrdersItem>> handler) : ControllerBase
{
    /// <param name="request">Search, filter, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of orders.</response>
    /// <response code="400">Unknown sort or status, or invalid page (see the errors).</response>
    [HttpGet]
    [EndpointSummary("Lists the orders (board)")]
    [EndpointDescription("Every order with its lines. Filter on the status (pending, paid, delivered, cancelled, expired); search on the buyer name; sort on createdAt (default, most recent first), totalCents or buyerName; paginated. Unpaid orders whose time is over are settled first.")]
    [ProducesResponseType<PagedResponse<ListShopOrdersItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListShopOrders([FromQuery] ListShopOrdersRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
