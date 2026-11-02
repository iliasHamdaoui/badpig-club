using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.UpdateVariant;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/variants/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UpdateVariantEndpoint(IHandler<UpdateVariantRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the variant.</param>
    /// <param name="body">The new values.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The variant is changed.</response>
    /// <response code="400">Invalid label, price or threshold (see the errors).</response>
    /// <response code="404">No variant with this identifier.</response>
    /// <response code="409">Another variant of the article has this label.</response>
    [HttpPut]
    [EndpointSummary("Changes a variant")]
    [EndpointDescription("Changes the label, the price, the alert threshold and whether the variant is offered. The stock is changed with the restock and correction actions. Orders already made keep the price they were made at.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateVariant(string id, UpdateVariantBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new UpdateVariantRequest(id, body), ct)).ToNoContent();
}
