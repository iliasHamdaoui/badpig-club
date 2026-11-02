using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.CreateProduct;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/products")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreateProductEndpoint(IHandler<CreateProductRequest, CreatedResponse> handler) : ControllerBase
{
    /// <param name="request">The article.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="201">The article is created: its identifier is returned.</response>
    /// <response code="400">Invalid name or description (see the errors).</response>
    [HttpPost]
    [EndpointSummary("Creates a shop article")]
    [EndpointDescription("Creates an article (a tube of shuttlecocks, a club shirt...). Add its variants (type, size, price, stock) afterwards: the article appears in the catalogue of the members once it has an active variant.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToCreated();
}
