using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Usage.Features.GetUsageTrend;

[ApiController]
[Tags("Usage")]
[Route(ApiRoutes.V1 + "/admin/usage/trend")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class GetUsageTrendEndpoint(IHandler<GetUsageTrendRequest, GetUsageTrendResponse> handler) : ControllerBase
{
    /// <param name="days">Period counted, ending today: 7, 30 (default), 90 or 365 days.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One entry per day of the period, with the views and the time spent, all pages together.</response>
    /// <response code="400">Unknown period (see the errors).</response>
    /// <response code="403">The person is not on the board.</response>
    [HttpGet]
    [EndpointSummary("Gets the usage of the website day by day")]
    [EndpointDescription("The series of the chart of the usage statistics: for each day of the last 7, 30 (default), 90 or 365 days (Paris), the pages opened and the time they stayed visible, all pages and all people together. Anonymous. It is a bounded series (365 days at most), not a list to page.")]
    [ProducesResponseType<GetUsageTrendResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromQuery] int? days, CancellationToken ct) =>
        (await handler.HandleAsync(new GetUsageTrendRequest(days), ct)).ToOk();
}
