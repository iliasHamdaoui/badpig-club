using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.MarkOrderDelivered;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/orders/{id}/deliver")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class MarkOrderDeliveredEndpoint(IHandler<MarkOrderDeliveredRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the order.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The order is marked as delivered.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No order with this identifier.</response>
    /// <response code="409">The order is not paid, or already delivered.</response>
    [HttpPost]
    [EndpointSummary("Marks an order as handed over")]
    [EndpointDescription("The board hands the paid order over to the member (at the gym) and marks it. Only a paid order can be handed over.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkOrderDelivered(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new MarkOrderDeliveredRequest(id), ct)).ToNoContent();
}
