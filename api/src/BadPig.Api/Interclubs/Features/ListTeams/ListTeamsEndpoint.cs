using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.ListTeams;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/teams")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListTeamsEndpoint(IHandler<ListTeamsRequest, PagedResponse<ListTeamsItem>> handler) : ControllerBase
{
    /// <param name="request">Season filter, search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of teams.</response>
    /// <response code="400">Unknown sort or invalid page (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    [HttpGet]
    [EndpointSummary("Lists the interclub teams")]
    [EndpointDescription("The teams of the club with their season and their captain. Reserved to the club members with an up-to-date membership (and the board). Optional filter on the season (its identifier, from the seasons of the interclubs); search on the team and captain names; sort on name or season (default: latest season first, then name); paginated.")]
    [ProducesResponseType<PagedResponse<ListTeamsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListTeams([FromQuery] ListTeamsRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
