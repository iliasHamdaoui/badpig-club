using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Usage.Features.ListUsageStatistics;

[ApiController]
[Tags("Usage")]
[Route(ApiRoutes.V1 + "/admin/usage")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListUsageStatisticsEndpoint(IHandler<ListUsageStatisticsRequest, PagedResponse<ListUsageStatisticsItem>> handler) : ControllerBase
{
    /// <param name="request">Period, search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of the pages of the website with their number of views and the time spent.</response>
    /// <response code="400">Unknown period or sort field (see the errors).</response>
    /// <response code="403">The person is not on the board.</response>
    [HttpGet]
    [EndpointSummary("Lists the usage statistics of the website")]
    [EndpointDescription("Which pages are the most opened and how long they stay open, all people together over the last 7, 30 (default), 90 or 365 days. The statistics are anonymous: nobody can be identified. Search on the page; sort on views (default, most opened first), seconds, average or path; paginated.")]
    [ProducesResponseType<PagedResponse<ListUsageStatisticsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List([FromQuery] ListUsageStatisticsRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
