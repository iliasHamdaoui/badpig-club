using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Orders.Features.GetMyOrder;

[ApiController]
[Tags("Orders")]
[Route(ApiRoutes.V1 + "/shop/orders/{id}")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class GetMyOrderEndpoint(IHandler<GetMyOrderRequest, GetMyOrderResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the order.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">The order.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No order with this identifier made by the signed-in person.</response>
    [HttpGet]
    [EndpointSummary("Reads one of my orders")]
    [EndpointDescription("The order with its lines and, while it waits for its payment, the address of its payment page (checkoutUrl). Looking at a pending order asks the payment provider whether it was paid, so the page shown when the person comes back from the payment is up to date even if the notification is late.")]
    [ProducesResponseType<GetMyOrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyOrder(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new GetMyOrderRequest(id), ct)).ToOk();
}
