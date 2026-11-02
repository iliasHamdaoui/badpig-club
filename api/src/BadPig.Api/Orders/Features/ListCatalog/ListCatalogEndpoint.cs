using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Orders.Features.ListCatalog;

[ApiController]
[Tags("Orders")]
[Route(ApiRoutes.V1 + "/shop/products")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListCatalogEndpoint(IHandler<ListCatalogRequest, PagedResponse<ListCatalogItem>> handler) : ControllerBase
{
    /// <param name="request">Search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of articles.</response>
    /// <response code="400">Unknown sort or invalid page (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    [HttpGet]
    [EndpointSummary("Lists the articles for sale")]
    [EndpointDescription("The catalogue of the shop: the offered articles with their offered variants, prices and available stock. Reserved to the club members with an up-to-date membership (and the board). Search on the names; sort on name; paginated.")]
    [ProducesResponseType<PagedResponse<ListCatalogItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListCatalog([FromQuery] ListCatalogRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
