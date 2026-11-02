using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.ListAuditLog;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/audit-log")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListAuditLogEndpoint(IHandler<ListAuditLogRequest, PagedResponse<ListAuditLogItem>> handler) : ControllerBase
{
    /// <response code="200">One page of the audit log.</response>
    /// <response code="400">Invalid sort field.</response>
    [HttpGet]
    [EndpointSummary("Lists the audit log")]
    [EndpointDescription("Actions of the board, most recent first by default. Search on the author, the action code or the concerned person; sort on date, actor or action; paginated. A deleted account no longer has a name (no copy is kept).")]
    [ProducesResponseType<PagedResponse<ListAuditLogItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] ListAuditLogRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
