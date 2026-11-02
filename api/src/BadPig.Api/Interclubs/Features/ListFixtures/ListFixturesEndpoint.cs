using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.ListFixtures;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/fixtures")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListFixturesEndpoint(IHandler<ListFixturesRequest, PagedResponse<ListFixturesItem>> handler) : ControllerBase
{
    /// <param name="request">Filters, search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of fixtures.</response>
    /// <response code="400">Unknown sort, invalid filter or invalid page (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    [HttpGet]
    [EndpointSummary("Lists the fixtures (calendar)")]
    [EndpointDescription("The calendar of the fixtures of all the teams. Reserved to the club members with an up-to-date membership (and the board). Optional filters on a season, on a team and on the period: upcoming (today, in Paris, or later), past, or live (being played right now: of today, matches not all decided, and the time of day within a window that opens a quarter of an hour before the start and lasts five hours; a fixture without a start time counts all day long). Each fixture also carries how many of its seven matches are decided and the matches won so far by each side, so that a result being entered can be followed. Search on the opponent and team names; sort on date, team or opponent; paginated. Default: by date (the past ones, latest first).")]
    [ProducesResponseType<PagedResponse<ListFixturesItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListFixtures([FromQuery] ListFixturesRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
