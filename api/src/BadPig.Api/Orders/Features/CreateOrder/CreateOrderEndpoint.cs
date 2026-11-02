using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Orders.Features.CreateOrder;

[ApiController]
[Tags("Orders")]
[Route(ApiRoutes.V1 + "/shop/orders")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreateOrderEndpoint(IHandler<CreateOrderRequest, CreatedResponse> handler) : ControllerBase
{
    /// <param name="request">The articles ordered.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="201">The order is made and its stock held: its identifier is returned.</response>
    /// <response code="400">No article, too many, or an invalid quantity (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">One of the variants does not exist.</response>
    /// <response code="409">An article is no longer offered or not available in this quantity, or the person already has an unpaid order.</response>
    /// <response code="500">Online payment or the site address is not configured on the server.</response>
    /// <response code="502">The payment provider is unreachable.</response>
    [HttpPost]
    [EndpointSummary("Orders articles")]
    [EndpointDescription("Makes an order of articles of the catalogue and holds their stock for 35 minutes. The order starts as pending; its payment page address (checkoutUrl) is read with GET /shop/orders/{id}: send the person there to pay (card, Apple Pay, Google Pay). Paid in time, the order is confirmed by e-mail; otherwise it expires and the stock is released. One unpaid order at a time per person.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToCreated();
}
