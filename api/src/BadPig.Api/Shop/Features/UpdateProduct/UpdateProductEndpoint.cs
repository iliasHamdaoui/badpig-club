using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.UpdateProduct;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/products/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UpdateProductEndpoint(IHandler<UpdateProductRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the article.</param>
    /// <param name="body">The new values.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The article is changed.</response>
    /// <response code="400">Invalid name or description (see the errors).</response>
    /// <response code="404">No article with this identifier.</response>
    [HttpPut]
    [EndpointSummary("Changes a shop article")]
    [EndpointDescription("Changes the name, the description and whether the article is offered. A hidden article (isActive = false) leaves the catalogue of the members; orders already made are not touched.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(string id, UpdateProductBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new UpdateProductRequest(id, body), ct)).ToNoContent();
}
