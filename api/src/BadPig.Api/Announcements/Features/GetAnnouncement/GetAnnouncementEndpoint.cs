using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.GetAnnouncement;

// No 401 here: visitors who are not signed in can read the public announcements.
[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/announcements/{id}")]
[AllowAnonymous]
public sealed class GetAnnouncementEndpoint(IHandler<GetAnnouncementRequest, GetAnnouncementResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement (the <c>id</c> returned by the list).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">The announcement.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No announcement with this identifier that the reader can see (unknown, draft, or internal for a visitor).</response>
    [HttpGet]
    [EndpointSummary("Gets an announcement")]
    [EndpointDescription("Same visibility rules as the list: a visitor reads public published announcements only, a club member also the internal ones, the board everything. An announcement the reader cannot see is reported as not found.")]
    [ProducesResponseType<GetAnnouncementResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new GetAnnouncementRequest(id), ct)).ToOk();
}
