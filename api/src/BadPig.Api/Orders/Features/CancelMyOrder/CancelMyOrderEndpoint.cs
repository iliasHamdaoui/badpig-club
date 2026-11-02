using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Orders.Features.CancelMyOrder;

[ApiController]
[Tags("Orders")]
[Route(ApiRoutes.V1 + "/shop/orders/{id}/cancel")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CancelMyOrderEndpoint(IHandler<CancelMyOrderRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the order.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The order is cancelled.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No order with this identifier made by the signed-in person.</response>
    /// <response code="409">The order is not waiting for its payment (already paid, cancelled or expired).</response>
    [HttpPost]
    [EndpointSummary("Cancels one of my orders")]
    [EndpointDescription("Cancels an order that is still waiting for its payment: the stock is released and the payment page is closed. A paid order cannot be cancelled here.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelMyOrder(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new CancelMyOrderRequest(id), ct)).ToNoContent();
}
