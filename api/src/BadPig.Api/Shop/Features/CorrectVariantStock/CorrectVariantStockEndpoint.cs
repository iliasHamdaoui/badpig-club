using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.CorrectVariantStock;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/variants/{id}/stock-correction")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CorrectVariantStockEndpoint(IHandler<CorrectVariantStockRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the variant.</param>
    /// <param name="body">The counted quantity and why.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The stock is set (nothing changes if it was already right).</response>
    /// <response code="400">Invalid quantity or missing note (see the errors).</response>
    /// <response code="404">No variant with this identifier.</response>
    /// <response code="409">The quantity is below the units held for unpaid orders, or the stock changed at the same time.</response>
    [HttpPost]
    [EndpointSummary("Sets the real stock of a variant")]
    [EndpointDescription("After counting the shelves, sets the quantity in stock (the units held for unpaid orders included). The difference is written in the stock history with the note, which is required. Refused (409) below the units held for unpaid orders.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CorrectVariantStock(string id, CorrectVariantStockBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new CorrectVariantStockRequest(id, body), ct)).ToNoContent();
}
