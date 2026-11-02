using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.DeleteVariant;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/variants/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteVariantEndpoint(IHandler<DeleteVariantRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the variant.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The variant is deleted.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No variant with this identifier.</response>
    /// <response code="409">Orders waiting for their payment hold some of its stock.</response>
    [HttpDelete]
    [EndpointSummary("Deletes a variant")]
    [EndpointDescription("Deletes the variant with its stock history. Orders already made keep their lines. Refused (409) while orders waiting for their payment hold some of its stock: hide the variant instead.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteVariant(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteVariantRequest(id), ct)).ToNoContent();
}
