using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.UnpublishAnnouncement;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}/unpublish")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UnpublishAnnouncementEndpoint(IHandler<UnpublishAnnouncementRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The announcement is a draft again (nothing changes if it already was).</response>
    /// <response code="404">No announcement with this identifier.</response>
    [HttpPost]
    [EndpointSummary("Unpublishes an announcement")]
    [EndpointDescription("The announcement goes back to draft: only the board can see it. Its publication date is forgotten (a new publication takes a new date).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnpublishAnnouncement(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new UnpublishAnnouncementRequest(id), ct)).ToNoContent();
}
