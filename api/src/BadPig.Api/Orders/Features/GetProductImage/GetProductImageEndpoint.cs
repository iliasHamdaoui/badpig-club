using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Orders.Features.GetProductImage;

[ApiController]
[Tags("Orders")]
[Route(ApiRoutes.V1 + "/shop/products/{id}/image")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class GetProductImageEndpoint(IHandler<GetProductImageRequest, GetProductImageResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the article.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">The picture (JPEG, PNG or WebP).</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">The article has no picture, or is not offered to the members.</response>
    [HttpGet]
    [EndpointSummary("Downloads the picture of an article")]
    [EndpointDescription("Reserved to the members with an up-to-date membership (and the board). The address carries the version of the picture (?v=), so the browser may keep it.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImage(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new GetProductImageRequest(id), ct)).ToActionResult(image =>
        {
            Response.Headers.CacheControl = "private, max-age=86400";
            return File(image.Content, image.ContentType);
        });
}
