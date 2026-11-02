using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.DeleteProduct;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/products/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteProductEndpoint(IHandler<DeleteProductRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the article.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The article is deleted.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No article with this identifier.</response>
    /// <response code="409">Orders waiting for their payment hold some of its stock.</response>
    [HttpDelete]
    [EndpointSummary("Deletes a shop article")]
    [EndpointDescription("Deletes the article with its variants and their stock history. Orders already made keep their lines (names and prices as sold). Refused (409) while orders waiting for their payment hold some of its stock: hide the article instead.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProduct(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteProductRequest(id), ct)).ToNoContent();
}
