using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.PinAnnouncement;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}/pin")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class PinAnnouncementEndpoint(IHandler<PinAnnouncementRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The announcement is pinned (nothing changes if it already is).</response>
    /// <response code="404">No announcement with this identifier.</response>
    [HttpPost]
    [EndpointSummary("Pins an announcement")]
    [EndpointDescription("A pinned announcement is listed before the others, whatever its date.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PinAnnouncement(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new PinAnnouncementRequest(id), ct)).ToNoContent();
}
