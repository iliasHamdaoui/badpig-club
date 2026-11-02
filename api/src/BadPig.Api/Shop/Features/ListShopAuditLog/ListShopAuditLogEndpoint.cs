using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.ListShopAuditLog;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/audit-log")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListShopAuditLogEndpoint(IHandler<ListShopAuditLogRequest, PagedResponse<ListShopAuditLogItem>> handler) : ControllerBase
{
    /// <param name="request">Search, sort and pagination.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of the shop audit log.</response>
    /// <response code="400">Invalid sort field.</response>
    [HttpGet]
    [EndpointSummary("Lists the shop audit log")]
    [EndpointDescription("Actions of the board on the shop (articles, variants, stock, handing over of orders), most recent first by default. Search on the author, the action code, the article or the details; sort on date, actor, action or subject; paginated.")]
    [ProducesResponseType<PagedResponse<ListShopAuditLogItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] ListShopAuditLogRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
