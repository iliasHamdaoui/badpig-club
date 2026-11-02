using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.ListClubSessions;

// No 401 here: a visitor who is not signed in reads the hours of the club like anybody else.
[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/club/sessions")]
[AllowAnonymous]
public sealed class ListClubSessionsEndpoint(IHandler<ListClubSessionsRequest, ListClubSessionsResponse> handler) : ControllerBase
{
    /// <param name="count">Number of sessions to come to return (1 to 10, 3 by default).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">The weekly schedule, today's answer and the next sessions.</response>
    /// <response code="400">The number of sessions is out of range.</response>
    [HttpGet]
    [EndpointSummary("Tells when the club plays")]
    [EndpointDescription("Open to everybody. The club plays the same evenings every week, except during the school holidays the board records for each season: this answer says whether there is a session today (Paris date), which holidays close the club when there is none, and the next sessions.")]
    [ProducesResponseType<ListClubSessionsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListSessions([FromQuery] int? count, CancellationToken ct) =>
        (await handler.HandleAsync(new ListClubSessionsRequest(count), ct)).ToOk();
}
