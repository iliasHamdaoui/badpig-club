using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.ListTeamCandidates;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/teams/{id}/candidates")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListTeamCandidatesEndpoint(IHandler<ListTeamCandidatesRequest, PagedResponse<ListTeamCandidatesItem>> handler) : ControllerBase
{
    /// <param name="id">Identifier of the team.</param>
    /// <param name="query">Search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of people.</response>
    /// <response code="400">Invalid identifier, unknown sort or invalid page (see the errors).</response>
    /// <response code="403">The person is neither the captain of this team nor on the board.</response>
    /// <response code="404">No team with this identifier.</response>
    [HttpGet]
    [EndpointSummary("Lists the people who can be added to a team")]
    [EndpointDescription("For the captain of the team (or the board), who picks the players: the active club members who are not in the team yet. Search on the names; sort on name; paginated.")]
    [ProducesResponseType<PagedResponse<ListTeamCandidatesItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListTeamCandidates(string id, [FromQuery] ListQuery query, CancellationToken ct) =>
        (await handler.HandleAsync(new ListTeamCandidatesRequest { Id = id, Search = query.Search, Sort = query.Sort, Direction = query.Direction, Page = query.Page, PageSize = query.PageSize }, ct)).ToOk();
}
