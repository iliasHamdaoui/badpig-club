using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.ListTeamMembers;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/teams/{id}/members")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListTeamMembersEndpoint(IHandler<ListTeamMembersRequest, PagedResponse<ListTeamMembersItem>> handler) : ControllerBase
{
    /// <param name="id">Identifier of the team.</param>
    /// <param name="query">Search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of players.</response>
    /// <response code="400">Invalid identifier, unknown sort or invalid page (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">No team with this identifier.</response>
    [HttpGet]
    [EndpointSummary("Lists the players of a team")]
    [EndpointDescription("The players of the team (its captain is marked). Reserved to the club members with an up-to-date membership (and the board). Search on the names; sort on name; paginated.")]
    [ProducesResponseType<PagedResponse<ListTeamMembersItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListTeamMembers(string id, [FromQuery] ListQuery query, CancellationToken ct) =>
        (await handler.HandleAsync(new ListTeamMembersRequest { Id = id, Search = query.Search, Sort = query.Sort, Direction = query.Direction, Page = query.Page, PageSize = query.PageSize }, ct)).ToOk();
}
