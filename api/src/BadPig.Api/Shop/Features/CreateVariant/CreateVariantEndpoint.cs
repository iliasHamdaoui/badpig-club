using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.CreateVariant;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/products/{id}/variants")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreateVariantEndpoint(IHandler<CreateVariantRequest, CreatedResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the article.</param>
    /// <param name="body">The variant.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="201">The variant is created: its identifier is returned.</response>
    /// <response code="400">Invalid label, price or quantity (see the errors).</response>
    /// <response code="404">No article with this identifier.</response>
    /// <response code="409">The article already has a variant with this label.</response>
    [HttpPost]
    [EndpointSummary("Adds a variant to an article")]
    [EndpointDescription("A variant is one way to buy the article (a type of shuttlecocks, a size and gender of shirt...) with its own price and stock. The initial quantity, if any, is written in the stock history.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateVariant(string id, CreateVariantBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new CreateVariantRequest(id, body), ct)).ToCreated();
}
