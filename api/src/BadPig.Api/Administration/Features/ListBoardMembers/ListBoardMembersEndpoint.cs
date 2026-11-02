using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.ListBoardMembers;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/club/board")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListBoardMembersEndpoint(IHandler<ListBoardMembersRequest, PagedResponse<ListBoardMembersItem>> handler) : ControllerBase
{
    /// <param name="query">Search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of the people of the board.</response>
    /// <response code="400">Unknown sort or invalid page (see the errors).</response>
    /// <response code="403">The person is not a club member.</response>
    [HttpGet]
    [EndpointSummary("Lists the people of the board")]
    [EndpointDescription("Who is on the board, with their title and the e-mail to write to them. Reserved to the club members (the board included): a visitor of the site never sees these e-mails. Only the active accounts of the board group are listed; search on the name and the title; sort on name (default) or title; paginated.")]
    [ProducesResponseType<PagedResponse<ListBoardMembersItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListBoardMembers([FromQuery] ListBoardMembersRequest query, CancellationToken ct) =>
        (await handler.HandleAsync(query, ct)).ToOk();
}
