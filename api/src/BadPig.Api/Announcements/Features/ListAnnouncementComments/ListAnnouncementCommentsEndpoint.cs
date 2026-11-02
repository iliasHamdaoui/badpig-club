using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.ListAnnouncementComments;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/announcements/{id}/comments")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListAnnouncementCommentsEndpoint(IHandler<ListAnnouncementCommentsRequest, PagedResponse<ListAnnouncementCommentsItem>> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="query">Search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of comments.</response>
    /// <response code="400">Unknown sort or invalid page (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">No published announcement with this identifier that the person can read.</response>
    [HttpGet]
    [EndpointSummary("Lists the comments of an announcement")]
    [EndpointDescription("Comments are only shown to the club members and the board. An answer carries a quote of the comment it answers (chat style; shown as deleted when that comment is gone). Oldest first by default.")]
    [ProducesResponseType<PagedResponse<ListAnnouncementCommentsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(string id, [FromQuery] ListQuery query, CancellationToken ct) =>
        (await handler.HandleAsync(new ListAnnouncementCommentsRequest { Id = id, Search = query.Search, Sort = query.Sort, Direction = query.Direction, Page = query.Page, PageSize = query.PageSize }, ct)).ToOk();
}
