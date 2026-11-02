using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.ListSeasonStatistics;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/seasons/statistics")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListSeasonStatisticsEndpoint(IHandler<ListSeasonStatisticsRequest, PagedResponse<ListSeasonStatisticsItem>> handler) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Lists the statistics of the seasons")]
    [EndpointDescription("Per season: validated memberships and fees collected, for people licensed by the club and for external members. Search on the season name; sort on startsOn (default, most recent first), name, totalCount or totalRevenue; paginated.")]
    [ProducesResponseType<PagedResponse<ListSeasonStatisticsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] ListSeasonStatisticsRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
