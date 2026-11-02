using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.PublishAnnouncement;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}/publish")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class PublishAnnouncementEndpoint(IHandler<PublishAnnouncementRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The announcement is published.</response>
    /// <response code="404">No announcement with this identifier.</response>
    [HttpPost]
    [EndpointSummary("Publishes an announcement")]
    [EndpointDescription("The announcement becomes visible to its readers (internal: club members; public: any visitor) and takes the publication date of today, which orders the list. Nothing changes if it is already published.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishAnnouncement(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new PublishAnnouncementRequest(id), ct)).ToNoContent();
}
