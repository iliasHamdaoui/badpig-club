using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.ListInterclubSeasons;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/seasons")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListInterclubSeasonsEndpoint(IHandler<ListInterclubSeasonsRequest, PagedResponse<ListInterclubSeasonsItem>> handler) : ControllerBase
{
    /// <param name="request">Search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of seasons.</response>
    /// <response code="400">Unknown sort or invalid page (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    [HttpGet]
    [EndpointSummary("Lists the seasons of the interclubs")]
    [EndpointDescription("The seasons the site offers to filter the interclubs: the seasons that have teams, plus the current season of the club even when it has none yet (it is the one marked isCurrent, selected by default). Reserved to the club members with an up-to-date membership (and the board). Search on the name; sort on name or start; paginated (the latest season first).")]
    [ProducesResponseType<PagedResponse<ListInterclubSeasonsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListSeasons([FromQuery] ListInterclubSeasonsRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
