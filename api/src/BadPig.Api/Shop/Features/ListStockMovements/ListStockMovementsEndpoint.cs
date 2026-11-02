using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.ListStockMovements;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/variants/{id}/movements")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListStockMovementsEndpoint(IHandler<ListStockMovementsRequest, PagedResponse<ListStockMovementsItem>> handler) : ControllerBase
{
    /// <param name="id">Identifier of the variant.</param>
    /// <param name="query">Search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of the history.</response>
    /// <response code="400">Unknown sort or invalid page (see the errors).</response>
    /// <response code="404">No variant with this identifier.</response>
    [HttpGet]
    [EndpointSummary("Lists the stock history of a variant")]
    [EndpointDescription("Deliveries, corrections and sales, with the board member who made them. Search on the note and the name; sort on createdAt (default, most recent first); paginated.")]
    [ProducesResponseType<PagedResponse<ListStockMovementsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListStockMovements(string id, [FromQuery] ListQuery query, CancellationToken ct) =>
        (await handler.HandleAsync(new ListStockMovementsRequest { Id = id, Search = query.Search, Sort = query.Sort, Direction = query.Direction, Page = query.Page, PageSize = query.PageSize }, ct)).ToOk();
}
