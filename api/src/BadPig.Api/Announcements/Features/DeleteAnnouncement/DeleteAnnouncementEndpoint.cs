using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.DeleteAnnouncement;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteAnnouncementEndpoint(IHandler<DeleteAnnouncementRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The announcement, its files and its likes are deleted.</response>
    /// <response code="404">No announcement with this identifier.</response>
    [HttpDelete]
    [EndpointSummary("Deletes an announcement")]
    [EndpointDescription("Any board member can delete any announcement, published or not. Its poster, attachments and likes are deleted with it.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAnnouncement(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteAnnouncementRequest(id), ct)).ToNoContent();
}
