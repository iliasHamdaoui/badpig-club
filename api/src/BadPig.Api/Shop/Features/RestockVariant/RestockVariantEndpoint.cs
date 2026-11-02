using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.RestockVariant;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/variants/{id}/restock")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class RestockVariantEndpoint(IHandler<RestockVariantRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the variant.</param>
    /// <param name="body">The units received.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The stock is increased.</response>
    /// <response code="400">Invalid quantity or note (see the errors).</response>
    /// <response code="404">No variant with this identifier.</response>
    [HttpPost]
    [EndpointSummary("Adds units to the stock of a variant")]
    [EndpointDescription("Records a delivery (units added to the stock) in the stock history, with the name of the board member and an optional note.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestockVariant(string id, RestockVariantBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new RestockVariantRequest(id, body), ct)).ToNoContent();
}
