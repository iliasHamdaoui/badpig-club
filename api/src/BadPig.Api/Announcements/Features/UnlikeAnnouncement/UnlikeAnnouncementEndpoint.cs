using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.UnlikeAnnouncement;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/announcements/{id}/like")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UnlikeAnnouncementEndpoint(IHandler<UnlikeAnnouncementRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The signed-in member no longer likes the announcement.</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">No published announcement with this identifier that the person can read.</response>
    [HttpDelete]
    [EndpointSummary("Removes my like of an announcement")]
    [EndpointDescription("A club member takes back their like. Nothing changes if they had not liked it.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlikeAnnouncement(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new UnlikeAnnouncementRequest(id), ct)).ToNoContent();
}
