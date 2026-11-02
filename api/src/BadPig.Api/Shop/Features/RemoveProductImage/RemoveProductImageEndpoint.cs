using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.RemoveProductImage;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/products/{id}/image")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class RemoveProductImageEndpoint(IHandler<RemoveProductImageRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the article.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The picture is removed (or the article had none).</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No article with this identifier.</response>
    [HttpDelete]
    [EndpointSummary("Removes the picture of an article")]
    [EndpointDescription("The article is shown without a picture afterwards.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveImage(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new RemoveProductImageRequest(id), ct)).ToNoContent();
}
