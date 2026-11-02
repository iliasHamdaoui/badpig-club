using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Orders.Features.ListMyOrders;

[ApiController]
[Tags("Orders")]
[Route(ApiRoutes.V1 + "/shop/orders")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListMyOrdersEndpoint(IHandler<ListMyOrdersRequest, PagedResponse<ListMyOrdersItem>> handler) : ControllerBase
{
    /// <param name="request">Filter, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of orders.</response>
    /// <response code="400">Unknown sort or status, or invalid page (see the errors).</response>
    [HttpGet]
    [EndpointSummary("Lists my orders")]
    [EndpointDescription("The orders of the signed-in person, whatever their state. Filter on the status; sort on createdAt (default, most recent first) or totalCents; paginated.")]
    [ProducesResponseType<PagedResponse<ListMyOrdersItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListMyOrders([FromQuery] ListMyOrdersRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
