using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.UnpinAnnouncement;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}/unpin")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UnpinAnnouncementEndpoint(IHandler<UnpinAnnouncementRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The announcement is no longer pinned (nothing changes if it was not).</response>
    /// <response code="404">No announcement with this identifier.</response>
    [HttpPost]
    [EndpointSummary("Unpins an announcement")]
    [EndpointDescription("The announcement goes back to its place in the chronological order.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnpinAnnouncement(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new UnpinAnnouncementRequest(id), ct)).ToNoContent();
}
